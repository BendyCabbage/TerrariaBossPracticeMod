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

	private static readonly List<PendingSetup> pendingSetups = [];
	private static readonly List<ActiveSession> activeSessions = [];
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
			PracticeNetwork.SendSpawnRequest(boss.Key, healthPercent, phaseIndex);
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

	public static void RequestDay()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) {
			PracticeNetwork.SendSimpleRequest(PracticeMessageType.MakeDay);
			return;
		}

		MakeDay(Main.LocalPlayer);
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

		string spawnError = boss.CheckCanSpawn?.Invoke(player);
		if (spawnError != null) {
			Reply(player, $"[Practice] {spawnError}", ErrorColor);
			return false;
		}

		if (ModContent.GetInstance<PracticeConfig>().DespawnBossesBeforeSpawning) {
			DespawnBosses();
		}

		int sessionId = ++lastSessionId;
		CapturingSessionId = sessionId;
		CapturedNPCs.Clear();
		try {
			(phase?.Spawner ?? boss.Spawner)(player, boss);
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
		activeSessions.Add(new ActiveSession(sessionId, boss, Main.GameUpdateCount));

		string phaseText = phase != null ? $" ({phase.Name})" : "";
		Announce($"[Practice] {player.name} spawned {boss.DisplayName} at {healthPercent * 100f:0.#}% health{phaseText}.", InfoColor);
		return true;
	}

	/// <summary>Server / singleplayer only. Called once per tick after NPCs update.</summary>
	internal static void Update()
	{
		activeSessions.RemoveAll(session => !HasSessionNPCs(session.Id));

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
			if (!ready && setup.Age < setup.Boss.SetupTimeoutTicks) {
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

	/// <summary>
	/// Finds the newest running practice fight that includes NPCs of this type. Used to tag parts that join a fight
	/// without a parent entity source, such as bosses that summon each other.
	/// </summary>
	internal static bool TryFindSessionFor(int type, out int sessionId, out uint startTick)
	{
		for (int i = activeSessions.Count - 1; i >= 0; i--) {
			if (activeSessions[i].Boss.AllTypes.Contains(type)) {
				sessionId = activeSessions[i].Id;
				startTick = activeSessions[i].StartTick;
				return true;
			}
		}

		sessionId = 0;
		startTick = 0;
		return false;
	}

	/// <summary>The boss of a running practice fight, or null if that fight is over.</summary>
	internal static BossDefinition GetSessionBoss(int sessionId) => activeSessions.Find(session => session.Id == sessionId)?.Boss;

	private static bool HasSessionNPCs(int sessionId)
	{
		foreach (NPC npc in Main.ActiveNPCs) {
			if (npc.GetGlobalNPC<PracticeGlobalNPC>().SessionId == sessionId) {
				return true;
			}
		}

		return false;
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
		activeSessions.Clear();
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
		activeSessions.Clear();
		CapturedNPCs.Clear();
		CapturingSessionId = 0;
		lastSessionId = 0;
	}

	private sealed record ActiveSession(int Id, BossDefinition Boss, uint StartTick);

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
