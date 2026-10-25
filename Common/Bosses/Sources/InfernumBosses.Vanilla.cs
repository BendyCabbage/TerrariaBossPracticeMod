using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossPractice.Common.Bosses.Sources;

public sealed partial class InfernumBosses
{
	/// <summary>Infernum presets for the vanilla bosses it reworks.</summary>
	partial void ModifyVanillaBosses(BossRegistrar registrar)
	{
		const string vanilla = BossDefinition.VanillaModName;

		AddVariant(registrar, vanilla, "kingslime", [
			new BossPhase("Phase 1", 1f, "Hops, big jumps and teleports. Shrinks and falls faster as it loses health."),
			new BossPhase("Phase 2", 0.74f, "The Ruby Jewel joins and fires beams, below 75% health. It returns 35 seconds after being destroyed."),
			new BossPhase("Phase 3", 0.29f, "A Ninja bursts out and fights alongside it, below 30% health."),
		]);

		AddVariant(registrar, vanilla, "eye", [
			new BossPhase("Phase 1", 1f, "Hover charges, servant summons and blood charges."),
			new BossPhase("Phase 2", 0.79f, "Mouth form, adding spin dashes and spat teeth, below 80% health. Plays its 3 second transition first."),
			new BossPhase("Phase 3", 0.34f, "Adds blood shot bursts, below 35% health."),
			new BossPhase("Phase 4", 0.14f, "Faster, shorter charges, below 15% health."),
		], "Enrages during the day.");

		AddEaterOfWorlds(registrar, vanilla);

		AddVariant(registrar, vanilla, "brain", [
			new BossPhase("Phase 1", 1f, "Blood dashes and dripping Creepers. The Creepers can't be hurt."),
			new BossPhase("Phase 2", 0.74f, "Adds diagonal charges, below 75% health."),
			new BossPhase("Phase 3", 0.44f, "Adds dashing illusions, psionic bombardments and a spinning pull, below 45% health."),
		], "Becomes invulnerable if you stay outside the Crimson or Corruption.");

		AddVariant(registrar, vanilla, "queenbee", [
			new BossPhase("Phase 1", 1f, "Charges, stinger bursts, honey blasts and bee minions."),
			new BossPhase("Hornet hives", 0.74f, "Adds hornet hives, below 75% health."),
			new BossPhase("Phase 2", 0.49f, "Poison honey, closing bee rings and Beelet Hell, below 50% health."),
			new BossPhase("Final phase", 0.22f, "Roars and summons a bee wave, then attacks faster, below 22.5% health."),
			new BossPhase("Last stand", 0.09f, "Only charges and Beelet Hell, below 10% health."),
		], "Spends its first 2.5 seconds flying in, invulnerable. Gains a lot of defense outside the Jungle.");

		// Both phase transitions take 1.5 seconds each and play after its 4 second invulnerable intro.
		AddVariant(registrar, vanilla, "skeletron", [
			new BossPhase("Phase 1", 1f, "Hovers and fires skulls. Its hands can't be hurt."),
			new BossPhase("Phase 2", 0.84f, "Spin charges, hand waves and shadowflame bursts, below 85% health."),
			new BossPhase("Phase 3", 0.47f, "Adds shadowflame waves and falling skulls, below 47.5% health."),
		], "Invulnerable and lethal during the day. Its hands can't be destroyed.");

		AddVariant(registrar, vanilla, "deerclops", [
			new BossPhase("Phase 1", 1f, "Fights inside an arena of ice spikes; it's invulnerable while you're outside it."),
			new BossPhase("Phase 2", 0.74f, "Shadow form with a ring of shadow hands, eye lasers and aimed hands, below 75% health."),
			// Normally the shadow hand ring is created as phase 2 starts and carries over. Starting below 35% skips
			// that, so create it here the way the transition does.
			new BossPhase("Phase 3", 0.34f, "Adds the Dying Beacon of Light, which shrinks the light around you, below 35% health.",
				PhaseSetups.ForEach(NPCID.Deerclops, SpawnShadowHandArena)),
		]);

		// Killing an eye detaches it instead (it deals 1350 damage to the wall and comes back to full health), and the
		// mouth takes 98% less damage while any eye is attached. Detaching them directly skips that damage.
		Action<IReadOnlyList<NPC>> detachEyes = PhaseSetups.ForEach(NPCID.WallofFleshEye, eye => {
			SetInfernumAI(eye, 2, 1f);
			eye.life = eye.lifeMax;
			eye.ai[1] = 0f;
		});
		AddVariant(registrar, vanilla, "wall", [
			new BossPhase("Phase 1", 1f, "Four eyes fire beams; the mouth barely takes damage until every eye is detached."),
			new BossPhase("Eyes detached", 1f, "The eyes circle you firing lasers while the mouth fires beams and spawns leeches.", detachEyes),
			new BossPhase("Laser volleys", 0.44f, "The eyes line up on the wall for horizontal laser volleys, below 45% health.", detachEyes),
		], "Must be spawned while you are in the Underworld.");

		AddVariant(registrar, vanilla, "queenslime", [
			new BossPhase("Phase 1", 1f, "Hops, crystal mazes, crown lasers and slime stomps. Drops in from far above."),
			new BossPhase("Phase 2", 0.62f, "Adds 4000 Blades and slime congregations after its next stomp, below 62.5% health."),
		]);

		AddVariant(registrar, vanilla, "twins", [
			new BossPhase("Phase 1", 1f, "Charges, downward charges and spins."),
			// A twin in its second form can't be hurt while the other is still in its first, so both start below it.
			new BossPhase("Phase 2", 0.74f, "Both twins open their mouths and add flamethrower bursts, below 75% health."),
			new BossPhase("Phase 3", 0.41f, "Adds chaotic fire and a downward laser, below 42.5% of their combined health."),
			// A twin refuses to die while its partner lives, so these spawn the survivor on its own instead.
			new BossPhase("Retinazer alone", 0.29f, "Spazmatism is already dead; Retinazer uses its solo overdrive attacks.") {
				Spawner = (player, boss) => BossSpawners.SpawnNear(player, NPCID.Retinazer, boss.FallbackOffset),
			},
			new BossPhase("Spazmatism alone", 0.29f, "Retinazer is already dead; Spazmatism uses its solo overdrive attacks.") {
				Spawner = (player, boss) => BossSpawners.SpawnNear(player, NPCID.Spazmatism, boss.FallbackOffset),
			},
		], "Flee during the day. A twin at 5% health shields itself until its partner dies, then heals to 30%.",
			// Apply health as soon as either twin exists; waiting for both would stall the solo presets.
			readyTypes: []);

		// The phase only changes the attack picked after its opening charge.
		AddVariant(registrar, vanilla, "destroyer", [
			new BossPhase("Phase 1", 1f, "Charges that release probes."),
			new BossPhase("Phase 2", 0.82f, "Adds laser walls, probe bombing and an upward bomb lunge, below 82.5% health."),
			new BossPhase("Phase 3", 0.44f, "Adds energy blasts and dive bombing, below 45% health."),
			new BossPhase("Phase 4", 0.19f, "Spinning lasers replace the laser walls, below 20% health."),
		], "Flees during the day. Starts 150 blocks below you.");

		// The head can't be hurt while an arm lives, nor for 4 seconds after (its shield). The arms share one health pool,
		// held by the cannon, and start firing faster below half of it.
		int[] primeArms = [NPCID.PrimeCannon, NPCID.PrimeLaser, NPCID.PrimeSaw, NPCID.PrimeVice];
		Action<IReadOnlyList<NPC>> killArms = PhaseSetups.KillAll(primeArms);
		AddVariant(registrar, vanilla, "prime", [
			new BossPhase("With arms", 1f, "The head is invulnerable while its arms fire and charge."),
			new BossPhase("Arms phase 2", 1f, "The arms fire faster, below 50% of their health.", PhaseSetups.SetHealth(NPCID.PrimeCannon, 0.49f)),
			new BossPhase("Head alone", 1f, "Arms destroyed: metal bursts, rockets and hover charges.", killArms),
			new BossPhase("Phase 2", 0.39f, "Arms destroyed. Adds tesla mines and lightning, below 40% health.", killArms),
			new BossPhase("Laser ray", 0.19f, "Arms destroyed. Fires its laser ray once, below 20% health.", killArms),
		], "Flees during the day. Spends its first 3.5 seconds in an invulnerable intro.");

		AddVariant(registrar, vanilla, "plantera", [
			new BossPhase("Phase 1", 1f, "Bulb form: hovers, fires seeds and exploding flowers."),
			new BossPhase("Phase 2", 0.79f, "Sheds its bulb: poison gas, tentacle snaps, nettle borders and charges, below 80% health."),
			new BossPhase("Phase 3", 0.34f, "Adds red blossoms, petal bursts and growing roses, below 35% health."),
		], "Invulnerable and enraged above the surface.");

		// The body copies the head's health, and both heads keep the lower of their two health values.
		AddVariant(registrar, vanilla, "golem", [
			new BossPhase("Phase 1", 1f, "Fist spins, floor fire, spike traps, heat rays and slingshots."),
			new BossPhase("Phase 2", 0.59f, "Platforms and spikes fill the arena; adds spinning lasers and spike rushes, below 60% health."),
			new BossPhase("Phase 3", 0.29f, "Drops the fist spins for its final attacks, below 30% health."),
		], "Builds its own arena around you (or a nearby altar) and enrages if you leave it. "
			+ "Phases start after its 4 second intro and a 3 second transition.");

		AddVariant(registrar, vanilla, "fishron", [
			new BossPhase("Phase 1", 1f, "Charges, bubble spit, shark tornadoes and bubble charges."),
			new BossPhase("Phase 2", 0.69f, "Adds bubble spins and tidal waves, below 70% health."),
			new BossPhase("Phase 3", 0.39f, "Teleport charges and razorblade storms, below 40% health."),
			new BossPhase("Phase 4", 0.19f, "Chains nine charges between tidal waves and storms, below 20% health."),
		], "Enrages away from the edges of the world or below the surface. Each skipped phase still plays its 2 second transition.");

		AddVariant(registrar, vanilla, "empress", [
			new BossPhase("Phase 1", 1f, "Opening attack pattern."),
			new BossPhase("Phase 2", 0.74f, "Light prisms, dancing swords and lance walls, below 75% health."),
			new BossPhase("Phase 3", 0.49f, "Adds majestic pierces, below 50% health."),
			new BossPhase("Phase 4", 0.19f, "Giant rainbow stars and her ultimate attack, below 20% health."),
		], "Enraged during the day.");

		AddVariant(registrar, vanilla, "cultist", [
			new BossPhase("Phase 1", 1f, "Fireballs, lightning, light blasts and clone rituals."),
			new BossPhase("Phase 2", 0.64f, "Adds ice storms and stronger attacks after a short transition, below 65% health."),
			// Its CheckDead starts the desperation attack instead of dying.
			new BossPhase("Desperation", 0.01f, "Its last attack after reaching 0 health.", PhaseSetups.ForEach(NPCID.CultistBoss, ForceDeath)),
		], "Fights inside a border centred on you; crossing it hurts.");

		// The core can only be hurt once its head and both hands have popped into True Eyes. A part normally pops when a
		// hit would take it below 18% health; this pops them the same way.
		Action<IReadOnlyList<NPC>> popParts = PhaseSetups.All(
			PhaseSetups.ForEach(NPCID.MoonLordHead, ForceDeath),
			PhaseSetups.ForEach(NPCID.MoonLordHand, ForceDeath));
		AddVariant(registrar, vanilla, "moonlord", [
			new BossPhase("Full fight", 1f, "Head and both hands; attacks change as each one pops."),
			new BossPhase("Core exposed", 1f, "Head and hands popped: the core dances, rushes and fires constellations.", popParts),
			new BossPhase("Phantasmal Wrath", 0.64f, "Core exposed; adds Phantasmal Wrath, below 65% health.", popParts),
			new BossPhase("Final phase", 0.33f, "Core exposed; opens with a black hole, then repeats it every fourth attack, below 33% health.",
				popParts),
		], "Fights inside an arena centred on you; leaving it enrages Moon Lord. "
			+ "Its head and hands pop at 18% of their health.");
	}

	/// <summary>
	/// Infernum's Eater of Worlds splits into two, then four, worms with the same total health each time. Each generation
	/// shares one health pool through realLife. This spawns a generation the way its split does (HandleSplit).
	/// </summary>
	private void AddEaterOfWorlds(BossRegistrar registrar, string vanilla)
	{
		BossSpawner SpawnGeneration(int generation) => (player, boss) => {
			int wormCount = 1 << generation;
			float segmentCount = 32 >> generation;
			NPC owner = null;
			for (int i = 0; i < wormCount; i++) {
				Vector2 offset = boss.FallbackOffset + new Vector2((i - (wormCount - 1) / 2f) * 160f, 0f);
				NPC worm = BossSpawners.SpawnAt(player, NPCID.EaterofWorldsHead, offset, ai2: generation, ai3: segmentCount);
				if (worm == null) {
					continue;
				}

				if (owner == null) {
					owner = worm;
				}
				else {
					worm.realLife = owner.whoAmI;
					worm.netUpdate = true;
				}
			}
		};

		// Worms that split or spawn are invulnerable for their first 1.5 seconds.
		AddVariant(registrar, vanilla, "eater", [
			new BossPhase("One worm", 1f, "Cursed bombs, vine charges and shadow orbs. Splits in two when killed."),
			new BossPhase("Two worms", 1f, "The first split: two worms sharing one health pool, adding Rain Hover.") {
				Spawner = SpawnGeneration(1),
			},
			new BossPhase("Four worms", 1f, "The final split: four worms sharing one health pool, adding downward slams.") {
				Spawner = SpawnGeneration(2),
			},
		], "Health applies to the current worms only; each split brings a fresh pool. "
			+ "Enrages outside the Corruption and Crimson.");
	}

	/// <summary>Runs Infernum's CheckDead for a boss that starts a new stage instead of dying.</summary>
	private static void ForceDeath(NPC npc)
	{
		npc.life = 0;
		npc.checkDead();
	}

	/// <summary>Creates Deerclops's ring of shadow hands around its target, as its phase 2 transition does.</summary>
	private void SpawnShadowHandArena(NPC npc)
	{
		if (!ModContent.TryFind(ModName, "ShadowHandArena", out ModProjectile arena)) {
			return;
		}

		// Infernum's own projectile helper halves hostile damage, twice in Expert, to undo vanilla's scaling.
		int damage = 125 / 2;
		if (Main.expertMode) {
			damage /= 2;
		}

		Player target = Main.player[npc.target];
		Projectile.NewProjectile(npc.GetSource_FromAI(), target.Center, Vector2.Zero, arena.Type, damage, 0f, Main.myPlayer);
	}
}
