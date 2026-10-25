using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace BossPractice.Common.Bosses.Sources;

// Infernum presets for Calamity's bosses from the Profaned Guardians onwards.
public sealed partial class InfernumBosses
{
	private void ModifyPostMoonLordBosses(BossRegistrar registrar)
	{
		// Phases follow how many guardians are alive. A guardian's CheckDead keeps it alive and starts the Commander's
		// animation for that death, so only one can die at a time. Phase 3 removes the Healer quietly, as if its death
		// had already played out, then kills the Defender the normal way.
		int healer = NPCType(Calamity, "ProfanedGuardianHealer");
		int defender = NPCType(Calamity, "ProfanedGuardianDefender");
		Action<IReadOnlyList<NPC>> healerGone = PhaseSetups.ForEach(healer, npc => {
			npc.active = false;
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
			}
		});
		AddVariant(registrar, Calamity, "ProfanedGuardians", [
			new BossPhase("Phase 1", 1f, "A fire wall corridor, then Healer and Defender combos. Only the Healer can really be hurt."),
			new BossPhase("Phase 2", 1f, "The Healer is dead: Commander and Defender spear dashes, rams and fireballs.", PhaseSetups.KillAll(healer)),
			new BossPhase("Phase 3", 1f, "Only the Commander remains: geysers, spear throws, rift charges and a laser ball.",
				PhaseSetups.All(healerGone, PhaseSetups.KillAll(defender))),
		], "Must be fought in the Profaned Garden arena; they leave if you're outside it. Health applies to all three guardians.");

		AddVariant(registrar, Calamity, "Dragonfolly", [
			new BossPhase("Phase 1", 1f, "Feather spreads, charges and plasma bursts."),
			new BossPhase("Phase 2", 0.74f, "Adds exploding energy orbs and stronger charges, below 75% health."),
			new BossPhase("Phase 3", 0.33f, "Adds a lightning supercharge and feather ruffles, below 33% health."),
		]);

		AddVariant(registrar, Calamity, "Providence", [
			new BossPhase("Phase 1", 1f, "Fire charges, cinder and bomb barrages, crystal fans and spear slams."),
			new BossPhase("Phase 2", 0.69f, "Fire form: rising lava, holy bomb spirals, Dogma lasers and more, below 70% health."),
		], "Built for the Profaned Garden arena; summoned at night she's enraged. "
			+ "Below 4% health she plays her death animation, so don't start lower.");

		int ceaselessVoid = NPCType(Calamity, "CeaselessVoid");
		// Directly spawned, it stays chained up and invulnerable until a Rune of Kos is used. Infernum's own fallback
		// spawn starts it on its opening attack (ai[0] = 1) and frees it from the chains (ai[3] = 1); this does the same.
		BossSpawner spawnFreed = (player, _) => BossSpawners.SpawnAt(player, ceaselessVoid, new Vector2(0f, -300f), ai0: 1f, ai3: 1f);
		AddVariant(registrar, Calamity, "CeaselessVoid", [
			new BossPhase("Phase 1", 1f, "Invulnerable while it swirls Dark Energy around you, until half of it is destroyed.") {
				Spawner = spawnFreed,
			},
			new BossPhase("Phase 2", 0.66f, "Its shell cracks: dark energy torrents and energy suction, below 67% health.") {
				Spawner = spawnFreed,
			},
			new BossPhase("Phase 3", 0.14f, "Its chains break: bursts, mirrored charges and converging barrages, below 15% health.") {
				Spawner = spawnFreed,
			},
		], "Invulnerable and enraged above the surface.", readyTypes: [ceaselessVoid]);

		// Directly spawned, it hunts sky critters and then leaves. A Rune of Kos normally starts the fight by resetting
		// its attack slots (ExtraAI 0-4) and switching it to its ice storm (ai[1] = 3); this does the same.
		int stormWeaver = NPCType(Calamity, "StormWeaverHead");
		Action<IReadOnlyList<NPC>> startStormWeaver = PhaseSetups.ForEach(stormWeaver, npc => {
			for (int i = 0; i < 5; i++) {
				SetInfernumAI(npc, i, 0f);
			}

			npc.Opacity = 1f;
			npc.ai[1] = 3f;
			npc.ai[2] = 0f;
		});
		AddVariant(registrar, Calamity, "StormWeaver", [
			new BossPhase("Phase 1", 1f, "Lightning bolts, ice storms and fakeout charges.", startStormWeaver),
			new BossPhase("Phase 2", 0.49f, "Wind gusts and charges out of the fog, below 50% health.", startStormWeaver),
		], "Brings rain with it.");

		// Directly spawned, he patrols invisibly and then leaves. A Rune of Kos normally starts the fight by resetting his
		// attack slots (ExtraAI 0-7) and switching him to kunai dashes (ai[1] = 1); this does the same.
		int signus = NPCType(Calamity, "Signus");
		Action<IReadOnlyList<NPC>> startSignus = PhaseSetups.ForEach(signus, npc => {
			for (int i = 0; i < 8; i++) {
				SetInfernumAI(npc, i, 0f);
			}

			SetInfernumAI(npc, 9, 0f);
			npc.ai[1] = 1f;
			npc.ai[2] = 0f;
		});
		AddVariant(registrar, Calamity, "Signus", [
			new BossPhase("Phase 1", 1f, "Kunai dashes, scythe throws, shadow dashes and charges.", startSignus),
			new BossPhase("Phase 2", 0.69f, "Adds cosmic flame bombs and more shadow dashes, below 70% health.", startSignus),
			new BossPhase("Phase 3", 0.29f, "More projectiles and shorter attack cycles, below 30% health.", startSignus),
		], "Vanishes if you get too far away.");

		int polterghast = NPCType(Calamity, "Polterghast");
		AddVariant(registrar, Calamity, "Polterghast", [
			new BossPhase("Phase 1", 1f, "Uppercut charges, leg swipes, wisp circles and spirit petals."),
			new BossPhase("Phase 2", 0.64f, "Adds soul rings, arcing souls and vortex charges, below 65% health."),
			new BossPhase("Phase 3", 0.34f, "Adds clones, below 35% health."),
			// Its CheckDead starts a 6 second death animation, then its desperation attack.
			new BossPhase("Desperation", 0.01f, "Its last attack after reaching 0 health.", PhaseSetups.ForEach(polterghast, ForceDeath)),
		], "Enrages above the surface.", readyTypes: [polterghast]);

		// Each phase plays its own 2.5 second transition, one after another.
		AddVariant(registrar, Calamity, "OldDuke", [
			new BossPhase("Phase 1", 1f, "Charges, gore and acid spit, and tooth balls."),
			new BossPhase("Phase 2", 0.74f, "Adds shark vortexes and faster patterns, below 75% health."),
			new BossPhase("Phase 3", 0.37f, "Rain, stronger hits and translucent teleports, below 37.5% health."),
			new BossPhase("Phase 4", 0.19f, "Even stronger hits and its final pattern, below 20% health."),
		], "Doesn't enrage away from the ocean.");

		AddDevourerOfGods(registrar);
		AddYharon(registrar);
		AddExoMechs(registrar);
		AddSupremeCalamitas(registrar);

		AddVariant(registrar, Calamity, "PrimordialWyrm", [
			new BossPhase("Phase 1", 1f, "Light gazes and beams. Hovers invulnerably for about 5 seconds first."),
			new BossPhase("Phase 2", 0.74f, "Adds dark rituals and split-form charges, below 75% health."),
			new BossPhase("Phase 3", 0.44f, "Adds crystal constriction and hammerhead rams, below 45% health."),
			new BossPhase("Phase 4", 0.14f, "Chases you relentlessly, below 15% health."),
		], "A new phase starts with its next attack. Using a Rod of Discord makes it kill you.");
	}

	private void AddDevourerOfGods(BossRegistrar registrar)
	{
		int head = NPCType(Calamity, "DevourerofGodsHead");

		// Phase 2 normally starts from the hit that would take it below 80% health, through a 9 second portal sequence.
		// ExtraAI[6] is its phase 2 flag; setting it skips the portals and plays the phase 2 intro (about 5 seconds, invulnerable).
		Action<IReadOnlyList<NPC>> startPhase2 = PhaseSetups.ForEach(head, npc => SetInfernumAI(npc, 6, 1f));
		AddVariant(registrar, Calamity, "DevourerofGods", [
			new BossPhase("Phase 1", 1f, "Alternates aggressive and passive passes with laser rings."),
			// ExtraAI[4] = 1 queues the portal sequence, as that hit would.
			new BossPhase("Portal transition", 1f, "Starts the 9 second portal sequence into phase 2.",
				PhaseSetups.ForEach(head, npc => SetInfernumAI(npc, 4, 1f))),
			new BossPhase("Phase 2", 0.79f, "Portal charges, laser walls, charge gates and sentinel attacks.", startPhase2),
			new BossPhase("Final phase", 0.19f, "Constant charge combos, below 20% health.", startPhase2),
		], "Teleports to your side and uncoils as it spawns.");
	}

	private void AddYharon(BossRegistrar registrar)
	{
		int yharon = NPCType(Calamity, "Yharon");

		// Phase 2 normally starts below 10% health with a long animation, then heals to full. ExtraAI[7] marks it as
		// entered, which skips both. Calamity's own phase 2 flag (startSecondAI) only changes its name and music.
		FieldInfo startSecondAI = null;
		Action<IReadOnlyList<NPC>> inPhase2 = PhaseSetups.ForEach(yharon, npc => {
			SetInfernumAI(npc, 7, 1f);
			startSecondAI ??= npc.ModNPC?.GetType().GetField("startSecondAI", BindingFlags.NonPublic | BindingFlags.Instance);
			startSecondAI?.SetValue(npc.ModNPC, true);
		});
		AddVariant(registrar, Calamity, "Yharon", [
			new BossPhase("Phase 1", 1f, "Opening attack pattern."),
			new BossPhase("Phase 1, part 2", 0.74f, "Second phase 1 pattern, below 75% health."),
			new BossPhase("Phase 1, part 3", 0.44f, "Third phase 1 pattern, below 45% health."),
			new BossPhase("Phase 2 transition", 0.09f, "Below 10% health: plays the full rebirth animation, then heals to full."),
			new BossPhase("Phase 2", 1f, "Skips the rebirth: phase 2 at full health.", inPhase2),
			new BossPhase("Phase 2, part 2", 0.79f, "Second phase 2 pattern, below 80% health.", inPhase2),
			new BossPhase("Phase 2, part 3", 0.39f, "Third phase 2 pattern, below 40% health.", inPhase2),
			new BossPhase("Phase 2, part 4", 0.14f, "Fourth phase 2 pattern, below 15% health.", inPhase2),
			new BossPhase("Desperation", 0.02f, "Illusions and a last stand, below 2.5% health.", inPhase2),
		], "Its arena is centred where you stand when it spawns; leaving it is lethal.");
	}

	private void AddExoMechs(BossRegistrar registrar)
	{
		int ares = NPCType(Calamity, "AresBody");
		int thanatos = NPCType(Calamity, "ThanatosHead");
		int apollo = NPCType(Calamity, "Apollo");
		string MechName(int type) => type == ares ? "Ares" : type == thanatos ? "Thanatos" : "Artemis and Apollo";

		// Infernum gives Ares its own pulse cannon and energy katanas, and no Gauss Nuke.
		int pulseCannon = NPCType("AresPulseCannon");
		int katana = NPCType("AresEnergyKatana");
		registrar.AddRelatedTypes(Calamity, "Ares", pulseCannon, katana);
		registrar.AddRelatedTypes(Calamity, "ExoMechs", pulseCannon, katana);

		// Draedon summons one mech and stores which one it is (ExtraAI[28]) and which joins it below 50% health
		// (ExtraAI[24]). The third mech arrives once that one drops below 50% too. Apollo summons Artemis itself.
		BossSpawner SpawnFight(int primary, int secondary) => (player, _) => {
			NPC mech;
			if (primary == thanatos) {
				mech = BossSpawners.SpawnAt(player, thanatos, new Vector2(0f, 2100f));
				if (mech != null) {
					mech.velocity = mech.DirectionTo(player.Center) * 40f;
				}
			}
			else {
				mech = BossSpawners.SpawnAt(player, primary, primary == ares ? new Vector2(0f, -1400f) : new Vector2(1100f, -1600f));
			}

			if (mech != null) {
				SetInfernumAI(mech, 28, primary);
				SetInfernumAI(mech, 24, secondary);
			}
		};

		// Each mech's phases follow its own health: 85%, 62.5%, then 50%, where the second mech joins.
		BossPhase[] MechPhases(int primary, int secondary)
		{
			BossSpawner spawner = SpawnFight(primary, secondary);
			return [
				new BossPhase("Phase 1", 1f, $"{MechName(primary)} alone.") { Spawner = spawner },
				new BossPhase("Phase 2", 0.84f, "New attacks, below 85% health.") { Spawner = spawner },
				new BossPhase("Phase 3", 0.62f, "New attacks, below 62.5% health. The twins change form first.") { Spawner = spawner },
				new BossPhase($"{MechName(secondary)} joins", 0.49f,
					$"{MechName(secondary)} joins for combo attacks right away, below 50% health.") { Spawner = spawner },
			];
		}

		const string mechNotes = "Fights the real Exo Mech sequence: a second mech joins below 50% health, then the third.";
		AddVariant(registrar, Calamity, "Ares", MechPhases(ares, thanatos), mechNotes, readyTypes: [ares]);
		AddVariant(registrar, Calamity, "Thanatos", MechPhases(thanatos, apollo), mechNotes);
		AddVariant(registrar, Calamity, "ExoTwins", MechPhases(apollo, ares), mechNotes);

		// The full fight in every order Draedon offers: first mech, then the one that joins it.
		int[] mechs = [ares, thanatos, apollo];
		var orders = new List<BossPhase>();
		foreach (int primary in mechs) {
			foreach (int secondary in mechs) {
				if (secondary != primary) {
					orders.Add(new BossPhase($"{MechName(primary)}, then {MechName(secondary)}", 1f,
						$"Starts with {MechName(primary)}; {MechName(secondary)} joins below 50% health.") {
						Spawner = SpawnFight(primary, secondary),
					});
				}
			}
		}

		AddVariant(registrar, Calamity, "ExoMechs", [.. orders],
			"Health applies to the first mech. The last mech standing heals to 50% for its final phase.");
	}

	private void AddSupremeCalamitas(BossRegistrar registrar)
	{
		int calamitas = NPCType(Calamity, "SupremeCalamitas");
		int shadowDemon = NPCType("ShadowDemon");

		// Phase 2 summons the Shadow Demon, which performs some of her later attacks. Starting below 45% passes every
		// threshold on the same tick and skips that summon, so the demon is created here.
		Action<IReadOnlyList<NPC>> summonShadowDemon = PhaseSetups.ForEach(calamitas, npc =>
			NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y - 850, shadowDemon, npc.whoAmI));

		AddVariant(registrar, Calamity, "Calamitas", [
			new BossPhase("Phase 1", 1f, "Opens with the Sepulcher, then dark souls, charges and bomber demons."),
			new BossPhase("Phase 2", 0.74f, "Summons the Shadow Demon, below 75% health. Skips the Sepulcher."),
			new BossPhase("Phase 3", 0.44f, "Her brothers join; she's invulnerable until they die, below 45% health.", summonShadowDemon),
			new BossPhase("Phase 4", 0.24f, "Soul Seekers protect her, below 25% health.", summonShadowDemon),
			// Her CheckDead starts the desperation phase instead of dying.
			new BossPhase("Desperation", 0.01f, "Her last stand after reaching 0 health.", PhaseSetups.ForEach(calamitas, ForceDeath)),
		], "Spawns on top of you so you start inside her arena; outside it she's enraged and invulnerable.",
			readyTypes: [calamitas]);
	}
}
