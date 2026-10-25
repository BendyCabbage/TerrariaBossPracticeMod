using Terraria.ID;

namespace BossPractice.Common.Bosses.Sources;

public sealed partial class FargosSoulsBosses
{
	/// <summary>
	/// Eternity Mode presets for the vanilla bosses. Another overhaul mod can switch Fargo's vanilla rework off
	/// (its "EternityVanillaBossBehaviour" flag), but that flag can't be read without changing it, so it isn't checked.
	/// </summary>
	partial void ModifyVanillaBosses(BossRegistrar registrar)
	{
		const string vanilla = BossDefinition.VanillaModName;

		registrar.AddPhaseVariant(vanilla, "kingslime", new PhaseVariant("Eternity", () => Eternity, [
			BossPhase.FullFight,
			new BossPhase("Phase 2", 0.65f, "Walls of falling slime join its attacks, below 66% health."),
		]));

		registrar.AddPhaseVariant(vanilla, "eye", new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Phase 1", 1f, "Eye form with Servants of Cthulhu."),
			new BossPhase("Phase 2", 0.64f, "Mouth form. Transforms immediately on spawn."),
			// Wiki: below 50%, after its current dash combo.
			new BossPhase("Phase 3", 0.49f, "Chains dashes and teleports while invincible."),
			new BossPhase("Phase 4", 0.09f, "Below 10% health. Stunned every third pattern, except in Masochist."),
		]));

		// Masochist skips phase 2: killing the Creepers goes straight to phase 3.
		registrar.AddPhaseVariant(vanilla, "brain", new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Phase 1", 1f, "Invincible Brain surrounded by Creepers."),
			new BossPhase("Phase 2", 1f, "Creepers destroyed; the Brain is vulnerable. Masochist goes straight to phase 3.",
				PhaseSetups.KillAll(NPCID.Creeper)),
			new BossPhase("Phase 3", 0.49f, "Creepers destroyed, below 50% health: its illusions fire lasers.", PhaseSetups.KillAll(NPCID.Creeper)),
		]));

		registrar.AddPhaseVariant(vanilla, "queenbee", new PhaseVariant("Eternity", () => Eternity, [
			BossPhase.FullFight,
			// She's harder to hurt while a Royal Subject lives.
			new BossPhase("Royal Subject", 0.65f, "A Royal Subject joins, below 66% health."),
			new BossPhase("Phase 2", 0.49f, "New attacks below 50% health, after the first Royal Subject."),
			new BossPhase("Phase 3", 0.32f, "More Royal Subjects join, below 33% health."),
		]));

		// Skeletron can't drop below 50% while its first hands live, and regrows a pair (two in Masochist) once they're gone.
		// Its CheckDead turns it into the Dungeon Guardian form instead of dying.
		registrar.AddPhaseVariant(vanilla, "skeletron", new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("With hands", 1f, "Head and both hands."),
			new BossPhase("Hands destroyed", 1f, "Only the head remains.", PhaseSetups.KillAll(NPCID.SkeletronHand)),
			new BossPhase("Phase 2", 0.74f, "Below 75% health, with both hands."),
			new BossPhase("Phase 3", 0.49f, "Its first hands are destroyed and it regrows new ones, below 50% health.",
				PhaseSetups.KillAll(NPCID.SkeletronHand)),
			new BossPhase("Dungeon Guardian", 0.01f, "Its last stand after reaching 0 health, with regrown hands.",
				PhaseSetups.KillAll(NPCID.SkeletronHand, NPCID.SkeletronHead)),
		]));

		registrar.AddPhaseVariant(vanilla, "deerclops", new PhaseVariant("Eternity", () => Eternity, [
			BossPhase.FullFight,
			new BossPhase("Phase 2", 0.65f, "Below 66% health."),
			new BossPhase("Phase 3", 0.32f, "Below 33% health."),
		]));

		registrar.AddPhaseVariant(vanilla, "wall", new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Phase 1", 1f, "Only one eye can be hurt at a time."),
			new BossPhase("Phase 2", () => Masochist ? 0.89f : 0.74f, "Below 75% health (90% in Masochist)."),
			new BossPhase("Phase 3", () => Masochist ? 0.79f : 0.49f, "Below 50% health (80% in Masochist)."),
			new BossPhase("Phase 4", () => Masochist ? 0.24f : 0.09f, "The eyes swap faster, below 10% health (25% in Masochist)."),
		]));

		// She takes a quarter damage while Gelatin Subjects live.
		registrar.AddPhaseVariant(vanilla, "queenslime", new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Phase 1", 1f, "Ground phase with Crystal, Bouncy and Heavenly Slimes."),
			new BossPhase("Gelatin Subjects", 0.74f, "A wave of Gelatin Subjects, below 75% health."),
			new BossPhase("Phase 2", 0.49f, "Flying phase below 50% health, after the first Gelatin wave."),
			new BossPhase("Second Gelatin wave", 0.24f, "Another wave of Gelatin Subjects, below 25% health."),
		]));

		// A twin brought to 0 health endures at 1 HP until both are down. In Masochist it dies and revives after 10 seconds.
		registrar.AddPhaseVariant(vanilla, "twins", new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Phase 1", 1f, "Both twins in eye form."),
			// Wiki: around 75% total health; the 1.7.3.7 source uses 66% of either twin. This starts below both.
			new BossPhase("Transition", 0.65f, "Both twins freeze and spin into their second form."),
			new BossPhase("Phase 2", 0.49f, "Both twins in their final form, below 50% health."),
			new BossPhase("Phase 2 - Retinazer down", 0.49f, "Retinazer endures at 1 HP (Masochist: revives in 10 seconds).",
				PhaseSetups.KillAll(NPCID.Retinazer)),
			new BossPhase("Phase 2 - Spazmatism down", 0.49f, "Spazmatism endures at 1 HP (Masochist: revives in 10 seconds).",
				PhaseSetups.KillAll(NPCID.Spazmatism)),
		], "Flee at dawn. A twin brought to 0 health endures at 1 HP until both are down; in Masochist it revives instead."));

		registrar.AddPhaseVariant(vanilla, "destroyer", new PhaseVariant("Eternity", () => Eternity, [
			BossPhase.FullFight,
			new BossPhase("Phase 2", () => Masochist ? 0.94f : 0.74f,
				"Cycles coils, probe barrages, light shows and pulses, below 75% health (95% in Masochist)."),
			new BossPhase("Passive probes", () => Masochist ? 0.79f : 0.39f, "Also releases probes, below 40% health (80% in Masochist)."),
			new BossPhase("Phase 3", 0.09f, "An endless coil, below 10% health."),
		]));

		// Prime's arms share its health and are crippled instead of destroyed, so there's no "Arms destroyed" preset.
		registrar.AddPhaseVariant(vanilla, "prime", new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("With arms", 1f, "Head and all four arms."),
			new BossPhase("Phase 2", () => Masochist ? 0.79f : 0.59f, "Four more arms join, below 60% health (80% in Masochist)."),
			// Its CheckDead turns it into the Dungeon Guardian form instead of dying.
			new BossPhase("Dungeon Guardian", 0.01f, "Its last stand after reaching 0 health. Keeps its arms only in Masochist.",
				PhaseSetups.KillAll(NPCID.SkeletronPrime)),
		], "Flees at dawn. Its arms share its health and can't be destroyed on their own."));

		registrar.AddPhaseVariant(vanilla, "plantera", new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Phase 1", 1f, "Bulb form with hooks."),
			new BossPhase("Phase 2", 0.49f, "Mouth form with tentacles, below 50% health."),
			new BossPhase("Phase 3", 0.24f, "Tougher, inside an arena of invulnerable Trappers, below 25% health."),
		]));

		// Fishron heals to full as phase 2 starts, so starting below 15% from phase 1 never reaches phase 3.
		// ai[0] = 5 is its phase 2 state; from there it starts the phase 3 transition below 15% health.
		registrar.AddPhaseVariant(vanilla, "fishron", new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Phase 1", 1f, "Dashes, bubbles and Sharkrons. Can't die before phase 2."),
			new BossPhase("Phase 2", 0.49f, "Heals to full as phase 2 starts, below 50% health."),
			new BossPhase("Phase 3", 0.14f, "Skips phase 2, then heals to 33% (50% in Masochist) as phase 3 starts.",
				PhaseSetups.SetAI(NPCID.DukeFishron, 0, 5f)),
		]));

		registrar.AddPhaseVariant(vanilla, "cultist", new PhaseVariant("Eternity", () => Eternity, [
			BossPhase.FullFight,
			new BossPhase("Phase 2", 0.49f, "Its ritual summons a Celestial Pillar to slam down, below 50% health."),
		]));

		// The core's attack cycle speeds up as its health drops, faster in Masochist.
		registrar.AddPhaseVariant(vanilla, "moonlord", new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Full fight", 1f, "Head and both hands active."),
			new BossPhase("Core exposed", 1f, "Head and hands destroyed; the core is vulnerable.",
				PhaseSetups.KillAll(NPCID.MoonLordHead, NPCID.MoonLordHand)),
			new BossPhase("Core exposed, 2x speed", () => Masochist ? 0.81f : 0.74f,
				"Attacks cycle twice as fast, below 75% health (82% in Masochist).",
				PhaseSetups.KillAll(NPCID.MoonLordHead, NPCID.MoonLordHand)),
			new BossPhase("Core exposed, 3x speed", () => Masochist ? 0.39f : 0.24f,
				"Attacks cycle three times as fast, below 25% health (40% in Masochist).",
				PhaseSetups.KillAll(NPCID.MoonLordHead, NPCID.MoonLordHand)),
		]));
	}
}
