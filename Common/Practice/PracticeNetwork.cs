using System.IO;
using Terraria;
using Terraria.ModLoader;
using BossPractice.Common.Bosses;

namespace BossPractice.Common.Practice;

public enum PracticeMessageType : byte
{
	Spawn,
	Despawn,
	MakeNight,
}

/// <summary>Client to server requests. The server performs the action and replies through chat.</summary>
public static class PracticeNetwork
{
	public static void SendSpawnRequest(int bossIndex, float healthPercent, int phaseIndex)
	{
		ModPacket packet = ModContent.GetInstance<BossPracticeMod>().GetPacket();
		packet.Write((byte)PracticeMessageType.Spawn);
		packet.Write((short)bossIndex);
		packet.Write(healthPercent);
		packet.Write((sbyte)phaseIndex);
		packet.Send();
	}

	public static void SendSimpleRequest(PracticeMessageType type)
	{
		ModPacket packet = ModContent.GetInstance<BossPracticeMod>().GetPacket();
		packet.Write((byte)type);
		packet.Send();
	}

	/// <summary>Only called on the server.</summary>
	public static void HandlePacket(BinaryReader reader, int whoAmI)
	{
		Player player = Main.player[whoAmI];
		var type = (PracticeMessageType)reader.ReadByte();
		switch (type) {
			case PracticeMessageType.Spawn: {
				int bossIndex = reader.ReadInt16();
				float healthPercent = reader.ReadSingle();
				int phaseIndex = reader.ReadSByte();

				// Boss indices line up because the server and clients always have the same mods loaded.
				BossDefinition boss = BossRegistry.Get(bossIndex);
				if (boss == null || float.IsNaN(healthPercent)) {
					PracticeManager.Reply(player, "[Practice] Invalid spawn request.", PracticeManager.ErrorColor);
					return;
				}

				PracticeManager.TrySpawn(player, boss, healthPercent, phaseIndex);
				break;
			}
			case PracticeMessageType.Despawn:
				PracticeManager.DespawnAll(player);
				break;
			case PracticeMessageType.MakeNight:
				PracticeManager.MakeNight(player);
				break;
		}
	}
}
