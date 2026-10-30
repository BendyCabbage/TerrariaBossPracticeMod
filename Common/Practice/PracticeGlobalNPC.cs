using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using BossPractice.Common.Bosses;
using BossPractice.Common.Configs;

namespace BossPractice.Common.Practice;

/// <summary>
/// Tags every NPC that belongs to a practice fight: the boss itself, plus anything it (or its parts) spawn.
/// Tags only exist on the server / in singleplayer, which is where loot and death events run.
/// </summary>
public sealed class PracticeGlobalNPC : GlobalNPC
{
	// Tags are stored per NPC slot rather than on a GlobalNPC instance. Bosses that change form with NPC.Transform
	// (Thorium's Borean Strider, Lich, ...) go through SetDefaults again, which replaces every GlobalNPC instance;
	// Transform keeps the slot, so the tag survives. Every newly spawned NPC overwrites its slot's tag in OnSpawn.
	private static readonly int[] sessionIds = new int[Main.maxNPCs + 1];
	private static readonly uint[] spawnTicks = new uint[Main.maxNPCs + 1];

	/// <summary>Identifies the practice fight this NPC belongs to. 0 means not part of a practice fight.</summary>
	public static int GetSessionId(NPC npc) => sessionIds[npc.whoAmI];

	public static bool IsPractice(NPC npc) => GetSessionId(npc) != 0;

	internal static void ClearTags()
	{
		Array.Clear(sessionIds);
		Array.Clear(spawnTicks);
	}

	public override void OnSpawn(NPC npc, IEntitySource source)
	{
		int sessionId = 0;
		uint startTick = 0;
		if (PracticeManager.CapturingSessionId != 0) {
			sessionId = PracticeManager.CapturingSessionId;
			startTick = Main.GameUpdateCount;
			PracticeManager.CapturedNPCs.Add(npc);
		}
		else if (source is EntitySource_Parent { Entity: NPC parent } && IsPractice(parent)) {
			sessionId = GetSessionId(parent);
			startTick = spawnTicks[parent.whoAmI];
		}
		else {
			PracticeManager.TryFindSessionFor(npc.type, out sessionId, out startTick);
		}

		sessionIds[npc.whoAmI] = sessionId;
		spawnTicks[npc.whoAmI] = startTick;
	}

	public override bool PreKill(NPC npc)
	{
		int sessionId = GetSessionId(npc);
		if (sessionId == 0) {
			return true;
		}

		if (npc.boss && !PracticeManager.ApplyingPhaseSetup) {
			float seconds = (Main.GameUpdateCount - spawnTicks[npc.whoAmI]) / 60f;
			PracticeManager.Announce($"[Practice] {npc.FullName} defeated in {seconds:0.0}s.", new Color(120, 220, 255));
		}

		if (!ModContent.GetInstance<PracticeConfig>().DisableLootAndProgression) {
			return true;
		}

		// Returning false also skips the NPC's own PreKill and OnKill. Run them, in tModLoader's order, for NPCs whose
		// death starts the next stage of the fight.
		BossDefinition boss = PracticeManager.GetSessionBoss(sessionId);
		if (npc.ModNPC != null && boss?.RunsOwnDeathCode?.Invoke(npc) == true && npc.ModNPC.PreKill()) {
			npc.ModNPC.OnKill();
		}

		// Returning false skips item drops, money, and death events such as "downed" flags,
		// starting hardmode (Wall of Flesh) and starting the lunar events (Lunatic Cultist).
		return false;
	}
}
