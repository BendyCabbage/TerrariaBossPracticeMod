using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;

namespace BossPractice.Common.Bosses.Sources;

// Calamity's bosses from Desert Scourge to Astrum Deus.
public sealed partial class CalamityBosses
{
	partial void AddPreMoonLordBosses(BossRegistrar registrar)
	{
		int desertScourgeHead = NPCType("DesertScourgeHead");
		int desertScourgeBody = NPCType("DesertScourgeBody");
		int desertScourgeTail = NPCType("DesertScourgeTail");
		int desertNuisanceHead = NPCType("DesertNuisanceHead");
		int desertNuisanceHeadYoung = NPCType("DesertNuisanceHeadYoung");
		registrar.Add(new BossDefinition("DesertScourge", desertScourgeHead) {
			Progression = 1.6f,
			Aliases = ["ds"],
			// Body and tail share the head's health pool.
			ReadyTypes = [desertScourgeHead, desertScourgeTail],
			// The Nuisances are summoned with NPC.SpawnOnPlayer, so they have no parent to inherit the fight from.
			RelatedTypes = [desertScourgeBody, desertScourgeTail, desertNuisanceHead, desertNuisanceHeadYoung],
			FallbackOffset = new Vector2(0f, 600f),
			Notes = "Enrages outside the Desert.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Chases you underground. Revengeance and Death: also burrows and lunges."),
				new BossPhase("Burrow lunges", () => Expert ? 0.74f : 0.49f,
					"Burrows and lunges up at you, below 75% health (50% in Normal)."),
				// Summoned once, the first tick below 50%.
				new BossPhase("Desert Nuisances", 0.49f,
					"Expert and Master only: two Desert Nuisances join and the Scourge hides until both are dead."),
				new BossPhase("After the Nuisances", 0.49f, "The Nuisances are already dealt with.",
					PhaseSetups.SetLocalAI(desertScourgeHead, 2, 1f)),
			],
		});

		int crabulon = NPCType("Crabulon");
		// Crabulon spawns Crab Shrooms each time it loses 5% health since localAI[0].
		// Resetting it stops the health change from triggering a burst of them.
		Action<IReadOnlyList<NPC>> resetCrabShroomTracker = PhaseSetups.ForEach(crabulon, npc => npc.localAI[0] = npc.life);
		registrar.Add(new BossDefinition("Crabulon", crabulon) {
			Progression = 2.7f,
			Aliases = ["crab"],
			Spawner = BossSpawners.AtOffset(new Vector2(0f, -400f)),
			DefaultPhases = [
				new BossPhase("Full fight", 1f, "Starts the fight from the beginning.", resetCrabShroomTracker),
				new BossPhase("Phase 2", 0.65f, "Expert and Master only: faster, with more mushroom bombs, below 66% health.",
					resetCrabShroomTracker),
				new BossPhase("Crab smash", 0.49f, "Death only: adds crab smash jumps below 50% health.", resetCrabShroomTracker),
				new BossPhase("Phase 3", 0.32f, "Expert and Master only: even more mushroom bombs, below 33% health.",
					resetCrabShroomTracker),
				new BossPhase("Phase 4", 0.14f, "Death only: rests between attacks but moves fastest, below 15% health.",
					resetCrabShroomTracker),
			],
		});

		int hiveMind = NPCType("HiveMind");
		registrar.Add(new BossDefinition("HiveMind", hiveMind) {
			Progression = 3.98f,
			Aliases = ["hm", "hive"],
			CheckCanSpawn = player => player.ZoneCorrupt ? null : "The Hive Mind can only be spawned in the Corruption.",
			Notes = "Must be fought in the Corruption; it burrows away anywhere else.",
			DefaultPhases = [
				// Phase 1 spawns minions each time it loses 5% health since ai[3]; reset it so the health change doesn't.
				new BossPhase("Full fight", 1f, "Burrowing ground form with Hive Blobs.",
					PhaseSetups.ForEach(hiveMind, npc => npc.ai[3] = npc.life)),
				new BossPhase("Phase 2", 0.79f, "Flying form, below 80% health."),
				new BossPhase("New attacks", 0.59f,
					"Revengeance and Death only: spinning lunges and minion arcs, below 60% health."),
				new BossPhase("Dark Hearts", 0.49f, "Expert and Master only: Dark Hearts spawn as it loses health, below 50% health."),
				new BossPhase("Rain dashes", 0.29f, "Revengeance only: adds rain dashes below 30% health (Death has them from 60%)."),
			],
		});

		int perforatorHive = NPCType("PerforatorHive");
		registrar.Add(new BossDefinition("Perforators", perforatorHive) {
			Progression = 3.99f,
			Aliases = ["perfs", "perforator"],
			CheckCanSpawn = player => player.ZoneCrimson ? null : "The Perforators can only be spawned in the Crimson.",
			Notes = "Must be fought in the Crimson; the hive leaves anywhere else. Worms always spawn in order: small, medium, then large.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "The hive alone."),
				new BossPhase("Small worm", 0.74f, "The small worm bursts out, below 75% health (two in Death)."),
				new BossPhase("Ichor blobs", 0.69f, "Small worm, plus ichor blob attacks below 70% health."),
				new BossPhase("Medium worm", 0.49f, "The small and medium worms spawn one after the other."),
				new BossPhase("Large worm", 0.24f, "All three worms spawn one after the other."),
			],
		});

		int slimeGodCore = NPCType("SlimeGodCore");
		int ebonianPaladin = NPCType("EbonianPaladin");
		int crimulanPaladin = NPCType("CrimulanPaladin");
		registrar.Add(new BossDefinition("SlimeGod", slimeGodCore) {
			Progression = 6.7f,
			Aliases = ["sg"],
			// The core can never be damaged; the large slimes hold the fight's health.
			HealthTypes = [ebonianPaladin, crimulanPaladin],
			ReadyTypes = [slimeGodCore, ebonianPaladin, crimulanPaladin],
			Notes = "Health applies to both large slimes. The core can't be hurt; the fight ends when every slime is dead.",
			DefaultPhases = [
				new BossPhase("Full fight", 1f, "Both large slimes with the core."),
				// Each large slime splits the tick it reaches 50% health.
				new BossPhase("Split slimes", 0.49f, "Expert and Master only: both large slimes split into two smaller ones."),
				new BossPhase("Ebonian Paladin defeated", 1f, "Only the Crimulan Paladin remains, enraged, and the core is more aggressive.",
					PhaseSetups.KillAll(ebonianPaladin)),
				new BossPhase("Crimulan Paladin defeated", 1f, "Only the Ebonian Paladin remains, enraged, and the core is more aggressive.",
					PhaseSetups.KillAll(crimulanPaladin)),
			],
		});

		int cryogen = NPCType("Cryogen");
		int cryogenShield = NPCType("CryogenShield");
		registrar.Add(new BossDefinition("Cryogen", cryogen) {
			Progression = 8.5f,
			Aliases = ["cryo"],
			ReadyTypes = [cryogen, cryogenShield],
			Notes = "Invulnerable while its shield is up.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Ice blasts. Death: skipped, starts in phase 2."),
				new BossPhase("Phase 2", () => ByDifficulty(0.79f, 0.79f, 0.84f, 1f),
					"Adds charges, below 80% health (85% Revengeance, always in Death)."),
				new BossPhase("Phase 3", () => ByDifficulty(0.59f, 0.59f, 0.69f, 0.79f),
					"Below 60% health (70% Revengeance, 80% Death)."),
				new BossPhase("Phase 4", () => ByDifficulty(0.39f, 0.39f, 0.54f, 0.59f),
					"Teleports and ice attacks, below 40% health (55% Revengeance, 60% Death)."),
				new BossPhase("Phase 5", () => ByDifficulty(0.29f, 0.29f, 0.44f, 0.49f),
					"Constant charges, below 30% health (45% Revengeance, 50% Death)."),
				new BossPhase("Phase 6", () => ByDifficulty(0.24f, 0.24f, 0.24f, 0.34f),
					"Revengeance and Death only: charges fire homing ice blasts, below 25% health (35% Death)."),
				new BossPhase("Phase 7", () => ByDifficulty(0.14f, 0.14f, 0.14f, 0.24f),
					"Revengeance and Death only: triple ice blast spreads, below 15% health (25% Death)."),
			],
		});

		int aquaticScourgeHead = NPCType("AquaticScourgeHead");
		int aquaticScourgeTail = NPCType("AquaticScourgeTail");
		registrar.Add(new BossDefinition("AquaticScourge", aquaticScourgeHead) {
			Progression = 9.5f,
			Aliases = ["as"],
			// Body and tail segments share the head's health pool.
			ReadyTypes = [aquaticScourgeHead, aquaticScourgeTail],
			FallbackOffset = new Vector2(0f, 600f),
			Notes = "Passive until hit, and swims away if left alone outside the Ocean or Sulphurous Sea. Once hostile it only enrages there.",
			DefaultPhases = [
				// It turns hostile at 99.9% health or below.
				new BossPhase("Full fight", 0.998f, "Starts at 99.8% health so it's hostile right away instead of passive."),
				new BossPhase("Phase 2", 0.74f, "Adds sand and poison cloud barrages, below 75% health."),
				new BossPhase("Phase 3", 0.49f, "More barrages and toxic cloud rings; body teeth stop, below 50% health."),
				new BossPhase("Phase 4", 0.24f, "Body teeth return, accelerating, below 25% health."),
			],
		});

		int brimstoneElemental = NPCType("BrimstoneElemental");
		registrar.Add(new BossDefinition("BrimstoneElemental", brimstoneElemental) {
			Progression = 10.5f,
			Aliases = ["brimstone", "brimmy", "be"],
			Notes = "Enrages outside the Brimstone Crag.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Teleports, flame cocoons and brimstone roses."),
				new BossPhase("Phase 2", 0.49f, "Revengeance and Death only: adds the laser beam, below 50% health."),
				new BossPhase("Phase 3", 0.32f, "Revengeance and Death only: stops teleporting, below 33% health."),
			],
		});

		int calamitasClone = NPCType("CalamitasClone");
		// Calamity counts her phase events down in newAI[0]: 1 at the start, then 0.3 less for each event
		// (bullet hell below 70%, brothers below 40%, bullet hell below 10%). It fires one event per tick
		// until it catches up with her health, so setting it skips the events before the chosen phase.
		Action<IReadOnlyList<NPC>> SkipEventsTo(float eventCounter, bool skipSoulSeekers = false) => PhaseSetups.ForEach(calamitasClone, npc => {
			SetCalamityAI(npc, 0, eventCounter);
			if (skipSoulSeekers) {
				// newAI[1] marks the Soul Seeker ring (below 35%) as already spawned.
				SetCalamityAI(npc, 1, 1f);
			}
		});
		registrar.Add(new BossDefinition("CalamitasClone", calamitasClone) {
			Progression = 11.7f,
			Aliases = ["calclone", "cc", "clone"],
			Notes = "Revengeance and Death: she's invulnerable while you're outside her arena.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Starts the fight from the beginning."),
				new BossPhase("Bullet hell 1", 0.69f, "Starts with the first bullet hell, below 70% health."),
				new BossPhase("Phase 2", 0.69f, "The first bullet hell is already over. Revengeance and Death: adds dashes.",
					SkipEventsTo(0.7f)),
				new BossPhase("Brothers", 0.39f, "Cataclysm and Catastrophe join; she's invulnerable until both die.",
					SkipEventsTo(0.7f)),
				new BossPhase("Soul Seekers", 0.34f,
					"Expert and Master only: a ring of Soul Seekers spawns, below 35% health. The brothers are already dead.",
					SkipEventsTo(0.4f)),
				new BossPhase("Bullet hell 2", 0.09f, "Starts with the second bullet hell, below 10% health.",
					SkipEventsTo(0.4f, skipSoulSeekers: true)),
				new BossPhase("Final stretch", 0.09f, "Every bullet hell is over. Death: constant charges.",
					SkipEventsTo(0.1f, skipSoulSeekers: true)),
			],
		});

		int anahita = NPCType("Anahita");
		int leviathan = NPCType("Leviathan");
		registrar.Add(new BossDefinition("Leviathan", leviathan, "The Leviathan and Anahita") {
			Progression = 12.8f,
			Aliases = ["levi", "anahita", "siren"],
			// The Leviathan only arrives once Anahita summons it, from a projectile.
			SpawnTypes = [anahita],
			HealthTypes = [anahita, leviathan],
			// Apply health as soon as either one exists; waiting for both would stall every Anahita-only start.
			ReadyTypes = [],
			Notes = "Both enrage away from the Ocean. Anahita summons the Leviathan below 40% health (straight away in Death) "
				+ "and hides until it drops below 40% (70% in Death).",
			DefaultPhases = [
				new BossPhase("Anahita", 1f, "Anahita alone, behind her ice shield."),
				new BossPhase("Anahita phase 2", 0.69f, "Stronger attacks and a second ice shield, below 70% health."),
				new BossPhase("Leviathan arrives", 0.39f, "Anahita dives to summon the Leviathan, which rises from below."),
				// Spawned slightly below the player because it rises for 3 seconds before attacking.
				new BossPhase("Leviathan alone", 1f, "Anahita is already dead, so the Leviathan uses every attack.") {
					Spawner = (player, _) => BossSpawners.SpawnAt(player, leviathan, new Vector2(0f, 500f)),
				},
				new BossPhase("Leviathan alone, final phase", () => Death ? 0.39f : 0.19f,
					"Anahita is already dead. Fastest attacks, below 20% health (40% in Death).") {
					Spawner = (player, _) => BossSpawners.SpawnAt(player, leviathan, new Vector2(0f, 500f)),
				},
			],
		});

		int astrumAureus = NPCType("AstrumAureus");
		registrar.Add(new BossDefinition("AstrumAureus", astrumAureus) {
			Progression = 12.81f,
			Aliases = ["aureus", "aa"],
			Spawner = BossSpawners.AtOffset(new Vector2(0f, -500f)),
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Walks, jumps and stomps."),
				new BossPhase("Phase 2", () => ByDifficulty(0.74f, 0.79f, 0.84f, 0.84f),
					"Laser spreads while walking and stomping, below 75% health (80% Expert, 85% Revengeance)."),
				new BossPhase("Phase 3", () => ByDifficulty(0.49f, 0.59f, 0.69f, 0.69f),
					"Adds the teleport, below 50% health (60% Expert, 70% Revengeance)."),
				new BossPhase("Phase 4", () => ByDifficulty(0.39f, 0.39f, 0.49f, 0.49f),
					"Expert and Master only: faster astral flame barrages, below 40% health (50% Revengeance)."),
				new BossPhase("Phase 5", 0.29f, "Revengeance and Death only: walks faster and stomps more, below 30% health."),
			],
		});

		int plaguebringerGoliath = NPCType("PlaguebringerGoliath");
		registrar.Add(new BossDefinition("PlaguebringerGoliath", plaguebringerGoliath) {
			Progression = 14.5f,
			Aliases = ["pbg", "plaguebringer"],
			Notes = "Enrages outside the Jungle.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Charges, missiles and mines."),
				new BossPhase("Phase 2", 0.74f, "Faster charges and more missiles, below 75% health. Revengeance and Death: diagonal dashes."),
				new BossPhase("Phase 3", 0.49f, "Bigger missile volleys, below 50% health. Expert and Master: the nuke barrage countdown starts."),
				new BossPhase("Phase 4", 0.24f, "Faster charges and missile volleys, below 25% health."),
				new BossPhase("Phase 5", 0.09f, "Fastest charges and the most missiles, below 10% health."),
			],
		});

		int ravagerBody = NPCType("RavagerBody");
		int ravagerHead = NPCType("RavagerHead");
		int ravagerClawLeft = NPCType("RavagerClawLeft");
		int ravagerClawRight = NPCType("RavagerClawRight");
		int ravagerLegLeft = NPCType("RavagerLegLeft");
		int ravagerLegRight = NPCType("RavagerLegRight");
		int ravagerFreeHead = NPCType("RavagerHead2");
		registrar.Add(new BossDefinition("Ravager", ravagerBody) {
			Progression = 16.5f,
			Aliases = ["rav"],
			// Every limb has its own health pool.
			HealthTypes = [ravagerBody, ravagerHead, ravagerClawLeft, ravagerClawRight, ravagerLegLeft, ravagerLegRight],
			// The head breaks free as a separate flying head when destroyed.
			RelatedTypes = [ravagerFreeHead],
			Spawner = BossSpawners.AtOffset(new Vector2(0f, -600f)),
			Notes = "Health applies to the body and every limb. The body is invulnerable until the head, claws and legs are destroyed.",
			DefaultPhases = [
				new BossPhase("Full fight", 1f, "Body with its head, claws and legs."),
				new BossPhase("Body exposed", 1f, "Every limb destroyed and the head flies free. Expert and Master: slam jumps.",
					PhaseSetups.KillAll(ravagerHead, ravagerClawLeft, ravagerClawRight, ravagerLegLeft, ravagerLegRight)),
			],
		});

		int astrumDeusHead = NPCType("AstrumDeusHead");
		int astrumDeusBody = NPCType("AstrumDeusBody");
		int astrumDeusTail = NPCType("AstrumDeusTail");
		// Recreates the split from Calamity's AI: newAI[0] marks each split worm (1 and 2),
		// and the second worm's flight timer (newAI[3]) is offset so the two take turns flying at you.
		BossSpawner spawnSplitWorms = (player, _) => {
			NPC first = BossSpawners.SpawnAt(player, astrumDeusHead, new Vector2(-400f, 600f));
			if (first != null) {
				SetCalamityAI(first, 0, 1f);
			}

			NPC second = BossSpawners.SpawnAt(player, astrumDeusHead, new Vector2(400f, 600f));
			if (second != null) {
				SetCalamityAI(second, 0, 2f);
				SetCalamityAI(second, 3, Main.getGoodWorld ? 300f : 600f);
			}
		};
		registrar.Add(new BossDefinition("AstrumDeus", astrumDeusHead) {
			Progression = 17.5f,
			Aliases = ["deus", "ad"],
			// Body and tail segments share their head's health pool.
			ReadyTypes = [astrumDeusHead, astrumDeusTail],
			RelatedTypes = [astrumDeusBody, astrumDeusTail],
			FallbackOffset = new Vector2(0f, 600f),
			Notes = "Splits into two full-health worms below 50% health. Killing one split worm kills the other, except in Death, where the survivor enrages.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "One long worm. Death: also flies at you from the start."),
				new BossPhase("Flight", 0.79f, "Alternates between burrowing and flying at you, below 80% health."),
				new BossPhase("Splitting mines", 0.69f, "Adds splitting mines, below 70% health."),
				new BossPhase("Split", 0.49f, "Fades out and splits into two worms, below 50% health."),
				new BossPhase("Split worms", 1f, "Starts with the two split worms.") {
					Spawner = spawnSplitWorms,
				},
				new BossPhase("Split worms phase 2", 0.49f, "Expert and Master only: the split worms speed up, below 50% health.") {
					Spawner = spawnSplitWorms,
				},
				new BossPhase("Split worms phase 3", 0.19f,
					"Expert and Master only: moving mines (below 30%) and top speed, below 20% health.") {
					Spawner = spawnSplitWorms,
				},
			],
		});
	}
}
