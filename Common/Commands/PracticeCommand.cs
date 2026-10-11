using System.Globalization;
using System.Linq;
using Terraria.ModLoader;
using TerrariaBossPracticeMod.Common.Bosses;
using TerrariaBossPracticeMod.Common.Practice;

namespace TerrariaBossPracticeMod.Common.Commands;

/// <summary>
/// Chat command alternative to the menu. Runs on the server in multiplayer.
/// Examples: "/practice eye 30", "/practice twins phase 2", "/practice moon lord p2 50".
/// </summary>
public sealed class PracticeCommand : ModCommand
{
	public override CommandType Type => CommandType.World;

	public override string Command => "practice";

	public override string Usage =>
		"/practice list\n" +
		"/practice <boss> [health%] [phase <n>]\n" +
		"/practice clear\n" +
		"/practice night\n" +
		"/practice day";

	public override string Description => "Spawn a boss for practice at a chosen health percentage or phase.";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (args.Length == 0) {
			throw new UsageException();
		}

		switch (args[0].ToLowerInvariant()) {
			case "list":
				ListBosses(caller);
				return;
			case "clear":
			case "despawn":
				PracticeManager.DespawnAll(caller.Player);
				return;
			case "night":
				PracticeManager.MakeNight(caller.Player);
				return;
			case "day":
				PracticeManager.MakeDay(caller.Player);
				return;
		}

		// Boss names can contain spaces, so match the longest run of leading words that names a boss.
		BossDefinition boss = null;
		int nameLength = args.Length;
		for (; nameLength > 0; nameLength--) {
			boss = BossRegistry.Find(string.Join(' ', args.Take(nameLength)));
			if (boss != null) {
				break;
			}
		}

		if (boss == null) {
			throw new UsageException($"Unknown boss '{args[0]}'. Use /practice list to see every boss.");
		}

		float? healthPercent = null;
		int phaseIndex = -1;
		string[] options = args.Skip(nameLength).ToArray();
		for (int i = 0; i < options.Length; i++) {
			string option = options[i].ToLowerInvariant().TrimEnd('%');
			if (option is "phase" or "p" && i + 1 < options.Length) {
				phaseIndex = ParsePhase(boss, options[++i]);
			}
			else if (option.StartsWith('p') && option.Length > 1 && char.IsDigit(option[1])) {
				phaseIndex = ParsePhase(boss, option[1..]);
			}
			else if (float.TryParse(option, NumberStyles.Float, CultureInfo.InvariantCulture, out float percent) && percent > 0f && percent <= 100f) {
				healthPercent = percent / 100f;
			}
			else {
				throw new UsageException($"Didn't understand '{options[i]}'. Health must be 1-100 and phases look like 'phase 2'.");
			}
		}

		// An explicit health percentage overrides the phase's default.
		float health = healthPercent ?? (phaseIndex >= 0 ? boss.Phases[phaseIndex].HealthPercent : 1f);
		PracticeManager.TrySpawn(caller.Player, boss, health, phaseIndex);
	}

	private static int ParsePhase(BossDefinition boss, string text)
	{
		if (!int.TryParse(text, out int number) || number < 1 || number > boss.Phases.Count) {
			throw new UsageException($"{boss.DisplayName} has phases 1-{boss.Phases.Count}:\n{DescribePhases(boss)}");
		}

		return number - 1;
	}

	private static string DescribePhases(BossDefinition boss) =>
		string.Join('\n', boss.Phases.Select((phase, i) => $"  {i + 1}: {phase.Name} ({phase.HealthPercent * 100f:0.#}%)"));

	private static void ListBosses(CommandCaller caller)
	{
		foreach (BossDefinition boss in BossRegistry.Bosses) {
			string phases = string.Join(", ", boss.Phases.Select((phase, i) => $"{i + 1}={phase.Name}"));
			caller.Reply($"{boss.Key} - {boss.DisplayName} [{phases}]", PracticeManager.InfoColor);
		}
	}
}
