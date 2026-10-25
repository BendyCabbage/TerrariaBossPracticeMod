# Boss Practice (tModLoader mod)

Spawn any Terraria boss on demand. You choose how much health it starts with, or start it in a specific phase.

## Features

- **Practice menu** (press `P` by default; you can rebind it in Settings → Controls):
  - Every vanilla boss in progression order, plus the bosses of every enabled mod (see [Mod support](#mod-support))
  - **Mod tabs** and a **search box** to filter the list; it stays in progression order
  - A **starting health** slider (1–100%) with fine-tune buttons. Markers on the slider show where each phase starts
  - **Starting point** presets for each boss phase (see below)
  - **Spawn**, **Despawn all** and **Make night** buttons
- **Chat command** that does the same as the menu (see below)
- **No side effects by default**: practice bosses drop no loot and don't trigger death events. That means no "downed" flags, no hardmode from the Wall of Flesh, and no lunar events from the Lunatic Cultist. You can turn this off in the mod config.
- When you kill a boss, chat shows how long the fight took, e.g. `[Practice] Plantera defeated in 84.2s.`
- **Multiplayer**: spawning runs on the server and is synced to every player.

## Phase presets

The health thresholds come from the vanilla AI code in tModLoader 1.4.4. Each preset starts just below its threshold, so the boss changes phase as soon as it spawns.

| Boss | Starting points |
| --- | --- |
| Eye of Cthulhu | Phase 1 · Phase 2 (49% Classic / 64% Expert+) · Phase 2 rapid dashes (34%, Expert+) |
| Brain of Cthulhu | Phase 1 · Phase 2 (Creepers destroyed) |
| Skeletron | With hands · Hands destroyed |
| Queen Slime | Phase 1 · Phase 2 (49%) |
| The Twins | Phase 1 · Phase 2 (39%) · Phase 2 Retinazer alone · Phase 2 Spazmatism alone |
| Skeletron Prime | With arms · Arms destroyed |
| Plantera | Phase 1 · Phase 2 (49%) |
| Golem | Head attached · Head detached |
| Duke Fishron | Phase 1 · Phase 2 (49%) · Phase 3 (14%, Expert+) |
| Empress of Light | Phase 1 · Phase 2 (49%) |
| Moon Lord | Full fight · Core exposed (head and hands destroyed) |
| King Slime, Eater of Worlds, Queen Bee, Deerclops, Wall of Flesh, The Destroyer, Lunatic Cultist, modded bosses | Full fight (use the health slider) |

You can change the health after picking a preset. For example, "Hands destroyed" with 30% health starts Skeletron's head alone at 30%.

## Chat command

```
/practice list [mod]                   list every boss (optionally only one mod's) and its phases
/practice <boss> [health%] [phase <n>] spawn a boss
/practice clear                        despawn all bosses and hostile projectiles
/practice night                        set the time to dusk
/practice day                          set the time to dawn
```

Examples: `/practice eoc 30`, `/practice twins phase 2`, `/practice moon lord p2 50`, `/practice plantera`.

You can name a boss by its short name (`eye`, `wof`, `ml`…), its full name, or its key (`CalamityMod/DevourerofGods`). `/practice list` shows every key.

## Mod support

Bosses from enabled mods are listed after (or between) the vanilla bosses in progression order, using the same scale as Boss Checklist.

| Mod | Support |
| --- | --- |
| Calamity | Every boss with phase presets, plus Revengeance/Death/Boss Rush presets for the vanilla bosses Calamity reworks |
| Thorium | Every boss with phase presets |
| Fargo's Souls | Every boss with phase presets, plus Eternity Mode presets for vanilla bosses |
| Any other mod | Bosses are found automatically (any NPC flagged as a boss) and support the health slider only |

When a difficulty mode changes a boss's phases, the menu shows which presets are active (e.g. *Calamity Mod - Revengeance presets*).

## Things to know

- **Wall of Flesh** spawns only while you're in the Underworld, because that's a vanilla requirement.
- Bosses keep their vanilla behaviour around time and biome. For example, the Eye and the mechanical bosses flee at dawn, Skeletron and the Empress are lethal during the day, and Plantera, Golem and Duke Fishron enrage outside their biomes. Use **Make night** and practice in the right biome.
- **Golem** and **Duke Fishron** normally need an altar or a fishing bobber. If you don't have one, they spawn next to you instead.
- For bosses made of several parts, the health percentage applies to the main health pool(s): every Eater of Worlds segment, the Golem body and head, and the Moon Lord core, head and hands.
- With **Despawn existing bosses before spawning** turned on (the default), each spawn first clears any bosses that are already alive.

## Building

1. Put this folder in your tModLoader `ModSources` directory. On macOS that's
   `~/Library/Application Support/Terraria/tModLoader/ModSources/BossPractice`.
   Clone or copy it there. Don't symlink it: the `.csproj` imports `../tModLoader.targets` relative to the folder's real path, so the build won't find it through a symlink.
2. In tModLoader, go to **Workshop → Develop Mods** and click **Build + Reload** on *Boss Practice*.

You can also build it from the command line with `dotnet build` inside `ModSources/BossPractice`.

## Project layout

```
BossPracticeMod.cs                 Mod entry point; routes network packets
Common/Bosses/                     Boss + phase model, spawners, phase setups, and BossRegistry (builds the list)
Common/Bosses/Sources/             One BossSource per supported mod (VanillaBosses, CalamityBosses, ...)
Common/Practice/PracticeManager.cs Server-side spawn / health / phase setup / despawn logic
Common/Practice/PracticeGlobalNPC  Tags practice NPCs (and anything they spawn); blocks loot and progression
Common/Practice/PracticeNetwork.cs Client → server requests
Common/Commands/                   /practice chat command
Common/UI/                         Practice menu, health slider, keybind
Common/Configs/                    Server-side config
```

## Adding support for another mod

Each supported mod has a `BossSource` subclass in `Common/Bosses/Sources/`. Sources are found automatically and only load while their mod is enabled, so adding a mod means adding one file:

```csharp
public sealed class ExampleModBosses : BossSource
{
	public override string ModName => "ExampleMod"; // the mod's internal name

	public override void AddBosses(BossRegistrar registrar)
	{
		int head = NPCType("ExampleWormHead"); // looked up by class name; no reference to the mod's DLL
		registrar.Add(new BossDefinition("ExampleWorm", head) {
			Progression = 5.5f, // between Skeletron (5) and Deerclops (6); use the value from the mod's Boss Checklist entry
			Aliases = ["worm"],
			RelatedTypes = [NPCType("ExampleWormBody"), NPCType("ExampleWormTail")], // parts flagged as bosses
			DefaultPhases = [
				BossPhase.FullFight,
				new BossPhase("Phase 2", 0.49f, "Below 50% health."),
			],
		});
	}
}
```

Things a source can use:

- **`BossDefinition`**: `HealthTypes` (which NPCs get the chosen health), `ReadyTypes` (parts to wait for before applying it), `RelatedTypes`, `Spawner` (see `BossSpawners`), `CheckCanSpawn`, `Notes`, `SetupTimeoutTicks`, and `StageTransitionTypes` (parts whose own `PreKill` spawns the next stage, which must still run when loot is disabled).
- **`BossPhase`**: a starting health (a fixed value, or a function for difficulty-dependent thresholds), an optional `Setup` run once the fight exists (see `PhaseSetups`: kill parts, set AI values, ...) and an optional per-phase `Spawner`.
- **`PhaseVariant`**: a whole set of phases that replaces the defaults while a condition holds, e.g. a difficulty mode. `ModifyBosses` can add these to other sources' bosses, including vanilla ones.
- **`CallMod`**: calls the mod's public `Mod.Call` API, for example to check a difficulty mode.

If the mod renames an NPC, only the bosses using it are skipped (with a warning in `client.log`); they then show up through automatic discovery instead.
