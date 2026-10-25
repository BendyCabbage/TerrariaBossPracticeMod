using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using BossPractice.Common.Practice;

namespace BossPractice.Common.Bosses;

/// <summary>
/// Spawn routines for <see cref="BossDefinition.Spawner"/>, plus building blocks for bosses that need their own.
/// Everything spawned while a spawner runs is tagged as part of the practice fight automatically.
/// </summary>
public static class BossSpawners
{
	/// <summary>Spawns each of <see cref="BossDefinition.SpawnTypes"/> with <see cref="SpawnNear"/>.</summary>
	public static readonly BossSpawner Default = (player, boss) => {
		foreach (int type in boss.SpawnTypes) {
			SpawnNear(player, type, boss.FallbackOffset);
		}
	};

	/// <summary>Uses vanilla <see cref="NPC.SpawnWOF"/>, which only works in the Underworld.</summary>
	public static readonly BossSpawner WallOfFlesh = (player, _) => {
		Main.wofNPCIndex = -1;
		NPC.SpawnWOF(player.Center);
	};

	/// <summary>Spawns every one of <see cref="BossDefinition.SpawnTypes"/> at a fixed offset from the player, bypassing the vanilla spawn routine.</summary>
	public static BossSpawner AtOffset(Vector2 offset) => (player, boss) => {
		foreach (int type in boss.SpawnTypes) {
			SpawnAt(player, type, offset);
		}
	};

	/// <summary>
	/// The vanilla routine finds an off-screen position and handles special cases (Golem needs an altar,
	/// Duke Fishron needs a bobber, ...). When it gives up, places the boss at <paramref name="fallbackOffset"/> instead.
	/// </summary>
	public static void SpawnNear(Player player, int type, Vector2 fallbackOffset)
	{
		int spawnedBefore = PracticeManager.CapturedNPCs.Count(npc => npc.type == type);
		NPC.SpawnOnPlayer(player.whoAmI, type);
		if (PracticeManager.CapturedNPCs.Count(npc => npc.type == type) == spawnedBefore) {
			Vector2 position = player.Center + fallbackOffset;
			NPC.SpawnBoss((int)position.X, (int)position.Y, type, player.whoAmI);
		}
	}

	/// <summary>
	/// Spawns an NPC at an exact offset from the player with the given starting AI values, for bosses whose AI
	/// reads them on the first tick. Returns null if there was no free NPC slot.
	/// </summary>
	public static NPC SpawnAt(Player player, int type, Vector2 offset, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f, float ai3 = 0f)
	{
		Vector2 position = player.Center + offset;
		int index = NPC.NewNPC(new EntitySource_BossSpawn(player), (int)position.X, (int)position.Y, type, 1, ai0, ai1, ai2, ai3, player.whoAmI);
		if (index >= Main.maxNPCs) {
			return null;
		}

		NPC npc = Main.npc[index];
		npc.target = player.whoAmI;
		if (Main.netMode == NetmodeID.Server) {
			NetMessage.SendData(MessageID.SyncNPC, number: index);
		}

		return npc;
	}
}
