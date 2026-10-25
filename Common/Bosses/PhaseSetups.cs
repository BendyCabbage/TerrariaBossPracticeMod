using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace BossPractice.Common.Bosses;

/// <summary>Reusable <see cref="BossPhase.Setup"/> actions. Each one only touches NPCs belonging to the practice fight.</summary>
public static class PhaseSetups
{
	/// <summary>Instantly kills every NPC of the given types that belongs to the practice fight.</summary>
	public static Action<IReadOnlyList<NPC>> KillAll(params int[] types) => npcs => {
		foreach (NPC npc in npcs) {
			if (npc.active && types.Contains(npc.type)) {
				npc.StrikeInstantKill();
			}
		}
	};

	/// <summary>Runs an action on every NPC of the given type, then syncs it.</summary>
	public static Action<IReadOnlyList<NPC>> ForEach(int type, Action<NPC> action) => npcs => {
		foreach (NPC npc in npcs) {
			if (npc.active && npc.type == type) {
				action(npc);
				npc.netUpdate = true;
			}
		}
	};

	/// <summary>Sets <c>npc.ai[index]</c> on every NPC of the given type, e.g. to jump an AI state machine to a phase.</summary>
	public static Action<IReadOnlyList<NPC>> SetAI(int type, int index, float value) => ForEach(type, npc => npc.ai[index] = value);

	/// <summary>Sets <c>npc.localAI[index]</c> on every NPC of the given type.</summary>
	public static Action<IReadOnlyList<NPC>> SetLocalAI(int type, int index, float value) => ForEach(type, npc => npc.localAI[index] = value);

	/// <summary>Sets the health of every NPC of the given type, for parts that aren't in <see cref="BossDefinition.HealthTypes"/>.</summary>
	public static Action<IReadOnlyList<NPC>> SetHealth(int type, float percent) =>
		ForEach(type, npc => npc.life = Math.Max(1, (int)Math.Round(npc.lifeMax * percent)));

	/// <summary>Runs several setup actions in order.</summary>
	public static Action<IReadOnlyList<NPC>> All(params Action<IReadOnlyList<NPC>>[] setups) => npcs => {
		foreach (Action<IReadOnlyList<NPC>> setup in setups) {
			setup(npcs);
		}
	};
}
