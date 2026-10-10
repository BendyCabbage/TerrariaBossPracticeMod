using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaBossPracticeMod.Common.Bosses;

/// <summary>
/// The list of bosses that can be practiced. Vanilla bosses have hand-tuned phase presets whose thresholds
/// match vanilla AI; modded bosses are discovered automatically and only support a health percentage.
/// </summary>
public sealed class BossRegistry : ModSystem
{
	private static readonly List<BossDefinition> bosses = [];

	public static IReadOnlyList<BossDefinition> Bosses => bosses;

	public static BossDefinition Get(int index) => index >= 0 && index < bosses.Count ? bosses[index] : null;

	public static int IndexOf(BossDefinition boss) => bosses.IndexOf(boss);

	public static BossDefinition Find(string query) => bosses.FirstOrDefault(boss => boss.Matches(query));

	public override void PostSetupContent()
	{
		bosses.Clear();
		bosses.AddRange(CreateVanillaBosses());
		bosses.AddRange(CreateModdedBosses());
	}

	public override void Unload()
	{
		bosses.Clear();
	}

	private static BossPhase FullFight => new("Full fight", 1f, "Starts the fight from the beginning.");

	private static IEnumerable<BossDefinition> CreateVanillaBosses()
	{
		yield return new BossDefinition("kingslime", NPCID.KingSlime) {
			Aliases = ["ks", "slime"],
			Phases = [FullFight],
		};

		yield return new BossDefinition("eye", NPCID.EyeofCthulhu) {
			Aliases = ["eoc", "eyeofcthulhu"],
			Notes = "Flees at dawn.",
			Phases = [
				new BossPhase("Phase 1", 1f, "Eye form with Servants of Cthulhu."),
				// Vanilla transforms below 50% health (65% in Expert and Master).
				new BossPhase("Phase 2", () => Main.expertMode ? 0.64f : 0.49f, "Mouth form. Transforms immediately on spawn."),
				new BossPhase("Phase 2 rapid dashes", 0.34f, "Expert and Master only: chains rapid dashes below 35% health."),
			],
		};

		yield return new BossDefinition("eater", NPCID.EaterofWorldsHead) {
			Aliases = ["eow", "eaterofworlds"],
			// Each segment has its own health pool.
			HealthTypes = [NPCID.EaterofWorldsHead, NPCID.EaterofWorldsBody, NPCID.EaterofWorldsTail],
			ReadyTypes = [NPCID.EaterofWorldsHead, NPCID.EaterofWorldsTail],
			FallbackOffset = new Vector2(0f, 600f),
			Notes = "Health applies to every segment.",
			Phases = [FullFight],
		};

		yield return new BossDefinition("brain", NPCID.BrainofCthulhu) {
			Aliases = ["boc", "brainofcthulhu"],
			ReadyTypes = [NPCID.BrainofCthulhu, NPCID.Creeper],
			Phases = [
				new BossPhase("Phase 1", 1f, "Brain surrounded by Creepers."),
				new BossPhase("Phase 2", 1f, "Creepers destroyed; the Brain starts teleporting and charging.", KillAll(NPCID.Creeper)),
			],
		};

		yield return new BossDefinition("queenbee", NPCID.QueenBee) {
			Aliases = ["qb", "bee"],
			Phases = [FullFight],
		};

		yield return new BossDefinition("skeletron", NPCID.SkeletronHead) {
			Aliases = ["skele"],
			ReadyTypes = [NPCID.SkeletronHead, NPCID.SkeletronHand],
			Notes = "Becomes invincible and lethal during the day.",
			Phases = [
				new BossPhase("With hands", 1f, "Head and both hands."),
				new BossPhase("Hands destroyed", 1f, "Only the head remains.", KillAll(NPCID.SkeletronHand)),
			],
		};

		yield return new BossDefinition("deerclops", NPCID.Deerclops) {
			Aliases = ["deer"],
			Phases = [FullFight],
		};

		yield return new BossDefinition("wall", NPCID.WallofFlesh) {
			Aliases = ["wof", "wallofflesh"],
			HealthTypes = [NPCID.WallofFlesh],
			Style = SpawnStyle.WallOfFlesh,
			Notes = "Must be spawned while you are in the Underworld.",
			Phases = [FullFight],
		};

		yield return new BossDefinition("queenslime", NPCID.QueenSlimeBoss) {
			Aliases = ["qs"],
			Phases = [
				new BossPhase("Phase 1", 1f, "Ground phase with Crystal, Bouncy and Heavenly Slimes."),
				new BossPhase("Phase 2", 0.49f, "Flying phase below 50% health."),
			],
		};

		yield return new BossDefinition("twins", NPCID.Retinazer, "The Twins") {
			Aliases = ["thetwins", "retinazer", "spazmatism"],
			SpawnTypes = [NPCID.Retinazer, NPCID.Spazmatism],
			HealthTypes = [NPCID.Retinazer, NPCID.Spazmatism],
			Notes = "Flee at dawn.",
			Phases = [
				new BossPhase("Phase 1", 1f, "Both twins in eye form."),
				// Each twin transforms below 40% of its own health.
				new BossPhase("Phase 2", 0.39f, "Both twins transform immediately on spawn."),
				new BossPhase("Phase 2 - Retinazer alone", 0.39f, "Spazmatism is already dead.", KillAll(NPCID.Spazmatism)),
				new BossPhase("Phase 2 - Spazmatism alone", 0.39f, "Retinazer is already dead.", KillAll(NPCID.Retinazer)),
			],
		};

		yield return new BossDefinition("destroyer", NPCID.TheDestroyer) {
			Aliases = ["thedestroyer"],
			// Body and tail segments share the head's health pool.
			FallbackOffset = new Vector2(0f, 600f),
			Notes = "Flees at dawn.",
			Phases = [FullFight],
		};

		yield return new BossDefinition("prime", NPCID.SkeletronPrime) {
			Aliases = ["skeletronprime", "sp"],
			ReadyTypes = [NPCID.SkeletronPrime, NPCID.PrimeCannon, NPCID.PrimeSaw, NPCID.PrimeVice, NPCID.PrimeLaser],
			Notes = "Flees at dawn.",
			Phases = [
				new BossPhase("With arms", 1f, "Head and all four arms."),
				new BossPhase("Arms destroyed", 1f, "Only the head remains.",
					KillAll(NPCID.PrimeCannon, NPCID.PrimeSaw, NPCID.PrimeVice, NPCID.PrimeLaser)),
			],
		};

		yield return new BossDefinition("plantera", NPCID.Plantera) {
			Aliases = ["plant"],
			Notes = "Enrages outside the Underground Jungle.",
			Phases = [
				new BossPhase("Phase 1", 1f, "Bulb form with hooks."),
				new BossPhase("Phase 2", 0.49f, "Mouth form with tentacles, below 50% health."),
			],
		};

		yield return new BossDefinition("golem", NPCID.Golem) {
			// The body is invulnerable until the head is destroyed; both use the chosen health.
			HealthTypes = [NPCID.Golem, NPCID.GolemHead],
			ReadyTypes = [NPCID.Golem, NPCID.GolemHead],
			FallbackOffset = new Vector2(0f, -200f),
			Notes = "Enrages outside the Jungle Temple. Health applies to both the body and head.",
			Phases = [
				new BossPhase("Head attached", 1f, "Head, fists and body."),
				new BossPhase("Head detached", 1f, "The head breaks free and the body becomes vulnerable.", KillAll(NPCID.GolemHead)),
			],
		};

		yield return new BossDefinition("fishron", NPCID.DukeFishron) {
			Aliases = ["duke", "dukefishron"],
			FallbackOffset = new Vector2(0f, -300f),
			Notes = "Enrages outside the Ocean.",
			Phases = [
				new BossPhase("Phase 1", 1f, "Dashes, bubbles and Sharkrons."),
				new BossPhase("Phase 2", 0.49f, "Faster dashes and Sharknadoes, below 50% health."),
				new BossPhase("Phase 3", 0.14f, "Expert and Master only: teleporting frenzy below 15% health."),
			],
		};

		yield return new BossDefinition("empress", NPCID.HallowBoss) {
			Aliases = ["eol", "empressoflight"],
			Notes = "Deals lethal damage during the day.",
			Phases = [
				new BossPhase("Phase 1", 1f, "Opening attack pattern."),
				new BossPhase("Phase 2", 0.49f, "Second attack pattern below 50% health."),
			],
		};

		yield return new BossDefinition("cultist", NPCID.CultistBoss) {
			Aliases = ["lunaticcultist", "lc"],
			Notes = "When loot is enabled, killing it starts the lunar events.",
			Phases = [FullFight],
		};

		yield return new BossDefinition("moonlord", NPCID.MoonLordCore, "Moon Lord") {
			Aliases = ["ml"],
			// The head and hands are spawned after a one second intro.
			HealthTypes = [NPCID.MoonLordCore, NPCID.MoonLordHead, NPCID.MoonLordHand],
			ReadyTypes = [NPCID.MoonLordHead, NPCID.MoonLordHand],
			Notes = "Health applies to the core, head and both hands.",
			Phases = [
				new BossPhase("Full fight", 1f, "Head and both hands active."),
				new BossPhase("Core exposed", 1f, "Head and hands destroyed; the core is vulnerable.",
					KillAll(NPCID.MoonLordHead, NPCID.MoonLordHand)),
			],
		};
	}

	private static IEnumerable<BossDefinition> CreateModdedBosses()
	{
		for (int type = NPCID.Count; type < NPCLoader.NPCCount; type++) {
			if (!ContentSamples.NpcsByNetId.TryGetValue(type, out NPC sample) || !sample.boss) {
				continue;
			}

			ModNPC modNPC = NPCLoader.GetNPC(type);
			yield return new BossDefinition($"{modNPC.Mod.Name}/{modNPC.Name}", type) {
				Aliases = [modNPC.Name],
				IsModded = true,
				Notes = $"From {modNPC.Mod.DisplayName}. Spawned directly; summoning requirements are skipped.",
				Phases = [FullFight],
			};
		}
	}

	/// <summary>Instantly kills every NPC of the given types that belongs to the practice fight.</summary>
	private static System.Action<IReadOnlyList<NPC>> KillAll(params int[] types) => npcs => {
		foreach (NPC npc in npcs) {
			if (npc.active && types.Contains(npc.type)) {
				npc.StrikeInstantKill();
			}
		}
	};
}
