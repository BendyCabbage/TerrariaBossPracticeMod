using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace BossPractice.Common.Bosses;

/// <summary>
/// Describes the bosses of one mod (or vanilla). To support a new mod, add a subclass in <c>Common/Bosses/Sources</c>:
/// it is found automatically and only loads while that mod is enabled. Other mods' content is looked up by
/// internal name (<see cref="NPCType"/>), so Boss Practice never needs their assemblies to build.
/// </summary>
public abstract class BossSource : ModType
{
	/// <summary>Internal name of the mod whose bosses this source describes, or <see cref="BossDefinition.VanillaModName"/>.</summary>
	public abstract string ModName { get; }

	public override bool IsLoadingEnabled(Mod mod) => ModName == BossDefinition.VanillaModName || ModLoader.HasMod(ModName);

	protected sealed override void Register()
	{
		ModTypeLookup<BossSource>.Register(this);
	}

	/// <summary>Registers this mod's bosses. Called once from PostSetupContent, after every mod's content exists.</summary>
	public abstract void AddBosses(BossRegistrar registrar);

	/// <summary>
	/// Changes bosses registered by other sources, for example adding phase variants for vanilla bosses that this
	/// mod reworks. Runs after every source's <see cref="AddBosses"/>.
	/// </summary>
	public virtual void ModifyBosses(BossRegistrar registrar)
	{
	}

	/// <summary>
	/// Finds an NPC from <see cref="ModName"/> by its class name. Returns 0 and logs a warning when it doesn't exist
	/// (for example after the mod renames it), which makes <see cref="BossRegistrar.Add"/> skip the boss using it.
	/// </summary>
	protected int NPCType(string name)
	{
		if (ModContent.TryFind(ModName, name, out ModNPC npc)) {
			return npc.Type;
		}

		Mod.Logger.Warn($"{ModName} has no NPC named {name}; bosses using it won't be listed.");
		return 0;
	}

	/// <summary>
	/// Calls the target mod's public Mod.Call API. Returns null if the mod isn't loaded or the call fails
	/// (some mods return an exception object instead of throwing; that counts as failing too).
	/// </summary>
	protected object CallMod(params object[] args)
	{
		if (!ModLoader.TryGetMod(ModName, out Mod target)) {
			return null;
		}

		object result;
		try {
			result = target.Call(args);
		}
		catch (System.Exception e) {
			result = e;
		}

		if (result is System.Exception error) {
			Mod.Logger.Warn($"{ModName}.Call({string.Join(", ", args)}) failed: {error.Message}");
			return null;
		}

		return result;
	}
}

/// <summary>Collects boss definitions from every <see cref="BossSource"/>.</summary>
public sealed class BossRegistrar
{
	private readonly List<BossDefinition> bosses = [];
	private readonly Dictionary<string, BossDefinition> byKey = [];
	private readonly HashSet<int> ignoredTypes = [];

	internal BossSource CurrentSource { get; set; }

	internal IReadOnlyList<BossDefinition> Bosses => bosses;

	/// <summary>NPC types that are flagged as bosses but must never be listed.</summary>
	internal IReadOnlySet<int> IgnoredTypes => ignoredTypes;

	/// <summary>Adds a boss from the current source. Bosses that reference missing NPCs are skipped with a warning.</summary>
	public void Add(BossDefinition boss)
	{
		boss.ModName = CurrentSource.ModName;
		if (boss.AllTypes.Any(type => type <= 0 || type >= NPCLoader.NPCCount)) {
			CurrentSource.Mod.Logger.Warn($"Skipping boss {boss.Key}: it references an NPC that doesn't exist.");
			return;
		}

		if (!byKey.TryAdd(boss.Key, boss)) {
			CurrentSource.Mod.Logger.Warn($"Skipping boss {boss.Key}: a boss with that key is already registered.");
			return;
		}

		bosses.Add(boss);
	}

	/// <summary>
	/// Keeps NPCs that are flagged as bosses but aren't real fights (test dummies, invisible controllers) out of the
	/// automatically discovered bosses. Missing types (0) are ignored.
	/// </summary>
	public void Ignore(params int[] types)
	{
		ignoredTypes.UnionWith(types.Where(type => type > 0));
	}

	/// <summary>Finds a registered boss, e.g. <c>Find("Terraria", "eye")</c>. Returns null if it isn't registered.</summary>
	public BossDefinition Find(string modName, string name) => byKey.GetValueOrDefault($"{modName}/{name}");

	/// <summary>Adds a phase variant to another source's boss. Logs a warning if that boss isn't registered.</summary>
	public void AddPhaseVariant(string modName, string name, PhaseVariant variant)
	{
		BossDefinition boss = Find(modName, name);
		if (boss == null) {
			CurrentSource.Mod.Logger.Warn($"{CurrentSource.Name} can't add phases to {modName}/{name}: that boss isn't registered.");
			return;
		}

		boss.AddPhaseVariant(variant);
	}

	/// <summary>Attaches NPCs from the current source's mod to another source's boss (see <see cref="BossDefinition.AddRelatedTypes"/>).</summary>
	public void AddRelatedTypes(string modName, string name, params int[] types)
	{
		BossDefinition boss = Find(modName, name);
		if (boss == null) {
			CurrentSource.Mod.Logger.Warn($"{CurrentSource.Name} can't add NPCs to {modName}/{name}: that boss isn't registered.");
			return;
		}

		boss.AddRelatedTypes(types);
	}
}
