using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossPractice.Common.Bosses;

/// <summary>Spawns every part of a boss fight that has to be spawned directly. Runs on the server (or in singleplayer).</summary>
public delegate void BossSpawner(Player player, BossDefinition boss);

/// <summary>
/// A preset starting point for a boss fight: a default health percentage plus an optional setup step
/// (for example destroying Skeletron's hands) that runs once all of the boss's parts exist.
/// </summary>
public sealed class BossPhase
{
	private readonly Func<float> healthPercent;

	public BossPhase(string name, Func<float> healthPercent, string description = null, Action<IReadOnlyList<NPC>> setup = null)
	{
		Name = name;
		this.healthPercent = healthPercent;
		Description = description;
		Setup = setup;
	}

	public BossPhase(string name, float healthPercent, string description = null, Action<IReadOnlyList<NPC>> setup = null)
		: this(name, () => healthPercent, description, setup)
	{
	}

	public static BossPhase FullFight => new("Full fight", 1f, "Starts the fight from the beginning.");

	public string Name { get; }

	public string Description { get; }

	/// <summary>Runs on the server (or in singleplayer) against every NPC belonging to the practice fight.</summary>
	public Action<IReadOnlyList<NPC>> Setup { get; }

	/// <summary>
	/// Replaces <see cref="BossDefinition.Spawner"/> for this phase, for starting points that need a different spawn,
	/// such as setting AI values before the boss's first tick. Null uses the boss's spawner.
	/// </summary>
	public BossSpawner Spawner { get; init; }

	/// <summary>Default health for this phase in the range (0, 1]. Can depend on world difficulty.</summary>
	public float HealthPercent => healthPercent();
}

/// <summary>
/// An alternative set of phases used while a condition holds, such as a difficulty mode from another mod
/// that changes when a boss switches phase (Calamity's Revengeance, Fargo's Eternity Mode, ...).
/// </summary>
public sealed class PhaseVariant(string label, Func<bool> isActive, IReadOnlyList<BossPhase> phases, string notes = null)
{
	/// <summary>Shown in the menu so players know which rules the presets follow, e.g. "Revengeance".</summary>
	public string Label { get; } = label;

	public Func<bool> IsActive { get; } = isActive;

	public IReadOnlyList<BossPhase> Phases { get; } = phases;

	/// <summary>Replaces the boss's own notes while this variant is active.</summary>
	public string Notes { get; } = notes;

	/// <summary>
	/// Variants with a higher priority win over lower ones when several are active at once. Used by mods that build on
	/// another mod's difficulty, e.g. Infernum replaces the AI that Calamity's Revengeance presets describe.
	/// </summary>
	public int Priority { get; init; }
}

public sealed class BossDefinition
{
	public const string VanillaModName = "Terraria";

	private readonly string displayName;
	private readonly List<PhaseVariant> phaseVariants = [];
	private HashSet<int> allTypes;
	private int[] relatedTypes = [];

	/// <param name="name">Identifier that is unique within the boss's mod and never changes; also accepted by the chat command.</param>
	public BossDefinition(string name, int primaryType, string displayName = null)
	{
		Name = name;
		PrimaryType = primaryType;
		this.displayName = displayName;
		SpawnTypes = [primaryType];
		HealthTypes = [primaryType];
	}

	public string Name { get; }

	/// <summary>Internal name of the mod the boss comes from, or <see cref="VanillaModName"/>. Set when the boss is registered.</summary>
	public string ModName { get; internal set; }

	/// <summary>Stable identifier used in network packets and accepted by the chat command, e.g. "CalamityMod/DevourerofGods".</summary>
	public string Key => $"{ModName}/{Name}";

	public bool IsModded => ModName != VanillaModName;

	public string ModDisplayName => IsModded && ModLoader.TryGetMod(ModName, out Mod mod) ? mod.DisplayName : "Vanilla";

	public string[] Aliases { get; init; } = [];

	/// <summary>
	/// Position in the boss list, on the same scale as Boss Checklist (King Slime 1, Wall of Flesh 7, Moon Lord 18).
	/// Null for automatically discovered bosses, which are listed after every known boss.
	/// </summary>
	public float? Progression { get; init; }

	/// <summary>The NPC used for the boss's name and, when it has one, map icon.</summary>
	public int PrimaryType { get; }

	/// <summary>NPC types spawned directly by <see cref="BossSpawners.Default"/>. Most bosses spawn their own parts from their AI.</summary>
	public int[] SpawnTypes { get; init; }

	/// <summary>NPC types whose health is scaled to the chosen percentage.</summary>
	public int[] HealthTypes { get; init; }

	/// <summary>
	/// NPC types that must all exist before health and phase setup are applied.
	/// Defaults to <see cref="HealthTypes"/>. Needed for bosses that spawn parts after an intro (e.g. Moon Lord).
	/// </summary>
	public int[] ReadyTypes { get; init; }

	/// <summary>
	/// Other NPC types that belong to this fight, such as minions or parts flagged as bosses.
	/// They never get their own entry in the list of automatically discovered bosses. Missing types (0) are ignored
	/// rather than skipping the boss, since these are optional.
	/// </summary>
	public int[] RelatedTypes { get => relatedTypes; init => relatedTypes = value.Where(type => type > 0).ToArray(); }

	/// <summary>
	/// NPC types whose own PreKill hook starts the next stage of the fight, such as spawning a second form. With loot and
	/// progression disabled, Boss Practice still runs that hook for them (but not loot or OnKill), so the fight continues.
	/// </summary>
	public int[] StageTransitionTypes { get; init; } = [];

	public BossSpawner Spawner { get; init; } = BossSpawners.Default;

	/// <summary>Returns a reason the boss can't be spawned for this player right now, or null if it can.</summary>
	public Func<Player, string> CheckCanSpawn { get; init; }

	/// <summary>Where to spawn the boss relative to the player if the vanilla spawn routine can't place it.</summary>
	public Vector2 FallbackOffset { get; init; } = new(0f, -400f);

	/// <summary>How many ticks to wait for <see cref="ReadyTypes"/> before applying setup anyway. Raise it for bosses with long intros.</summary>
	public int SetupTimeoutTicks { get; init; } = 600;

	public string Notes { get; init; }

	public IReadOnlyList<BossPhase> DefaultPhases { get; init; } = [BossPhase.FullFight];

	/// <summary>The variant whose condition currently holds, or null when the default phases apply.</summary>
	public PhaseVariant ActiveVariant => phaseVariants
		.OrderByDescending(variant => variant.Priority) // stable, so equal priorities keep the order they were added in
		.FirstOrDefault(variant => variant.IsActive());

	/// <summary>Phase presets for the current world. Evaluate again whenever they're used, because they follow difficulty modes.</summary>
	public IReadOnlyList<BossPhase> Phases => ActiveVariant?.Phases ?? DefaultPhases;

	public string CurrentNotes => ActiveVariant?.Notes ?? Notes;

	public string DisplayName => displayName ?? Lang.GetNPCNameValue(PrimaryType);

	public IEnumerable<int> RequiredTypes => ReadyTypes ?? HealthTypes;

	/// <summary>Every NPC type that belongs to this fight.</summary>
	public IReadOnlySet<int> AllTypes => allTypes ??= [.. SpawnTypes, .. HealthTypes, .. ReadyTypes ?? [], .. RelatedTypes, PrimaryType];

	/// <summary>
	/// Index into the boss head textures, or -1 if none. Some primary NPCs have no icon of their own
	/// (Golem's body, Moon Lord's core), so this falls back to the other parts of the fight.
	/// </summary>
	public int HeadIconIndex => HealthTypes.Prepend(PrimaryType)
		.Select(type => NPCID.Sets.BossHeadTextures[type])
		.FirstOrDefault(index => index >= 0, -1);

	/// <summary>
	/// Adds a set of phases that replaces <see cref="DefaultPhases"/> while <see cref="PhaseVariant.IsActive"/> holds.
	/// Variants are checked by <see cref="PhaseVariant.Priority"/>, then in the order they were added; the first active one wins.
	/// </summary>
	public void AddPhaseVariant(PhaseVariant variant) => phaseVariants.Add(variant);

	/// <summary>Adds to <see cref="RelatedTypes"/>, e.g. minions another mod adds to this fight. Missing types (0) are ignored.</summary>
	public void AddRelatedTypes(params int[] types)
	{
		relatedTypes = [.. relatedTypes, .. types.Where(type => type > 0)];
		allTypes = null;
	}

	public bool Matches(string query)
	{
		string normalized = Normalize(query);
		return Normalize(Key) == normalized
			|| Normalize(Name) == normalized
			|| Normalize(DisplayName) == normalized
			|| Aliases.Any(alias => Normalize(alias) == normalized);
	}

	internal static string Normalize(string text) => new(text.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
}
