using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaBossPracticeMod.Common.Practice;

namespace TerrariaBossPracticeMod;

public sealed class BossPracticeMod : Mod
{
	public override void HandlePacket(BinaryReader reader, int whoAmI)
	{
		// Clients only ever send requests; the server never sends packets back (replies go through chat).
		if (Main.netMode == NetmodeID.Server) {
			PracticeNetwork.HandlePacket(reader, whoAmI);
		}
	}
}
