using Terraria;

namespace BossPractice.Common.Bosses.Sources;

/// <summary>
/// Thorium's bosses. Thresholds come from the Thorium wiki (1.7.x); internal names come from Thorium's localization
/// and public crossmods that reference its NPCs. Later forms that are separate NPCs (Borean Strider's popped form,
/// Soul of the Void, the unmasked Lich, the Forgotten One's cracked and released forms) are reached by starting the
/// first form just below its threshold, so Thorium's own AI performs the transformation.
/// Progression values are estimated from the wiki's boss order unless noted.
/// </summary>
public sealed class ThoriumBosses : BossSource
{
	public override string ModName => "ThoriumMod";

	public override void AddBosses(BossRegistrar registrar)
	{
		int thunderBird = NPCType("TheGrandThunderBird");
		registrar.Add(new BossDefinition("TheGrandThunderBird", thunderBird) {
			Progression = 0.5f, // ESTIMATED: before King Slime.
			Aliases = ["gtb", "thunderbird"],
			RelatedTypes = [NPCType("StormHatchling")],
			Notes = "Normally fought in the Desert during the day.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Dives, tornado bolts and Storm Hatchlings."),
				// Wiki: flies faster below 45%, and only summons hatchlings above 25%.
				new BossPhase("Faster flight", 0.44f, "Faster flight below 45% health."),
				new BossPhase("No hatchlings", 0.24f, "Stops summoning Storm Hatchlings below 25% health."),
			],
		});

		int queenJellyfish = NPCType("QueenJellyfish");
		registrar.Add(new BossDefinition("QueenJellyfish", queenJellyfish) {
			Progression = 3.5f, // ESTIMATED: after the Eater of Worlds and Brain of Cthulhu.
			Aliases = ["qj", "jellyfish"],
			RelatedTypes = [NPCType("ZealousJellyfish"), NPCType("SpittingJellyfish"), NPCType("DistractingJellyfish")],
			Notes = "Normally fought above the Ocean during the day.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Zealous Jellyfish only."),
				// Wiki: each minion type joins below its health threshold.
				new BossPhase("Spitting Jellyfish", 0.89f, "Spitting Jellyfish join below 90% health."),
				new BossPhase("Distracting Jellyfish", 0.49f, "Distracting Jellyfish join below 50% health."),
				new BossPhase("Tentacles", 0.39f, "Expert and Master only: extending tentacles below 40% health."),
			],
		});

		int viscount = NPCType("Viscount");
		registrar.Add(new BossDefinition("Viscount", viscount) {
			Progression = 3.7f, // ESTIMATED: after Queen Jellyfish, before Queen Bee.
			Aliases = ["bat"],
			RelatedTypes = [NPCType("BiteyBaby")],
			Notes = "Normally summoned at the Blood Altar underground.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Swoops and blood attacks."),
				// Wiki: each attack is added at a fixed health percentage.
				new BossPhase("Ground stomp", 0.89f, "Adds the stunning ground stomp below 90% health."),
				new BossPhase("Falling rocks", 0.74f, "Adds the rock-dropping scream below 75% health."),
				new BossPhase("Bitey Babies", 0.59f, "Summons healing Bitey Babies below 60% health."),
				new BossPhase("Vampire's Curse", 0.32f, "Expert and Master only: bat-curse rings below 33% health."),
			],
		});

		int energyStorm = NPCType("GraniteEnergyStorm");
		registrar.Add(new BossDefinition("GraniteEnergyStorm", energyStorm) {
			Progression = 6.4f, // ESTIMATED: after Skeletron, before Star Scouter.
			Aliases = ["ges"],
			RelatedTypes = [NPCType("CoalescedEnergy"), NPCType("EncroachingEnergy"), NPCType("EnergyConduit"), NPCType("EnergyBarrier")],
			Notes = "Normally fought in the Granite Caves.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Shielded by orbiting Coalesced Energy."),
				new BossPhase("Energy balls", 0.74f, "Shoots energy balls below 75% health."),
				new BossPhase("Energy Conduits", 0.49f, "Expert and Master only: summons Energy Conduits below 50% health."),
				new BossPhase("Spinning shields", 0.34f, "Coalesced Energy spins rapidly below 35% health."),
			],
		});

		int buriedChampion = NPCType("BuriedChampion");
		registrar.Add(new BossDefinition("BuriedChampion", buriedChampion) {
			Progression = 6.5f, // ESTIMATED: after Granite Energy Storm, before Star Scouter.
			Aliases = ["champion", "bc"],
			// The Fallen Champion echoes and the reflectable Magical Burst are NPCs too.
			RelatedTypes = [NPCType("FallenChampion1"), NPCType("FallenChampion2"), NPCType("MagicalBurst")],
			Notes = "Normally fought in the Marble Caves.",
			DefaultPhases = [
				// Wiki: switches weapon each time it loses a third of its health.
				new BossPhase("Blade", 1f, "Sword stage."),
				new BossPhase("Bow", 0.65f, "Bow stage below 66% health."),
				new BossPhase("Rapid bow", 0.49f, "The bow fires faster below 50% health."),
				new BossPhase("Staff", 0.32f, "Staff stage with Fallen Champion echoes below 33% health."),
			],
		});

		int starScouter = NPCType("StarScouter");
		registrar.Add(new BossDefinition("StarScouter", starScouter) {
			Progression = 6.9f, // Thorium's own Boss Checklist value.
			Aliases = ["scouter", "ss"],
			RelatedTypes = [NPCType("BioCore"), NPCType("CryoCore"), NPCType("PyroCore")],
			Notes = "Normally fought in Space.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Opening attack pattern."),
				// Wiki: each attack is added at a fixed health percentage.
				new BossPhase("Laser slam", 0.89f, "Adds the eye-core laser slam below 90% health."),
				new BossPhase("Graviton Surge", 0.69f, "Adds Graviton Surge below 70% health."),
				new BossPhase("Triple beams", 0.59f, "Fast triple beams replace the big blast below 60% health."),
				new BossPhase("Falling energy", 0.49f, "Adds falling energy balls below 50% health."),
				new BossPhase("Scouter Cores", 0.32f, "Expert and Master only: invulnerable until its three cores are destroyed, below 33% health."),
				new BossPhase("Big blast returns", 0.19f, "The big blast follows the triple beams below 20% health."),
			],
		});

		int boreanStrider = NPCType("BoreanStrider");
		registrar.Add(new BossDefinition("BoreanStrider", boreanStrider) {
			Progression = 7.1f, // ESTIMATED: first Thorium Hardmode boss.
			Aliases = ["strider"],
			// Below 50% the boss is replaced by a separate popped NPC.
			RelatedTypes = [NPCType("BoreanStriderPopped"), NPCType("BoreanHopper"), NPCType("BoreanMyte")],
			Notes = "Normally fought in the Snow biome during a blizzard.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Egg sac intact."),
				new BossPhase("Egg sac popped", 0.49f, "Starts below 50% health, so the egg sac ruptures straight away."),
			],
		});

		int fallenBeholder = NPCType("FallenBeholder");
		registrar.Add(new BossDefinition("FallenBeholder", fallenBeholder) {
			Progression = 7.3f, // ESTIMATED: early Hardmode, before Queen Slime.
			Aliases = ["coznix", "beholder"],
			// In Expert the boss is replaced by a separate Soul of the Void NPC below 40%.
			RelatedTypes = [NPCType("FallenBeholder2"), NPCType("Beholder")],
			Notes = "Normally fought in the Underworld. When loot is enabled, killing it generates Lodestone or Valadium ore in the world.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Opening attack pattern."),
				new BossPhase("Dark pulse", 0.79f, "Adds the dark-magic pulse below 80% health."),
				new BossPhase("Void gates", 0.59f, "Adds void gates below 60% health."),
				new BossPhase("Soul of the Void", 0.39f, "Expert and Master only: starts below 40% health, so it transforms straight away."),
			],
		});

		int lich = NPCType("Lich");
		registrar.Add(new BossDefinition("Lich", lich) {
			Progression = 11.5f, // ESTIMATED: after the mechanical bosses, before Plantera.
			// The unmasked form is a separate NPC that summons the phylactery.
			// PhylacteryofaThousandSouls is unconfirmed (taken from Thorium's localization key).
			RelatedTypes = [NPCType("LichHeadless"), NPCType("PhylacteryofaThousandSouls")],
			Notes = "Normally fought at night.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Masked form."),
				new BossPhase("Unmasked", 0.24f, "Expert and Master only: starts below 25% health, so it tears off its mask straight away."),
			],
		});

		int forgottenOne = NPCType("ForgottenOne");
		registrar.Add(new BossDefinition("ForgottenOne", forgottenOne) {
			Progression = 13.5f, // ESTIMATED: post-Plantera, after Golem.
			Aliases = ["abyssion", "fo"],
			// The cracked and released forms are separate NPCs that replace the first form.
			RelatedTypes = [NPCType("ForgottenOneCracked"), NPCType("ForgottenOneReleased"), NPCType("AbyssalSpawn")],
			Notes = "Enrages while you are out of water. Normally fought in the Aquatic Depths.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Shelled form."),
				// Wiki: cracks below 50% (66% in Expert and Master), breaks free below 33% in Expert and Master.
				new BossPhase("Cracked", () => Main.expertMode ? 0.65f : 0.49f, "Starts below the threshold, so the shell cracks straight away."),
				new BossPhase("Released", 0.32f, "Expert and Master only: starts below 33% health, so it cracks and breaks free straight away."),
			],
		});

		int slagFury = NPCType("SlagFury");
		int aquaius = NPCType("Aquaius");
		int omnicide = NPCType("Omnicide");
		registrar.Add(new BossDefinition("ThePrimordials", slagFury, "The Primordials") {
			Progression = 19.5f, // Thorium's own Boss Checklist value.
			Aliases = ["primordials", "ragnarok"],
			SpawnTypes = [slagFury, aquaius, omnicide],
			HealthTypes = [slagFury, aquaius, omnicide],
			// Dream Eater (Expert) follows the last Primordial's death; the rest are minions and its rings.
			RelatedTypes = [
				NPCType("DreamEater"), NPCType("AquaiusBubble"), NPCType("LucidBubble"),
				NPCType("ImpendingDread"), NPCType("UnstableAnger"), NPCType("InnerDespair"),
			],
			Notes = "Health applies to all three Primordials, which take turns attacking. They don't flee at dawn.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "All three in their first phase."),
				// Wiki: Slag Fury and Omnicide change phase every 33% lost, Aquaius every 25%.
				new BossPhase("Phase 2", 0.64f, "All three in their second phase."),
				new BossPhase("Phase 3", 0.32f, "Slag Fury and Omnicide in their last phase, Aquaius in its third."),
				new BossPhase("Last phases", 0.24f, "All three in their last phase."),
			],
		});
	}
}
