using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace BossPractice.Common.Configs;

public sealed class PracticeConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;

	[DefaultValue(true)]
	public bool DisableLootAndProgression = true;

	[DefaultValue(true)]
	public bool DespawnBossesBeforeSpawning = true;

	[DefaultValue(true)]
	public bool ClearHostileProjectilesOnDespawn = true;
}
