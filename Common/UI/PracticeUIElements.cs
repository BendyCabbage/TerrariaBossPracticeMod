using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;
using TerrariaBossPracticeMod.Common.Bosses;

namespace TerrariaBossPracticeMod.Common.UI;

/// <summary>A clickable text panel that highlights on hover and when selected.</summary>
public class PracticeButton : UITextPanel<string>
{
	private static readonly Color NormalColor = new Color(63, 82, 151) * 0.8f;
	private static readonly Color SelectedColor = new(90, 120, 210);

	public PracticeButton(string text, float textScale = 0.9f) : base(text, textScale)
	{
		BackgroundColor = NormalColor;
	}

	public bool Selected { get; set; }

	public override void LeftClick(UIMouseEvent evt)
	{
		base.LeftClick(evt);
		SoundEngine.PlaySound(SoundID.MenuTick);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		BackgroundColor = Selected ? SelectedColor : NormalColor;
		BorderColor = IsMouseHovering ? Colors.FancyUIFatButtonMouseOver : (Selected ? Color.White : Color.Black);
		base.DrawSelf(spriteBatch);
	}
}

/// <summary>A boss list row showing the boss's map icon and name.</summary>
public sealed class BossListEntry : PracticeButton
{
	private const int IconSize = 26;

	public BossListEntry(BossDefinition boss) : base(boss.DisplayName, 0.85f)
	{
		Boss = boss;
		Width.Set(0f, 1f);
		Height.Set(36f, 0f);
		TextHAlign = 0f;
		PaddingLeft = IconSize + 14f;
		if (boss.IsModded) {
			TextColor = new Color(180, 220, 255);
		}
	}

	public BossDefinition Boss { get; }

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		int headIndex = NPCID.Sets.BossHeadTextures[Boss.PrimaryType];
		if (headIndex < 0 || headIndex >= TextureAssets.NpcHeadBoss.Length) {
			return;
		}

		Texture2D icon = TextureAssets.NpcHeadBoss[headIndex].Value;
		CalculatedStyle dims = GetDimensions();
		float scale = MathHelper.Min(1f, IconSize / (float)MathHelper.Max(icon.Width, icon.Height));
		Vector2 center = new(dims.X + 8f + IconSize / 2f, dims.Y + dims.Height / 2f);
		spriteBatch.Draw(icon, center, null, Color.White, 0f, icon.Size() / 2f, scale, SpriteEffects.None, 0f);
	}
}
