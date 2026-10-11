using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using BossPractice.Common.Configs;

namespace BossPractice.Common.Practice;

/// <summary>
/// Tags every NPC that belongs to a practice fight: the boss itself, plus anything it (or its parts) spawn.
/// Tags only exist on the server / in singleplayer, which is where loot and death events run.
/// </summary>
public sealed class PracticeGlobalNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	/// <summary>Identifies the practice fight this NPC belongs to. 0 means not part of a practice fight.</summary>
	public int SessionId { get; set; }

	public uint SpawnedAtTick { get; set; }

	public bool IsPractice => SessionId != 0;

	public override void OnSpawn(NPC npc, IEntitySource source)
	{
		if (PracticeManager.CapturingSessionId != 0) {
			SessionId = PracticeManager.CapturingSessionId;
			SpawnedAtTick = Main.GameUpdateCount;
			PracticeManager.CapturedNPCs.Add(npc);
			return;
		}

		if (source is EntitySource_Parent { Entity: NPC parent } && parent.TryGetGlobalNPC(out PracticeGlobalNPC parentPractice) && parentPractice.IsPractice) {
			SessionId = parentPractice.SessionId;
			SpawnedAtTick = parentPractice.SpawnedAtTick;
		}
	}

	public override bool PreKill(NPC npc)
	{
		if (!IsPractice) {
			return true;
		}

		if (npc.boss && !PracticeManager.ApplyingPhaseSetup) {
			float seconds = (Main.GameUpdateCount - SpawnedAtTick) / 60f;
			PracticeManager.Announce($"[Practice] {npc.FullName} defeated in {seconds:0.0}s.", new Color(120, 220, 255));
		}

		// Returning false skips item drops, money, and death events such as "downed" flags,
		// starting hardmode (Wall of Flesh) and starting the lunar events (Lunatic Cultist).
		return !ModContent.GetInstance<PracticeConfig>().DisableLootAndProgression;
	}
}
