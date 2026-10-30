using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using BossPractice.Common.Bosses;
using BossPractice.Common.Practice;

namespace BossPractice.Common.UI;

public sealed class PracticeMenuState : UIState
{
	private const float PanelWidth = 760f;
	private const float PanelHeight = 560f;
	private const float ListWidth = 250f;
	private const float RightColumnLeft = ListWidth + 32f;
	private const float TitleHeight = 44f;
	private const float TabHeight = 28f;
	private const float TabSpacing = 4f;
	private const float SearchHeight = 34f;
	private const string AllModsTab = "";

	private readonly List<BossListEntry> bossEntries = [];
	private readonly List<PracticeButton> phaseButtons = [];
	private readonly Dictionary<string, PracticeButton> modTabs = [];

	private UIPanel panel;
	private UIList bossList;
	private UIPanel searchPanel;
	private UISearchBar searchBar;
	private UIText bossNameText;
	private UIText sourceText;
	private UIText notesText;
	private UIText healthText;
	private HealthSlider slider;
	private UIList phaseList;
	private UIText phaseDescriptionText;

	private BossDefinition selectedBoss;
	private int selectedPhase = -1;
	private string modFilter = AllModsTab;
	private string searchText = "";
	private bool clickedSearchBar;
	private bool clickedElsewhere;

	public bool IsTyping => searchBar?.IsWritingText == true;

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

		float contentTop = BuildModTabs();
		BuildBossList(contentTop);
		BuildRightColumn(contentTop);

		if (BossRegistry.Bosses.Count > 0) {
			SelectBoss(BossRegistry.Bosses[0]);
		}
	}

	public override void OnActivate()
	{
		// Phase presets and notes can follow difficulty modes, which may have changed since the menu was last open.
		if (selectedBoss != null) {
			SelectBoss(selectedBoss);
		}
	}

	public override void OnDeactivate()
	{
		if (IsTyping) {
			searchBar.ToggleTakingText();
		}
	}

	/// <summary>Adds one tab per mod that has bosses, wrapping onto more rows as needed. Returns the y position below the tabs.</summary>
	private float BuildModTabs()
	{
		var tabs = new List<(string modName, string label)> { (AllModsTab, "All") };
		tabs.AddRange(BossRegistry.Bosses
			.Select(boss => (boss.ModName, boss.ModDisplayName))
			.Distinct());

		float innerWidth = PanelWidth - panel.PaddingLeft - panel.PaddingRight;
		float left = 0f;
		float top = TitleHeight;
		foreach ((string modName, string label) in tabs) {
			const float textScale = 0.8f;
			float width = FontAssets.MouseText.Value.MeasureString(label).X * textScale + 24f;
			if (left > 0f && left + width > innerWidth) {
				left = 0f;
				top += TabHeight + TabSpacing;
			}

			var tab = new PracticeButton(label, textScale);
			tab.Left.Set(left, 0f);
			tab.Top.Set(top, 0f);
			tab.Width.Set(width, 0f);
			tab.Height.Set(TabHeight, 0f);
			tab.OnLeftClick += (_, _) => SetModFilter(modName);
			panel.Append(tab);
			modTabs[modName] = tab;
			left += width + TabSpacing;
		}

		modTabs[AllModsTab].Selected = true;
		return top + TabHeight + 10f;
	}

	private void BuildBossList(float top)
	{
		searchPanel = new UIPanel {
			BackgroundColor = new Color(35, 40, 83),
			BorderColor = new Color(35, 40, 83),
		};
		searchPanel.SetPadding(0f);
		searchPanel.Top.Set(top, 0f);
		searchPanel.Width.Set(ListWidth, 0f);
		searchPanel.Height.Set(SearchHeight, 0f);
		searchPanel.OnLeftClick += (_, _) => {
			searchBar.ToggleTakingText();
			clickedSearchBar = true;
		};
		panel.Append(searchPanel);

		searchBar = new UISearchBar(ModContent.GetInstance<BossPracticeMod>().GetLocalization("UI.SearchHint"), 0.8f) {
			IgnoresMouseInteraction = true,
			VAlign = 0.5f,
		};
		searchBar.Left.Set(8f, 0f);
		searchBar.Width.Set(-16f, 1f);
		searchBar.Height.Set(0f, 1f);
		searchBar.OnContentsChanged += text => {
			searchText = BossDefinition.Normalize(text ?? "");
			RefreshBossList();
		};
		searchBar.OnStartTakingInput += () => searchPanel.BorderColor = Main.OurFavoriteColor;
		searchBar.OnEndTakingInput += () => searchPanel.BorderColor = new Color(35, 40, 83);
		searchPanel.Append(searchBar);

		float listTop = top + SearchHeight + 6f;
		bossList = new UIList {
			ListPadding = 4f,
		};
		bossList.Top.Set(listTop, 0f);
		bossList.Width.Set(ListWidth, 0f);
		bossList.Height.Set(-listTop, 1f);
		panel.Append(bossList);

		var scrollbar = new UIScrollbar();
		scrollbar.Left.Set(ListWidth + 4f, 0f);
		scrollbar.Top.Set(listTop + 6f, 0f);
		scrollbar.Height.Set(-listTop - 12f, 1f);
		panel.Append(scrollbar);
		bossList.SetScrollbar(scrollbar);

		// UIList sorts its items; keep progression order instead of alphabetical.
		bossList.ManualSortMethod = _ => { };
		foreach (BossDefinition boss in BossRegistry.Bosses) {
			var entry = new BossListEntry(boss);
			entry.OnLeftClick += (_, _) => SelectBoss(boss);
			bossEntries.Add(entry);
		}

		RefreshBossList();
	}

	private void BuildRightColumn(float top)
	{
		var column = new UIElement();
		column.Left.Set(RightColumnLeft, 0f);
		column.Top.Set(top, 0f);
		column.Width.Set(-RightColumnLeft, 1f);
		column.Height.Set(-top, 1f);
		panel.Append(column);

		bossNameText = new UIText("", 0.5f, large: true);
		column.Append(bossNameText);

		sourceText = new UIText("", 0.75f) {
			TextColor = new Color(180, 220, 255),
		};
		sourceText.Top.Set(30f, 0f);
		column.Append(sourceText);

		notesText = new UIText("", 0.8f) {
			IsWrapped = true,
			TextColor = new Color(200, 200, 200),
		};
		notesText.Top.Set(52f, 0f);
		notesText.Width.Set(0f, 1f);
		notesText.Height.Set(40f, 0f);
		column.Append(notesText);

		healthText = new UIText("");
		healthText.Top.Set(100f, 0f);
		column.Append(healthText);

		slider = new HealthSlider();
		slider.Top.Set(128f, 0f);
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
		phaseLabel.Top.Set(202f, 0f);
		column.Append(phaseLabel);

		phaseList = new UIList {
			ListPadding = 4f,
			ManualSortMethod = _ => { },
		};
		phaseList.Top.Set(228f, 0f);
		phaseList.Width.Set(-26f, 1f);
		phaseList.Height.Set(-228f - 96f, 1f);
		column.Append(phaseList);

		var phaseScrollbar = new UIScrollbar();
		phaseScrollbar.Left.Set(-20f, 1f);
		phaseScrollbar.Top.Set(228f + 6f, 0f);
		phaseScrollbar.Height.Set(-228f - 96f - 12f, 1f);
		column.Append(phaseScrollbar);
		phaseList.SetScrollbar(phaseScrollbar);

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
		button.Top.Set(158f, 0f);
		button.Left.Set(left, 0f);
		button.Width.Set(64f, 0f);
		button.Height.Set(32f, 0f);
		button.OnLeftClick += (_, _) => onClick();
		parent.Append(button);
		left += 70f;
	}

	private void SetModFilter(string modName)
	{
		modFilter = modName;
		foreach ((string tabMod, PracticeButton tab) in modTabs) {
			tab.Selected = tabMod == modName;
		}

		RefreshBossList();
	}

	private bool IsShown(BossDefinition boss)
	{
		if (modFilter != AllModsTab && boss.ModName != modFilter) {
			return false;
		}

		return searchText.Length == 0
			|| BossDefinition.Normalize(boss.DisplayName).Contains(searchText)
			|| BossDefinition.Normalize(boss.Name).Contains(searchText)
			|| boss.Aliases.Any(alias => BossDefinition.Normalize(alias).Contains(searchText));
	}

	/// <summary>Shows the bosses matching the mod tab and search text, still in progression order.</summary>
	private void RefreshBossList()
	{
		bossList.Clear();
		foreach (BossListEntry entry in bossEntries) {
			if (IsShown(entry.Boss)) {
				bossList.Add(entry);
			}
		}

		bossList.ViewPosition = 0f;
	}

	private void SelectBoss(BossDefinition boss)
	{
		selectedBoss = boss;
		foreach (BossListEntry entry in bossEntries) {
			entry.Selected = entry.Boss == boss;
		}

		PhaseVariant variant = boss.ActiveVariant;
		bossNameText.SetText(boss.DisplayName);
		sourceText.SetText(variant != null ? $"{boss.ModDisplayName} - {variant.Label} presets" : boss.ModDisplayName);
		notesText.SetText(boss.CurrentNotes ?? "");

		IReadOnlyList<BossPhase> phases = boss.Phases;
		slider.Markers = phases.Select(phase => phase.HealthPercent).ToArray();

		phaseList.Clear();
		phaseButtons.Clear();
		for (int i = 0; i < phases.Count; i++) {
			int phaseIndex = i;
			var button = new PracticeButton($"{i + 1}. {phases[i].Name}", 0.85f);
			button.Width.Set(0f, 1f);
			button.Height.Set(32f, 0f);
			button.OnLeftClick += (_, _) => SelectPhase(phaseIndex);
			phaseList.Add(button);
			phaseButtons.Add(button);
		}

		phaseList.ViewPosition = 0f;
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

	public override void LeftClick(UIMouseEvent evt)
	{
		base.LeftClick(evt);
		clickedElsewhere = true;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		// Clicking anywhere other than the search box stops typing, like the Bestiary's search.
		if (clickedElsewhere && !clickedSearchBar && IsTyping) {
			searchBar.ToggleTakingText();
		}

		clickedElsewhere = false;
		clickedSearchBar = false;

		if (panel.ContainsPoint(Main.MouseScreen)) {
			Main.LocalPlayer.mouseInterface = true;
			PlayerInput.LockVanillaMouseScroll("BossPractice/PracticeMenu");
		}
	}
}
