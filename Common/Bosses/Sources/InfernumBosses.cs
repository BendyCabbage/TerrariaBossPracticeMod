using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace BossPractice.Common.Bosses.Sources;

/// <summary>
/// Infernum Mode, a Calamity addon that replaces the AI of most vanilla and Calamity bosses while its difficulty is on.
/// Adds "Infernum" phase variants to those bosses, plus Infernum's own Bereft Vassal.
/// Split across files: <c>InfernumBosses.Vanilla.cs</c>, <c>InfernumBosses.Calamity.cs</c> and <c>InfernumBosses.PostMoonLord.cs</c>.
/// Thresholds come from each boss's <c>PhaseLifeRatioThresholds</c> in Infernum's source (2.0.1.35).
/// </summary>
public sealed partial class InfernumBosses : BossSource
{
	private const string Calamity = "CalamityMod";

	/// <summary>Infernum replaces the AI that Calamity's Revengeance presets describe, so its variants win over them.</summary>
	private const int VariantPriority = 1;

	private FieldInfo extraAIField;
	private GlobalNPC infernumGlobalNPC;

	public override string ModName => "InfernumMode";

	/// <summary>
	/// True while the world has Infernum's difficulty on. Infernum swaps a boss's AI every tick it runs,
	/// so unlike Calamity's Revengeance AI this also applies to bosses that are already alive.
	/// </summary>
	private bool Active => CallMod("GetInfernumActive") is true;

	public override void AddBosses(BossRegistrar registrar)
	{
		AddBereftVassal(registrar);
	}

	public override void ModifyBosses(BossRegistrar registrar)
	{
		ModifyVanillaBosses(registrar);
		ModifyCalamityBosses(registrar);
	}

	partial void AddBereftVassal(BossRegistrar registrar);

	partial void ModifyVanillaBosses(BossRegistrar registrar);

	partial void ModifyCalamityBosses(BossRegistrar registrar);

	/// <summary>Adds the Infernum presets to another source's boss.</summary>
	private void AddVariant(BossRegistrar registrar, string modName, string name, BossPhase[] phases, string notes = null, int[] readyTypes = null)
	{
		registrar.AddPhaseVariant(modName, name, new PhaseVariant("Infernum", () => Active, phases, notes) {
			Priority = VariantPriority,
			ReadyTypes = readyTypes,
		});
	}

	/// <summary>
	/// Writes one of Infernum's extra AI slots (<c>npc.Infernum().ExtraAI</c>), which its reworked AIs keep their state in.
	/// Infernum has no Mod.Call for these, so the field is found by reflection; Infernum syncs the slots with the NPC.
	/// </summary>
	private void SetInfernumAI(NPC npc, int slot, float value)
	{
		if (infernumGlobalNPC == null) {
			if (!ModContent.TryFind(ModName, "GlobalNPCOverrides", out infernumGlobalNPC)) {
				Mod.Logger.Warn("InfernumMode has no GlobalNPCOverrides; its phase setups won't run.");
				return;
			}

			extraAIField = infernumGlobalNPC.GetType().GetField("ExtraAI", BindingFlags.Public | BindingFlags.Instance);
		}

		if (extraAIField != null && npc.TryGetGlobalNPC(infernumGlobalNPC, out GlobalNPC global) && extraAIField.GetValue(global) is float[] extraAI) {
			extraAI[slot] = value;
			npc.netUpdate = true;
		}
	}
}
