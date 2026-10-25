using Terraria;

namespace BossPractice.Common.Bosses.Sources;

/// <summary>
/// Calamity's bosses, plus phase variants for the vanilla bosses whose AI Calamity replaces in Revengeance and Boss Rush.
/// Split across files: <c>CalamityBosses.PreMoonLord.cs</c>, <c>CalamityBosses.PostMoonLord.cs</c> and <c>CalamityBosses.Vanilla.cs</c>.
/// Thresholds come from Calamity's source (2.2.2). Progression values match Calamity's own Boss Checklist entries.
/// </summary>
public sealed partial class CalamityBosses : BossSource
{
	public override string ModName => "CalamityMod";

	/// <summary>Revengeance, Death or Boss Rush. Calamity's boss AI treats Boss Rush as Death.</summary>
	private bool Revengeance => Difficulty("revengeance") || BossRush;

	private bool Death => Difficulty("death") || BossRush;

	private bool BossRush => Difficulty("bossrush");

	/// <summary>Calamity's AI treats Master Mode and Boss Rush as Expert.</summary>
	private bool Expert => Main.expertMode || BossRush;

	public override void AddBosses(BossRegistrar registrar)
	{
		AddPreMoonLordBosses(registrar);
		AddPostMoonLordBosses(registrar);
	}

	public override void ModifyBosses(BossRegistrar registrar)
	{
		ModifyVanillaBosses(registrar);
	}

	partial void AddPreMoonLordBosses(BossRegistrar registrar);

	partial void AddPostMoonLordBosses(BossRegistrar registrar);

	partial void ModifyVanillaBosses(BossRegistrar registrar);

	private bool Difficulty(string name) => CallMod("GetDifficultyActive", name) is true;

	/// <summary>Picks a value by Calamity difficulty, checked from hardest to easiest.</summary>
	private float ByDifficulty(float normal, float expert, float revengeance, float death) =>
		Death ? death : Revengeance ? revengeance : Expert ? expert : normal;

	/// <summary>
	/// Writes one of Calamity's extra AI slots (<c>CalamityGlobalNPC.newAI</c>). Calamity only accepts a float here.
	/// The write happens on the server; Calamity's bosses send these slots in their own NPC sync (SendExtraAI).
	/// </summary>
	private void SetCalamityAI(NPC npc, int slot, float value)
	{
		CallMod("SetCalamityAI", npc, slot, value);
		npc.netUpdate = true;
	}
}
