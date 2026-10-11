using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using BossPractice.Common.Bosses;
using BossPractice.Common.Configs;

namespace BossPractice.Common.Practice;

/// <summary>
/// Spawns and despawns practice bosses. Everything that changes the world runs on the server
/// (or locally in singleplayer); clients go through the Request* methods, which send packets.
/// </summary>
public static class PracticeManager
{
	public const float MinHealthPercent = 0.01f;

	/// <summary>Give up waiting for a boss's parts after this many ticks and apply health with whatever exists.</summary>
	private const int SetupTimeoutTicks = 600;

	private static readonly List<PendingSetup> pendingSetups = [];
	private static int lastSessionId;

	/// <summary>While non-zero, every NPC spawned is tagged with this practice session.</summary>
	internal static int CapturingSessionId { get; private set; }

	internal static List<NPC> CapturedNPCs { get; } = [];

	/// <summary>True while a phase preset is killing boss parts, so those kills aren't reported as victories.</summary>
	internal static bool ApplyingPhaseSetup { get; private set; }

	public static Color InfoColor => new(120, 220, 255);

	public static Color ErrorColor => new(255, 120, 120);

	public static void RequestSpawn(BossDefinition boss, float healthPercent, int phaseIndex)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) {
			PracticeNetwork.SendSpawnRequest(BossRegistry.IndexOf(boss), healthPercent, phaseIndex);
			return;
		}

		TrySpawn(Main.LocalPlayer, boss, healthPercent, phaseIndex);
	}

	public static void RequestDespawn()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) {
			PracticeNetwork.SendSimpleRequest(PracticeMessageType.Despawn);
			return;
		}

		DespawnAll(Main.LocalPlayer);
	}

	public static void RequestNight()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) {
			PracticeNetwork.SendSimpleRequest(PracticeMessageType.MakeNight);
			return;
		}

		MakeNight(Main.LocalPlayer);
	}

	/// <summary>Server / singleplayer only.</summary>
	public static bool TrySpawn(Player player, BossDefinition boss, float healthPercent, int phaseIndex)
	{
		healthPercent = Math.Clamp(healthPercent, MinHealthPercent, 1f);
		BossPhase phase = phaseIndex >= 0 && phaseIndex < boss.Phases.Count ? boss.Phases[phaseIndex] : null;

		if (!player.active || player.dead) {
			Reply(player, "[Practice] You can't spawn a boss while dead.", ErrorColor);
			return false;
		}

		if (boss.Style == SpawnStyle.WallOfFlesh && !player.ZoneUnderworldHeight) {
			Reply(player, $"[Practice] {boss.DisplayName} can only be spawned in the Underworld.", ErrorColor);
			return false;
		}

		if (ModContent.GetInstance<PracticeConfig>().DespawnBossesBeforeSpawning) {
			DespawnBosses();
		}

		int sessionId = ++lastSessionId;
		CapturingSessionId = sessionId;
		CapturedNPCs.Clear();
		try {
			foreach (int type in boss.SpawnTypes) {
				SpawnPart(player, boss, type);
			}
		}
		finally {
			CapturingSessionId = 0;
		}

		if (CapturedNPCs.Count == 0) {
			Reply(player, $"[Practice] Couldn't spawn {boss.DisplayName} here. Try somewhere more open.", ErrorColor);
			return false;
		}

		CapturedNPCs.Clear();
		pendingSetups.Add(new PendingSetup(sessionId, boss, healthPercent, phase));

		string phaseText = phase != null ? $" ({phase.Name})" : "";
		Announce($"[Practice] {player.name} spawned {boss.DisplayName} at {healthPercent * 100f:0.#}% health{phaseText}.", InfoColor);
		return true;
	}

	private static void SpawnPart(Player player, BossDefinition boss, int type)
	{
		int spawnedBefore = CapturedNPCs.Count(npc => npc.type == type);

		if (boss.Style == SpawnStyle.WallOfFlesh) {
			Main.wofNPCIndex = -1;
			NPC.SpawnWOF(player.Center);
			return;
		}

		// The vanilla routine finds an off-screen position and handles special cases (Golem needs an altar,
		// Duke Fishron needs a bobber, ...). When it gives up, place the boss next to the player instead.
		NPC.SpawnOnPlayer(player.whoAmI, type);
		if (CapturedNPCs.Count(npc => npc.type == type) == spawnedBefore) {
			Vector2 position = player.Center + boss.FallbackOffset;
			NPC.SpawnBoss((int)position.X, (int)position.Y, type, player.whoAmI);
		}
	}

	/// <summary>Server / singleplayer only. Called once per tick after NPCs update.</summary>
	internal static void Update()
	{
		for (int i = pendingSetups.Count - 1; i >= 0; i--) {
			PendingSetup setup = pendingSetups[i];
			setup.Age++;

			List<NPC> npcs = GetSessionNPCs(setup.SessionId);
			if (npcs.Count == 0) {
				// Despawned or killed before it was ready.
				pendingSetups.RemoveAt(i);
				continue;
			}

			// Bosses spawn their parts during their first AI update (Moon Lord after a short intro).
			bool ready = setup.Age >= 2 && setup.Boss.RequiredTypes.All(type => npcs.Any(npc => npc.type == type));
			if (!ready && setup.Age < SetupTimeoutTicks) {
				continue;
			}

			Apply(setup, npcs);
			pendingSetups.RemoveAt(i);
		}
	}

	private static void Apply(PendingSetup setup, List<NPC> npcs)
	{
		if (setup.HealthPercent < 1f) {
			foreach (NPC npc in npcs) {
				// Segments that share a health pool (Destroyer, Wall of Flesh eyes) point realLife at the owner.
				bool ownsHealth = npc.realLife < 0 || npc.realLife == npc.whoAmI;
				if (ownsHealth && setup.Boss.HealthTypes.Contains(npc.type)) {
					npc.life = Math.Max(1, (int)Math.Round(npc.lifeMax * setup.HealthPercent));
					npc.netUpdate = true;
				}
			}
		}

		if (setup.Phase?.Setup == null) {
			return;
		}

		ApplyingPhaseSetup = true;
		try {
			setup.Phase.Setup(npcs);
		}
		finally {
			ApplyingPhaseSetup = false;
		}
	}

	private static List<NPC> GetSessionNPCs(int sessionId)
	{
		List<NPC> npcs = [];
		foreach (NPC npc in Main.ActiveNPCs) {
			if (npc.GetGlobalNPC<PracticeGlobalNPC>().SessionId == sessionId) {
				npcs.Add(npc);
			}
		}

		return npcs;
	}

	/// <summary>Server / singleplayer only.</summary>
	public static void DespawnAll(Player requester)
	{
		int count = DespawnBosses();
		if (ModContent.GetInstance<PracticeConfig>().ClearHostileProjectilesOnDespawn) {
			ClearHostileProjectiles();
		}

		Reply(requester, count > 0 ? $"[Practice] Despawned {count} boss NPC(s)." : "[Practice] No bosses to despawn.", InfoColor);
	}

	/// <summary>Removes every boss, every practice NPC and every segment attached to a boss. Returns how many were removed.</summary>
	private static int DespawnBosses()
	{
		bool[] remove = new bool[Main.maxNPCs];
		foreach (NPC npc in Main.ActiveNPCs) {
			remove[npc.whoAmI] = npc.boss || npc.GetGlobalNPC<PracticeGlobalNPC>().IsPractice;
		}

		foreach (NPC npc in Main.ActiveNPCs) {
			if (npc.realLife >= 0 && npc.realLife < Main.maxNPCs && remove[npc.realLife]) {
				remove[npc.whoAmI] = true;
			}
		}

		int count = 0;
		for (int i = 0; i < Main.maxNPCs; i++) {
			if (!remove[i]) {
				continue;
			}

			NPC npc = Main.npc[i];
			if (npc.type == NPCID.WallofFlesh) {
				Main.wofNPCIndex = -1;
			}

			npc.active = false;
			npc.life = 0;
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.SyncNPC, number: i);
			}

			count++;
		}

		pendingSetups.Clear();
		return count;
	}

	private static void ClearHostileProjectiles()
	{
		foreach (Projectile projectile in Main.ActiveProjectiles) {
			if (!projectile.hostile) {
				continue;
			}

			// Deactivate rather than Kill() so death effects can't spawn more projectiles.
			projectile.active = false;
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.KillProjectile, number: projectile.identity, number2: projectile.owner);
			}
		}
	}

	/// <summary>Server / singleplayer only.</summary>
	public static void MakeNight(Player requester)
	{
		Main.dayTime = false;
		Main.time = 0;
		if (Main.netMode == NetmodeID.Server) {
			NetMessage.SendData(MessageID.WorldData);
		}

		Announce($"[Practice] {requester.name} set the time to dusk.", InfoColor);
	}

	/// <summary>Server / singleplayer only.</summary>
	public static void MakeDay(Player requester)
	{
		Main.dayTime = true;
		Main.time = 0;
		if (Main.netMode == NetmodeID.Server) {
			NetMessage.SendData(MessageID.WorldData);
		}

		Announce($"[Practice] {requester.name} set the time to dawn.", InfoColor);
	}

	public static void Announce(string message, Color color)
	{
		if (Main.netMode == NetmodeID.Server) {
			ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), color);
		}
		else {
			Main.NewText(message, color);
		}
	}

	public static void Reply(Player player, string message, Color color)
	{
		if (Main.netMode == NetmodeID.Server) {
			ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(message), color, player.whoAmI);
		}
		else {
			Main.NewText(message, color);
		}
	}

	internal static void Unload()
	{
		pendingSetups.Clear();
		CapturedNPCs.Clear();
		CapturingSessionId = 0;
		lastSessionId = 0;
	}

	private sealed class PendingSetup(int sessionId, BossDefinition boss, float healthPercent, BossPhase phase)
	{
		public int SessionId { get; } = sessionId;

		public BossDefinition Boss { get; } = boss;

		public float HealthPercent { get; } = healthPercent;

		public BossPhase Phase { get; } = phase;

		public int Age { get; set; }
	}
}

public sealed class PracticeSystem : ModSystem
{
	public override void PostUpdateNPCs()
	{
		if (Main.netMode != NetmodeID.MultiplayerClient) {
			PracticeManager.Update();
		}
	}

	public override void OnWorldUnload()
	{
		PracticeManager.Unload();
	}

	public override void Unload()
	{
		PracticeManager.Unload();
	}
}
