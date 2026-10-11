using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;
using BossPractice.Common.Bosses;
using BossPractice.Common.Practice;

namespace BossPractice.Common.UI;

public sealed class PracticeMenuState : UIState
{
	private const float PanelWidth = 700f;
	private const float PanelHeight = 520f;
	private const float ListWidth = 230f;
	private const float RightColumnLeft = ListWidth + 32f;
	private const float HeaderHeight = 44f;

	private readonly List<BossListEntry> bossEntries = [];
	private readonly List<PracticeButton> phaseButtons = [];

	private UIPanel panel;
	private UIList bossList;
	private UIText bossNameText;
	private UIText notesText;
	private UIText healthText;
	private HealthSlider slider;
	private UIList phaseList;
	private UIText phaseDescriptionText;

	private BossDefinition selectedBoss;
	private int selectedPhase = -1;

	public override void OnInitialize()
	{
		panel = new UIPanel {
			HAlign = 0.5f,
			VAlign = 0.5f,
		};
		panel.Width.Set(PanelWidth, 0f);
		panel.Height.Set(PanelHeight, 0f);
		Append(panel);

		panel.Append(new UIText("Boss Practice", 0.6f, large: true));

		var closeButton = new PracticeButton("X");
		closeButton.Width.Set(34f, 0f);
		closeButton.Height.Set(34f, 0f);
		closeButton.HAlign = 1f;
		closeButton.OnLeftClick += (_, _) => PracticeUISystem.Hide();
		panel.Append(closeButton);

		BuildBossList();
		BuildRightColumn();

		if (BossRegistry.Bosses.Count > 0) {
			SelectBoss(BossRegistry.Bosses[0]);
		}
	}

	private void BuildBossList()
	{
		bossList = new UIList {
			ListPadding = 4f,
		};
		bossList.Top.Set(HeaderHeight, 0f);
		bossList.Width.Set(ListWidth, 0f);
		bossList.Height.Set(-HeaderHeight, 1f);
		panel.Append(bossList);

		var scrollbar = new UIScrollbar();
		scrollbar.Left.Set(ListWidth + 4f, 0f);
		scrollbar.Top.Set(HeaderHeight + 6f, 0f);
		scrollbar.Height.Set(-HeaderHeight - 12f, 1f);
		panel.Append(scrollbar);
		bossList.SetScrollbar(scrollbar);

		// UIList sorts its items; keep progression order instead of alphabetical.
		bossList.ManualSortMethod = _ => { };
		foreach (BossDefinition boss in BossRegistry.Bosses) {
			var entry = new BossListEntry(boss);
			entry.OnLeftClick += (_, _) => SelectBoss(boss);
			bossList.Add(entry);
			bossEntries.Add(entry);
		}
	}

	private void BuildRightColumn()
	{
		var column = new UIElement();
		column.Left.Set(RightColumnLeft, 0f);
		column.Top.Set(HeaderHeight, 0f);
		column.Width.Set(-RightColumnLeft, 1f);
		column.Height.Set(-HeaderHeight, 1f);
		panel.Append(column);

		bossNameText = new UIText("", 0.5f, large: true);
		column.Append(bossNameText);

		notesText = new UIText("", 0.8f) {
			IsWrapped = true,
			TextColor = new Color(200, 200, 200),
		};
		notesText.Top.Set(34f, 0f);
		notesText.Width.Set(0f, 1f);
		notesText.Height.Set(40f, 0f);
		column.Append(notesText);

		healthText = new UIText("");
		healthText.Top.Set(82f, 0f);
		column.Append(healthText);

		slider = new HealthSlider();
		slider.Top.Set(110f, 0f);
		slider.Left.Set(6f, 0f);
		slider.Width.Set(-12f, 1f);
		slider.Height.Set(18f, 0f);
		slider.OnValueChanged += _ => RefreshHealthText();
		column.Append(slider);

		float buttonLeft = 0f;
		foreach (int step in new[] { -10, -1, 1, 10 }) {
			AddNudgeButton(column, step > 0 ? $"+{step}%" : $"{step}%", ref buttonLeft, () => SetHealth(slider.Value + step / 100f));
		}

		AddNudgeButton(column, "100%", ref buttonLeft, () => SetHealth(1f));

		var phaseLabel = new UIText("Starting point");
		phaseLabel.Top.Set(184f, 0f);
		column.Append(phaseLabel);

		phaseList = new UIList {
			ListPadding = 4f,
			ManualSortMethod = _ => { },
		};
		phaseList.Top.Set(210f, 0f);
		phaseList.Width.Set(0f, 1f);
		phaseList.Height.Set(-210f - 96f, 1f);
		column.Append(phaseList);

		phaseDescriptionText = new UIText("", 0.8f) {
			IsWrapped = true,
			TextColor = new Color(200, 200, 200),
		};
		phaseDescriptionText.Top.Set(-90f, 1f);
		phaseDescriptionText.Width.Set(0f, 1f);
		phaseDescriptionText.Height.Set(44f, 0f);
		column.Append(phaseDescriptionText);

		var spawnButton = new PracticeButton("Spawn", 1f);
		spawnButton.OnLeftClick += (_, _) => Spawn();
		var despawnButton = new PracticeButton("Despawn all");
		despawnButton.OnLeftClick += (_, _) => PracticeManager.RequestDespawn();
		var nightButton = new PracticeButton("Make night");
		nightButton.OnLeftClick += (_, _) => PracticeManager.RequestNight();

		PracticeButton[] bottomButtons = [spawnButton, despawnButton, nightButton];
		for (int i = 0; i < bottomButtons.Length; i++) {
			PracticeButton button = bottomButtons[i];
			button.Top.Set(-40f, 1f);
			button.Left.Set(0f, i / 3f);
			button.Width.Set(-6f, 1f / 3f);
			button.Height.Set(40f, 0f);
			column.Append(button);
		}
	}

	private static void AddNudgeButton(UIElement parent, string text, ref float left, System.Action onClick)
	{
		var button = new PracticeButton(text, 0.8f);
		button.Top.Set(140f, 0f);
		button.Left.Set(left, 0f);
		button.Width.Set(64f, 0f);
		button.Height.Set(32f, 0f);
		button.OnLeftClick += (_, _) => onClick();
		parent.Append(button);
		left += 70f;
	}

	private void SelectBoss(BossDefinition boss)
	{
		selectedBoss = boss;
		foreach (BossListEntry entry in bossEntries) {
			entry.Selected = entry.Boss == boss;
		}

		bossNameText.SetText(boss.DisplayName);
		notesText.SetText(boss.Notes ?? "");
		slider.Markers = boss.Phases.Select(phase => phase.HealthPercent).ToArray();

		phaseList.Clear();
		phaseButtons.Clear();
		for (int i = 0; i < boss.Phases.Count; i++) {
			int phaseIndex = i;
			BossPhase phase = boss.Phases[i];
			var button = new PracticeButton($"{i + 1}. {phase.Name}", 0.85f);
			button.Width.Set(0f, 1f);
			button.Height.Set(32f, 0f);
			button.OnLeftClick += (_, _) => SelectPhase(phaseIndex);
			phaseList.Add(button);
			phaseButtons.Add(button);
		}

		SelectPhase(0);
	}

	private void SelectPhase(int index)
	{
		selectedPhase = index;
		for (int i = 0; i < phaseButtons.Count; i++) {
			phaseButtons[i].Selected = i == index;
		}

		BossPhase phase = selectedBoss.Phases.ElementAtOrDefault(index);
		phaseDescriptionText.SetText(phase?.Description ?? "");
		SetHealth(phase?.HealthPercent ?? 1f);
	}

	private void SetHealth(float value)
	{
		slider.Value = value;
		RefreshHealthText();
	}

	private void RefreshHealthText()
	{
		healthText.SetText($"Starting health: {slider.Value * 100f:0}%");
	}

	private void Spawn()
	{
		if (selectedBoss == null) {
			return;
		}

		PracticeManager.RequestSpawn(selectedBoss, slider.Value, selectedPhase);
		PracticeUISystem.Hide();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (panel.ContainsPoint(Main.MouseScreen)) {
			Main.LocalPlayer.mouseInterface = true;
			PlayerInput.LockVanillaMouseScroll("BossPractice/PracticeMenu");
		}
	}
}
