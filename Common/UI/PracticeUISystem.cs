using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerrariaBossPracticeMod.Common.UI;

[Autoload(Side = ModSide.Client)]
public sealed class PracticeUISystem : ModSystem
{
	private static UserInterface userInterface;
	private static PracticeMenuState menuState;
	private static GameTime lastGameTime;

	public static ModKeybind ToggleMenuKeybind { get; private set; }

	public static bool IsVisible => userInterface?.CurrentState != null;

	public override void Load()
	{
		ToggleMenuKeybind = KeybindLoader.RegisterKeybind(Mod, "TogglePracticeMenu", "P");
		userInterface = new UserInterface();
	}

	public override void Unload()
	{
		ToggleMenuKeybind = null;
		userInterface = null;
		menuState = null;
		lastGameTime = null;
	}

	public override void OnWorldUnload()
	{
		Hide();
	}

	public static void Toggle()
	{
		if (IsVisible) {
			Hide();
		}
		else {
			Show();
		}
	}

	public static void Show()
	{
		// Created lazily so the boss list is built after every mod has finished loading.
		menuState ??= new PracticeMenuState();
		userInterface?.SetState(menuState);
		SoundEngine.PlaySound(SoundID.MenuOpen);
	}

	public static void Hide()
	{
		if (!IsVisible) {
			return;
		}

		userInterface.SetState(null);
		SoundEngine.PlaySound(SoundID.MenuClose);
	}

	public override void UpdateUI(GameTime gameTime)
	{
		lastGameTime = gameTime;
		if (IsVisible) {
			userInterface.Update(gameTime);
		}
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		int mouseTextIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
		if (mouseTextIndex < 0) {
			return;
		}

		layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
			"TerrariaBossPracticeMod: Practice Menu",
			() => {
				if (IsVisible) {
					userInterface.Draw(Main.spriteBatch, lastGameTime);
				}

				return true;
			},
			InterfaceScaleType.UI));
	}
}

public sealed class PracticeKeybindPlayer : ModPlayer
{
	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (PracticeUISystem.ToggleMenuKeybind?.JustPressed == true) {
			PracticeUISystem.Toggle();
		}
	}
}
