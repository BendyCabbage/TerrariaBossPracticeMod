using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace BossPractice.Common.Bosses.Sources;

/// <summary>
/// Vanilla bosses. Their phase presets are hand-tuned so each starts just below a threshold in the vanilla AI.
/// Progression values match Boss Checklist.
/// </summary>
public sealed class VanillaBosses : BossSource
{
	public override string ModName => BossDefinition.VanillaModName;

	public override void AddBosses(BossRegistrar registrar)
	{
		registrar.Add(new BossDefinition("kingslime", NPCID.KingSlime) {
			Progression = 1f,
			Aliases = ["ks", "slime"],
		});

		registrar.Add(new BossDefinition("eye", NPCID.EyeofCthulhu) {
			Progression = 2f,
			Aliases = ["eoc", "eyeofcthulhu"],
			Notes = "Flees at dawn.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Eye form with Servants of Cthulhu."),
				// Vanilla transforms below 50% health (65% in Expert and Master).
				new BossPhase("Phase 2", () => Main.expertMode ? 0.64f : 0.49f, "Mouth form. Transforms immediately on spawn."),
				new BossPhase("Phase 2 rapid dashes", 0.34f, "Expert and Master only: chains rapid dashes below 35% health."),
			],
		});

		registrar.Add(new BossDefinition("eater", NPCID.EaterofWorldsHead) {
			Progression = 3f,
			Aliases = ["eow", "eaterofworlds"],
			// Each segment has its own health pool.
			HealthTypes = [NPCID.EaterofWorldsHead, NPCID.EaterofWorldsBody, NPCID.EaterofWorldsTail],
			ReadyTypes = [NPCID.EaterofWorldsHead, NPCID.EaterofWorldsTail],
			FallbackOffset = new Vector2(0f, 600f),
			Notes = "Health applies to every segment.",
		});

		registrar.Add(new BossDefinition("brain", NPCID.BrainofCthulhu) {
			Progression = 3f,
			Aliases = ["boc", "brainofcthulhu"],
			ReadyTypes = [NPCID.BrainofCthulhu, NPCID.Creeper],
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Brain surrounded by Creepers."),
				new BossPhase("Phase 2", 1f, "Creepers destroyed; the Brain starts teleporting and charging.", PhaseSetups.KillAll(NPCID.Creeper)),
			],
		});

		registrar.Add(new BossDefinition("queenbee", NPCID.QueenBee) {
			Progression = 4f,
			Aliases = ["qb", "bee"],
		});

		registrar.Add(new BossDefinition("skeletron", NPCID.SkeletronHead) {
			Progression = 5f,
			Aliases = ["skele"],
			ReadyTypes = [NPCID.SkeletronHead, NPCID.SkeletronHand],
			Notes = "Becomes invincible and lethal during the day.",
			DefaultPhases = [
				new BossPhase("With hands", 1f, "Head and both hands."),
				new BossPhase("Hands destroyed", 1f, "Only the head remains.", PhaseSetups.KillAll(NPCID.SkeletronHand)),
			],
		});

		registrar.Add(new BossDefinition("deerclops", NPCID.Deerclops) {
			Progression = 6f,
			Aliases = ["deer"],
		});

		registrar.Add(new BossDefinition("wall", NPCID.WallofFlesh) {
			Progression = 7f,
			Aliases = ["wof", "wallofflesh"],
			HealthTypes = [NPCID.WallofFlesh],
			Spawner = BossSpawners.WallOfFlesh,
			CheckCanSpawn = player => player.ZoneUnderworldHeight ? null : "Wall of Flesh can only be spawned in the Underworld.",
			Notes = "Must be spawned while you are in the Underworld.",
		});

		registrar.Add(new BossDefinition("queenslime", NPCID.QueenSlimeBoss) {
			Progression = 8f,
			Aliases = ["qs"],
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Ground phase with Crystal, Bouncy and Heavenly Slimes."),
				new BossPhase("Phase 2", 0.49f, "Flying phase below 50% health."),
			],
		});

		registrar.Add(new BossDefinition("twins", NPCID.Retinazer, "The Twins") {
			Progression = 9f,
			Aliases = ["thetwins", "retinazer", "spazmatism"],
			SpawnTypes = [NPCID.Retinazer, NPCID.Spazmatism],
			HealthTypes = [NPCID.Retinazer, NPCID.Spazmatism],
			Notes = "Flee at dawn.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Both twins in eye form."),
				// Each twin transforms below 40% of its own health.
				new BossPhase("Phase 2", 0.39f, "Both twins transform immediately on spawn."),
				new BossPhase("Phase 2 - Retinazer alone", 0.39f, "Spazmatism is already dead.", PhaseSetups.KillAll(NPCID.Spazmatism)),
				new BossPhase("Phase 2 - Spazmatism alone", 0.39f, "Retinazer is already dead.", PhaseSetups.KillAll(NPCID.Retinazer)),
			],
		});

		registrar.Add(new BossDefinition("destroyer", NPCID.TheDestroyer) {
			Progression = 10f,
			Aliases = ["thedestroyer"],
			// Body and tail segments share the head's health pool.
			FallbackOffset = new Vector2(0f, 600f),
			Notes = "Flees at dawn.",
		});

		registrar.Add(new BossDefinition("prime", NPCID.SkeletronPrime) {
			Progression = 11f,
			Aliases = ["skeletronprime", "sp"],
			ReadyTypes = [NPCID.SkeletronPrime, NPCID.PrimeCannon, NPCID.PrimeSaw, NPCID.PrimeVice, NPCID.PrimeLaser],
			Notes = "Flees at dawn.",
			DefaultPhases = [
				new BossPhase("With arms", 1f, "Head and all four arms."),
				new BossPhase("Arms destroyed", 1f, "Only the head remains.",
					PhaseSetups.KillAll(NPCID.PrimeCannon, NPCID.PrimeSaw, NPCID.PrimeVice, NPCID.PrimeLaser)),
			],
		});

		registrar.Add(new BossDefinition("plantera", NPCID.Plantera) {
			Progression = 12f,
			Aliases = ["plant"],
			Notes = "Enrages outside the Underground Jungle.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Bulb form with hooks."),
				new BossPhase("Phase 2", 0.49f, "Mouth form with tentacles, below 50% health."),
			],
		});

		registrar.Add(new BossDefinition("golem", NPCID.Golem) {
			Progression = 13f,
			// The body is invulnerable until the head is destroyed; both use the chosen health.
			HealthTypes = [NPCID.Golem, NPCID.GolemHead],
			ReadyTypes = [NPCID.Golem, NPCID.GolemHead],
			FallbackOffset = new Vector2(0f, -200f),
			Notes = "Enrages outside the Jungle Temple. Health applies to both the body and head.",
			DefaultPhases = [
				new BossPhase("Head attached", 1f, "Head, fists and body."),
				new BossPhase("Head detached", 1f, "The head breaks free and the body becomes vulnerable.", PhaseSetups.KillAll(NPCID.GolemHead)),
			],
		});

		registrar.Add(new BossDefinition("fishron", NPCID.DukeFishron) {
			Progression = 14f,
			Aliases = ["duke", "dukefishron"],
			FallbackOffset = new Vector2(0f, -300f),
			Notes = "Enrages outside the Ocean.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Dashes, bubbles and Sharkrons."),
				new BossPhase("Phase 2", 0.49f, "Faster dashes and Sharknadoes, below 50% health."),
				new BossPhase("Phase 3", 0.14f, "Expert and Master only: teleporting frenzy below 15% health."),
			],
		});

		registrar.Add(new BossDefinition("empress", NPCID.HallowBoss) {
			Progression = 15f,
			Aliases = ["eol", "empressoflight"],
			Notes = "Deals lethal damage during the day.",
			DefaultPhases = [
				new BossPhase("Phase 1", 1f, "Opening attack pattern."),
				new BossPhase("Phase 2", 0.49f, "Second attack pattern below 50% health."),
			],
		});

		registrar.Add(new BossDefinition("cultist", NPCID.CultistBoss) {
			Progression = 17f,
			Aliases = ["lunaticcultist", "lc"],
			Notes = "When loot is enabled, killing it starts the lunar events.",
		});

		registrar.Add(new BossDefinition("moonlord", NPCID.MoonLordCore, "Moon Lord") {
			Progression = 18f,
			Aliases = ["ml"],
			// The head and hands are spawned after a one second intro.
			HealthTypes = [NPCID.MoonLordCore, NPCID.MoonLordHead, NPCID.MoonLordHand],
			ReadyTypes = [NPCID.MoonLordHead, NPCID.MoonLordHand],
			Notes = "Health applies to the core, head and both hands.",
			DefaultPhases = [
				new BossPhase("Full fight", 1f, "Head and both hands active."),
				new BossPhase("Core exposed", 1f, "Head and hands destroyed; the core is vulnerable.",
					PhaseSetups.KillAll(NPCID.MoonLordHead, NPCID.MoonLordHand)),
			],
		});
	}
}
