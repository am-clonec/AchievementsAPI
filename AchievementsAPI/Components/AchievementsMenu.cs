using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AchievementsAPI.API;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using static AchievementsAPI.MenuUi;

namespace AchievementsAPI
{
    [RegisterInIl2Cpp]
    public class AchievementsMenu(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public static AchievementsMenu? Instance { get; private set; }
        internal static bool IsOpen => Instance != null;

        public MainMenuManager mainMenuManager;
        public OptionsMenuBehaviour OptionsMenuBehaviour;
        public string FocusAchievementId;

        private static readonly Color32 SelectedColor = AccentHover;
        private static readonly Color32 HoverColor = new(200, 200, 200, 255);
        private static readonly Color32 PressedTint = new(150, 150, 150, 255);

        private RectTransform _panel = null!;
        private TextMeshProUGUI _title = null!;
        private Image _titleIcon = null!;
        private RectTransform _statsCard = null!;
        private RectTransform _filterRow = null!;
        private ScrollRect _scroll = null!;
        private RectTransform _listContent = null!;
        private GameObject _listScrollbar = null!;
        private RectTransform _categoryContent;
        private const int CategoryPageSize = 7;
        private RectTransform _categoryViewport;
        private GameObject _categoryPager;
        private Button _categoryPrevious;
        private Button _categoryNext;
        private TextMeshProUGUI _categoryPageLabel;
        private int _categoryPage;
        private GameObject _settingsPanel;
        private Button _settingsButton;
        private GameObject _detailPanel;
        private GameObject _dialog;
        private Action _closeDialog;
        private RectTransform _toolbar;
        private TMP_InputField _searchField;
        private Image _searchIcon;
        private Image _searchBox;
        private Image _modFilterIcon;
        private TextMeshProUGUI _modFilterLabel;
        private RectTransform _modFilterRect;
        private GameObject _modPopover;
        private RectTransform _modPopoverRect;
        private readonly List<(string mod, Button button)> _modRows = new();
        private List<AchievementsTab> _selectableTabs = new();
        private List<AchievementsTab> _mods = new();
        private string _currentMod;             // null = all mods
        private string _appliedSearch = string.Empty;
        private IEnumerator _searchDebounce;
        private List<AchievementCategory> _categories = new();
        private AchievementCategory _currentCategory;   // null = All
        private int _filter;                    // 0 All, 1 Completed, 2 In Progress, 3 Locked
        private readonly List<(Button button, TextMeshProUGUI label)> _filterTabs = new();
        private readonly List<(AchievementCategory category, Button button)> _categoryRows = new();
        private readonly List<ListRow> _rows = new();
        private readonly List<ListHeader> _headers = new();
        private GameObject _emptyState;
        private TextMeshProUGUI _emptyText;
        private int _rebuildGen;
        private bool _controllerMode;
        private float _navUnlockTime;
        private bool _restoreNavigationEvents;
        private Image _focusRing;
        private BaseAchievement _detailAchievement;
        private GameObject _lastSelected;
        private Vector3 _lastMousePos;
        private bool _mousePrimed;
        private readonly Il2CppStructArray<Vector3> _itemCorners = new(4);
        private readonly Il2CppStructArray<Vector3> _viewCorners = new(4);

        private sealed class ListRow
        {
            public GameObject Root = null!;
            public Image Card = null!;
            public Image Rim = null!;
            public Image Ring = null!;
            public Image Icon = null!;
            public Image SubIcon = null!;
            public TextMeshProUGUI NameText = null!;
            public TextMeshProUGUI DescText = null!;
            public Image Fill = null!;
            public RectTransform FillRect = null!;
            public TextMeshProUGUI ProgressLabel = null!;
            public Image Pill = null!;
            public TextMeshProUGUI PillText = null!;
            public GameObject Badge = null!;
            public TextMeshProUGUI BadgeText = null!;
            public Button Button = null!;
            public BaseAchievement Achievement = null!;
            public int FillGen;
        }

        private sealed class ListHeader
        {
            public GameObject Root = null!;
            public Image Icon = null!;
            public TextMeshProUGUI NameText = null!;
        }

        private void Awake() => Instance = this;

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (EventSystem.current != null) EventSystem.current.sendNavigationEvents = _restoreNavigationEvents;
        }

        private void Start()
        {
            _selectableTabs = AchievementsManager.Tabs.Where(t => t.IsSelectable).ToList();
            _mods = _selectableTabs.GroupBy(t => t.ModName).Select(g => g.First()).ToList();
            foreach (var tab in _selectableTabs) AchievementStorage.AchievementStorageGet(tab);

            // Only categories that actually contain something, in declared order.
            _categories = _selectableTabs
                .SelectMany(tab => tab.GetAchievements().Select(a => AchievementsManager.ResolveCategory(tab, a)))
                .Distinct()
                .OrderBy(c => c.Order)
                .ThenBy(c => AchievementsManager.Categories.IndexOf(c))
                .ToList();

            var focused = FocusAchievementId != null ? AchievementsManager.FindAchievement(FocusAchievementId) : null;
            _currentCategory = _categories.Count == 1 ? _categories[0]
                : focused is { } f && f.Tab.IsSelectable ? AchievementsManager.ResolveCategory(f.Tab, f.Achievement) : null;

            BuildChrome();
            _settingsPanel = AchievementsSettingsPanel.Create(_panel, ShowResetAllDialog);
            RebuildStatsCard();
            RebuildCategories();
            RebuildList();
            SetInitialTitle();

            if (mainMenuManager)
            {
                mainMenuManager.DeactivateMainMenuUI();
            }
            else if (OptionsMenuBehaviour)
            {
                OptionsMenuBehaviour.gameObject.SetActive(false);
                OptionsMenuBehaviour.Background.enabled = false;
            }

            if (focused != null && focused.Value.Tab.IsSelectable)
            {
                ShowDetail(focused.Value.Achievement);
            }

            _navUnlockTime = Time.unscaledTime + 0.1f;

            if (EventSystem.current != null)
            {
                _restoreNavigationEvents = EventSystem.current.sendNavigationEvents;
                EventSystem.current.sendNavigationEvents = false;
            }
        }

        // Scrim, panel, close button, title, filter tabs, left column and the
        // achievement list scroll. Everything below the panel is anchor-based.
        private void BuildChrome()
        {
            var scrim = CreateImage(transform, "Scrim", null, new Color(0f, 0f, 0f, 0f));
            var scrimRect = scrim.rectTransform;
            scrimRect.anchorMin = Vector2.zero;
            scrimRect.anchorMax = Vector2.one;
            scrimRect.offsetMin = Vector2.zero;
            scrimRect.offsetMax = Vector2.zero;

            var panelImg = CreateImage(transform, "Panel", ToastSprites.WikiPanel, Color.white);
            panelImg.type = Image.Type.Sliced;
            panelImg.color = new Color(0.68f, 0.68f, 0.68f, 1f);
            _panel = panelImg.rectTransform;
            _panel.anchorMin = _panel.anchorMax = new Vector2(0.5f, 0.5f);
            _panel.sizeDelta = new Vector2(1560f, 960f);
            _panel.localScale = new Vector3(0.92f, 0.92f, 1f);
            var group = _panel.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            Coroutines.Start(CoPopIn(_panel, group, scrim));

            // Top-left X like the game's options window
            var closeImg = CreateImage(_panel, "Close", ToastSprites.CloseX, Color.white);
            closeImg.sprite = CloseButtonSprite ?? ToastSprites.CloseX;
            var closeRect = closeImg.rectTransform;
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(0f, 1f);
            closeRect.pivot = new Vector2(0.5f, 0.5f);
            closeRect.anchoredPosition = new Vector2(-14f, -48f);
            closeRect.sizeDelta = new Vector2(64f, 64f);
            var close = closeImg.gameObject.AddComponent<Button>();
            close.transition = Selectable.Transition.ColorTint;
            var closeColors = close.colors;
            closeColors.normalColor = Color.white;
            closeColors.highlightedColor = new Color(0.85f, 0.85f, 0.85f);
            closeColors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            closeColors.selectedColor = Color.white;
            close.colors = closeColors;
            close.onClick.AddListener((UnityAction)Close);
            AttachGameSounds(closeImg.gameObject);

            // Wiki-style title: page icon left, title text next to it
            _titleIcon = CreateImage(_panel, "TitleIcon", null, Color.white);
            _titleIcon.preserveAspect = true;
            _titleIcon.raycastTarget = false;
            _titleIcon.enabled = false;
            var iconRect = _titleIcon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 1f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(108f, -70f);
            iconRect.sizeDelta = new Vector2(38f, 38f);

            _title = CreateText(_panel, "Title", AchievementsStrings.GetText(AchievementsStrings.MenuTitle),
                38f, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);
            _title.overflowMode = TextOverflowModes.Overflow;
            var titleRect = _title.rectTransform;
            titleRect.anchorMin = titleRect.anchorMax = new Vector2(0f, 1f);
            titleRect.pivot = new Vector2(0f, 1f);
            titleRect.anchoredPosition = new Vector2(134f, -44f);
            titleRect.sizeDelta = new Vector2(900f, 52f);

            // Filter pills centered over the right column (x 430..1436 -> 183)
            var rowGo = new GameObject("FilterRow");
            _filterRow = rowGo.AddComponent<RectTransform>();
            _filterRow.SetParent(_panel, false);
            _filterRow.anchorMin = _filterRow.anchorMax = new Vector2(0.5f, 1f);
            _filterRow.pivot = new Vector2(0.5f, 0.5f);
            _filterRow.anchoredPosition = new Vector2(183f, -70f);
            _filterRow.sizeDelta = new Vector2(836f, 50f);
            var names = FilterNames();
            for (var i = 0; i < names.Length; i++)
            {
                var pill = CreateImage(_filterRow, names[i], ToastSprites.ButtonBox, Color.white);
                pill.type = Image.Type.Sliced;
                var pillRect = pill.rectTransform;
                pillRect.anchorMin = pillRect.anchorMax = new Vector2(0.5f, 0.5f);
                pillRect.pivot = new Vector2(0.5f, 0.5f);
                pillRect.anchoredPosition = new Vector2((i - 1.5f) * 212f, 0f);
                pillRect.sizeDelta = new Vector2(200f, 50f);
                var label = CreateText(pillRect, "Label", names[i], 22f, Color.white,
                    FontStyles.Bold, TextAlignmentOptions.Center);
                label.rectTransform.anchorMin = Vector2.zero;
                label.rectTransform.anchorMax = Vector2.one;
                label.rectTransform.offsetMin = Vector2.zero;
                label.rectTransform.offsetMax = Vector2.zero;
                var button = pill.gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.ColorTint;
                var colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = HoverColor;
                colors.pressedColor = PressedTint;
                colors.selectedColor = Color.white;
                colors.disabledColor = SelectedColor;
                button.colors = colors;
                var index = i;
                button.onClick.AddListener((UnityAction)(() => SetFilter(index)));
                AttachGameSounds(pill.gameObject);
                _filterTabs.Add((button, label));
            }

            // Toolbar row under the pills: search field on the left, mod
            // filter dropdown on the right (only when 2+ mods contribute)
            var modFilterShown = _mods.Count >= 2;
            var toolbarGo = new GameObject("Toolbar");
            _toolbar = toolbarGo.AddComponent<RectTransform>();
            _toolbar.SetParent(_panel, false);
            _toolbar.anchorMin = new Vector2(0f, 1f);
            _toolbar.anchorMax = new Vector2(1f, 1f);
            _toolbar.pivot = new Vector2(0.5f, 1f);
            _toolbar.offsetMin = new Vector2(450f, -186f);
            _toolbar.offsetMax = new Vector2(-84f, -136f);

            _searchBox = CreateImage(_toolbar, "SearchIconBox", ToastSprites.ButtonBox, Color.white);
            _searchBox.type = Image.Type.Sliced;
            var searchBoxRect = _searchBox.rectTransform;
            searchBoxRect.anchorMin = searchBoxRect.anchorMax = new Vector2(0f, 0.5f);
            searchBoxRect.pivot = new Vector2(0f, 0.5f);
            searchBoxRect.anchoredPosition = Vector2.zero;
            searchBoxRect.sizeDelta = new Vector2(50f, 50f);

            _searchIcon = CreateImage(searchBoxRect, "SearchIcon", SearchGlyph, Color.white);
            _searchIcon.preserveAspect = true;
            _searchIcon.raycastTarget = false;
            // Tints the PNG's flat grey box to exactly the ButtonBox colour so its
            // square edges vanish into the rounded box beneath.
            _searchIcon.color = new Color(61f / 79f, 64f / 82f, 64f / 82f, 1f);
            var searchIconRect = _searchIcon.rectTransform;
            searchIconRect.anchorMin = searchIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            searchIconRect.pivot = new Vector2(0.5f, 0.5f);
            searchIconRect.anchoredPosition = Vector2.zero;
            searchIconRect.sizeDelta = new Vector2(40f, 40f);
            var searchButton = searchBoxRect.gameObject.AddComponent<Button>();
            searchButton.transition = Selectable.Transition.ColorTint;
            var searchColors = searchButton.colors;
            searchColors.normalColor = Color.white;
            searchColors.highlightedColor = HoverColor;
            searchColors.pressedColor = PressedTint;
            searchColors.selectedColor = Color.white;
            searchButton.colors = searchColors;
            UpdateSearchIcon();
            searchButton.onClick.AddListener((UnityAction)(() =>
            {
                if (SearchText.Length > 0) ClearSearch();
                else _searchField.ActivateInputField();
            }));
            AttachGameSounds(_searchBox.gameObject);

            var fieldBg = CreateImage(_toolbar, "SearchField", ToastSprites.ButtonBox, Color.white);
            fieldBg.type = Image.Type.Sliced;
            var fieldRect = fieldBg.rectTransform;
            fieldRect.anchorMin = new Vector2(0f, 0f);
            fieldRect.anchorMax = new Vector2(1f, 1f);
            fieldRect.offsetMin = new Vector2(58f, 0f);
            fieldRect.offsetMax = new Vector2(modFilterShown ? -268f : 0f, 0f);

            var viewportGo = new GameObject("Viewport");
            var viewportRect = viewportGo.AddComponent<RectTransform>();
            viewportRect.SetParent(fieldRect, false);
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(16f, 0f);
            viewportRect.offsetMax = new Vector2(-16f, 0f);
            viewportGo.AddComponent<RectMask2D>();

            var searchText = CreateText(viewportRect, "Text", "", 22f, new Color(0.85f, 0.85f, 0.85f),
                FontStyles.Normal, TextAlignmentOptions.Left);
            searchText.overflowMode = TextOverflowModes.Overflow;
            searchText.enableWordWrapping = false;
            searchText.richText = false;
            searchText.enableAutoSizing = false;
            searchText.fontSizeMin = 22f;
            searchText.fontSizeMax = 22f;
            searchText.autoSizeTextContainer = false;
            var searchTextRect = searchText.rectTransform;
            searchTextRect.anchorMin = searchTextRect.anchorMax = new Vector2(0f, 0.5f);
            searchTextRect.pivot = new Vector2(0f, 0.5f);
            searchTextRect.anchoredPosition = Vector2.zero;
            searchTextRect.sizeDelta = new Vector2(2000f, 50f);

            var placeholder = CreateText(viewportRect, "Placeholder",
                AchievementsStrings.GetText(AchievementsStrings.SearchPlaceholder),
                22f, new Color(0.55f, 0.55f, 0.55f), FontStyles.Italic, TextAlignmentOptions.Left);
            placeholder.overflowMode = TextOverflowModes.Overflow;
            placeholder.enableWordWrapping = false;
            placeholder.enableAutoSizing = false;
            placeholder.autoSizeTextContainer = false;
            var placeholderRect = placeholder.rectTransform;
            placeholderRect.anchorMin = placeholderRect.anchorMax = new Vector2(0f, 0.5f);
            placeholderRect.pivot = new Vector2(0f, 0.5f);
            placeholderRect.anchoredPosition = Vector2.zero;
            placeholderRect.sizeDelta = new Vector2(2000f, 50f);

            // Deactivate so TMP_InputField.OnEnable runs only after textComponent is assigned below —
            // the caret renderer is only created in OnEnable when m_TextComponent is already set.
            fieldBg.gameObject.SetActive(false);
            _searchField = fieldBg.gameObject.AddComponent<TMP_InputField>();
            _searchField.textViewport = viewportRect;
            _searchField.textComponent = searchText;
            _searchField.placeholder = placeholder;
            _searchField.lineType = TMP_InputField.LineType.SingleLine;
            _searchField.characterLimit = 40;
            _searchField.richText = false;
            _searchField.customCaretColor = true;
            _searchField.caretColor = Color.white;
            _searchField.selectionColor = new Color(
                AccentHover.r / 255f, AccentHover.g / 255f, AccentHover.b / 255f, 0.5f);
            _searchField.caretWidth = 2;
            _searchField.restoreOriginalTextOnEscape = false;
            _searchField.transition = Selectable.Transition.None;
            _searchField.onValueChanged.AddListener((UnityAction<string>)(_ => QueueSearchRefresh()));
            fieldBg.gameObject.SetActive(true);
            AttachGameSounds(fieldBg.gameObject, hoverSound: false);

            if (modFilterShown)
            {
                var modFilter = CreateImage(_toolbar, "ModFilter", ToastSprites.ButtonBox, Color.white);
                modFilter.type = Image.Type.Sliced;
                _modFilterRect = modFilter.rectTransform;
                _modFilterRect.anchorMin = _modFilterRect.anchorMax = new Vector2(1f, 0.5f);
                _modFilterRect.pivot = new Vector2(1f, 0.5f);
                _modFilterRect.anchoredPosition = Vector2.zero;
                _modFilterRect.sizeDelta = new Vector2(260f, 50f);

                _modFilterIcon = CreateImage(_modFilterRect, "Icon", AchievementIcon, Color.white);
                _modFilterIcon.preserveAspect = true;
                _modFilterIcon.raycastTarget = false;
                var modIconRect = _modFilterIcon.rectTransform;
                modIconRect.anchorMin = modIconRect.anchorMax = new Vector2(0f, 0.5f);
                modIconRect.pivot = new Vector2(0.5f, 0.5f);
                modIconRect.anchoredPosition = new Vector2(28f, 0f);
                modIconRect.sizeDelta = new Vector2(28f, 28f);

                _modFilterLabel = CreateText(_modFilterRect, "Label",
                    AchievementsStrings.GetText(AchievementsStrings.FilterAllMods),
                    20f, Color.white, FontStyles.Bold);
                var modLabelRect = _modFilterLabel.rectTransform;
                modLabelRect.anchorMin = Vector2.zero;
                modLabelRect.anchorMax = Vector2.one;
                modLabelRect.offsetMin = new Vector2(52f, 0f);
                modLabelRect.offsetMax = new Vector2(-36f, 0f);

                var chevron = CreateText(_modFilterRect, "Chevron", "▼", 14f, ValueColor,
                    FontStyles.Normal, TextAlignmentOptions.Center);
                var chevronRect = chevron.rectTransform;
                chevronRect.anchorMin = chevronRect.anchorMax = new Vector2(1f, 0.5f);
                chevronRect.pivot = new Vector2(1f, 0.5f);
                chevronRect.anchoredPosition = new Vector2(-14f, 0f);
                chevronRect.sizeDelta = new Vector2(20f, 20f);

                var modFilterButton = modFilter.gameObject.AddComponent<Button>();
                modFilterButton.transition = Selectable.Transition.ColorTint;
                var modFilterColors = modFilterButton.colors;
                modFilterColors.normalColor = Color.white;
                modFilterColors.highlightedColor = HoverColor;
                modFilterColors.pressedColor = PressedTint;
                modFilterColors.selectedColor = Color.white;
                modFilterButton.colors = modFilterColors;
                modFilterButton.onClick.AddListener((UnityAction)ToggleModPopover);
                AttachGameSounds(modFilter.gameObject);
                UpdateModFilterButton();

                var pop = CreateImage(_panel, "ModPopover", ToastSprites.ButtonBox, Color.white);
                pop.type = Image.Type.Sliced;
                _modPopoverRect = pop.rectTransform;
                _modPopoverRect.anchorMin = _modPopoverRect.anchorMax = new Vector2(1f, 1f);
                _modPopoverRect.pivot = new Vector2(1f, 1f);
                _modPopoverRect.anchoredPosition = new Vector2(-84f, -190f);
                _modPopoverRect.sizeDelta = new Vector2(300f, 12f + (_mods.Count + 1) * 50f);
                _modPopover = pop.gameObject;

                var popContentGo = new GameObject("Content");
                var popContent = popContentGo.AddComponent<RectTransform>();
                popContent.SetParent(_modPopoverRect, false);
                popContent.anchorMin = Vector2.zero;
                popContent.anchorMax = Vector2.one;
                popContent.offsetMin = Vector2.zero;
                popContent.offsetMax = Vector2.zero;
                var popVlg = popContentGo.AddComponent<VerticalLayoutGroup>();
                popVlg.padding = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 };
                popVlg.spacing = 6f;
                popVlg.childControlWidth = true;
                popVlg.childControlHeight = true;
                popVlg.childForceExpandWidth = true;
                popVlg.childForceExpandHeight = false;

                AddModRow(popContent, null);
                foreach (var modTab in _mods) AddModRow(popContent, modTab);
                _modPopover.SetActive(false);
            }

            var leftGo = new GameObject("LeftColumn");
            var left = leftGo.AddComponent<RectTransform>();
            left.SetParent(_panel, false);
            left.anchorMin = new Vector2(0f, 0f);
            left.anchorMax = new Vector2(0f, 1f);
            left.offsetMin = new Vector2(60f, 60f);
            left.offsetMax = new Vector2(420f, -126f);

            var stats = CreateImage(left, "StatsCard", ToastSprites.ButtonBox, Color.white);
            stats.type = Image.Type.Sliced;
            stats.raycastTarget = false;
            _statsCard = stats.rectTransform;
            _statsCard.anchorMin = new Vector2(0f, 1f);
            _statsCard.anchorMax = new Vector2(1f, 1f);
            _statsCard.offsetMin = new Vector2(0f, -220f);
            _statsCard.offsetMax = new Vector2(0f, 0f);

            if (_categories.Count >= 2)
            {
                var catGo = new GameObject("CategoryScroll");
                _categoryViewport = catGo.AddComponent<RectTransform>();
                _categoryViewport.SetParent(left, false);
                _categoryViewport.anchorMin = Vector2.zero;
                _categoryViewport.anchorMax = Vector2.one;
                _categoryViewport.offsetMin = new Vector2(0f, 68f);
                _categoryViewport.offsetMax = new Vector2(-22f, -236f);
                var catScroll = catGo.AddComponent<ScrollRect>();
                catGo.AddComponent<RectMask2D>();
                var catRaycast = CreateImage(_categoryViewport, "Raycast", null, new Color(0f, 0f, 0f, 0f));
                catRaycast.rectTransform.anchorMin = Vector2.zero;
                catRaycast.rectTransform.anchorMax = Vector2.one;
                catRaycast.rectTransform.offsetMin = Vector2.zero;
                catRaycast.rectTransform.offsetMax = Vector2.zero;

                var catContentGo = new GameObject("Content");
                _categoryContent = catContentGo.AddComponent<RectTransform>();
                _categoryContent.SetParent(_categoryViewport, false);
                _categoryContent.anchorMin = new Vector2(0f, 1f);
                _categoryContent.anchorMax = new Vector2(1f, 1f);
                _categoryContent.pivot = new Vector2(0.5f, 1f);
                _categoryContent.offsetMin = Vector2.zero;
                _categoryContent.offsetMax = Vector2.zero;
                var catVlg = catContentGo.AddComponent<VerticalLayoutGroup>();
                catVlg.spacing = 8f;
                catVlg.childControlWidth = true;
                catVlg.childControlHeight = true;
                catVlg.childForceExpandWidth = true;
                catVlg.childForceExpandHeight = false;
                var catFitter = catContentGo.AddComponent<ContentSizeFitter>();
                catFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                catScroll.viewport = _categoryViewport;
                catScroll.content = _categoryContent;
                catScroll.horizontal = false;
                catScroll.vertical = false;
                catScroll.movementType = ScrollRect.MovementType.Clamped;
                catScroll.scrollSensitivity = 56f;

                _categoryPager = new GameObject("CategoryPager");
                var pagerRect = _categoryPager.AddComponent<RectTransform>();
                pagerRect.SetParent(left, false);
                pagerRect.anchorMin = new Vector2(0f, 0f);
                pagerRect.anchorMax = new Vector2(1f, 0f);
                pagerRect.offsetMin = new Vector2(0f, 64f);
                pagerRect.offsetMax = new Vector2(0f, 108f);

                _categoryPrevious = CreatePagerArrow(pagerRect, "Previous", "<", new Vector2(-58f, 0f));
                _categoryPrevious.onClick.AddListener((UnityAction)(() => ChangeCategoryPage(-1)));
                _categoryNext = CreatePagerArrow(pagerRect, "Next", ">", new Vector2(58f, 0f));
                _categoryNext.onClick.AddListener((UnityAction)(() => ChangeCategoryPage(1)));

                _categoryPageLabel = CreateText(pagerRect, "PageLabel", "1 / 1", 18f, ValueColor,
                    FontStyles.Normal, TextAlignmentOptions.Center);
                var pageLabelRect = _categoryPageLabel.rectTransform;
                pageLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
                pageLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
                pageLabelRect.pivot = new Vector2(0.5f, 0.5f);
                pageLabelRect.anchoredPosition = Vector2.zero;
                pageLabelRect.sizeDelta = new Vector2(70f, 40f);

                _categoryPager.SetActive(false);
            }

            var settingsImg = CreateImage(left, "SettingsButton", ToastSprites.ButtonBox, Color.white);
            settingsImg.type = Image.Type.Sliced;
            var settingsRect = settingsImg.rectTransform;
            settingsRect.anchorMin = new Vector2(0f, 0f);
            settingsRect.anchorMax = new Vector2(1f, 0f);
            settingsRect.offsetMin = Vector2.zero;
            settingsRect.offsetMax = new Vector2(0f, 52f);
            var gear = CreateImage(settingsRect, "Gear", ToastSprites.Gear, Color.white);
            gear.raycastTarget = false;
            var gearRect = gear.rectTransform;
            gearRect.anchorMin = gearRect.anchorMax = new Vector2(0f, 0.5f);
            gearRect.pivot = new Vector2(0.5f, 0.5f);
            gearRect.anchoredPosition = new Vector2(36f, 0f);
            gearRect.sizeDelta = new Vector2(28f, 28f);
            var settingsLabel = CreateText(settingsRect, "Label", AchievementsStrings.GetText(AchievementsStrings.Settings),
                22f, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            settingsLabel.rectTransform.anchorMin = Vector2.zero;
            settingsLabel.rectTransform.anchorMax = Vector2.one;
            settingsLabel.rectTransform.offsetMin = new Vector2(34f, 0f);
            settingsLabel.rectTransform.offsetMax = Vector2.zero;
            // Behaves like a category row: disabled (teal) while its page is open
            _settingsButton = settingsImg.gameObject.AddComponent<Button>();
            _settingsButton.transition = Selectable.Transition.ColorTint;
            var settingsColors = _settingsButton.colors;
            settingsColors.normalColor = Color.white;
            settingsColors.highlightedColor = HoverColor;
            settingsColors.pressedColor = PressedTint;
            settingsColors.selectedColor = Color.white;
            settingsColors.disabledColor = SelectedColor;
            _settingsButton.colors = settingsColors;
            _settingsButton.onClick.AddListener((UnityAction)ShowSettingsPage);
            AttachGameSounds(settingsImg.gameObject);

            var scrollGo = new GameObject("ListScroll");
            var scrollRect = scrollGo.AddComponent<RectTransform>();
            scrollRect.SetParent(_panel, false);
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(450f, 60f);
            scrollRect.offsetMax = new Vector2(-84f, -196f);
            _scroll = scrollGo.AddComponent<ScrollRect>();
            scrollGo.AddComponent<RectMask2D>();
            var scrollRaycast = CreateImage(scrollRect, "Raycast", null, new Color(0f, 0f, 0f, 0f));
            scrollRaycast.rectTransform.anchorMin = Vector2.zero;
            scrollRaycast.rectTransform.anchorMax = Vector2.one;
            scrollRaycast.rectTransform.offsetMin = Vector2.zero;
            scrollRaycast.rectTransform.offsetMax = Vector2.zero;

            var contentGo = new GameObject("Content");
            _listContent = contentGo.AddComponent<RectTransform>();
            _listContent.SetParent(scrollRect, false);
            _listContent.anchorMin = new Vector2(0f, 1f);
            _listContent.anchorMax = new Vector2(1f, 1f);
            _listContent.pivot = new Vector2(0.5f, 1f);
            _listContent.offsetMin = Vector2.zero;
            _listContent.offsetMax = Vector2.zero;
            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            var fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            _scroll.viewport = scrollRect;
            _scroll.content = _listContent;
            _scroll.horizontal = false;
            _scroll.movementType = ScrollRect.MovementType.Clamped;
            _scroll.scrollSensitivity = 100f;
            _listScrollbar = CreateScrollbar(_panel, _scroll, new Vector2(-76f, 64f), new Vector2(-62f, -192f));

            _focusRing = CreateImage(transform, "FocusRing", ToastSprites.OutlineBoxSmall, AccentHover);
            _focusRing.type = Image.Type.Sliced;
            _focusRing.raycastTarget = false;
            var ringRect = _focusRing.rectTransform;
            ringRect.anchorMin = ringRect.anchorMax = new Vector2(0.5f, 0.5f);
            ringRect.pivot = new Vector2(0.5f, 0.5f);
            ringRect.SetAsLastSibling();
            _focusRing.gameObject.SetActive(false);
        }

        // Wiki-scroller-style vertical scrollbar: dark translucent track with a
        // light handle that brightens on hover. ScrollRect drives the handle;
        // it auto-hides when the content fits the viewport.
        private static GameObject CreateScrollbar(Transform parent, ScrollRect scroll,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var track = CreateImage(parent, "Scrollbar", ToastSprites.RoundedRectTiny,
                new Color(0f, 0f, 0f, 0.35f));
            track.type = Image.Type.Sliced;
            track.raycastTarget = false;
            var trackRect = track.rectTransform;
            trackRect.anchorMin = new Vector2(1f, 0f);
            trackRect.anchorMax = new Vector2(1f, 1f);
            trackRect.offsetMin = offsetMin;
            trackRect.offsetMax = offsetMax;

            var slideGo = new GameObject("Sliding Area");
            var slideRect = slideGo.AddComponent<RectTransform>();
            slideRect.SetParent(trackRect, false);
            slideRect.anchorMin = Vector2.zero;
            slideRect.anchorMax = Vector2.one;
            slideRect.offsetMin = new Vector2(2f, 3f);
            slideRect.offsetMax = new Vector2(-2f, -3f);

            var handle = CreateImage(slideRect, "Handle", ToastSprites.RoundedRectTiny, Color.white);
            handle.type = Image.Type.Sliced;
            var handleRect = handle.rectTransform;
            handleRect.anchorMin = new Vector2(0f, 0f);
            handleRect.anchorMax = new Vector2(1f, 0f);
            handleRect.pivot = new Vector2(0.5f, 0f);
            handleRect.sizeDelta = new Vector2(0f, 60f);

            // UnityEngine.UI.Scrollbar: a plain `Scrollbar` here resolves to
            // the game's own scrollbar class
            var bar = track.gameObject.AddComponent<UnityEngine.UI.Scrollbar>();
            bar.navigation = new Navigation { mode = Navigation.Mode.None };
            bar.handleRect = handleRect;
            bar.targetGraphic = handle;
            bar.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
            bar.transition = Selectable.Transition.ColorTint;
            var colors = bar.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0.75f);
            colors.highlightedColor = Color.white;
            colors.pressedColor = AccentHover;
            colors.selectedColor = new Color(1f, 1f, 1f, 0.75f);
            colors.fadeDuration = 0.1f;
            bar.colors = colors;

            scroll.verticalScrollbar = bar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            return track.gameObject;
        }

        [HideFromIl2Cpp]
        private Button CreatePagerArrow(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            var img = CreateImage(parent, name, ToastSprites.ButtonBox, Color.white);
            img.type = Image.Type.Sliced;
            var rect = img.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(44f, 40f);

            var text = CreateText(rect, "Label", label, 22f, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;

            var button = img.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = HoverColor;
            colors.pressedColor = PressedTint;
            colors.selectedColor = Color.white;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            AttachGameSounds(img.gameObject);
            return button;
        }

        [HideFromIl2Cpp]
        private IEnumerable<(AchievementsTab Tab, BaseAchievement Achievement)> EnumerateAchievements()
        {
            foreach (var tab in _selectableTabs)
            {
                foreach (var a in tab.GetAchievements())
                {
                    if ((_currentMod == null || tab.ModName == _currentMod)
                        && (_currentCategory == null || AchievementsManager.ResolveCategory(tab, a) == _currentCategory))
                        yield return (tab, a);
                }
            }
        }

        private static bool InProgress(BaseAchievement a)
            => a is CountAchievement c && !a.Unlocked && c.CurrentValue > 0 && !(c.HideProgress && c.Hidden);

        private static bool PassesFilter(BaseAchievement a, int filter) => filter switch
        {
            1 => a.Unlocked,
            2 => InProgress(a),
            3 => !a.Unlocked,
            _ => true
        };

        private static int SortGroup(BaseAchievement a) => a.Unlocked ? 0 : InProgress(a) ? 1 : 2;

        private static float Fraction(BaseAchievement a)
            => a is CountAchievement c && c.RequiredValue > 0
                ? Mathf.Clamp01((float) c.CurrentValue / c.RequiredValue) : 0f;

        // The list filter labels ("All", "Completed", ...) in the current language.
        private static string[] FilterNames() => new[]
        {
            AchievementsStrings.FilterAll,
            AchievementsStrings.FilterCompleted,
            AchievementsStrings.FilterInProgress,
            AchievementsStrings.FilterLocked
        }.Select(AchievementsStrings.GetText).ToArray();

        private void RebuildStatsCard()
        {
            ClearChildren(_statsCard);
            var all = EnumerateAchievements().Select(x => x.Achievement).ToList();
            var earned = all.Where(a => a.Unlocked).Sum(a => a.Points);
            var total = all.Sum(a => a.Points);
            var done = all.Count(a => a.Unlocked);

            // Row 1: gold star, "Skeld Points" caption over the big earned/total
            var emblem = CreateScoreEmblem(_statsCard, 64f);
            emblem.anchorMin = emblem.anchorMax = new Vector2(0f, 1f);
            emblem.pivot = new Vector2(0f, 1f);
            emblem.anchoredPosition = new Vector2(16f, -28f);

            var cap = CreateText(_statsCard, "PointsLabel", AchievementsStrings.GetText(AchievementsStrings.SkeldPoints), 17f, ValueColor, FontStyles.Bold);
            cap.overflowMode = TextOverflowModes.Overflow;
            var capRect = cap.rectTransform;
            capRect.anchorMin = new Vector2(0f, 1f);
            capRect.anchorMax = new Vector2(1f, 1f);
            capRect.offsetMin = new Vector2(94f, -44f);
            capRect.offsetMax = new Vector2(-16f, -20f);

            // Two texts bottom-aligned in a layout row so the small "/ total"
            // sits on the big number's baseline instead of floating mid-line
            var pointsRowGo = new GameObject("Points");
            var pointsRow = pointsRowGo.AddComponent<RectTransform>();
            pointsRow.SetParent(_statsCard, false);
            pointsRow.anchorMin = new Vector2(0f, 1f);
            pointsRow.anchorMax = new Vector2(1f, 1f);
            pointsRow.offsetMin = new Vector2(94f, -92f);
            pointsRow.offsetMax = new Vector2(-16f, -44f);
            var pointsHlg = pointsRowGo.AddComponent<HorizontalLayoutGroup>();
            pointsHlg.spacing = 8f;
            pointsHlg.childAlignment = TextAnchor.LowerLeft;
            pointsHlg.childControlWidth = true;
            pointsHlg.childControlHeight = true;
            pointsHlg.childForceExpandWidth = false;
            pointsHlg.childForceExpandHeight = false;
            // TMP's Ellipsis mode drops a single line taller than its rect,
            // which hid the number entirely; Overflow keeps it visible
            var earnedText = CreateText(pointsRow, "Earned", earned.ToString(), 36f, Color.white,
                FontStyles.Bold, TextAlignmentOptions.BottomLeft);
            earnedText.overflowMode = TextOverflowModes.Overflow;
            var totalText = CreateText(pointsRow, "Total", $"/ {total}", 22f, ValueColor,
                FontStyles.Normal, TextAlignmentOptions.BottomLeft);
            totalText.overflowMode = TextOverflowModes.Overflow;
            totalText.margin = new Vector4(0f, 0f, 0f, 6f);

            // Row 2: completed count with an animated bar
            var completed = CreateText(_statsCard, "Completed", AchievementsStrings.GetText(AchievementsStrings.FilterCompleted), 20f, Color.white);
            var completedRect = completed.rectTransform;
            completedRect.anchorMin = new Vector2(0f, 1f);
            completedRect.anchorMax = new Vector2(1f, 1f);
            completedRect.offsetMin = new Vector2(20f, -130f);
            completedRect.offsetMax = new Vector2(-20f, -104f);

            var completedValue = CreateText(_statsCard, "CompletedValue", $"{done}/{all.Count}",
                20f, ValueColor, FontStyles.Normal, TextAlignmentOptions.MidlineRight);
            var cvRect = completedValue.rectTransform;
            cvRect.anchorMin = new Vector2(0f, 1f);
            cvRect.anchorMax = new Vector2(1f, 1f);
            cvRect.offsetMin = new Vector2(20f, -130f);
            cvRect.offsetMax = new Vector2(-20f, -104f);

            var bar = CreateProgressBar(_statsCard, AccentHover, out var fill);
            bar.anchorMin = new Vector2(0f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.offsetMin = new Vector2(20f, -144f);
            bar.offsetMax = new Vector2(-20f, -134f);
            Coroutines.Start(MenuUi.CoFill(fill, all.Count > 0 ? (float) done / all.Count : 0f, 0f, 0.5f));

            // Row 3: per-rarity completion as a 2x2 grid of labeled chips
            // (legendary, epic, rare, common; rarities with none are skipped)
            var chips = new List<(int rarity, int done, int total)>();
            for (var rarity = 3; rarity >= 0; rarity--)
            {
                var items = all.Where(a => a.Rarity == rarity).ToList();
                if (items.Count == 0) continue;
                chips.Add((rarity, items.Count(a => a.Unlocked), items.Count));
            }

            for (var i = 0; i < chips.Count; i++)
            {
                var x = 20f + (i % 2) * 170f;
                var chipY = -168f - (i / 2) * 30f;
                var rarityColor = chips[i].rarity == 0 ? (Color) new Color32(230, 230, 230, 255) : RarityColor(chips[i].rarity);
                var hex = ColorUtility.ToHtmlStringRGB(rarityColor);
                var chip = CreateText(_statsCard, "RarityChip",
                    $"<b><color=#{hex}>{RarityName(chips[i].rarity)}</color></b> <color=#BEBEBE>{chips[i].done}/{chips[i].total}</color>",
                    18f, Color.white);
                chip.overflowMode = TextOverflowModes.Overflow;
                var chipRect = chip.rectTransform;
                chipRect.anchorMin = chipRect.anchorMax = new Vector2(0f, 1f);
                chipRect.pivot = new Vector2(0f, 0.5f);
                chipRect.anchoredPosition = new Vector2(x, chipY);
                chipRect.sizeDelta = new Vector2(166f, 24f);
            }
        }

        private void RebuildCategories()
        {
            if (_categoryContent == null) return;
            ClearChildren(_categoryContent);
            _categoryRows.Clear();

            var totalEntries = _categories.Count + 1;
            var pageCount = Mathf.Max(1, Mathf.CeilToInt(totalEntries / (float)CategoryPageSize));
            _categoryPage = Mathf.Clamp(_categoryPage, 0, pageCount - 1);

            var pagerVisible = pageCount > 1;
            if (_categoryPager != null) _categoryPager.SetActive(pagerVisible);
            if (_categoryViewport != null)
            {
                _categoryViewport.offsetMin = new Vector2(0f, pagerVisible ? 120f : 68f);
            }

            var startIndex = _categoryPage * CategoryPageSize;
            var endIndex = Mathf.Min(startIndex + CategoryPageSize, totalEntries);
            for (var i = startIndex; i < endIndex; i++)
            {
                AddCategoryRow(i == 0 ? null : _categories[i - 1]);
            }

            if (_categoryPageLabel != null) _categoryPageLabel.text = $"{_categoryPage + 1} / {pageCount}";
            if (_categoryPrevious != null) _categoryPrevious.interactable = _categoryPage > 0;
            if (_categoryNext != null) _categoryNext.interactable = _categoryPage < pageCount - 1;

            UpdateCategorySelection();
        }

        private void ChangeCategoryPage(int delta)
        {
            _categoryPage += delta;
            RebuildCategories();
        }

        [HideFromIl2Cpp]
        private void ShowCategoryPage(AchievementCategory category)
        {
            var index = category == null ? 0 : _categories.IndexOf(category) + 1;
            if (index < 0) return;
            _categoryPage = index / CategoryPageSize;
            RebuildCategories();
        }

        [HideFromIl2Cpp]
        private void AddCategoryRow(AchievementCategory category)
        {
            var name = category != null
                ? AchievementsStrings.GetText(category.Name)
                : AchievementsStrings.GetText(AchievementsStrings.FilterAll);
            var img = CreateImage(_categoryContent!, name, ToastSprites.ButtonBox, Color.white);
            img.type = Image.Type.Sliced;
            var le = img.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 52f;
            var rect = img.rectTransform;

            var icon = CreateImage(rect, "Icon", category?.GetIcon() ?? AchievementIcon, Color.white);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            var iconRect = icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(30f, 0f);
            iconRect.sizeDelta = new Vector2(32f, 32f);
            var textX = 56f;

            var label = CreateText(rect, "Label", name, 20f, Color.white, FontStyles.Bold);
            var labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(textX, 0f);
            labelRect.offsetMax = new Vector2(-8f, 0f);

            var button = img.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = HoverColor;
            colors.pressedColor = PressedTint;
            colors.selectedColor = Color.white;
            colors.disabledColor = SelectedColor;
            button.colors = colors;
            button.onClick.AddListener((UnityAction)(() => SelectCategory(category)));
            AttachGameSounds(img.gameObject);
            _categoryRows.Add((category, button));
        }

        // The settings tab is the "selected" entry while its page is open, so
        // no category row is marked then.
        private void UpdateCategorySelection()
        {
            var settingsOpen = _settingsPanel != null && _settingsPanel.activeSelf;
            foreach (var (category, button) in _categoryRows)
            {
                button.interactable = settingsOpen || category != _currentCategory;
            }
            if (_settingsButton != null) _settingsButton.interactable = !settingsOpen;
        }

        [HideFromIl2Cpp]
        private void AddModRow(Transform parent, AchievementsTab tab)
        {
            var name = tab != null
                ? tab.ModName
                : AchievementsStrings.GetText(AchievementsStrings.FilterAllMods);
            var img = CreateImage(parent, "Row", ToastSprites.ButtonBox, Color.white);
            img.type = Image.Type.Sliced;
            var le = img.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 44f;
            var rect = img.rectTransform;

            var icon = CreateImage(rect, "Icon",
                tab != null ? ToastSprites.TrimTransparent(tab.ModIcon) : AchievementIcon, Color.white);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            var iconRect = icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(24f, 0f);
            iconRect.sizeDelta = new Vector2(24f, 24f);

            var label = CreateText(rect, "Label", name, 20f, Color.white, FontStyles.Bold);
            var labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(46f, 0f);
            labelRect.offsetMax = new Vector2(-8f, 0f);

            var button = img.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = HoverColor;
            colors.pressedColor = PressedTint;
            colors.selectedColor = Color.white;
            colors.disabledColor = SelectedColor;
            button.colors = colors;
            var mod = tab?.ModName;
            button.onClick.AddListener((UnityAction)(() => SelectMod(mod)));
            AttachGameSounds(img.gameObject);
            _modRows.Add((mod, button));
        }

        private void UpdateModSelection()
        {
            foreach (var (mod, button) in _modRows)
            {
                button.interactable = mod != _currentMod;
            }
        }

        private void SelectMod(string mod)
        {
            _currentMod = mod;
            CloseModPopover();
            UpdateModFilterButton();
            BackToList();
            RebuildStatsCard();
            RebuildList();
        }

        private void ToggleModPopover()
        {
            if (_modPopover == null) return;
            _modPopover.SetActive(!_modPopover.activeSelf);
            if (_modPopover.activeSelf)
            {
                _modPopover.transform.SetAsLastSibling();
                UpdateModSelection();
            }
        }

        private void CloseModPopover()
        {
            if (_modPopover != null) _modPopover.SetActive(false);
        }

        private void UpdateModFilterButton()
        {
            if (_modFilterLabel == null) return;
            var tab = _currentMod == null ? null : _mods.FirstOrDefault(t => t.ModName == _currentMod);
            _modFilterIcon.sprite = tab != null ? ToastSprites.TrimTransparent(tab.ModIcon) : AchievementIcon;
            _modFilterLabel.text = tab != null ? tab.ModName
                : AchievementsStrings.GetText(AchievementsStrings.FilterAllMods);
        }

        private string SearchText => _searchField != null ? _searchField.text.Trim() : string.Empty;

        // Only revealed achievements are searchable, so typing a hidden
        // achievement's name can't leak its existence.
        [HideFromIl2Cpp]
        private bool MatchesSearch(BaseAchievement a)
        {
            if (_appliedSearch.Length == 0) return true;
            if (a.Hidden && !a.Unlocked) return false;
            return a.GetName()?.Contains(_appliedSearch, StringComparison.OrdinalIgnoreCase) == true
                || a.GetDescription()?.Contains(_appliedSearch, StringComparison.OrdinalIgnoreCase) == true
                || a.Tags.Any(t => t.Contains(_appliedSearch, StringComparison.OrdinalIgnoreCase));
        }

        // The icon doubles as the clear button once there is text.
        private void UpdateSearchIcon()
        {
            _searchIcon.sprite = SearchText.Length > 0 ? SearchClearGlyph : SearchGlyph;
        }

        private void QueueSearchRefresh()
        {
            UpdateSearchIcon();
            if (_searchDebounce != null) Coroutines.Stop(_searchDebounce);
            _searchDebounce = Coroutines.Start(CoSearchRefresh());
        }

        [HideFromIl2Cpp]
        private IEnumerator CoSearchRefresh()
        {
            yield return new WaitForSecondsRealtime(0.3f);
            _searchDebounce = null;
            if (_searchField == null) yield break;
            var text = SearchText;
            if (text == _appliedSearch) yield break;
            _appliedSearch = text;
            RebuildList();
        }

        private void ClearSearch()
        {
            if (_searchDebounce != null)
            {
                Coroutines.Stop(_searchDebounce);
                _searchDebounce = null;
            }
            _appliedSearch = string.Empty;
            _searchField.SetTextWithoutNotify(string.Empty);
            UpdateSearchIcon();
            RebuildList();
        }

        [HideFromIl2Cpp]
        private void SelectCategory(AchievementCategory category)
        {
            _currentCategory = category;
            BackToList();
            UpdateCategorySelection();
            RebuildStatsCard();
            RebuildList();
        }

        private void SetInitialTitle()
        {
            SetTitle(CurrentTitleText(), _currentCategory?.GetIcon() ?? AchievementIcon);
        }

        // The menu title for the current category, or the generic menu title when "All" is shown.
        private string CurrentTitleText() => _currentCategory != null
            ? AchievementsStrings.GetText(_currentCategory.Name)
            : AchievementsStrings.GetText(AchievementsStrings.MenuTitle);

        private void SetFilter(int filter)
        {
            _filter = filter;
            for (var i = 0; i < _filterTabs.Count; i++)
            {
                _filterTabs[i].button.interactable = i != filter;
            }
            RebuildList();
        }

        private void RebuildList()
        {
            var counts = EnumerateAchievements().Select(x => x.Achievement).Where(MatchesSearch).ToList();
            var filterCounts = new[]
            {
                counts.Count,
                counts.Count(a => a.Unlocked),
                counts.Count(InProgress),
                counts.Count(a => !a.Unlocked)
            };
            var names = FilterNames();
            for (var i = 0; i < _filterTabs.Count; i++)
            {
                _filterTabs[i].label.text =
                    $"{names[i]} <color=#BEBEBE>({filterCounts[i]})</color>";
                _filterTabs[i].button.interactable = i != _filter;
            }

            var filter = _filter;
            // Grouped by the mod that owns each achievement; headers only when the
            // current view mixes several mods.
            var allGroups = EnumerateAchievements()
                .GroupBy(x => x.Tab.ModName)
                .ToList();
            var groups = allGroups
                .Select(g => (tab: g.First().Tab, items: g.Where(x => PassesFilter(x.Achievement, filter)
                        && MatchesSearch(x.Achievement))
                    .Select(x => x.Achievement).ToList()))
                .Where(g => g.items.Count > 0)
                .ToList();

            var showHeaders = allGroups.Count >= 2;
            var plan = new List<(AchievementsTab Tab, BaseAchievement Achievement)>();
            foreach (var (tab, items) in groups)
            {
                items.Sort(CompareRows);
                if (showHeaders) plan.Add((tab, null));
                foreach (var achievement in items)
                {
                    plan.Add((null, achievement));
                }
            }

            _rebuildGen++;
            var emptyName = _appliedSearch.Length > 0 ? AchievementsStrings.EmptyNoResults
                : _filter == 3 ? AchievementsStrings.EmptyAllUnlocked : AchievementsStrings.EmptyNothingHere;
            Coroutines.Start(CoRebuildList(plan, AchievementsStrings.GetText(emptyName), _rebuildGen));
        }

        [HideFromIl2Cpp]
        private IEnumerator CoRebuildList(List<(AchievementsTab Tab, BaseAchievement Achievement)> plan,
            string emptyText, int gen)
        {
            var usedRows = 0;
            var usedHeaders = 0;
            var sibling = 0;
            foreach (var r in _rows) r.Root.SetActive(false);
            foreach (var h in _headers) h.Root.SetActive(false);
            if (_emptyState != null) _emptyState.SetActive(false);
            var frameStart = Time.realtimeSinceStartup;
            foreach (var item in plan)
            {
                if (gen != _rebuildGen || _listContent == null) yield break;
                if (item.Achievement == null)
                {
                    var header = GetHeader(usedHeaders++);
                    header.Icon.sprite = item.Tab.ModIcon;
                    header.NameText.text = item.Tab.ModName;
                    header.Root.SetActive(true);
                    header.Root.transform.SetSiblingIndex(sibling++);
                }
                else
                {
                    var row = GetRow(usedRows++);
                    BindRow(row, item.Achievement, usedRows - 1);
                    row.Root.transform.SetSiblingIndex(sibling++);
                }

                if (Time.realtimeSinceStartup - frameStart > 0.004f)
                {
                    yield return null;
                    frameStart = Time.realtimeSinceStartup;
                }
            }

            if (gen != _rebuildGen || _listContent == null) yield break;

            if (usedRows == 0)
            {
                EnsureEmptyState();
                _emptyText.text = emptyText;
                _emptyState.SetActive(true);
                _emptyState.transform.SetSiblingIndex(sibling);
            }
            else if (_emptyState != null)
            {
                _emptyState.SetActive(false);
            }

            _scroll.verticalNormalizedPosition = 1f;
            if (_controllerMode) EnsureSelection();
        }

        [HideFromIl2Cpp]
        private void EnsureEmptyState()
        {
            if (_emptyState != null) return;
            var go = new GameObject("Empty");
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(_listContent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 300f;
            _emptyText = CreateText(rect, "Text", "", 26f, ValueColor,
                FontStyles.Normal, TextAlignmentOptions.Center);
            _emptyText.rectTransform.anchorMin = Vector2.zero;
            _emptyText.rectTransform.anchorMax = Vector2.one;
            _emptyText.rectTransform.offsetMin = Vector2.zero;
            _emptyText.rectTransform.offsetMax = Vector2.zero;
            _emptyState = go;
            go.SetActive(false);
        }

        // Unlocked first (most recent first), then in-progress by fraction, then
        // locked; hidden achievements last.
        private static int CompareRows(BaseAchievement x, BaseAchievement y)
        {
            var c = (x.Hidden && !x.Unlocked ? 1 : 0).CompareTo(y.Hidden && !y.Unlocked ? 1 : 0);
            if (c != 0) return c;
            c = SortGroup(x).CompareTo(SortGroup(y));
            if (c != 0) return c;
            if (x.Unlocked && y.Unlocked)
            {
                return (y.UnlockedAt ?? DateTime.MinValue).CompareTo(x.UnlockedAt ?? DateTime.MinValue);
            }
            return Fraction(y).CompareTo(Fraction(x));
        }

        [HideFromIl2Cpp]
        private ListHeader GetHeader(int i)
        {
            while (_headers.Count <= i) _headers.Add(CreateHeader());
            return _headers[i];
        }

        [HideFromIl2Cpp]
        private ListHeader CreateHeader()
        {
            var go = new GameObject("Header");
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(_listContent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 36f;

            var icon = CreateImage(rect, "Icon", null, Color.white);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            var iconRect = icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(12f, 0f);
            iconRect.sizeDelta = new Vector2(24f, 24f);

            var name = CreateText(rect, "Name", "", 20f, ValueColor, FontStyles.Bold);
            var nameRect = name.rectTransform;
            nameRect.anchorMin = Vector2.zero;
            nameRect.anchorMax = Vector2.one;
            nameRect.offsetMin = new Vector2(44f, 0f);
            nameRect.offsetMax = Vector2.zero;

            return new ListHeader { Root = go, Icon = icon, NameText = name };
        }

        [HideFromIl2Cpp]
        private ListRow GetRow(int i)
        {
            while (_rows.Count <= i) _rows.Add(CreateRow());
            return _rows[i];
        }

        [HideFromIl2Cpp]
        private ListRow CreateRow()
        {
            var row = new ListRow();
            var card = CreateCard(_listContent, "Row");
            row.Card = card;
            var le = card.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 128f;
            var rowRect = card.rectTransform;

            var rim = CreateImage(rowRect, "Rim", ToastSprites.CardRim, Color.white);
            rim.type = Image.Type.Sliced;
            rim.raycastTarget = false;
            var rimRect = rim.rectTransform;
            rimRect.anchorMin = Vector2.zero;
            rimRect.anchorMax = Vector2.one;
            rimRect.offsetMin = Vector2.zero;
            rimRect.offsetMax = Vector2.zero;
            rim.gameObject.SetActive(false);
            row.Rim = rim;

            var disc = CreateRarityDisc(rowRect, "Disc", 76f, RimColor, out var inner);
            disc.anchorMin = disc.anchorMax = new Vector2(0f, 0.5f);
            disc.pivot = new Vector2(0.5f, 0.5f);
            disc.anchoredPosition = new Vector2(56f, 0f);
            row.Ring = disc.GetComponent<Image>();

            var icon = CreateImage(inner, "Icon", null, Color.white);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            var iconRect = icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(54f, 54f);
            row.Icon = icon;

            var sub = CreateImage(disc, "SubIcon", null, Color.white);
            sub.preserveAspect = true;
            sub.raycastTarget = false;
            var subRect = sub.rectTransform;
            subRect.anchorMin = subRect.anchorMax = new Vector2(1f, 0f);
            subRect.pivot = new Vector2(1f, 0f);
            subRect.anchoredPosition = new Vector2(0f, 0f);
            subRect.sizeDelta = new Vector2(48f, 48f);
            sub.gameObject.SetActive(false);
            row.SubIcon = sub;

            var name = CreateText(rowRect, "Name", "", 26f, Color.white, FontStyles.Bold);
            name.overflowMode = TextOverflowModes.Overflow;
            var nameRect = name.rectTransform;
            nameRect.anchorMin = new Vector2(0f, 1f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.offsetMin = new Vector2(112f, -52f);
            nameRect.offsetMax = new Vector2(-180f, -22f);
            row.NameText = name;

            var desc = CreateText(rowRect, "Description", "", 19f, ValueColor);
            desc.enableWordWrapping = false;
            var descRect = desc.rectTransform;
            descRect.anchorMin = new Vector2(0f, 1f);
            descRect.anchorMax = new Vector2(1f, 1f);
            descRect.offsetMin = new Vector2(112f, -80f);
            descRect.offsetMax = new Vector2(-180f, -56f);
            row.DescText = desc;

            var bar = CreateProgressBar(rowRect, RimColor, out var fill);
            bar.anchorMin = new Vector2(0f, 0f);
            bar.anchorMax = new Vector2(1f, 0f);
            bar.offsetMin = new Vector2(112f, 30f);
            bar.offsetMax = new Vector2(-180f, 40f);
            row.Fill = fill.GetComponent<Image>();
            row.FillRect = fill;

            var progressLabel = CreateText(rowRect, "ProgressLabel", "", 16f, ValueColor,
                FontStyles.Normal, TextAlignmentOptions.MidlineRight);
            var progressLabelRect = progressLabel.rectTransform;
            progressLabelRect.anchorMin = progressLabelRect.anchorMax = new Vector2(1f, 0f);
            progressLabelRect.pivot = new Vector2(1f, 0f);
            progressLabelRect.anchoredPosition = new Vector2(-180f, 42f);
            progressLabelRect.sizeDelta = new Vector2(200f, 20f);
            progressLabel.gameObject.SetActive(false);
            row.ProgressLabel = progressLabel;

            var pill = CreateImage(rowRect, "PointsPill", ToastSprites.ButtonBox, Color.white);
            pill.type = Image.Type.Sliced;
            pill.raycastTarget = false;
            var pillRect = pill.rectTransform;
            pillRect.anchorMin = pillRect.anchorMax = new Vector2(1f, 0.5f);
            pillRect.pivot = new Vector2(1f, 0.5f);
            pillRect.anchoredPosition = new Vector2(-22f, 0f);
            pillRect.sizeDelta = new Vector2(96f, 44f);
            row.Pill = pill;

            var pillEmblem = CreateScoreEmblem(pillRect, 22f);
            pillEmblem.anchorMin = pillEmblem.anchorMax = new Vector2(0f, 0.5f);
            pillEmblem.pivot = new Vector2(0.5f, 0.5f);
            pillEmblem.anchoredPosition = new Vector2(20f, 0f);

            var pillText = CreateText(pillRect, "Label", "", 22f, Color.white, FontStyles.Bold,
                TextAlignmentOptions.Center);
            var pillTextRect = pillText.rectTransform;
            pillTextRect.anchorMin = Vector2.zero;
            pillTextRect.anchorMax = Vector2.one;
            pillTextRect.offsetMin = new Vector2(34f, 0f);
            pillTextRect.offsetMax = Vector2.zero;
            row.PillText = pillText;

            var badge = CreateMultiplierBadge(pillRect, 2, new Vector2(6f, 6f));
            row.Badge = badge.gameObject;
            row.BadgeText = badge.GetComponentInChildren<TextMeshProUGUI>();
            badge.gameObject.SetActive(false);

            // Translucent overlay carries the hover tint so the rarity card
            // color stays unchanged; it also gives the ScrollRect a raycast hit.
            var overlay = CreateImage(rowRect, "HoverOverlay", ToastSprites.Card, Color.white);
            overlay.type = Image.Type.Sliced;
            var overlayRect = overlay.rectTransform;
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            overlay.raycastTarget = true;

            var button = card.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = overlay;
            var colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0f);
            colors.highlightedColor = new Color(0f, 0f, 0f, 0.25f);
            colors.pressedColor = new Color(0f, 0f, 0f, 0.4f);
            colors.selectedColor = new Color(1f, 1f, 1f, 0f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            button.onClick.AddListener((UnityAction)(() => ShowDetail(row.Achievement)));
            AttachGameSounds(card.gameObject, hoverSound: false);
            row.Button = button;
            row.Root = card.gameObject;
            return row;
        }

        [HideFromIl2Cpp]
        private void BindRow(ListRow row, BaseAchievement achievement, int index)
        {
            row.Achievement = achievement;
            var revealed = !achievement.Hidden || achievement.Unlocked;
            var showRarity = revealed || !achievement.HideRarity;
            var rarity = showRarity ? achievement.Rarity : 0;

            if (rarity > 0)
            {
                var c = RarityColor(rarity);
                row.Card.color = Color.Lerp(Color.white, c, 0.12f);
                row.Rim.color = new Color(c.r, c.g, c.b, 0.65f);
                row.Rim.gameObject.SetActive(true);
            }
            else
            {
                row.Card.color = Color.white;
                row.Rim.gameObject.SetActive(false);
            }

            row.Ring.color = rarity > 0 ? RarityColor(rarity) : RimColor;

            var iconSprite = revealed ? achievement.Icon
                : SpriteTools.LoadSpriteFromPath("AchievementsAPI.Resources.ExampleIcon.png",
                    typeof(AchievementsMenu).Assembly, 100);
            row.Icon.sprite = ToastSprites.TrimTransparent(iconSprite);
            row.Icon.color = achievement.Unlocked ? Color.white : new Color(0.35f, 0.35f, 0.35f);

            var subIcon = achievement.MenuSubIcon;
            row.SubIcon.gameObject.SetActive(subIcon != null);
            if (subIcon != null)
            {
                row.SubIcon.sprite = ToastSprites.TrimTransparent(subIcon);
                row.SubIcon.rectTransform.localScale = achievement.MenuSubIconScale;
            }

            row.NameText.text = revealed ? achievement.GetName() : AchievementsStrings.GetText(AchievementsStrings.HiddenAchievement);
            row.NameText.color = achievement.Unlocked ? Color.white : new Color(0.56f, 0.56f, 0.56f);

            row.DescText.text = revealed ? achievement.GetDescription() : AchievementsStrings.GetText(AchievementsStrings.HiddenAchievement);
            row.DescText.color = achievement.Unlocked ? (Color) ValueColor : new Color(0.42f, 0.42f, 0.42f);

            row.Fill.color = showRarity ? ProgressColor(achievement.Rarity) : RimColor;

            float target;
            string progressLabel = null;
            var progressLabelColor = (Color) ValueColor;
            if (achievement is CountAchievement count && count.RequiredValue > 0)
            {
                var hiddenProgress = count.HideProgress && count.Hidden && !achievement.Unlocked;
                target = hiddenProgress ? 0f : Mathf.Clamp01((float) count.CurrentValue / count.RequiredValue);
                if (!hiddenProgress)
                {
                    progressLabel = $"{Percent(count.CurrentValue, count.RequiredValue)}%  {count.CurrentValue}/{count.RequiredValue}";
                }
            }
            else
            {
                target = achievement.Unlocked ? 1f : 0f;
                if (achievement.Unlocked)
                {
                    progressLabel = "100%";
                    progressLabelColor = AccentHover;
                }
            }
            SetProgress(row.FillRect, 0f);
            var fillGen = ++row.FillGen;
            Coroutines.Start(CoFill(row, target, index * 0.03f, fillGen));

            row.ProgressLabel.gameObject.SetActive(progressLabel != null);
            if (progressLabel != null)
            {
                row.ProgressLabel.text = progressLabel;
                row.ProgressLabel.color = progressLabelColor;
            }

            row.Pill.color = achievement.Unlocked
                ? RarityColor(achievement.Rarity)
                : new Color(0.55f, 0.55f, 0.55f);
            row.PillText.text = $"{achievement.BasePoints}";
            row.PillText.color = achievement.Unlocked ? Color.white : (Color) ValueColor;

            var multiplier = achievement.PointsMultiplier;
            row.Badge.SetActive(multiplier > 1);
            if (multiplier > 1) row.BadgeText.text = $"x{multiplier}";

            row.Root.SetActive(true);
        }

        // Hides the list and shows `page` over the right column (null = list back).
        private void ShowPage(GameObject page)
        {
            var listsVisible = page == null;
            _scroll.gameObject.SetActive(listsVisible);
            _listScrollbar?.SetActive(listsVisible);
            _filterRow.gameObject.SetActive(listsVisible);
            _toolbar?.gameObject.SetActive(listsVisible);
            _settingsPanel?.SetActive(page == _settingsPanel);
            _detailPanel?.SetActive(page == _detailPanel);
            if (page != null)
            {
                CloseModPopover();
                var r = page.GetComponent<RectTransform>();
                MatchRect(r, _scroll.GetComponent<RectTransform>());
                // Also cover the toolbar row above the list
                r.offsetMax = new Vector2(r.offsetMax.x, -126f);
            }
        }

        // Wiki-style title with the page icon to its left (icon can be null).
        private void SetTitle(string text, Sprite icon)
        {
            _title.text = text;
            if (icon != null)
            {
                _titleIcon.sprite = ToastSprites.TrimTransparent(icon);
                _titleIcon.enabled = true;
                _titleIcon.rectTransform.anchoredPosition = new Vector2(108f, -70f);
                _title.rectTransform.anchoredPosition = new Vector2(154f, -44f);
            }
            else
            {
                _titleIcon.enabled = false;
                _title.rectTransform.anchoredPosition = new Vector2(110f, -44f);
            }
        }

        private void DeselectIfInside(GameObject root)
        {
            var current = EventSystem.current;
            if (root == null || current == null) return;
            var go = current.currentSelectedGameObject;
            if (go != null && go.transform.IsChildOf(root.transform)) current.SetSelectedGameObject(null);
        }

        private void BackToList()
        {
            if (_detailPanel != null)
            {
                DeselectIfInside(_detailPanel);
                _detailPanel.Destroy();
                _detailPanel = null;
            }
            SetTitle(CurrentTitleText(), _currentCategory?.GetIcon() ?? AchievementIcon);
            DeselectIfInside(_settingsPanel);
            ShowPage(null);
            UpdateCategorySelection();

            if (_controllerMode)
            {
                var row = _detailAchievement == null ? null
                    : _rows.FirstOrDefault(r => r.Achievement == _detailAchievement
                        && r.Root.activeInHierarchy && r.Button.interactable);
                _detailAchievement = null;
                if (row != null) Select(row.Button);
                else EnsureSelection();
            }
        }

        private void ShowSettingsPage()
        {
            if (_detailPanel != null)
            {
                DeselectIfInside(_detailPanel);
                _detailPanel.Destroy();
                _detailPanel = null;
            }
            SetTitle(AchievementsStrings.GetText(AchievementsStrings.Settings), ToastSprites.Gear);
            ShowPage(_settingsPanel);
            UpdateCategorySelection();
            if (_controllerMode) EnsureSelection();
        }

        private void ShowResetAllDialog()
        {
            if (_dialog != null) return;
            DeselectIfInside(_settingsPanel);
            _dialog = AchievementsConfirmDialog.Show(transform,
                AchievementsStrings.GetText(AchievementsStrings.ResetAllTitle),
                AchievementsStrings.GetText(AchievementsStrings.ResetAllBody),
                AchievementsStrings.GetText(AchievementsStrings.ResetAllConfirm),
                AchievementsStrings.GetText(AchievementsStrings.ResetAllCancel),
                ResetAllAchievements,
                () =>
                {
                    _dialog = null;
                    _closeDialog = null;
                    if (_controllerMode) EnsureSelection();
                },
                out _closeDialog);
            if (_controllerMode) EnsureSelection();
        }

        private void ResetAllAchievements()
        {
            AchievementStorage.ResetAll();
            foreach (var tab in _selectableTabs) AchievementStorage.AchievementStorageGet(tab);
            RebuildStatsCard();
            RebuildList();
        }

        [HideFromIl2Cpp]
        internal void FocusAchievement(string achievementId)
        {
            var found = AchievementsManager.FindAchievement(achievementId);
            if (found == null) return;

            var (tab, achievement) = found.Value;
            var category = AchievementsManager.ResolveCategory(tab, achievement);
            if (tab.IsSelectable && _categories.Contains(category))
            {
                _currentCategory = category;
                ShowCategoryPage(category);
                RebuildStatsCard();
                RebuildList();
            }
            if (tab.IsSelectable) ShowDetail(achievement);
        }

        [HideFromIl2Cpp]
        private void ShowDetail(BaseAchievement achievement)
        {
            if (_detailPanel != null)
            {
                DeselectIfInside(_detailPanel);
                _detailPanel.Destroy();
            }
            var revealed = !achievement.Hidden || achievement.Unlocked;
            SetTitle(revealed ? achievement.GetName() : AchievementsStrings.GetText(AchievementsStrings.HiddenAchievement),
                revealed ? achievement.Icon : null);
            _detailPanel = AchievementDetailPanel.Create(_panel, achievement, BackToList);
            ShowPage(_detailPanel);
            _detailAchievement = achievement;
            if (_controllerMode) EnsureSelection();
        }

        private void Update()
        {
            MenuInput.Poll();

            var mouse = Input.mousePosition;
            if (_mousePrimed && _controllerMode &&
                ((mouse - _lastMousePos).sqrMagnitude > 4f
                 || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(3)))
            {
                _controllerMode = false;
                _lastSelected = CurrentSelectable()?.gameObject;
            }
            _lastMousePos = mouse;
            _mousePrimed = true;

            var typing = _searchField != null && _searchField.isFocused;
            var fieldSelected = _searchField != null && EventSystem.current != null
                && EventSystem.current.currentSelectedGameObject == _searchField.gameObject;

            if (_modPopover != null && _modPopover.activeSelf && Input.GetMouseButtonDown(0)
                && !RectTransformUtility.RectangleContainsScreenPoint(_modPopoverRect, Input.mousePosition, null)
                && !RectTransformUtility.RectangleContainsScreenPoint(_modFilterRect, Input.mousePosition, null))
            {
                CloseModPopover();
            }

            if (!_controllerMode && !fieldSelected && CurrentSelectable() != null) EventSystem.current.SetSelectedGameObject(null);
            if (typing)
            {
                if (MenuInput.Cancel)
                {
                    _searchField.DeactivateInputField();
                    return;
                }

                if (MenuInput.AnyDirection)
                {
                    _searchField.DeactivateInputField();
                }
                else if (MenuInput.Confirm || MenuInput.PrevCategory || MenuInput.NextCategory
                         || MenuInput.PrevFilter || MenuInput.NextFilter)
                {
                    return;
                }
            }

            if (MenuInput.AnyDirection && !_controllerMode && Time.unscaledTime >= _navUnlockTime)
            {
                _controllerMode = true;
                var last = _lastSelected != null && _lastSelected.activeInHierarchy ? _lastSelected.GetComponent<Selectable>() : null;
                if (last != null && last.interactable && (ActivePage() == null || last.transform.IsChildOf(ActivePage().transform))) Select(last);
                else EnsureSelection();
            }

            if (MenuInput.Cancel)
            {
                CancelPage();
                return;
            }

            if (MenuInput.Up) MoveSelection(NavDirection.Up);
            else if (MenuInput.Down) MoveSelection(NavDirection.Down);
            else if (MenuInput.Left) MoveSelection(NavDirection.Left);
            else if (MenuInput.Right) MoveSelection(NavDirection.Right);

            if (MenuInput.Confirm) ConfirmSelection();
            if (MenuInput.PrevCategory) CycleCategory(-1);
            if (MenuInput.NextCategory) CycleCategory(1);
            if (MenuInput.PrevFilter) CycleFilter(-1);
            if (MenuInput.NextFilter) CycleFilter(1);
        }

        private void LateUpdate()
        {
            if (_searchBox != null && _searchIcon != null)
                _searchIcon.canvasRenderer.SetColor(_searchBox.canvasRenderer.GetColor());
            if (_focusRing == null) return;
            var selected = _controllerMode ? CurrentSelectable() : null;
            _focusRing.gameObject.SetActive(selected != null);
            if (selected == null) return;

            var rect = selected.GetComponent<RectTransform>();
            if (rect == null) return;
            rect.GetWorldCorners(_itemCorners);
            var parent = _focusRing.rectTransform.parent;
            var min = parent.InverseTransformPoint(_itemCorners[0]);
            var max = parent.InverseTransformPoint(_itemCorners[2]);
            var ring = _focusRing.rectTransform;
            ring.anchoredPosition = (min + max) * 0.5f;
            ring.sizeDelta = new Vector2(max.x - min.x + 8f, max.y - min.y + 8f);
            ring.SetAsLastSibling();
        }

        private Selectable CurrentSelectable()
        {
            var current = EventSystem.current;
            if (current == null) return null;
            var go = current.currentSelectedGameObject;
            if (go == null || !go.activeInHierarchy) return null;
            var sel = go.GetComponent<Selectable>();
            if (sel == null || !sel.interactable || !sel.transform.IsChildOf(transform)) return null;
            return sel;
        }

        private void Select(Selectable sel)
        {
            if (sel == null || EventSystem.current == null) return;
            EventSystem.current.SetSelectedGameObject(sel.gameObject);
            EnsureVisible(sel);
        }

        private GameObject ActivePage()
        {
            if (_dialog != null && _dialog.activeSelf) return _dialog;
            if (_detailPanel != null && _detailPanel.activeSelf) return _detailPanel;
            if (_settingsPanel != null && _settingsPanel.activeSelf) return _settingsPanel;
            return null;
        }

        private static Selectable FirstSelectableIn(GameObject page)
        {
            var selectables = page.GetComponentsInChildren<Selectable>(false);
            foreach (var s in selectables)
            {
                if (s.interactable && s.TryCast<Button>() != null
                    && s.name.IndexOf("Back", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return s;
                }
            }
            foreach (var s in selectables)
            {
                if (s.interactable) return s;
            }
            return null;
        }

        [HideFromIl2Cpp]
        private void EnsureSelection()
        {
            var current = CurrentSelectable();
            var page = ActivePage();
            if (page != null)
            {
                if (current != null && current.transform.IsChildOf(page.transform)) return;
                Select(FirstSelectableIn(page));
                return;
            }

            if (current != null)
            {
                EnsureVisible(current);
                return;
            }

            var row = _rows.FirstOrDefault(r =>
                r.Root != null && r.Root.activeInHierarchy && r.Button.interactable);
            Select(row != null ? (Selectable) row.Button
                : _filterTabs.Count > 0 ? _filterTabs[0].button : null);
        }

        private enum NavDirection { Up, Down, Left, Right }

        private void MoveSelection(NavDirection direction)
        {
            var sel = CurrentSelectable();
            if (sel == null)
            {
                EnsureSelection();
                return;
            }

            var slider = sel.TryCast<Slider>();
            if (slider != null && (direction == NavDirection.Left || direction == NavDirection.Right))
            {
                var step = (slider.maxValue - slider.minValue) / 20f;
                slider.value += direction == NavDirection.Right ? step : -step;
                return;
            }

            var next = direction switch
            {
                NavDirection.Up => sel.FindSelectableOnUp(),
                NavDirection.Down => sel.FindSelectableOnDown(),
                NavDirection.Left => sel.FindSelectableOnLeft(),
                _ => sel.FindSelectableOnRight()
            };
            if (next == null || !next.gameObject.activeInHierarchy || !next.interactable
                || !next.transform.IsChildOf(NavRoot())) return;
            Select(next);
        }

        private void ConfirmSelection()
        {
            var sel = CurrentSelectable();
            if (sel == null) return;
            var button = sel.TryCast<Button>();
            if (button != null)
            {
                PlayUiSound(true);
                button.onClick.Invoke();
                return;
            }
            var input = sel.TryCast<TMP_InputField>();
            if (input != null)
            {
                input.ActivateInputField();
                return;
            }
            var toggle = sel.TryCast<Toggle>();
            if (toggle != null)
            {
                toggle.isOn = !toggle.isOn;
                PlayUiSound(true);
            }
        }

        // Controller focus stays inside the modal dialog when one is open.
        private Transform NavRoot() => _dialog != null ? _dialog.transform : transform;

        private void CancelPage()
        {
            if (_dialog != null)
            {
                _closeDialog?.Invoke();
                return;
            }
            if (_modPopover != null && _modPopover.activeSelf)
            {
                CloseModPopover();
                return;
            }
            if (_settingsPanel != null && _settingsPanel.activeSelf)
            {
                // Settings is a sibling tab, not a page over the list: cancel
                // closes the whole menu like anywhere else.
                Close();
                return;
            }
            if (_detailPanel != null && _detailPanel.activeSelf)
            {
                BackToList();
                return;
            }
            Close();
        }

        [HideFromIl2Cpp]
        private void CycleCategory(int delta)
        {
            var total = _categories.Count + 1;
            if (total <= 1) return;
            var cur = _currentCategory == null ? 0 : _categories.IndexOf(_currentCategory) + 1;
            var index = (cur + delta + total) % total;
            _categoryPage = index / CategoryPageSize;
            RebuildCategories();
            SelectCategory(index == 0 ? null : _categories[index - 1]);
        }

        private void CycleFilter(int delta) => SetFilter((_filter + delta + _filterTabs.Count) % _filterTabs.Count);

        private void EnsureVisible(Selectable sel)
        {
            if (_scroll == null || _scroll.content == null || _scroll.viewport == null) return;
            if (!sel.transform.IsChildOf(_scroll.content)) return;
            Canvas.ForceUpdateCanvases();
            var item = sel.GetComponent<RectTransform>();
            if (item == null) return;
            var viewport = _scroll.viewport;
            item.GetWorldCorners(_itemCorners);
            viewport.GetWorldCorners(_viewCorners);
            var dy = 0f;
            if (_itemCorners[0].y < _viewCorners[0].y) dy = _itemCorners[0].y - _viewCorners[0].y;
            else if (_itemCorners[2].y > _viewCorners[2].y) dy = _itemCorners[2].y - _viewCorners[2].y;
            if (dy == 0f) return;
            var overflow = _scroll.content.rect.height - viewport.rect.height;
            if (overflow <= 0f) return;
            _scroll.verticalNormalizedPosition = Mathf.Clamp01(
                _scroll.verticalNormalizedPosition + dy / _scroll.content.lossyScale.y / overflow);
        }

        public void Close()
        {
            _controllerMode = false;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            if (mainMenuManager)
            {
                mainMenuManager.ActivateMainMenuUI();
            }
            else if (OptionsMenuBehaviour)
            {
                OptionsMenuBehaviour.gameObject.SetActive(true);
                OptionsMenuBehaviour.Background.enabled = true;
            }
            gameObject.Destroy();
        }

        [HideFromIl2Cpp]
        private IEnumerator CoPopIn(RectTransform rect, CanvasGroup group, Image scrim)
        {
            const float duration = 0.18f;
            for (var t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                if (rect == null) yield break;
                var k = Mathf.Clamp01(t / duration);
                var s = Mathf.Lerp(0.92f, 1f, EaseOutBack(k));
                rect.localScale = new Vector3(s, s, 1f);
                group.alpha = k;
                scrim.color = new Color(0f, 0f, 0f, 0.5f * k);
                yield return null;
            }
            if (rect == null) yield break;
            rect.localScale = Vector3.one;
            group.alpha = 1f;
            scrim.color = new Color(0f, 0f, 0f, 0.5f);
        }

        [HideFromIl2Cpp]
        private IEnumerator CoFill(ListRow row, float target, float delay, int gen)
        {
            for (var t = -delay; t < 0.45f; t += Time.unscaledDeltaTime)
            {
                if (row.FillRect == null || row.FillGen != gen) yield break;
                if (t >= 0f) SetProgress(row.FillRect, EaseOutCubic(Mathf.Clamp01(t / 0.45f)) * target);
                yield return null;
            }
            if (row.FillRect == null || row.FillGen != gen) yield break;
            SetProgress(row.FillRect, target);
        }
    }
}
