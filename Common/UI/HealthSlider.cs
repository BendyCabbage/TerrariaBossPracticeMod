using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;
using TerrariaBossPracticeMod.Common.Practice;

namespace TerrariaBossPracticeMod.Common.UI;

/// <summary>A draggable bar for picking a health percentage, with markers at each phase's starting health.</summary>
public sealed class HealthSlider : UIElement
{
	private static readonly Color TrackColor = new(20, 20, 40, 220);
	private static readonly Color FillColor = new(200, 50, 60);
	private static readonly Color MarkerColor = new(255, 230, 120);

	private bool dragging;
	private float value = 1f;

	public event Action<float> OnValueChanged;

	public float[] Markers { get; set; } = [];

	public float Value {
		get => value;
		set => this.value = Math.Clamp(MathF.Round(value * 100f) / 100f, PracticeManager.MinHealthPercent, 1f);
	}

	public override void LeftMouseDown(UIMouseEvent evt)
	{
		base.LeftMouseDown(evt);
		dragging = true;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (!dragging) {
			return;
		}

		if (!Main.mouseLeft) {
			dragging = false;
			return;
		}

		CalculatedStyle dims = GetInnerDimensions();
		float newValue = (Main.MouseScreen.X - dims.X) / dims.Width;
		float oldValue = Value;
		Value = newValue;
		if (Value != oldValue) {
			OnValueChanged?.Invoke(Value);
		}
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		Texture2D pixel = TextureAssets.MagicPixel.Value;
		Rectangle track = GetInnerDimensions().ToRectangle();

		spriteBatch.Draw(pixel, track, TrackColor);
		spriteBatch.Draw(pixel, new Rectangle(track.X, track.Y, (int)(track.Width * Value), track.Height), FillColor);

		foreach (float marker in Markers) {
			if (marker < 1f) {
				int x = track.X + (int)(track.Width * marker);
				spriteBatch.Draw(pixel, new Rectangle(x - 1, track.Y - 3, 2, track.Height + 6), MarkerColor);
			}
		}

		int thumbX = track.X + (int)(track.Width * Value);
		Color thumbColor = IsMouseHovering || dragging ? Color.White : new Color(220, 220, 220);
		spriteBatch.Draw(pixel, new Rectangle(thumbX - 4, track.Y - 5, 8, track.Height + 10), thumbColor);
	}
}
