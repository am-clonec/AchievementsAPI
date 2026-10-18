using System.Linq;
using AchievementsAPI.API;
using Reactor.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static AchievementsAPI.MenuUi;

namespace AchievementsAPI;

/// <summary>
/// Procedurally built detail page for a single achievement, shown in place of
/// the achievement lists when an item is clicked or a toast asks to view it.
/// The menu positions the card (MenuUi.MatchRect); layout is anchor-based so it
/// works at any size.
/// </summary>
public static class AchievementDetailPanel
{
    private const float P = 48f;

    public static GameObject Create(Transform parent, BaseAchievement achievement, System.Action onBack)
    {
        var revealed = !achievement.Hidden || achievement.Unlocked;
        var showRarity = revealed || !achievement.HideRarity;

        var found = AchievementsManager.FindAchievement(achievement.Id);
        var tab = found?.Tab;
        var category = tab != null ? AchievementsManager.ResolveCategory(tab, achievement) : null;
        var multipleMods = AchievementsManager.Tabs
            .Where(t => t.IsSelectable)
            .Select(t => t.ModName)
            .Distinct()
            .Count() >= 2;

        var bg = CreateRarityCard(parent, "AchievementDetail", showRarity ? achievement.Rarity : 0);
        var panel = bg.gameObject;
        var rect = bg.rectTransform;

        var x0 = P + 220f + 36f;

        // HERO: rarity-ringed disc with the achievement icon
        var discRect = CreateRarityDisc(rect, "Disc", 220f,
            showRarity ? RarityColor(achievement.Rarity) : RimColor, out var inner);
        discRect.anchorMin = discRect.anchorMax = new Vector2(0f, 1f);
        discRect.pivot = new Vector2(0.5f, 0.5f);
        discRect.anchoredPosition = new Vector2(P + 110f, -(P + 110f));

        var iconSprite = revealed ? achievement.Icon
            : Assets.LoadSprite("ExampleIcon");
        var iconColor = achievement.Unlocked ? Color.white : new Color(0.35f, 0.35f, 0.35f);
        var icon = CreateImage(inner, "Icon", ToastSprites.TrimTransparent(iconSprite), iconColor);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        var iconRect = icon.rectTransform;
        iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(150f, 150f);

        // Subicon positioned at bottom-right of the disc
        var subIcon = achievement.MenuSubIcon;
        if (subIcon != null)
        {
            var sub = CreateImage(discRect, "SubIcon", ToastSprites.TrimTransparent(subIcon), Color.white);
            sub.preserveAspect = true;
            sub.raycastTarget = false;
            var subRect = sub.rectTransform;
            subRect.anchorMin = subRect.anchorMax = new Vector2(1f, 0f);
            subRect.pivot = new Vector2(1f, 0f);
            subRect.anchoredPosition = new Vector2(0f, 0f);
            subRect.sizeDelta = new Vector2(100f, 100f);
            subRect.localScale = achievement.MenuSubIconScale;
        }

        // Name
        var name = CreateText(rect, "Name",
            revealed ? achievement.GetName() : AchievementsStrings.GetText(AchievementsStrings.HiddenAchievement),
            38f, Color.white, FontStyles.Bold);
        name.enableWordWrapping = false;
        name.overflowMode = TextOverflowModes.Ellipsis;
        StretchTop(name.rectTransform, x0, -52f, 48f);

        // Chip row: rarity, category, mod
        var row = new GameObject("ChipRow").AddComponent<RectTransform>();
        row.SetParent(rect, false);
        StretchTop(row, x0, -108f, 32f);
        var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 10f;
        rowLayout.childAlignment = TextAnchor.MiddleLeft;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;

        if (showRarity)
        {
            var rarityColor = achievement.Rarity > 0
                ? RarityColor(achievement.Rarity)
                : (Color) new Color32(230, 230, 230, 255);
            AddChip(row, null, RarityName(achievement.Rarity), rarityColor);
        }
        if (category != null)
        {
            var catIcon = category.GetIcon();
            AddChip(row, catIcon != null ? ToastSprites.TrimTransparent(catIcon) : null,
                AchievementsStrings.GetText(category.Name), Color.white);
        }
        if (tab != null && multipleMods)
        {
            var modIcon = tab.ModIcon;
            AddChip(row, modIcon != null ? ToastSprites.TrimTransparent(modIcon) : null,
                tab.ModName, Color.white);
        }

        // Description
        var desc = CreateText(rect, "Description",
            revealed ? achievement.GetDescription() : AchievementsStrings.GetText(AchievementsStrings.HiddenAchievement),
            24f, ValueColor, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        desc.enableWordWrapping = true;
        desc.overflowMode = TextOverflowModes.Ellipsis;
        var descRect = desc.rectTransform;
        descRect.anchorMin = new Vector2(0f, 1f);
        descRect.anchorMax = new Vector2(1f, 1f);
        descRect.pivot = new Vector2(0f, 1f);
        descRect.offsetMin = new Vector2(x0, -270f);
        descRect.offsetMax = new Vector2(-P, -156f);

        // STAT TILES
        var tiles = new GameObject("Tiles").AddComponent<RectTransform>();
        tiles.SetParent(rect, false);
        StretchTop(tiles, P, -302f, 120f);
        var tilesLayout = tiles.gameObject.AddComponent<HorizontalLayoutGroup>();
        tilesLayout.spacing = 20f;
        tilesLayout.childControlWidth = true;
        tilesLayout.childControlHeight = true;
        tilesLayout.childForceExpandWidth = true;
        tilesLayout.childForceExpandHeight = true;

        var scoreTile = CreateTile(tiles, AchievementsStrings.GetText(AchievementsStrings.SkeldPoints), out _);
        // Base points with the xN badge, same as the list rows
        var scoreValue = AddTileValue(scoreTile.transform, $"{achievement.BasePoints}", Color.white);
        if (achievement.PointsMultiplier > 1)
        {
            scoreValue.ForceMeshUpdate();
            var badge = CreateMultiplierBadge(scoreValue.rectTransform, achievement.PointsMultiplier, Vector2.zero);
            badge.anchorMin = badge.anchorMax = new Vector2(0f, 0.5f);
            badge.pivot = new Vector2(0f, 0.5f);
            badge.anchoredPosition = new Vector2(scoreValue.preferredWidth + 10f, 0f);
            badge.sizeDelta = new Vector2(40f, 24f);
        }
        var emblem = CreateScoreEmblem(scoreTile.transform, 52f);
        emblem.anchorMin = emblem.anchorMax = new Vector2(1f, 0.5f);
        emblem.pivot = new Vector2(1f, 0.5f);
        emblem.anchoredPosition = new Vector2(-20f, 0f);

        var rarityTile = CreateTile(tiles, AchievementsStrings.GetText(AchievementsStrings.DetailRarity), out _);
        if (showRarity)
        {
            var rarityColor = achievement.Rarity > 0
                ? RarityColor(achievement.Rarity)
                : (Color) new Color32(230, 230, 230, 255);
            AddTileValue(rarityTile.transform, RarityName(achievement.Rarity), rarityColor);
        }
        else
        {
            AddTileValue(rarityTile.transform, "???", ValueColor);
        }

        var statusTile = CreateTile(tiles, AchievementsStrings.GetText(AchievementsStrings.DetailStatus), out _);
        var inProgress = achievement is CountAchievement prog
            && prog.RequiredValue > 0 && prog.CurrentValue > 0
            && !(prog.HideProgress && prog.Hidden);
        string statusValue;
        Color statusColor;
        if (achievement.Unlocked)
        {
            statusValue = AchievementsStrings.GetText(AchievementsStrings.StatusUnlocked);
            statusColor = AccentHover;
        }
        else if (inProgress)
        {
            statusValue = AchievementsStrings.GetText(AchievementsStrings.FilterInProgress);
            statusColor = Color.white;
        }
        else
        {
            statusValue = AchievementsStrings.GetText(AchievementsStrings.StatusLocked);
            statusColor = ValueColor;
        }
        AddTileValue(statusTile.transform, statusValue, statusColor);
        if (achievement.Unlocked && achievement.UnlockedAt is { } at)
            AddTileSubline(statusTile.transform, FormatUnlockTime(at));

        // PROGRESS CARD
        var hideCard = achievement is CountAchievement hidden
            && hidden.HideProgress && hidden.Hidden && !achievement.Unlocked;
        if (!hideCard)
        {
            var card = CreateImage(rect, "ProgressCard", ToastSprites.ButtonBox, Color.white);
            card.type = Image.Type.Sliced;
            card.raycastTarget = false;
            var cardRect = card.rectTransform;
            StretchTop(cardRect, P, -450f, 100f);

            var caption = CreateText(cardRect, "Caption",
                AchievementsStrings.GetText(AchievementsStrings.DetailProgress),
                16f, ValueColor, FontStyles.Bold);
            StretchTop(caption.rectTransform, 18f, -14f, 22f, -18f);

            // Same label rules as the list rows: count achievements show
            // "pct%  cur/req", one-shots show a teal 100% once unlocked.
            var count = achievement as CountAchievement;
            var isCount = count != null && count.RequiredValue > 0;
            var valueText = isCount
                ? $"{Percent(count.CurrentValue, count.RequiredValue)}%  {count.CurrentValue}/{count.RequiredValue}"
                : achievement.Unlocked ? "100%" : null;
            if (valueText != null)
            {
                // Same size and 2px gap above the bar as the list rows
                var value = CreateText(cardRect, "Value", valueText, 16f,
                    isCount ? (Color) ValueColor : AccentHover, FontStyles.Normal, TextAlignmentOptions.MidlineRight);
                value.overflowMode = TextOverflowModes.Overflow;
                var valueRect = value.rectTransform;
                valueRect.anchorMin = valueRect.anchorMax = new Vector2(1f, 1f);
                valueRect.pivot = new Vector2(1f, 0f);
                valueRect.anchoredPosition = new Vector2(-18f, -54f);
                valueRect.sizeDelta = new Vector2(300f, 20f);
            }

            var bar = CreateProgressBar(cardRect, ProgressColor(showRarity ? achievement.Rarity : 0), out var fill);
            bar.anchorMin = new Vector2(0f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.pivot = new Vector2(0f, 1f);
            bar.offsetMin = new Vector2(18f, -56f - 14f);
            bar.offsetMax = new Vector2(-18f, -56f);
            var target = isCount ? Mathf.Clamp01((float) count.CurrentValue / count.RequiredValue)
                : achievement.Unlocked ? 1f : 0f;
            SetProgress(fill, 0f);
            Coroutines.Start(CoFill(fill, target));
        }

        var back = CreateButton(rect, AchievementsStrings.GetText(AchievementsStrings.Back), Vector2.zero, "Back");
        var backRect = back.GetComponent<RectTransform>();
        backRect.anchorMin = backRect.anchorMax = new Vector2(1f, 0f);
        backRect.pivot = new Vector2(1f, 0.5f);
        backRect.anchoredPosition = new Vector2(-P, 64f);
        back.onClick.AddListener((UnityAction)(() => onBack()));
        AttachGameSounds(back.gameObject);

        return panel;
    }

    // A ButtonBox chip inside a HorizontalLayoutGroup row: optional icon + label,
    // width fitted to content, fixed 32px tall.
    private static void AddChip(Transform row, Sprite? icon, string text, Color textColor)
    {
        var chip = CreateImage(row, "Chip", ToastSprites.ButtonBox, Color.white);
        chip.type = Image.Type.Sliced;
        chip.raycastTarget = false;
        var chipRect = chip.rectTransform;

        var layout = chip.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset { left = 12, right = 12, top = 0, bottom = 0 };
        layout.spacing = 6f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var fitter = chip.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        var element = chip.gameObject.AddComponent<LayoutElement>();
        element.preferredHeight = 32f;

        if (icon != null)
        {
            var iconImg = CreateImage(chipRect, "Icon", icon, Color.white);
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            var iconElement = iconImg.gameObject.AddComponent<LayoutElement>();
            iconElement.preferredWidth = 20f;
            iconElement.preferredHeight = 20f;
        }

        var label = CreateText(chipRect, "Label", text, 18f, textColor, FontStyles.Bold);
        label.overflowMode = TextOverflowModes.Overflow;
        var labelElement = label.gameObject.AddComponent<LayoutElement>();
        labelElement.preferredHeight = 32f;
    }

    // A ButtonBox stat tile: caption in the top-left corner; returns the tile image.
    private static Image CreateTile(Transform parent, string caption, out RectTransform rect)
    {
        var tile = CreateImage(parent, "Tile", ToastSprites.ButtonBox, Color.white);
        tile.type = Image.Type.Sliced;
        tile.raycastTarget = false;
        rect = tile.rectTransform;

        var captionText = CreateText(rect, "Caption", caption, 16f, ValueColor, FontStyles.Bold);
        StretchTop(captionText.rectTransform, 18f, -14f, 22f, -18f);
        return tile;
    }

    // Large white value text inside a tile.
    private static TextMeshProUGUI AddTileValue(Transform tile, string text, Color color)
    {
        var value = CreateText(tile, "Value", text, 32f, color, FontStyles.Bold);
        value.overflowMode = TextOverflowModes.Overflow;
        StretchTop(value.rectTransform, 18f, -42f, 44f, -18f);
        return value;
    }

    // Small secondary line near the bottom of a tile.
    private static void AddTileSubline(Transform tile, string text)
    {
        var subline = CreateText(tile, "Subline", text, 16f, ValueColor);
        subline.overflowMode = TextOverflowModes.Overflow;
        StretchTop(subline.rectTransform, 18f, -88f, 22f, -18f);
    }

    // Stretches a rect horizontally from `left` to `right` (offsets), top edge at
    // `top` (negative from the parent's top), `height` tall.
    private static void StretchTop(RectTransform rect, float left, float top, float height, float right = -P)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.offsetMin = new Vector2(left, top - height);
        rect.offsetMax = new Vector2(right, top);
    }
}
