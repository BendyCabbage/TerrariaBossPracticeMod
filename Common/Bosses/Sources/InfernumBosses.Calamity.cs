using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;

namespace BossPractice.Common.Bosses.Sources;

/// <summary>Infernum presets for Calamity's bosses, and Infernum's own Bereft Vassal.</summary>
public sealed partial class InfernumBosses
{
	partial void AddBereftVassal(BossRegistrar registrar)
	{
		int vassal = NPCType("BereftVassal");
		int greatSandShark = NPCType(Calamity, "GreatSandShark");

		// It normally waits in the Lost Colosseum and plays an intro that locks your controls once you approach.
		// Starting it on its first attack (ai[0] = 1) skips both. It walks, so it drops in from above.
		BossSpawner spawnFighting = (player, boss) => BossSpawners.SpawnAt(player, vassal, new Vector2(0f, -300f), ai0: 1f);

		registrar.Add(new BossDefinition("BereftVassal", vassal) {
			// Infernum's own Boss Checklist entry.
			Progression = 17.75f,
			Aliases = ["vassal", "argus"],
			// It summons the Great Sand Shark from a projectile, which carries no link back to the fight.
			RelatedTypes = [greatSandShark],
			Spawner = spawnFighting,
			Notes = "Normally fought in the Lost Colosseum; here it skips its intro. Fight it on flat ground.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Spear charges, sand slams and water torrents."),
				new BossPhase("Great Sand Shark", 0.59f,
					"Blows its horn to summon the Great Sand Shark, below 60% health. It can't be hurt while the shark lives."),
				// ExtraAI[5] marks the shark as already summoned; with no shark alive it goes straight to its final phase.
				new BossPhase("Final phase", 0.59f, "The shark is already dead: enraged in a sandstorm.",
					PhaseSetups.ForEach(vassal, npc => SetInfernumAI(npc, 5, 1f))),
			],
		});
	}

	partial void ModifyCalamityBosses(BossRegistrar registrar)
	{
		ModifyPreMoonLordBosses(registrar);
		ModifyPostMoonLordBosses(registrar);
	}

	private void ModifyPreMoonLordBosses(BossRegistrar registrar)
	{
		AddVariant(registrar, Calamity, "DesertScourge", [
			new BossPhase("Phase 1", 1f, "Sand spit, sand rush charges and a sandstorm."),
			new BossPhase("Phase 2", 0.54f, "Adds ground slams, below 55% health."),
			new BossPhase("Phase 3", 0.24f, "Summons vultures often, below 25% health."),
		], "Its intro lasts about 11 seconds before the first attack. Enrages outside the Desert.");

		AddVariant(registrar, Calamity, "Crabulon", [
			new BossPhase("Phase 1", 1f, "Walks and jumps. Waits 3.5 seconds, or until hit, before attacking."),
			new BossPhase("Phase 2", 0.84f, "Adds ground mushrooms and spore volleys while walking, below 85% health."),
			new BossPhase("Phase 3", 0.44f, "Adds mushroom waves from claw slams, below 45% health."),
		], "Enrages on the surface outside a glowing mushroom biome.");

		AddVariant(registrar, Calamity, "HiveMind", [
			new BossPhase("Phase 1", 1f, "Opening attack pattern."),
			new BossPhase("Eater walls", 0.71f, "Adds walls of Eaters of Souls, below 72% health."),
			new BossPhase("Flame dashes", 0.55f, "Adds underground flame dashes, below 56% health."),
			new BossPhase("Cursed rain", 0.38f, "Adds cursed rain, below 39% health."),
			// Crossing 20% makes it invulnerable for 1.5 seconds while it heals to 40%.
			new BossPhase("Final phase", 0.19f, "Faster attacks and blob bursts. Heals to 40% as the phase starts."),
		], "Enrages outside the Corruption and Crimson.");

		AddVariant(registrar, Calamity, "Perforators", [
			new BossPhase("Phase 1", 1f, "The hive alone."),
			new BossPhase("Small worm", 0.69f, "The small worm erupts, below 70% health. The hive can't be hurt until it dies."),
			new BossPhase("Medium worm", 0.49f, "The medium worm erupts, below 50% health."),
			new BossPhase("Large worm", 0.24f, "The large worm erupts, then stronger ichor attacks once it's dead, below 25% health."),
		], "Each worm skipped by a preset never appears. Enrages outside the Crimson.");

		int ebonianPaladin = NPCType(Calamity, "EbonianPaladin");
		int crimulanPaladin = NPCType(Calamity, "CrimulanPaladin");
		int slimeGodCore = NPCType(Calamity, "SlimeGodCore");
		// The core starts with one large slime (Crimulan in Crimson worlds, Ebonian in Corruption worlds) and only
		// summons the other when the first drops below 60% health.
		AddVariant(registrar, Calamity, "SlimeGod", [
			new BossPhase("One slime", 1f, "One large slime, matching your world's evil."),
			new BossPhase("Both slimes", 0.59f, "The second slime joins at full health and the two combine their attacks."),
			new BossPhase("Core alone", 1f, "Both slimes are dead; the core spins, charges and fires bursts.",
				PhaseSetups.KillAll(ebonianPaladin, crimulanPaladin)),
		], "The core can't be hurt while a large slime lives. A slime left alone enrages.",
			readyTypes: [slimeGodCore]);

		int cryogen = NPCType(Calamity, "Cryogen");
		AddVariant(registrar, Calamity, "Cryogen", [
			new BossPhase("Phase 1", 1f, "Ice blasts."),
			new BossPhase("Phase 2", 0.89f, "Adds ice pillars, below 90% health."),
			new BossPhase("Phase 3", 0.69f, "Adds icicle teleport dashes, below 70% health."),
			new BossPhase("Phase 4", 0.54f, "Adds horizontal dashes, below 55% health."),
			new BossPhase("Phase 5", 0.39f, "Adds an aurora bullet hell, below 40% health."),
			new BossPhase("Phase 6", 0.24f, "Adds Eternal Winter, below 25% health."),
		], "Has no separate shield. Becomes invulnerable after about 10 seconds outside the Snow biome.",
			readyTypes: [cryogen]);

		AddVariant(registrar, Calamity, "AquaticScourge", [
			new BossPhase("Phase 1", 1f, "Opening attack pattern after a 6.5 second intro."),
			new BossPhase("Phase 2", 0.66f, "Adds perpendicular spike barrages, below 67% health."),
			new BossPhase("Phase 3", 0.24f, "Gas breath, acid rain, typhoons and rising acid, below 25% health."),
		], "Can't be hurt while you're out of the water until its final phase.");

		AddVariant(registrar, Calamity, "BrimstoneElemental", [
			new BossPhase("Phase 1", 1f, "Bombardments and fire, after a 4 second invulnerable intro."),
			new BossPhase("Phase 2", 0.49f, "More bombardments, faster fire and eye laser beams, below 50% health."),
		], "Invulnerable unless you're in the Underworld.");

		AddVariant(registrar, Calamity, "CalamitasClone", [
			new BossPhase("Phase 1", 1f, "Wand fireballs and other spells, with one hex on you."),
			new BossPhase("Brothers", 0.54f,
				"Cataclysm and Catastrophe share one health pool while she waits, invulnerable, below 55% health. "
				+ "She returns with two hexes once both die."),
			// Starting below 20% passes both thresholds on the same tick, so the brothers never spawn.
			new BossPhase("Final phase", 0.19f, "Darts, fire slashes and fire bursts with no hexes, below 20% health. Skips the brothers."),
		]);

		int anahita = NPCType(Calamity, "Anahita");
		int leviathan = NPCType(Calamity, "Leviathan");
		// Anahita's ai[3] = 1 marks the Leviathan as already summoned. With both alive and the Leviathan at or below
		// half health, the two fight together.
		BossSpawner spawnTogether = (player, _) => {
			BossSpawners.SpawnAt(player, anahita, new Vector2(0f, -300f), ai3: 1f);
			BossSpawners.SpawnAt(player, leviathan, new Vector2(0f, 500f));
		};
		AddVariant(registrar, Calamity, "Leviathan", [
			new BossPhase("Anahita", 1f, "Anahita alone."),
			new BossPhase("Leviathan arrives", 0.49f,
				"Anahita dives to summon the Leviathan, below 50% health, and hides until it's below 50%. She needs water to dive."),
			new BossPhase("Together", 0.49f, "Both fight at once with combined attacks.") {
				Spawner = spawnTogether,
			},
			// Spawned slightly below the player because it rises for 3 seconds before attacking.
			new BossPhase("Leviathan alone", 1f, "Anahita is already dead, so the Leviathan is enraged.") {
				Spawner = (player, _) => BossSpawners.SpawnAt(player, leviathan, new Vector2(0f, 500f)),
			},
		], "Both are invulnerable away from the ocean edges of the world. Whichever is left alone enrages.");

		AddVariant(registrar, Calamity, "AstrumAureus", [
			new BossPhase("Phase 1", 1f, "Walks, jumps and fires lasers. Waits 1.5 seconds, or until hit, before attacking."),
			new BossPhase("Phase 2", 0.59f, "More missiles on landing, below 60% health."),
			new BossPhase("Phase 3", 0.44f, "Adds the astral drill laser and faster comets, below 45% health."),
		]);

		AddVariant(registrar, Calamity, "PlaguebringerGoliath", [
			new BossPhase("Phase 1", 1f, "Charges and carpet bombing."),
			new BossPhase("Phase 2", 0.74f, "Heavier carpet bombing and drones, below 75% health."),
			new BossPhase("Phase 3", 0.29f, "Bomb constructors and the nuke, below 30% health."),
		], "Enrages above the underground.");

		int ravagerHead = NPCType(Calamity, "RavagerHead");
		int[] ravagerLimbs = [
			NPCType(Calamity, "RavagerClawLeft"), NPCType(Calamity, "RavagerClawRight"),
			NPCType(Calamity, "RavagerLegLeft"), NPCType(Calamity, "RavagerLegRight"),
		];
		AddVariant(registrar, Calamity, "Ravager", [
			new BossPhase("Full fight", 1f, "Body with its head, claws and legs. The body can't be hurt yet."),
			new BossPhase("Body exposed", 1f, "Every limb and the head destroyed: wall slams and flame pillars inside a locked arena.",
				PhaseSetups.KillAll([ravagerHead, .. ravagerLimbs])),
		], "Health applies to the body and every limb. Its claws share one pool, as do its legs.");

		AddVariant(registrar, Calamity, "AstrumDeus", [
			new BossPhase("Phase 1", 1f, "Can't be hurt until its first attack ends."),
			new BossPhase("Phase 2", 0.59f, "Adds constellation explosions, vortexes and a solar system, below 60% health."),
			new BossPhase("Phase 3", 0.32f, "Flies up to shed its shell, then warp charges and dark god outbursts, below 33% health."),
		], "Enrages if you go far to the side of its beacon. Doesn't split in two.");
	}

	private void ModifyPostMoonLordBosses(BossRegistrar registrar)
	{
	}
}
