using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace BossPractice.Common.Bosses.Sources;

/// <summary>
/// Fargo's Souls bosses, plus Eternity Mode phase variants for the vanilla bosses it reworks (<c>FargosSoulsBosses.Vanilla.cs</c>).
/// Thresholds come from the Fargo's Souls wiki, checked against the mod's public source (1.7.3.7) where the wiki was unclear.
/// Progression values match Fargo's own Boss Checklist entries.
/// </summary>
public sealed partial class FargosSoulsBosses : BossSource
{
	public override string ModName => "FargowiltasSouls";

	/// <summary>Eternity Mode, which includes Masochist Mode.</summary>
	private bool Eternity => CallMod("EternityMode") is true;

	/// <summary>Eternity in a Master world. Fargo's "MasochistMode" call reports Eternity, so the world is checked instead.</summary>
	private bool Masochist => Eternity && Main.masterMode;

	public override void AddBosses(BossRegistrar registrar)
	{
		AddTrojanSquirrel(registrar);
		AddCursedCoffin(registrar);
		AddDeviantt(registrar);
		AddBanishedBaron(registrar);
		AddLifelight(registrar);
		AddChampions(registrar);
		AddAbominationn(registrar);
		AddMutant(registrar);
	}

	public override void ModifyBosses(BossRegistrar registrar)
	{
		ModifyVanillaBosses(registrar);
	}

	partial void ModifyVanillaBosses(BossRegistrar registrar);

	/// <summary>Where Champions of Terra and Nature fight: below the surface, above the Underworld.</summary>
	private static bool IsUnderground(Player player) => player.Center.Y >= Main.worldSurface * 16 && !player.ZoneUnderworldHeight;

	private static bool IsOnSurface(Player player) => player.Center.Y <= Main.worldSurface * 16;

	private void AddTrojanSquirrel(BossRegistrar registrar)
	{
		int body = NPCType("TrojanSquirrel");
		int head = NPCType("TrojanSquirrelHead");
		int arms = NPCType("TrojanSquirrelArms");

		// The body spawns its head and arms on its first tick. Each part has its own health pool.
		BossDefinition trojan = new("TrojanSquirrel", body) {
			Progression = 0.5f,
			Aliases = ["trojan", "squirrel"],
			ReadyTypes = [body, head, arms],
			Notes = "Health applies to the body only.",
			DefaultPhases = [
				new BossPhase("Full fight", 1f, "Body, head and arms."),
				new BossPhase("Head and arms destroyed", 1f, "Only the body remains, walking faster.", PhaseSetups.KillAll(head, arms)),
			],
		};
		trojan.AddPhaseVariant(new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Full fight", 1f, "Body, head and arms."),
			// Wiki: crippled once brought to 50%.
			new BossPhase("Crippled", 0.49f, "The body is invulnerable and slowed until the head and arms are destroyed."),
			new BossPhase("Head and arms destroyed", 1f, "Only the body remains, walking faster.", PhaseSetups.KillAll(head, arms)),
		]));
		registrar.Add(trojan);
	}

	private void AddCursedCoffin(BossRegistrar registrar)
	{
		int coffin = NPCType("CursedCoffin");

		// The Coffin removes itself on spawn when you're far from its arena, and drops from above, so spawn it inside the room.
		BossDefinition definition = new("CursedCoffin", coffin) {
			Progression = 2.1f,
			Aliases = ["coffin"],
			Spawner = BossSpawners.AtOffset(new Vector2(0f, -300f)),
			// The arena's back wall is Sandstone Brick, like the rest of the Pyramid.
			CheckCanSpawn = player => Framing.GetTileSafely(player.Center).WallType == WallID.SandstoneBrick
				? null
				: "Cursed Coffin must be spawned inside its arena under the Pyramid.",
			Notes = "Must be spawned inside its arena under the Pyramid; elsewhere it vanishes.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Slams, shockwaves and grabbing hands."),
				new BossPhase("Phase 2", 0.49f, "The Cursed Spirit joins after the current attack, below 50% health."),
			],
		};
		// Wiki: 60% in Eternity, 75% in Masochist.
		definition.AddPhaseVariant(new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Phase 1", 1f, "Slams, shockwaves and grabbing hands."),
			new BossPhase("Phase 2", () => Masochist ? 0.74f : 0.59f,
				"The Cursed Spirit joins after the current attack, below 60% health (75% in Masochist)."),
		]));
		registrar.Add(definition);
	}

	private void AddDeviantt(BossRegistrar registrar)
	{
		int devi = NPCType("DeviBoss");

		BossDefinition definition = new("DeviBoss", devi) {
			Progression = 6.9999f,
			Aliases = ["devi", "deviantt"],
			Notes = "Heals when hit by a player with Lovestruck.",
			DefaultPhases = [
				new BossPhase("Stage 1", 1f, "Opening attack pattern."),
				new BossPhase("Stage 2", 0.49f, "Expert and Master only: second stage below 50% health."),
			],
		};
		// Source 1.7.3.7: 66% in Eternity, but 50% in Masochist, where she then heals to full.
		definition.AddPhaseVariant(new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Stage 1", 1f, "One strong attack after every three weak ones."),
			new BossPhase("Stage 2", () => Masochist ? 0.49f : 0.65f,
				"One strong attack after every two weak ones, below 66% health (50% in Masochist, then heals to full)."),
		]));
		registrar.Add(definition);
	}

	private void AddBanishedBaron(BossRegistrar registrar)
	{
		int baron = NPCType("BanishedBaron");

		// Source 1.7.3.7: below 2/3 health in Expert and above, 50% in Normal (the wiki says 33%). Both wait for the current attack.
		registrar.Add(new BossDefinition("BanishedBaron", baron) {
			Progression = 8.7f,
			Aliases = ["baron"],
			Notes = "Baron's Burden hurts you out of water until phase 2, so practise phase 1 in the Ocean.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Underwater fight."),
				new BossPhase("Phase 2", () => Main.expertMode ? 0.65f : 0.49f,
					"Throws you above water into a shrinking whirlpool arena, below 66% health (50% in Normal)."),
			],
		});
	}

	private void AddLifelight(BossRegistrar registrar)
	{
		int lifelight = NPCType("LifeChallenger");

		// Source 1.7.3.7: no phase 2 in Normal, 50% in Expert, 66% in Eternity and 75% in Masochist.
		BossDefinition definition = new("LifeChallenger", lifelight) {
			Progression = 11.49f,
			Aliases = ["lifelight"],
			Notes = "Invulnerable until its shard shield has formed.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Opening attack pattern."),
				new BossPhase("Phase 2", 0.49f, "Expert and Master only: transitions after the current attack, below 50% health."),
			],
		};
		definition.AddPhaseVariant(new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Phase 1", 1f, "Opening attack pattern."),
			new BossPhase("Phase 2", () => Masochist ? 0.74f : 0.65f,
				"Transitions after the current attack, below 66% health (75% in Masochist, with an arena)."),
		]));
		registrar.Add(definition);
	}

	private void AddChampions(BossRegistrar registrar)
	{
		AddTimberChampion(registrar);

		int terra = NPCType("TerraChampion");
		int terraBody = NPCType("TerraChampionBody");
		int terraTail = NPCType("TerraChampionTail");
		// Body and tail segments share the head's health pool.
		BossDefinition terraDefinition = new("TerraChampion", terra) {
			Progression = 18.15f,
			Aliases = ["terra"],
			RelatedTypes = [terraBody, terraTail],
			FallbackOffset = new Vector2(0f, 600f),
			CheckCanSpawn = player => IsUnderground(player) ? null : "Champion of Terra must be fought underground, above the Underworld.",
			Notes = "Only the head takes real damage. Flees if you leave the underground.",
		};
		terraDefinition.AddPhaseVariant(new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Phase 1", 1f, "The full worm."),
			// Wiki: 10%. It then holds at exactly 10% while its body breaks apart.
			new BossPhase("Phase 2", 0.09f, "Its body breaks into electric orbs, below 10% health."),
		]));
		registrar.Add(terraDefinition);

		int earth = NPCType("EarthChampion");
		int earthHand = NPCType("EarthChampionHand");
		// The hands appear after a one second intro and can't be hurt.
		registrar.Add(new BossDefinition("EarthChampion", earth) {
			Progression = 18.2f,
			Aliases = ["earth"],
			ReadyTypes = [earth, earthHand],
			CheckCanSpawn = player => player.ZoneUnderworldHeight ? null : "Champion of Earth must be fought in the Underworld.",
			Notes = "Only the head can be hurt. Flees if you leave the Underworld.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Head and two hands."),
				// Wiki: 33%.
				new BossPhase("Phase 2", 0.32f, "Crosses its hands and heals back up to 50% (more in Masochist), below 33% health."),
			],
		});

		int nature = NPCType("NatureChampion");
		int natureHead = NPCType("NatureChampionHead");
		// The six heads share the body's health pool.
		registrar.Add(new BossDefinition("NatureChampion", nature) {
			Progression = 18.25f,
			Aliases = ["nature"],
			RelatedTypes = [natureHead],
			CheckCanSpawn = player => IsUnderground(player) ? null : "Champion of Nature must be fought underground, above the Underworld.",
			Notes = "Flees if you leave the underground. Eternity: leaving its aura enrages it.",
		});

		int life = NPCType("LifeChampion");
		BossDefinition lifeDefinition = new("LifeChampion", life) {
			Progression = 18.3f,
			Aliases = ["life"],
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Opening attack pattern."),
				new BossPhase("Phase 2", 0.65f, "Below 66% health. Masochist: heals to full on entry."),
			],
		};
		lifeDefinition.AddPhaseVariant(new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Phase 1", 1f, "Opening attack pattern, inside a border that follows it."),
			new BossPhase("Phase 2", 0.65f, "Below 66% health. Masochist: heals to full on entry."),
			// localAI[2] counts its phases (1 = phase 2). Without it, phase 3 would wait for the phase 2 transition first.
			new BossPhase("Phase 3", 0.32f, "Stops moving and fires deathrays and rings of bees, below 33% health.",
				PhaseSetups.SetLocalAI(life, 2, 1f)),
		]));
		registrar.Add(lifeDefinition);

		int shadow = NPCType("ShadowChampion");
		int shadowOrb = NPCType("ShadowOrbNPC");
		BossDefinition shadowDefinition = new("ShadowChampion", shadow) {
			Progression = 18.35f,
			Aliases = ["death", "shadow"],
			RelatedTypes = [shadowOrb],
			Notes = "Invulnerable until you hit each of its shadow orbs.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Surrounded by shadow orbs."),
				// Wiki: 66% in Expert, 50% in Normal.
				new BossPhase("Phase 2", () => Main.expertMode ? 0.65f : 0.49f,
					"A new ring of orbs to hit, below 66% health (50% in Normal)."),
			],
		};
		shadowDefinition.AddPhaseVariant(new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Phase 1", 1f, "Surrounded by shadow orbs."),
			new BossPhase("Phase 2", 0.65f, "A new ring of orbs to hit, below 66% health."),
			// localAI[3] is its phase (2 = phase 2), so it goes straight to phase 3 instead of chaining both transitions.
			new BossPhase("Phase 3", 0.32f, "A third ring of orbs and new attacks, below 33% health.",
				PhaseSetups.SetLocalAI(shadow, 3, 2f)),
		]));
		registrar.Add(shadowDefinition);

		int spirit = NPCType("SpiritChampion");
		int spiritHand = NPCType("SpiritChampionHand");
		// The hands appear after an intro and share the body's health pool.
		BossDefinition spiritDefinition = new("SpiritChampion", spirit) {
			Progression = 18.4f,
			Aliases = ["spirit"],
			ReadyTypes = [spirit, spiritHand],
			CheckCanSpawn = player => Framing.GetTileSafely(player.Center).WallType != WallID.None || player.ZoneUndergroundDesert
				? null
				: "Champion of Spirit must be fought in the Underground Desert or in front of a background wall.",
			Notes = "Flees unless you're in the Underground Desert or in front of a background wall.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Body and four hands."),
				new BossPhase("Phase 2", 0.65f, "New attacks below 66% health."),
			],
		};
		spiritDefinition.AddPhaseVariant(new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Phase 1", 1f, "Body and its hands, inside an aura."),
			new BossPhase("Phase 2", 0.65f, "New attacks below 66% health."),
			// Instead of dying it starts an invincible last stand.
			new BossPhase("Last stand", 0.01f, "Starts at 1 HP: the next hit starts its last stand. Survive it without being grabbed.",
				PhaseSetups.ForEach(spirit, npc => npc.life = 1)),
		]));
		registrar.Add(spiritDefinition);

		int will = NPCType("WillChampion");
		// Wiki: the same thresholds in every mode.
		registrar.Add(new BossDefinition("WillChampion", will) {
			Progression = 18.45f,
			Aliases = ["will"],
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Opening attack pattern."),
				new BossPhase("Phase 2", 0.65f, "A golden shell makes it invincible during a bomb and spear sequence, below 66% health."),
				new BossPhase("Phase 3", 0.32f, "The shell sequence repeats below 33% health."),
			],
		});

		int cosmos = NPCType("CosmosChampion");
		BossDefinition cosmosDefinition = new("CosmosChampion", cosmos) {
			Progression = 18.5f,
			Aliases = ["eridanus", "eri", "cosmos"],
			CheckCanSpawn = player => IsOnSurface(player) ? null : "Eridanus must be fought on the surface or in space.",
			Notes = "Flees if you go underground.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Emerges from a vortex, then its opening attacks."),
				new BossPhase("Phase 2", 0.49f, "Below 50% health."),
			],
		};
		cosmosDefinition.AddPhaseVariant(new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Phase 1", 1f, "Emerges from a vortex, then its opening attacks."),
			new BossPhase("Phase 2", 0.79f, "Below 80% health."),
			new BossPhase("Phase 3", 0.19f, "Frozen in place with orbiting Moons and a shrinking arena, below 20% health."),
		]));
		registrar.Add(cosmosDefinition);
	}

	private void AddTimberChampion(BossRegistrar registrar)
	{
		int timber = NPCType("TimberChampion");
		int timberHead = NPCType("TimberChampionHead");

		// In Expert and above the body's PreKill spawns the head, a separate boss with its own health, as phase 2.
		BossPhase headPhase = new("Phase 2", 1f, "Expert and Master only: the head that carries on after the body is beaten.") {
			Spawner = (player, _) => BossSpawners.SpawnAt(player, timberHead, new Vector2(0f, -400f)),
		};

		BossDefinition definition = new("TimberChampion", timber) {
			Progression = 18.1f,
			Aliases = ["timber"],
			HealthTypes = [timber, timberHead],
			// The phase 2 preset spawns only the head, so there's nothing to wait for.
			ReadyTypes = [],
			RelatedTypes = [timberHead],
			// In Expert and above the body's PreKill spawns the head. In Classic the body is the final kill.
			RunsOwnDeathCode = npc => npc.type == timber && Main.expertMode,
			Notes = "Expert and Master: the head carries on after the body is beaten.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Jumps, acorns and snowballs."),
				headPhase,
			],
		};
		// Wiki: palm tree turrets below 66% and 33%, Masochist only.
		definition.AddPhaseVariant(new PhaseVariant("Masochist", () => Masochist, [
			new BossPhase("Phase 1", 1f, "Jumps, acorns and snowballs."),
			new BossPhase("Palm tree turret", 0.65f, "A palm tree turret joins on its next jump, below 66% health."),
			new BossPhase("Second palm tree turret", 0.32f, "Both palm tree turrets, below 33% health."),
			headPhase,
		]));
		registrar.Add(definition);
	}

	private void AddAbominationn(BossRegistrar registrar)
	{
		int abom = NPCType("AbomBoss");

		BossDefinition definition = new("AbomBoss", abom) {
			Progression = 20f,
			Aliases = ["abom", "abominationn"],
			CheckCanSpawn = player => IsOnSurface(player) ? null : "Abominationn must be fought on the surface or above.",
			Notes = "Flees if you go underground. Straying too far inflicts God Eater.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Opening attack pattern."),
				new BossPhase("Phase 2", 0.49f, "Expert and Master only: the Abominable Seal arena forms below 50% health."),
			],
		};
		// localAI[3] = 2 marks phase 2, which brings the seal back. Its CheckDead starts the desperation attack in Masochist.
		definition.AddPhaseVariant(new PhaseVariant("Masochist", () => Masochist, [
			new BossPhase("Phase 1", 1f, "Opening attack pattern."),
			new BossPhase("Phase 2", 0.65f, "The Abominable Seal arena forms and it heals to full, below 66% health."),
			new BossPhase("Desperation", 0.01f, "Starts in phase 2 at 1 HP: the next hit starts its desperation attack.",
				PhaseSetups.ForEach(abom, npc => {
					npc.life = 1;
					npc.localAI[3] = 2f;
				})),
		]));
		definition.AddPhaseVariant(new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Phase 1", 1f, "Opening attack pattern."),
			new BossPhase("Phase 2", 0.65f, "The Abominable Seal arena forms below 66% health."),
		]));
		registrar.Add(definition);
	}

	private void AddMutant(BossRegistrar registrar)
	{
		int mutant = NPCType("MutantBoss");

		BossDefinition definition = new("MutantBoss", mutant) {
			Progression = 23f,
			Aliases = ["mutant"],
			CheckCanSpawn = player => IsOnSurface(player) ? null : "Mutant must be fought on the surface or above.",
			Notes = "Flees if you go underground. You can't use items during its phase transitions.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Opening attack pattern."),
				// Wiki: 66.6%.
				new BossPhase("Phase 2", 0.65f, "Expert and Master only: second phase below 66% health."),
			],
		};
		// Its CheckDead in phase 2 jumps its state (ai[0]) to -1, the final phase transition, which heals it to full.
		definition.AddPhaseVariant(new PhaseVariant("Eternity", () => Eternity, [
			new BossPhase("Phase 1", 1f, "Opening attack pattern."),
			new BossPhase("Phase 2", 0.65f, "Fully heals as it enters its second phase, below 66% health."),
			new BossPhase("Phase 3", 1f, "Skips to its final desperation phase, which drains its health over time.",
				PhaseSetups.ForEach(mutant, npc => {
					npc.ai[0] = -1f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.ai[3] = 0f;
					npc.localAI[0] = 0f;
					npc.localAI[1] = 0f;
					npc.localAI[2] = 0f;
					npc.dontTakeDamage = true;
				})),
		]));
		registrar.Add(definition);
	}
}
