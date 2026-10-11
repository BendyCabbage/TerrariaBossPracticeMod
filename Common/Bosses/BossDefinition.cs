using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace BossPractice.Common.Bosses;

public enum SpawnStyle
{
	/// <summary>Uses vanilla <see cref="NPC.SpawnOnPlayer"/>, falling back to spawning at a fixed offset from the player.</summary>
	Default,

	/// <summary>Uses vanilla <see cref="NPC.SpawnWOF"/>, which only works in the Underworld.</summary>
	WallOfFlesh,
}

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

	public string Name { get; }

	public string Description { get; }

	/// <summary>Runs on the server (or in singleplayer) against every NPC belonging to the practice fight.</summary>
	public Action<IReadOnlyList<NPC>> Setup { get; }

	/// <summary>Default health for this phase in the range (0, 1]. Can depend on world difficulty.</summary>
	public float HealthPercent => healthPercent();
}

public sealed class BossDefinition
{
	private readonly string displayName;

	public BossDefinition(string key, int primaryType, string displayName = null)
	{
		Key = key;
		PrimaryType = primaryType;
		this.displayName = displayName;
		SpawnTypes = [primaryType];
		HealthTypes = [primaryType];
	}

	/// <summary>Short identifier used by the chat command and in network packets.</summary>
	public string Key { get; }

	public string[] Aliases { get; init; } = [];

	/// <summary>The NPC used for the boss's name and, when it has one, map icon.</summary>
	public int PrimaryType { get; }

	/// <summary>NPC types spawned directly. Most bosses spawn their own parts from their AI.</summary>
	public int[] SpawnTypes { get; init; }

	/// <summary>NPC types whose health is scaled to the chosen percentage.</summary>
	public int[] HealthTypes { get; init; }

	/// <summary>
	/// NPC types that must all exist before health and phase setup are applied.
	/// Defaults to <see cref="HealthTypes"/>. Needed for bosses that spawn parts after an intro (e.g. Moon Lord).
	/// </summary>
	public int[] ReadyTypes { get; init; }

	public SpawnStyle Style { get; init; } = SpawnStyle.Default;

	/// <summary>Where to spawn the boss relative to the player if the vanilla spawn routine can't place it.</summary>
	public Vector2 FallbackOffset { get; init; } = new(0f, -400f);

	public string Notes { get; init; }

	public bool IsModded { get; init; }

	public IReadOnlyList<BossPhase> Phases { get; init; } = [];

	public string DisplayName => displayName ?? Lang.GetNPCNameValue(PrimaryType);

	public IEnumerable<int> RequiredTypes => ReadyTypes ?? HealthTypes;

	/// <summary>
	/// Index into the boss head textures, or -1 if none. Some primary NPCs have no icon of their own
	/// (Golem's body, Moon Lord's core), so this falls back to the other parts of the fight.
	/// </summary>
	public int HeadIconIndex => HealthTypes.Prepend(PrimaryType)
		.Select(type => NPCID.Sets.BossHeadTextures[type])
		.FirstOrDefault(index => index >= 0, -1);

	public bool Matches(string query)
	{
		string normalized = Normalize(query);
		return Normalize(Key) == normalized
			|| Normalize(DisplayName) == normalized
			|| Aliases.Any(alias => Normalize(alias) == normalized);
	}

	private static string Normalize(string text) => new(text.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
}
