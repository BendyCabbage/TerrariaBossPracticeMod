using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace BossPractice.Common.Bosses.Sources;

public sealed partial class CalamityBosses
{
	/// <summary>
	/// True when Calamity replaces vanilla boss AI: Revengeance, Death or Boss Rush, unless another mod switched the override off.
	/// Calamity picks the AI once, when the boss spawns (CalamityVanillaAIOverrideNPC.SetDefaults), so this is checked at spawn time too.
	/// </summary>
	private bool VanillaAIOverridden => Revengeance && CallMod("GetVanillaAIOverrideEnabled") is true;

	private bool DeathAIOverridden => VanillaAIOverridden && Death;

	partial void ModifyVanillaBosses(BossRegistrar registrar)
	{
		const string vanilla = BossDefinition.VanillaModName;

		// Spawned by the Brain's AI but flagged as bosses, so they'd otherwise be listed on their own.
		registrar.AddRelatedTypes(vanilla, "brain", NPCType("FalseBrain"), NPCType("BrainIllusion"));

		// The Crown Jewel spawns below 50% health (75% in Death). In Death its attacks change at 55% and 35%.
		registrar.AddPhaseVariant(vanilla, "kingslime", new PhaseVariant("Death", () => DeathAIOverridden, [
			BossPhase.FullFight,
			new BossPhase("Crown Jewel", 0.74f, "Faster jumps and the Crown Jewel, which can't be hurt while King Slime lives."),
			new BossPhase("Emerald jewel", 0.54f, "The jewel switches to its emerald attacks."),
			new BossPhase("Alternating jewel", 0.34f, "The jewel alternates between ruby and emerald attacks."),
			new BossPhase("Rapid hops", 0.19f, "Chains quick, low hops below 20% health."),
		]));
		registrar.AddPhaseVariant(vanilla, "kingslime", new PhaseVariant("Revengeance", () => VanillaAIOverridden, [
			BossPhase.FullFight,
			new BossPhase("Faster jumps", 0.74f, "Longer, faster jumps below 75% health."),
			new BossPhase("Crown Jewel", 0.49f, "Spawns the Crown Jewel below 50% health."),
		]));

		// Death's thresholds: transform below 75%, phase 3 below 40%.
		registrar.AddPhaseVariant(vanilla, "eye", new PhaseVariant("Revengeance", () => VanillaAIOverridden, [
			new BossPhase("Phase 1", 1f, "Eye form with Servants of Cthulhu."),
			new BossPhase("Phase 2", () => Death ? 0.74f : 0.59f, "Mouth form. Transforms immediately on spawn."),
			new BossPhase("Phase 3", () => Death ? 0.39f : 0.29f, "Hovers below you, then mixes rapid dashes with charges from the side."),
			new BossPhase("Final phase", 0.14f, "Shorter dash chains and fewer servants below 15% health."),
		]));

		// Calamity's Brain keeps its state in ai[0] (9 = phase 2 transition) and that state's timer in ai[1].
		Action<IReadOnlyList<NPC>> brainPhase2 = PhaseSetups.All(
			PhaseSetups.KillAll(NPCID.Creeper),
			PhaseSetups.SetAI(NPCID.BrainofCthulhu, 0, 9f),
			PhaseSetups.SetAI(NPCID.BrainofCthulhu, 1, 0f));
		registrar.AddPhaseVariant(vanilla, "brain", new PhaseVariant("Revengeance", () => VanillaAIOverridden, [
			new BossPhase("Phase 1", 1f, "Brain surrounded by Creepers, after a long intro."),
			// Phase 2 only starts if the Brain is below 70% health when its Creepers are gone.
			new BossPhase("Second Creeper wave", 1f, "Creepers destroyed above 70% health: the Brain is stunned, then summons a new wave.",
				PhaseSetups.KillAll(NPCID.Creeper)),
			new BossPhase("Phase 2", 0.69f, "Creepers destroyed. Skips the intro and starts the phase 2 transition.", brainPhase2),
			new BossPhase("Illusion Trick", 0.24f, "Phase 2 below 25% health, which forces the Illusion Trick once.", brainPhase2),
		], "Despawns if you leave the Crimson."));

		// Queen Bee also gets Calamity's AI in any Zenith seed world. Her thresholds are the same in every mode.
		registrar.AddPhaseVariant(vanilla, "queenbee", new PhaseVariant("Revengeance",
			() => (Revengeance || Main.zenithWorld) && CallMod("GetVanillaAIOverrideEnabled") is true, [
			BossPhase.FullFight,
			new BossPhase("Phase 2", 0.49f, "Stops summoning bees; adds stinger arcs and diagonal dashes below 50% health."),
			new BossPhase("Phase 3", 0.29f, "Chains many short dashes below 30% health."),
			new BossPhase("Final phase", 0.09f, "Stinger bombardment and longer dashes below 10% health."),
		], "Enrages outside the Jungle or above the surface, unless you're in front of a hive wall."));

		// Calamity only replaces Skeletron's AI in singleplayer. It respawns the hands once below 50% health;
		// newAI[0] = 1 marks that respawn as done.
		Action<IReadOnlyList<NPC>> skeletronHandsGone = PhaseSetups.All(
			PhaseSetups.KillAll(NPCID.SkeletronHand),
			PhaseSetups.ForEach(NPCID.SkeletronHead, npc => SetCalamityAI(npc, 0, 1f)));
		registrar.AddPhaseVariant(vanilla, "skeletron", new PhaseVariant("Revengeance", () => VanillaAIOverridden && Main.netMode == NetmodeID.SinglePlayer, [
			new BossPhase("With hands", 1f, "Head and hands. The head barely takes damage near 50% health while they live."),
			new BossPhase("Hands destroyed", 1f, "Only the head remains. The hands return once, below 50% health.", PhaseSetups.KillAll(NPCID.SkeletronHand)),
			new BossPhase("Spin charges", () => Death ? 0.89f : 0.69f, "Hands destroyed; the head adds a spinning charge.", PhaseSetups.KillAll(NPCID.SkeletronHand)),
			new BossPhase("Hands return", 0.49f, "A new set of hands spawns immediately.", PhaseSetups.KillAll(NPCID.SkeletronHand)),
			new BossPhase("Phase 4", () => Death ? 0.39f : 0.29f, "Hands gone for good; fires cursed skulls during charges.", skeletronHandsGone),
			new BossPhase("Final phase", () => Death ? 0.14f : 0.09f, "Rapid teleports and charges with no rest.", skeletronHandsGone),
		]));

		// Death fires Demon Sickles from the start and detaches the eyes below 40% health.
		registrar.AddPhaseVariant(vanilla, "wall", new PhaseVariant("Death", () => DeathAIOverridden, [
			BossPhase.FullFight,
			new BossPhase("Detached eyes", 0.39f, "The eyes leave the wall and hover around you below 40% health."),
			new BossPhase("Triple leeches", 0.32f, "Leeches come faster and three at a time below 33% health."),
		]));
		registrar.AddPhaseVariant(vanilla, "wall", new PhaseVariant("Revengeance", () => VanillaAIOverridden, [
			BossPhase.FullFight,
			new BossPhase("Demon Sickles", 0.65f, "Leeches come faster, each with a Demon Sickle, below 66% health."),
			new BossPhase("Triple leeches", 0.32f, "Leeches come faster and three at a time below 33% health."),
		]));

		// In Death, phases 2 to 4 all start at 50% health.
		registrar.AddPhaseVariant(vanilla, "queenslime", new PhaseVariant("Death", () => DeathAIOverridden, [
			new BossPhase("Phase 1", 1f, "Ground phase with Crystal, Bouncy and Heavenly Slimes."),
			new BossPhase("Phase 2", 0.49f, "Flying phase with crystal spikes and cascades below 50% health."),
			new BossPhase("Final phase", 0.24f, "Adds a second spike ring and stops summoning slimes below 25% health."),
		]));
		registrar.AddPhaseVariant(vanilla, "queenslime", new PhaseVariant("Revengeance", () => VanillaAIOverridden, [
			new BossPhase("Phase 1", 1f, "Ground phase with Crystal, Bouncy and Heavenly Slimes."),
			new BossPhase("Phase 2", 0.49f, "Flying phase below 50% health."),
			new BossPhase("Phase 3", 0.39f, "Slams erupt into crystal spikes below 40% health."),
			new BossPhase("Phase 4", 0.19f, "Faster attacks and no more slimes below 20% health."),
		]));

		// Each twin transforms below 70% of its own health (85% in Death); final phase below 25% (40% in Death).
		registrar.AddPhaseVariant(vanilla, "twins", new PhaseVariant("Revengeance", () => VanillaAIOverridden, [
			new BossPhase("Phase 1", 1f, "Both twins in eye form."),
			new BossPhase("Phase 2", () => Death ? 0.84f : 0.69f, "Both twins transform immediately on spawn."),
			new BossPhase("Phase 2 - Retinazer alone", () => Death ? 0.84f : 0.69f, "Spazmatism is already dead.", PhaseSetups.KillAll(NPCID.Spazmatism)),
			new BossPhase("Phase 2 - Spazmatism alone", () => Death ? 0.84f : 0.69f, "Retinazer is already dead.", PhaseSetups.KillAll(NPCID.Retinazer)),
			new BossPhase("Final phase", () => Death ? 0.39f : 0.24f, "Both twins add lined-up charges and new fireball patterns."),
		]));

		registrar.AddPhaseVariant(vanilla, "destroyer", new PhaseVariant("Revengeance", () => VanillaAIOverridden, [
			BossPhase.FullFight,
			new BossPhase("Flight cycle", 0.49f, "Alternates 15 seconds on the ground with 15 seconds of flight below 50% health."),
			new BossPhase("Phase 4", () => Death ? 0.39f : 0.24f, "Shorter ground phases and faster flight."),
			new BossPhase("Permanent flight", () => Death ? 0.19f : 0.09f, "Flies at you for the rest of the fight."),
		], "Flees at dawn. Takes greatly reduced damage for its first 10 seconds."));

		// Same thresholds in every Calamity mode.
		registrar.AddPhaseVariant(vanilla, "prime", new PhaseVariant("Revengeance", () => VanillaAIOverridden, [
			new BossPhase("With arms", 1f, "Head and all four arms. The head only spins below 66% health."),
			new BossPhase("Phase 2", 0.65f, "Fires rings of lasers while spinning below 66% health."),
			new BossPhase("Phase 3", 0.32f, "Rocket barrages after each spin below 33% health."),
			new BossPhase("Arms destroyed", 1f, "Only the head remains.",
				PhaseSetups.KillAll(NPCID.PrimeCannon, NPCID.PrimeSaw, NPCID.PrimeVice, NPCID.PrimeLaser)),
			new BossPhase("Phase 3 - Head alone", 0.32f, "Arms destroyed, with laser rings and rocket barrages.",
				PhaseSetups.KillAll(NPCID.PrimeCannon, NPCID.PrimeSaw, NPCID.PrimeVice, NPCID.PrimeLaser)),
		]));

		// ai[2] = -1 marks Plantera's 20 second tentacle phase as over, so she charges straight away.
		registrar.AddPhaseVariant(vanilla, "plantera", new PhaseVariant("Revengeance", () => VanillaAIOverridden, [
			new BossPhase("Phase 1", 1f, "Bulb form with hooks."),
			new BossPhase("Phase 2", 0.49f, "Mouth form. Slows down for up to 20 seconds while her tentacles live."),
			new BossPhase("Phase 2 charges", 0.49f, "Skips the tentacle phase and starts charging.", PhaseSetups.SetAI(NPCID.Plantera, 2, -1f)),
			new BossPhase("Phase 3", 0.34f, "Faster charges that leave spore gas.", PhaseSetups.SetAI(NPCID.Plantera, 2, -1f)),
			new BossPhase("Phase 4", 0.19f, "Fastest charges.", PhaseSetups.SetAI(NPCID.Plantera, 2, -1f)),
		], "Enrages on the surface or in the Underworld, and heals while you're on the surface."));

		// The body can't be hurt while the head or a fist lives, and the attached head can't be hurt while a fist lives.
		// The free head's phases follow the body's health: 66% and 33%.
		registrar.AddPhaseVariant(vanilla, "golem", new PhaseVariant("Revengeance", () => VanillaAIOverridden, [
			new BossPhase("Head attached", 1f, "Head, fists and body."),
			new BossPhase("Fists destroyed", 1f, "The head can be hurt and fires lasers.", PhaseSetups.KillAll(NPCID.GolemFistLeft, NPCID.GolemFistRight)),
			new BossPhase("Head detached", 1f, "Fists destroyed and the head breaks free; the body becomes vulnerable.",
				PhaseSetups.KillAll(NPCID.GolemFistLeft, NPCID.GolemFistRight, NPCID.GolemHead)),
			new BossPhase("Free head phase 2", 0.65f, "The free head flies between corners and sweeps lasers.",
				PhaseSetups.KillAll(NPCID.GolemFistLeft, NPCID.GolemFistRight, NPCID.GolemHead)),
			new BossPhase("Free head phase 3", 0.32f, "The free head circles you and adds Inferno bolts.",
				PhaseSetups.KillAll(NPCID.GolemFistLeft, NPCID.GolemFistRight, NPCID.GolemHead)),
		]));

		// Same thresholds in every Calamity mode: 70%, 40%, and 20% for the final part of phase 3.
		registrar.AddPhaseVariant(vanilla, "fishron", new PhaseVariant("Revengeance", () => VanillaAIOverridden, [
			new BossPhase("Phase 1", 1f, "Dashes, bubbles and Sharkrons."),
			new BossPhase("Phase 2", 0.69f, "Faster dashes and Sharknadoes below 70% health."),
			new BossPhase("Phase 3", 0.39f, "Teleporting frenzy and Cthulhunadoes below 40% health."),
			new BossPhase("Final phase", 0.19f, "No more Cthulhunadoes and a faster attack cycle below 20% health."),
		]));

		registrar.AddPhaseVariant(vanilla, "empress", new PhaseVariant("Revengeance", () => VanillaAIOverridden, [
			new BossPhase("Phase 1", 1f, "Opening attack pattern."),
			new BossPhase("Phase 2", () => Death ? 0.69f : 0.59f, "Second attack pattern below 60% health (70% in Death)."),
			new BossPhase("Phase 3", () => Death ? 0.29f : 0.14f, "Fades and adds extra lances and rainbows below 15% health (30% in Death)."),
		]));

		// Calamity revives destroyed parts as invulnerable attackers and adds a True Eye for each.
		registrar.AddPhaseVariant(vanilla, "moonlord", new PhaseVariant("Revengeance", () => VanillaAIOverridden, [
			new BossPhase("Full fight", 1f, "Head and both hands active."),
			new BossPhase("Core exposed", 1f, "The core is vulnerable. The destroyed head and hands keep attacking, joined by three True Eyes.",
				PhaseSetups.KillAll(NPCID.MoonLordHead, NPCID.MoonLordHand)),
		]));
	}
}
