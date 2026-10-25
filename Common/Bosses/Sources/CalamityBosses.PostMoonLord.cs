using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;

namespace BossPractice.Common.Bosses.Sources;

public sealed partial class CalamityBosses
{
	partial void AddPostMoonLordBosses(BossRegistrar registrar)
	{
		AddProfanedGuardians(registrar);
		AddDragonfolly(registrar);
		AddProvidence(registrar);
		AddCeaselessVoid(registrar);
		AddStormWeaver(registrar);
		AddSignus(registrar);
		AddPolterghast(registrar);
		AddOldDuke(registrar);
		AddDevourerOfGods(registrar);
		AddYharon(registrar);
		AddExoMechs(registrar);
		AddSupremeCalamitas(registrar);
		AddPrimordialWyrm(registrar);
	}

	private void AddProfanedGuardians(BossRegistrar registrar)
	{
		int commander = NPCType("ProfanedGuardianCommander");
		int defender = NPCType("ProfanedGuardianDefender");
		int healer = NPCType("ProfanedGuardianHealer");

		// The Commander spawns the other two on its first tick. Each guardian has its own health pool.
		registrar.Add(new BossDefinition("ProfanedGuardians", commander, "Profaned Guardians") {
			Progression = 18.5f,
			Aliases = ["guardians", "pg"],
			HealthTypes = [commander, defender, healer],
			ReadyTypes = [commander, defender, healer],
			Notes = "Health applies to all three guardians.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "All three guardians; the Defender shields the Commander."),
				new BossPhase("Phase 2", 1f, "The Healer is already dead.", PhaseSetups.KillAll(healer)),
				// The Defender's rock shield destroys itself once the Defender is gone.
				new BossPhase("Phase 3", 1f, "Only the Commander remains, adding spear volleys.", PhaseSetups.KillAll(healer, defender)),
			],
		});
	}

	private void AddDragonfolly(BossRegistrar registrar)
	{
		int dragonfolly = NPCType("Dragonfolly");

		registrar.Add(new BossDefinition("Dragonfolly", dragonfolly) {
			Progression = 18.6f,
			Aliases = ["bumblebirb", "folly"],
			Notes = "Enrages outside the Jungle.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Charges, red lightning and swarmers."),
				new BossPhase("Phase 2", () => ByDifficulty(0.49f, 0.49f, 0.74f, 0.74f), "More red lightning, below 50% health (75% in Revengeance)."),
				new BossPhase("Phase 3", () => ByDifficulty(0.09f, 0.09f, 0.24f, 0.39f),
					"Expert and Master only: new attacks and more swarmers, below 10% health (25% Revengeance, 40% Death)."),
			],
		});
	}

	private void AddProvidence(BossRegistrar registrar)
	{
		int providence = NPCType("Providence");

		registrar.Add(new BossDefinition("Providence", providence) {
			Progression = 19f,
			Aliases = ["prov", "provi"],
			Notes = "Only uses her Holy Ray in the Hallow and her crystal in the Underworld. Her For the Worthy full-power mode can't be practised.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Opening attack rotation."),
				new BossPhase("Phase 2", 0.74f, "Holy Ray (Hallow) or crystal (Underworld) join below 75% health."),
				// She summons her guardians once she has lost 66% of her health since spawning.
				new BossPhase("Guardians", 0.33f, "Her guardians spawn right away and empower her while they live."),
				new BossPhase("Final phase", () => ByDifficulty(0.14f, 0.14f, 0.14f, 0.19f),
					"Below 15% health (20% in Death) her guardians no longer empower her."),
			],
		});
	}

	private void AddCeaselessVoid(BossRegistrar registrar)
	{
		int ceaselessVoid = NPCType("CeaselessVoid");
		int darkEnergy = NPCType("DarkEnergy");

		// newAI[0] is the health ratio (plus 30%) at which the next Dark Energy wave spawns. It starts at 1 and drops
		// by 0.3 per wave, so setting it stops the presets from spawning every earlier wave at once.
		registrar.Add(new BossDefinition("CeaselessVoid", ceaselessVoid) {
			Progression = 19.6f,
			Aliases = ["cv", "ceaseless"],
			ReadyTypes = [ceaselessVoid, darkEnergy],
			CheckCanSpawn = player => player.position.Y >= Main.worldSurface * 16.0 ? null : "Ceaseless Void can only be fought underground.",
			Notes = "Leaves if you go above the surface.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Invulnerable until its Dark Energies are destroyed."),
				new BossPhase("Phase 2", 0.69f, "Adds rings of dark energy balls in Expert and above.",
					PhaseSetups.ForEach(ceaselessVoid, npc => SetCalamityAI(npc, 0, 0.7f))),
				new BossPhase("Phase 3", 0.39f, "Bigger Dark Energy waves.",
					PhaseSetups.ForEach(ceaselessVoid, npc => SetCalamityAI(npc, 0, 0.4f))),
				new BossPhase("Final wave", 0.09f, "Last Dark Energy wave, then the final pull.",
					PhaseSetups.ForEach(ceaselessVoid, npc => SetCalamityAI(npc, 0, 0.1f))),
				new BossPhase("Final pull", 0.09f, "Dark Energies already destroyed; it drains itself for about 20 seconds, then implodes.",
					PhaseSetups.All(
						PhaseSetups.ForEach(ceaselessVoid, npc => SetCalamityAI(npc, 0, 0.1f)),
						PhaseSetups.KillAll(darkEnergy))),
			],
		});
	}

	private void AddStormWeaver(BossRegistrar registrar)
	{
		int head = NPCType("StormWeaverHead");
		int body = NPCType("StormWeaverBody");
		int tail = NPCType("StormWeaverTail");

		// Body and tail segments share the head's health pool.
		registrar.Add(new BossDefinition("StormWeaver", head) {
			Progression = 19.61f,
			Aliases = ["sw", "weaver"],
			ReadyTypes = [head, body, tail],
			RelatedTypes = [body, tail],
			FallbackOffset = new Vector2(0f, 600f),
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Armored: only the tail takes full damage."),
				new BossPhase("Phase 2", 0.79f, "Armor shed; starts charging, below 80% health."),
				new BossPhase("Phase 3", 0.54f, "Frost waves replace lightning during charges, below 55% health."),
				new BossPhase("Phase 4", 0.29f, "Tornadoes, frost waves and a storm, below 30% health."),
			],
		});
	}

	private void AddSignus(BossRegistrar registrar)
	{
		int signus = NPCType("Signus");

		registrar.Add(new BossDefinition("Signus", signus) {
			Progression = 19.62f,
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Teleports, scythes and lantern charges."),
				new BossPhase("Phase 2", 0.74f, "Expert and Master only: extra lanterns during charges, below 75% health."),
				new BossPhase("Final phase", 0.32f, "Stops teleporting, below 33% health."),
			],
		});
	}

	private void AddPolterghast(BossRegistrar registrar)
	{
		int polterghast = NPCType("Polterghast");
		int hook = NPCType("PolterghastHook");

		// The clone only spawns when phase 3 starts from phase 2 (ai[0] == 1), so presets past phase 3 set it.
		var spawnClone = PhaseSetups.SetAI(polterghast, 0, 1f);

		registrar.Add(new BossDefinition("Polterghast", polterghast) {
			Progression = 20f,
			Aliases = ["polter"],
			ReadyTypes = [polterghast, hook],
			Notes = "Enrages, then leaves, unless you're in the Dungeon or underground.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Hooked in place, firing blue shots."),
				new BossPhase("Phase 2", () => ByDifficulty(0.49f, 0.64f, 0.79f, 0.89f),
					"Necroghast: red shots and harder charges, below 50% health (65% Expert, 80% Revengeance, 90% Death)."),
				new BossPhase("Phase 3", () => ByDifficulty(0.19f, 0.34f, 0.49f, 0.59f),
					"Necroplasm: its clone spawns, below 20% health (35% Expert, 50% Revengeance, 60% Death).", spawnClone),
				new BossPhase("Phase 4", () => ByDifficulty(0.09f, 0.19f, 0.34f, 0.44f),
					"Adds spirit volleys, below 10% health (20% Expert, 35% Revengeance, 45% Death). The clone spawns too.", spawnClone),
				new BossPhase("Phase 5", () => ByDifficulty(0.04f, 0.09f, 0.14f, 0.19f),
					"Fastest charges, below 5% health (10% Expert, 15% Revengeance, 20% Death). The clone spawns too.", spawnClone),
			],
		});
	}

	private void AddOldDuke(BossRegistrar registrar)
	{
		int oldDuke = NPCType("OldDuke");

		// ai[0] 4 and 9 are its phase 2 and phase 3 transitions. Jumping to one skips the spawn intro and any
		// earlier transition. The intro leaves it invisible, and state 9 doesn't fade it back in.
		Action<IReadOnlyList<NPC>> JumpToTransition(float state) => PhaseSetups.ForEach(oldDuke, npc => {
			npc.localAI[0] = 1f;
			npc.ai[0] = state;
			npc.ai[1] = 0f;
			npc.ai[2] = 0f;
			npc.alpha = 0;
		});

		registrar.Add(new BossDefinition("OldDuke", oldDuke) {
			Progression = 20.5f,
			Aliases = ["od"],
			FallbackOffset = new Vector2(0f, -300f),
			Notes = "Enrages unless you're at the surface near an ocean.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Dashes, tooth balls and Sulphurous Sharkrons."),
				new BossPhase("Phase 2", () => ByDifficulty(0.49f, 0.49f, 0.69f, 0.79f),
					"Faster attacks and vortexes, below 50% health (70% Revengeance, 80% Death).", JumpToTransition(4f)),
				new BossPhase("Phase 3", () => ByDifficulty(0.19f, 0.19f, 0.34f, 0.49f),
					"Expert and Master only: teleporting frenzy, below 20% health (35% Revengeance, 50% Death).", JumpToTransition(9f)),
			],
		});
	}

	private void AddDevourerOfGods(BossRegistrar registrar)
	{
		int head = NPCType("DevourerofGodsHead");
		int body = NPCType("DevourerofGodsBody");
		int tail = NPCType("DevourerofGodsTail");

		// Segments spawn on the head's first tick and share its health. Its phase 2 portal transition can't be skipped.
		registrar.Add(new BossDefinition("DevourerofGods", head) {
			Progression = 21f,
			Aliases = ["dog", "devourer"],
			ReadyTypes = [head, body, tail],
			RelatedTypes = [body, tail],
			Notes = "Phase 2 always opens with a 12 second invulnerable portal transition.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Ground and flying passes with laser walls."),
				new BossPhase("Phase 2", 0.64f, "God Slayer form: rift teleports and laser walls, below 65% health."),
				new BossPhase("Final phase", 0.24f, "Chained rift dashes and more laser walls, below 25% health."),
			],
		});
	}

	private void AddYharon(BossRegistrar registrar)
	{
		int yharon = NPCType("Yharon");

		// ai[0] 4, 10 and 17 are the roars into phase 1 part 2, part 3 and phase 2. Jumping to one skips the spawn
		// intro and the earlier roars; localAI[0] marks the intro as started.
		Action<IReadOnlyList<NPC>> JumpToRoar(float state) => PhaseSetups.ForEach(yharon, npc => {
			npc.localAI[0] = 1f;
			npc.localAI[1] = 0f;
			npc.ai[0] = state;
			npc.ai[1] = 0f;
			npc.ai[2] = 0f;
		});

		registrar.Add(new BossDefinition("Yharon", yharon) {
			Progression = 22f,
			Notes = "Its arena is centred where you stand when it spawns; leaving it enrages Yharon.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Charges, flare bombs and tornadoes."),
				new BossPhase("Phase 1, part 2", () => ByDifficulty(0.79f, 0.84f, 0.89f, 1f),
					"Bullet hell and fireball rings. Death skips straight here.", JumpToRoar(4f)),
				new BossPhase("Phase 1, part 3", () => ByDifficulty(0.64f, 0.69f, 0.74f, 0.79f),
					"Flare dust bullet hell and Infernados.", JumpToRoar(10f)),
				// Phase 2 is a private flag, so every phase 2 preset passes through the roar into it.
				new BossPhase("Phase 2", 0.54f, "Resplendent Phoenix, after a 5 second invincibility.", JumpToRoar(17f)),
				new BossPhase("Phase 2, part 2", () => ByDifficulty(0.29f, 0.34f, 0.44f, 0.54f),
					"Second phase 2 pattern. Death skips straight here.", JumpToRoar(17f)),
				new BossPhase("Phase 2, part 3", () => ByDifficulty(0.14f, 0.19f, 0.29f, 0.34f),
					"Third phase 2 pattern; plays each earlier phase 2 roar first.", JumpToRoar(17f)),
				new BossPhase("Phase 2, part 4", () => ByDifficulty(0.14f, 0.14f, 0.14f, 0.19f),
					"Revengeance and Death only: final pattern; plays each earlier phase 2 roar first.", JumpToRoar(17f)),
			],
		});
	}

	private void AddExoMechs(BossRegistrar registrar)
	{
		int ares = NPCType("AresBody");
		int aresLaser = NPCType("AresLaserCannon");
		int aresPlasma = NPCType("AresPlasmaFlamethrower");
		int aresTesla = NPCType("AresTeslaCannon");
		int aresNuke = NPCType("AresGaussNuke");
		int thanatos = NPCType("ThanatosHead");
		int thanatosBody1 = NPCType("ThanatosBody1");
		int thanatosBody2 = NPCType("ThanatosBody2");
		int thanatosTail = NPCType("ThanatosTail");
		int artemis = NPCType("Artemis");
		int apollo = NPCType("Apollo");

		// Every mech part is flagged as a boss. Ares's arms and Thanatos's segments share their owner's health.
		int[] aresParts = [aresLaser, aresPlasma, aresTesla, aresNuke];
		int[] thanatosParts = [thanatosBody1, thanatosBody2, thanatosTail];

		// A mech below 70% health with ai[3] == 0 and no other mech alive summons the others. ai[3] = 1 marks the
		// others as already summoned. Apollo decides for both twins. It has to be set in the same tick as health.
		var noSummon = PhaseSetups.All(PhaseSetups.SetAI(ares, 3, 1f), PhaseSetups.SetAI(thanatos, 3, 1f), PhaseSetups.SetAI(apollo, 3, 1f));

		// Each mech goes berserk below 40% health, or below 70% once it's the only mech left.
		const float Retreating = 0.69f;
		const float Berserk = 0.39f;

		BossSpawner SpawnOnly(params int[] types) => (player, boss) => {
			foreach (int type in types) {
				BossSpawners.SpawnNear(player, type, boss.FallbackOffset);
			}
		};

		registrar.Add(new BossDefinition("Ares", ares) {
			Progression = 22.99f,
			ReadyTypes = [ares, .. aresParts],
			// The other mechs are summoned with no parent entity, so list them to include them in the fight.
			RelatedTypes = [.. aresParts, thanatos, .. thanatosParts, artemis, apollo],
			Notes = "Full fight and Phase 2 bring in the other mechs; Solo presets never do.",
			DefaultPhases = [
				new BossPhase("Full fight", 1f, "Starts the Exo Mech fight with Ares. The others arrive below 70% health."),
				new BossPhase("Phase 2", Retreating, "Ares retreats at once while Thanatos and the twins attack."),
				new BossPhase("Solo", 1f, "Ares alone; goes berserk below 70% health.", noSummon),
				new BossPhase("Solo berserk", Retreating, "Ares as the last mech standing, berserk.", noSummon),
			],
		});

		registrar.Add(new BossDefinition("Thanatos", thanatos) {
			Progression = 22.99f,
			ReadyTypes = [thanatos, thanatosTail],
			RelatedTypes = [.. thanatosParts, ares, .. aresParts, artemis, apollo],
			FallbackOffset = new Vector2(0f, 600f),
			Notes = "Full fight and Phase 2 bring in the other mechs; Solo presets never do.",
			DefaultPhases = [
				new BossPhase("Full fight", 1f, "Starts the Exo Mech fight with Thanatos. The others arrive below 70% health."),
				new BossPhase("Phase 2", Retreating, "Thanatos retreats at once while Ares and the twins attack."),
				new BossPhase("Solo", 1f, "Thanatos alone; goes berserk below 70% health.", noSummon),
				new BossPhase("Solo berserk", Retreating, "Thanatos as the last mech standing, berserk.", noSummon),
			],
		});

		// The twins have separate health pools, but each keeps its health at or below the other's.
		registrar.Add(new BossDefinition("ExoTwins", apollo, "Artemis and Apollo") {
			Progression = 22.99f,
			Aliases = ["artemis", "apollo"],
			SpawnTypes = [artemis, apollo],
			HealthTypes = [artemis, apollo],
			RelatedTypes = [ares, .. aresParts, thanatos, .. thanatosParts],
			Notes = "Full fight and Phase 2 bring in the other mechs; Solo presets never do.",
			DefaultPhases = [
				new BossPhase("Full fight", 1f, "Starts the Exo Mech fight with the twins. The others arrive below 70% health."),
				new BossPhase("Phase 2", Retreating, "The twins retreat at once while Ares and Thanatos attack."),
				new BossPhase("Solo", 1f, "The twins alone; berserk below 70%, new forms below 60% health.", noSummon),
				new BossPhase("Solo berserk", Retreating, "The twins as the last mechs standing, berserk.", noSummon),
				new BossPhase("Solo berserk, final form", 0.59f, "As Solo berserk, after their form change.", noSummon),
			],
		});

		// Several mechs at once. Absent mechs count as destroyed, so the pair presets continue into the real
		// fight's last phases. A retreating mech only returns once another mech is below 70% health, so every
		// preset starts below that or a mech can stay hidden forever.
		int[] allMechs = [ares, thanatos, artemis, apollo];
		registrar.Add(new BossDefinition("ExoMechs", ares, "Exo Mechs") {
			Progression = 22.99f,
			Aliases = ["exo", "exos", "draedon"],
			SpawnTypes = allMechs,
			HealthTypes = allMechs,
			// Pair presets leave a mech out, so no type is always present. Every part spawns on its mech's first tick,
			// before the two-tick minimum.
			ReadyTypes = [],
			RelatedTypes = [.. aresParts, .. thanatosParts],
			Notes = "Presets start every mech below 70% health; above that, a retreating mech may never come back.",
			DefaultPhases = [
				new BossPhase("All three", Retreating, "All three mechs attack at a reduced rate.", noSummon),
				new BossPhase("Ares berserk", Retreating, "Ares goes berserk while the others retreat.",
					PhaseSetups.All(noSummon, PhaseSetups.SetHealth(ares, Berserk))),
				new BossPhase("Thanatos berserk", Retreating, "Thanatos goes berserk while the others retreat.",
					PhaseSetups.All(noSummon, PhaseSetups.SetHealth(thanatos, Berserk))),
				new BossPhase("Twins berserk", Retreating, "The twins go berserk while the others retreat.",
					PhaseSetups.All(noSummon, PhaseSetups.SetHealth(artemis, Berserk), PhaseSetups.SetHealth(apollo, Berserk))),
				new BossPhase("Ares and Thanatos", Retreating, "The twins are destroyed; both remaining mechs attack fully.", noSummon) {
					Spawner = SpawnOnly(ares, thanatos),
				},
				new BossPhase("Ares and twins", Retreating, "Thanatos is destroyed; both remaining mechs attack fully.", noSummon) {
					Spawner = SpawnOnly(ares, artemis, apollo),
				},
				new BossPhase("Thanatos and twins", Retreating, "Ares is destroyed; both remaining mechs attack fully.", noSummon) {
					Spawner = SpawnOnly(thanatos, artemis, apollo),
				},
			],
		});
	}

	private void AddSupremeCalamitas(BossRegistrar registrar)
	{
		int calamitas = NPCType("SupremeCalamitas");
		int sepulcher = NPCType("SepulcherHead");
		int heart = NPCType("BrimstoneHeart");
		int cataclysm = NPCType("SupremeCataclysm");
		int catastrophe = NPCType("SupremeCatastrophe");

		// Her phase progress lives in C# fields, so starting lower replays every earlier bullet hell. Waiting for the
		// first Sepulcher applies health after the first bullet hell (15 s, invulnerable), which then always plays.
		var skipSepulcher = PhaseSetups.KillAll(heart);

		registrar.Add(new BossDefinition("Calamitas", calamitas) {
			Progression = 23f,
			Aliases = ["scal", "supremecalamitas"],
			ReadyTypes = [calamitas, sepulcher, heart],
			// The brothers are summoned with no parent entity.
			RelatedTypes = [cataclysm, catastrophe],
			// Her arena is centred on her, so spawn her where Boss Rush does: just above the player (player.Top - (42, 84)).
			Spawner = (player, boss) => BossSpawners.SpawnAt(player, boss.PrimaryType, new Vector2(-42f, -player.height / 2f - 84f)),
			SetupTimeoutTicks = 1200,
			Notes = "Spawns on top of you so you start inside her arena. Earlier bullet hells always replay before a later phase.",
			DefaultPhases = [
				new BossPhase("Full fight", 1f, "First bullet hell, then the first Sepulcher."),
				new BossPhase("Sepulcher skipped", 1f, "The first Sepulcher's hearts break as it appears.", skipSepulcher),
				new BossPhase("Second bullet hell", 0.74f, "Second bullet hell, below 75% health.", skipSepulcher),
				new BossPhase("Brothers", 0.49f, "Bullet hells 2 and 3 replay, then Cataclysm and Catastrophe arrive.", skipSepulcher),
				new BossPhase("Final form", 0.29f, "Bullet hells 2 to 4 and the brothers, then her final form below 30% health.", skipSepulcher),
			],
		});
	}

	private void AddPrimordialWyrm(BossRegistrar registrar)
	{
		int head = NPCType("PrimordialWyrmHead");
		int body = NPCType("PrimordialWyrmBody");
		int bodyAlt = NPCType("PrimordialWyrmBodyAlt");
		int tail = NPCType("PrimordialWyrmTail");

		// Calamity's Boss Checklist entry is commented out; 23.5 is the value it would use.
		registrar.Add(new BossDefinition("PrimordialWyrm", head) {
			Progression = 23.5f,
			Aliases = ["wyrm", "aew", "eidolonwyrm"],
			ReadyTypes = [head, body, bodyAlt, tail],
			FallbackOffset = new Vector2(0f, 600f),
			Notes = "Enrages outside the deepest layer of the Abyss.",
			// Each phase changes the attack that follows its charges, so a new phase starts after the current attack.
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Charges, lightning rain and Eidolon Wyrms."),
				new BossPhase("Phase 2", 0.79f, "Adds shadow fireball spins, below 80% health."),
				new BossPhase("Phase 3", 0.59f, "Adds Ancient Doom summons, below 60% health."),
				new BossPhase("Phase 4", 0.39f, "Adds lightning charges, below 40% health."),
				new BossPhase("Phase 5", 0.19f, "Fast charges replace its minion summons, below 20% health."),
				// newAI[0] is its attack state; 10 is the final phase, entered the same way its own check does.
				new BossPhase("Final phase", 0.04f, "Circles you, summoning Ancient Dooms and shadow fireballs.",
					PhaseSetups.ForEach(head, npc => {
						npc.localAI[1] = 0f;
						SetCalamityAI(npc, 0, 10f);
						SetCalamityAI(npc, 1, 0f);
						SetCalamityAI(npc, 2, 0f);
					})),
			],
		});
	}
}
