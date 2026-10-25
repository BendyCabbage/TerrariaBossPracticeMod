using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossPractice.Common.Bosses;

/// <summary>
/// The list of bosses that can be practiced, built from every loaded <see cref="BossSource"/>. Bosses from mods
/// without a source (or that a source doesn't know about yet) are discovered automatically and only support a
/// health percentage.
/// </summary>
public sealed class BossRegistry : ModSystem
{
	private static readonly List<BossDefinition> bosses = [];
	private static readonly Dictionary<string, BossDefinition> byKey = [];

	/// <summary>Every boss in progression order. Only contains bosses from mods that are currently enabled.</summary>
	public static IReadOnlyList<BossDefinition> Bosses => bosses;

	public static BossDefinition Get(string key) => byKey.GetValueOrDefault(key);

	/// <summary>Finds a boss by key, name, display name or alias. Earlier bosses win when names collide.</summary>
	public static BossDefinition Find(string query) => Get(query) ?? bosses.FirstOrDefault(boss => boss.Matches(query));

	public override void PostSetupContent()
	{
		bosses.Clear();
		byKey.Clear();

		var registrar = new BossRegistrar();
		List<BossSource> sources = ModContent.GetContent<BossSource>().ToList();
		RunSources(registrar, sources, source => source.AddBosses(registrar));
		RunSources(registrar, sources, source => source.ModifyBosses(registrar));

		// Stable sorts, so bosses with equal progression keep their registration order.
		bosses.AddRange(registrar.Bosses.OrderBy(boss => boss.Progression ?? float.MaxValue));
		bosses.AddRange(DiscoverUnknownBosses(registrar.Bosses));

		foreach (BossDefinition boss in bosses) {
			byKey[boss.Key] = boss;
		}
	}

	public override void Unload()
	{
		bosses.Clear();
		byKey.Clear();
	}

	private void RunSources(BossRegistrar registrar, List<BossSource> sources, Action<BossSource> action)
	{
		foreach (BossSource source in sources) {
			registrar.CurrentSource = source;
			try {
				action(source);
			}
			catch (Exception e) {
				// One broken integration (usually after the other mod updates) shouldn't take down the whole list.
				Mod.Logger.Error($"Boss source {source.Name} for {source.ModName} failed.", e);
			}
		}

		registrar.CurrentSource = null;
	}

	/// <summary>
	/// Lists modded NPCs flagged as bosses that no source claims. Their progression is unknown, so they're grouped
	/// by mod and ordered by base health as a rough stand-in.
	/// </summary>
	private static IEnumerable<BossDefinition> DiscoverUnknownBosses(IEnumerable<BossDefinition> known)
	{
		var claimed = known.SelectMany(boss => boss.AllTypes).ToHashSet();
		var discovered = new List<(ModNPC modNPC, int lifeMax)>();
		for (int type = NPCID.Count; type < NPCLoader.NPCCount; type++) {
			if (!claimed.Contains(type) && ContentSamples.NpcsByNetId.TryGetValue(type, out NPC sample) && sample.boss) {
				discovered.Add((NPCLoader.GetNPC(type), sample.lifeMax));
			}
		}

		return discovered
			.GroupBy(entry => entry.modNPC.Mod)
			.SelectMany(group => group.OrderBy(entry => entry.lifeMax))
			.Select(entry => new BossDefinition(entry.modNPC.Name, entry.modNPC.Type) {
				ModName = entry.modNPC.Mod.Name,
				Notes = $"From {entry.modNPC.Mod.DisplayName}. Spawned directly; summoning requirements are skipped.",
			});
	}
}
