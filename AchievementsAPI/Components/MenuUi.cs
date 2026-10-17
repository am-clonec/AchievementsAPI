using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AchievementsAPI.API;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AchievementsAPI;

/// <summary>
/// Shared procedural UI helpers (colors, images, text, buttons, progress bars)
/// used by the achievements menu panels. Styled to match the toast: dark
/// surfaces, green accents, white text.
/// </summary>
public static class MenuUi
{
    internal static Color32 AccentColor => AchievementsTheme.Current.Accent;   // AU teal
    internal static Color32 AccentHover => AchievementsTheme.Current.AccentHover;
    internal static Color32 PressedColor => AchievementsTheme.Current.AccentPressed;
    internal static Color32 TrackOffColor => AchievementsTheme.Current.Track;
    internal static Color32 ValueColor => AchievementsTheme.Current.SecondaryText;
    internal static Color32 TextColor => AchievementsTheme.Current.PrimaryText;
    internal static Color32 RimColor => AchievementsTheme.Current.Rim;         // neutral ring for common/no-rarity
    internal static Color32 CommonLabelColor => AchievementsTheme.Current.CommonLabel;
    internal static Color32 DangerColor => AchievementsTheme.Current.Danger;
    internal static Color32 DangerHover => AchievementsTheme.Current.DangerHover;
    internal static Color32 DangerPressed => AchievementsTheme.Current.DangerPressed;

    internal static Image CreateImage(Transform parent, string name, Sprite? sprite, Color color)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        if (sprite != null) img.sprite = SpriteTools.PreserveSprite(sprite);
        img.color = color;
        return img;
    }

    internal static void ApplyGameFont(TextMeshProUGUI tmp)
    {
        if (AchievementsTheme.Current.Font is { } f) { tmp.font = f; return; }
        if (DisconnectPopup.Instance && DisconnectPopup.Instance._textArea)
        {
            var source = DisconnectPopup.Instance._textArea;
            tmp.font = source.font;
            tmp.fontMaterial = source.fontMaterial;
            return;
        }

        if (HudManager.InstanceExists && HudManager.Instance.TaskPanel && HudManager.Instance.TaskPanel.taskText)
        {
            var source = HudManager.Instance.TaskPanel.taskText;
            tmp.font = source.font;
            tmp.fontMaterial = source.fontMaterial;
            return;
        }

        var prefabText = Assets.achievementToastPrefab != null
            ? Assets.achievementToastPrefab.transform.Find("AchievementName")?.GetComponent<TextMeshProUGUI>()
            : null;
        if (prefabText != null && prefabText.font != null)
        {
            tmp.font = prefabText.font;
            tmp.fontMaterial = prefabText.fontMaterial;
            return;
        }

        var font = AchievementToast.ResolveFont();
        if (font != null) tmp.font = font;
    }

    internal static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size,
        Color color, FontStyles style = FontStyles.Normal, TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        ApplyGameFont(tmp);
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.raycastTarget = false;
        return tmp;
    }

    internal static Image CreateCard(Transform parent, string name)
    {
        var img = CreateImage(parent, name, AchievementsTheme.Current.CardBackground ?? ToastSprites.Card, Color.white);
        img.type = Image.Type.Sliced;
        return img;
    }

    // Card frame with a rarity-tinted rim (common keeps the neutral rim). Returns the card image; the rim overlay is a stretched child.
    internal static Image CreateRarityCard(Transform parent, string name, int rarity)
    {
        var card = CreateCard(parent, name);
        if (rarity > 0)
        {
            card.color = Color.Lerp(Color.white, RarityColor(rarity), 0.12f);
            var c = RarityColor(rarity);
            var rim = CreateImage(card.rectTransform, "Rim", ToastSprites.CardRim, new Color(c.r, c.g, c.b, 0.65f));
            rim.type = Image.Type.Sliced;
            rim.raycastTarget = false;
            var rimRect = rim.rectTransform;
            rimRect.anchorMin = Vector2.zero;
            rimRect.anchorMax = Vector2.one;
            rimRect.offsetMin = Vector2.zero;
            rimRect.offsetMax = Vector2.zero;
        }
        return card;
    }

    // Dark disc with a rarity-colored ring (4px). Returns the ring (outer) rect; `inner` is the dark face to parent icons to.
    internal static RectTransform CreateRarityDisc(Transform parent, string name, float size, Color ringColor, out RectTransform inner)
    {
        var ring = CreateImage(parent, name, AchievementsTheme.Current.RarityDisc ?? ToastSprites.Circle, ringColor);
        ring.raycastTarget = false;
        var ringRect = ring.rectTransform;
        ringRect.sizeDelta = new Vector2(size, size);

        var face = CreateImage(ringRect, "Inner", AchievementsTheme.Current.RarityDisc ?? ToastSprites.Circle, AchievementsTheme.Current.CardFace);
        face.raycastTarget = false;
        inner = face.rectTransform;
        inner.anchorMin = Vector2.zero;
        inner.anchorMax = Vector2.one;
        inner.offsetMin = new Vector2(4f, 4f);
        inner.offsetMax = new Vector2(-4f, -4f);
        return ringRect;
    }

    internal static void Outline(TextMeshProUGUI tmp)
    {
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = Color.black;
    }

    private static Sprite _trophy;

    internal static Sprite AchievementIcon
    {
        get
        {
            if (_trophy == null)
            {
                _trophy = SpriteTools.LoadSpriteFromPath("AchievementsAPI.Resources.AchievementsIcon.png",
                    typeof(MenuUi).Assembly, 100);
            }
            return ToastSprites.TrimTransparent(_trophy);
        }
    }

    private static Sprite _searchIcon;
    private static Sprite _searchIconHover;
    private static Sprite _searchClear;
    private static Sprite _searchClearHover;

    internal static Sprite SearchIcon => _searchIcon ??= SpriteTools.LoadSpriteFromPath(
        "AchievementsAPI.Resources.SearchIconIdle.png", typeof(MenuUi).Assembly, 100);

    internal static Sprite SearchIconHover => _searchIconHover ??= SpriteTools.LoadSpriteFromPath(
        "AchievementsAPI.Resources.SearchIconHover.png", typeof(MenuUi).Assembly, 100);

    internal static Sprite SearchClear => _searchClear ??= SpriteTools.LoadSpriteFromPath(
        "AchievementsAPI.Resources.SearchClearIdle.png", typeof(MenuUi).Assembly, 100);

    internal static Sprite SearchClearHover => _searchClearHover ??= SpriteTools.LoadSpriteFromPath(
        "AchievementsAPI.Resources.SearchClearHover.png", typeof(MenuUi).Assembly, 100);

    private static Sprite _searchGlyph;
    private static Sprite _searchClearGlyph;

    // Shared crop rect (bottom-up texture coords) over both 104x104 search PNGs:
    // glyph bbox union x:12..87 y:13..81 (top-down) + 3px margin, clamped inside
    // the flat-grey box interior x:6..95 y:6..87 so no outline/shadow/transparent
    // pixels are included.
    private const int SearchGlyphX = 9;
    private const int SearchGlyphY = 19;
    private const int SearchGlyphW = 82;
    private const int SearchGlyphH = 75;

    internal static Sprite SearchGlyph => _searchGlyph ??= CropSprite(SearchIcon,
        SearchGlyphX, SearchGlyphY, SearchGlyphW, SearchGlyphH, "SearchGlyph");

    internal static Sprite SearchClearGlyph => _searchClearGlyph ??= CropSprite(SearchClear,
        SearchGlyphX, SearchGlyphY, SearchGlyphW, SearchGlyphH, "SearchClearGlyph");

    private static Sprite CropSprite(Sprite src, int x, int y, int w, int h, string name)
    {
        var sprite = Sprite.Create(src.texture, new Rect(x, y, w, h), new Vector2(0.5f, 0.5f),
            100f, 0, SpriteMeshType.FullRect);
        sprite.name = name;
        sprite.DontUnload();
        return sprite;
    }

    // Skeld Points emblem: the achievements trophy icon (same sprite as the
    // in-game settings tab), untinted.
    internal static RectTransform CreateScoreEmblem(Transform parent, float size)
    {
        var icon = CreateImage(parent, "ScoreEmblem", AchievementIcon, Color.white);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        var rect = icon.rectTransform;
        rect.sizeDelta = new Vector2(size, size);
        return rect;
    }

    /// <summary>
    /// Small teal "xN" tag pinned to the top-right corner of <paramref name="parent"/>, used to
    /// show a points multiplier next to the base score rather than inflating the number.
    /// </summary>
    internal static RectTransform CreateMultiplierBadge(Transform parent, int multiplier, Vector2 overhang)
    {
        var theme = AchievementsTheme.Current;
        var badge = CreateImage(parent, "MultiplierBadge", theme.ProgressBarBackground ?? ToastSprites.RoundedRectTiny, AccentHover);
        badge.type = Image.Type.Sliced;
        badge.raycastTarget = false;
        var rect = badge.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = overhang;
        rect.sizeDelta = new Vector2(34f, 20f);

        var text = CreateText(rect, "Label", $"x{multiplier}", theme.CaptionFontSize, Color.black, FontStyles.Bold,
            TextAlignmentOptions.Center);
        text.overflowMode = TextOverflowModes.Overflow;
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        return rect;
    }

    private static AudioClip _clickSound;
    private static AudioClip _hoverSound;
    private static Sprite _closeButtonSprite;

    /// <summary>
    /// The game's own close-button sprite (the options menu's Close button),
    /// scraped at runtime so it matches the shipped UI exactly. Null when no
    /// options menu exists in the scene yet or none could be found.
    /// </summary>
    internal static Sprite CloseButtonSprite
    {
        get
        {
            if (_closeButtonSprite == null) ResolveCloseButtonSprite();
            return _closeButtonSprite;
        }
    }

    // Scans the first options menu's buttons for a SpriteRenderer whose own,
    // parent's or grandparent's name contains "close" (the back button's
    // renderers live under activeSprites and their parents are unnamed).
    private static void ResolveCloseButtonSprite()
    {
        bool IsCloseName(string n) => n != null && n.IndexOf("close", StringComparison.OrdinalIgnoreCase) >= 0;

        var menus = UnityEngine.Object.FindObjectsByType<OptionsMenuBehaviour>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (menus == null || menus.Length == 0) return;

        foreach (var button in menus[0].GetComponentsInChildren<PassiveButton>(true))
        {
            foreach (var renderer in button.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer.sprite == null) continue;
                var t = renderer.transform;
                if (IsCloseName(t.name) || (t.parent != null && (IsCloseName(t.parent.name)
                    || (t.parent.parent != null && IsCloseName(t.parent.parent.name)))))
                {
                    _closeButtonSprite = renderer.sprite;
                    return;
                }
            }
        }
    }

    // Lazily resolved game UI clips; cached, but `== null` is a Unity null
    // check so a destroyed clip is re-resolved.
    internal static AudioClip ClickSound
    {
        get
        {
            if (_clickSound == null) ResolveUiSounds();
            return _clickSound;
        }
    }

    internal static AudioClip HoverSound
    {
        get
        {
            if (_hoverSound == null) ResolveUiSounds();
            return _hoverSound;
        }
    }

    // HudManager does not exist at the main menu, so prefer its clips when the
    // instance is up and fall back to any button in the scene (options menus
    // have no hover clip, but the main menu's own buttons do).
    private static void ResolveUiSounds()
    {
        if (HudManager.InstanceExists && HudManager.Instance.MapButton)
        {
            _clickSound = HudManager.Instance.MapButton.ClickSound;
            _hoverSound = HudManager.Instance.MapButton.HoverSound;
        }

        if (_clickSound && _hoverSound) return;

        // Prefer the game's standard UI_Select click over whatever button
        // happens to come first.
        foreach (var button in UnityEngine.Object.FindObjectsByType<PassiveButton>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (button.ClickSound && button.ClickSound.name == "UI_Select")
            {
                _clickSound = button.ClickSound;
                if (button.HoverSound) _hoverSound = button.HoverSound;
                break;
            }
            if (_clickSound == null && button.ClickSound) _clickSound = button.ClickSound;
            if (_hoverSound == null && button.HoverSound) _hoverSound = button.HoverSound;
        }
    }

    internal static void PlayUiSound(bool click)
    {
        var clip = click ? ClickSound : HoverSound;
        if (clip && Constants.ShouldPlaySfx() && SoundManager.Instance)
        {
            SoundManager.Instance.PlaySound(clip, false);
        }
    }

    /// <summary>
    /// Adds an <see cref="EventTrigger"/> that plays the game's click sound on
    /// pointer down and its hover sound on pointer enter (unless
    /// <paramref name="hoverSound"/> is false). Silently does nothing when no
    /// game clips could be found.
    /// </summary>
    internal static void AttachGameSounds(GameObject go, bool hoverSound = true)
    {
        var trigger = go.GetComponent<EventTrigger>();
        if (trigger == null) trigger = go.AddComponent<EventTrigger>();

        var press = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        press.callback.AddListener((UnityAction<BaseEventData>)(_ => PlayUiSound(true)));
        trigger.triggers.Add(press);

        // EventTrigger implements IScrollHandler, so event propagation stops at
        // the button and the mouse wheel never reaches the ScrollRect while
        // hovering it; forward the scroll explicitly
        var scroll = new EventTrigger.Entry { eventID = EventTriggerType.Scroll };
        scroll.callback.AddListener((UnityAction<BaseEventData>)(data =>
        {
            var pointer = data.TryCast<PointerEventData>();
            var scrollRect = go.GetComponentInParent<ScrollRect>();
            if (pointer != null && scrollRect != null) scrollRect.OnScroll(pointer);
        }));
        trigger.triggers.Add(scroll);

        if (hoverSound)
        {
            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((UnityAction<BaseEventData>)(_ => PlayUiSound(false)));
            trigger.triggers.Add(enter);
        }
    }

    // Charcoal, white-outline pill button in the game's settings style (back/preview).
    // `name` keeps the object name stable (used for controller focus lookups) while
    // the visible label is translated.
    internal static Button CreateButton(Transform panel, string label, Vector2 position, string? name = null,
        bool danger = false)
    {
        var theme = AchievementsTheme.Current;
        // ButtonBox is pre-baked charcoal, so a tint would multiply into near-black; the
        // danger variant tints a white box of the same radius instead.
        var bg = CreateImage(panel, $"{name ?? label}Button", theme.ButtonBackground ?? (danger ? ToastSprites.RoundedRectSmall : ToastSprites.ButtonBox), Color.white);
        bg.type = Image.Type.Sliced;
        var rect = bg.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(theme.ButtonWidth, theme.ButtonHeight);

        var text = CreateText(rect, "Label", label, theme.BodyFontSize, TextColor, FontStyles.Bold, TextAlignmentOptions.Center);
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var button = bg.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        var colors = button.colors;
        colors.normalColor = danger ? DangerColor : Color.white;
        colors.highlightedColor = danger ? DangerHover : new Color32(200, 200, 200, 255);
        colors.pressedColor = danger ? DangerPressed : new Color32(150, 150, 150, 255);
        colors.selectedColor = danger ? DangerColor : Color.white;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        button.colors = colors;
        return button;
    }

    // Il2CppInterop's Transform enumerator yields Il2CppSystem.Object wrappers
    // (foreach + cast throws); detach so layout groups don't draw pending-destroy
    // children for a frame.
    internal static void ClearChildren(Transform parent)
    {
        for (var i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            child.SetParent(null, false);
            UnityEngine.Object.Destroy(child.gameObject);
        }
    }

    internal static void EnsureEventSystem(GameObject host)
    {
        if (EventSystem.current == null && host.GetComponent<EventSystem>() == null)
        {
            host.AddComponent<EventSystem>();
            host.AddComponent<StandaloneInputModule>();
        }
    }

    // Thin horizontal progress bar; returns the fill rect so callers can set its anchorMax.x.
    internal static RectTransform CreateProgressBar(Transform parent, Color fillColor, out RectTransform fill)
    {
        var bg = CreateImage(parent, "ProgressBar", AchievementsTheme.Current.ProgressBarBackground ?? ToastSprites.RoundedRectTiny, TrackOffColor);
        bg.type = Image.Type.Sliced;

        var fillImg = CreateImage(bg.rectTransform, "Fill", AchievementsTheme.Current.ProgressBarBackground ?? ToastSprites.RoundedRectTiny, fillColor);
        fillImg.type = Image.Type.Sliced;
        fill = fillImg.rectTransform;
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(0f, 1f);
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;

        return bg.rectTransform;
    }

    internal static void SetProgress(RectTransform fill, float t) => fill.anchorMax = new Vector2(Mathf.Clamp01(t), 1f);

    internal static IEnumerator CoFill(RectTransform fill, float target, float delay = 0f, float duration = 0.45f)
    {
        for (var t = -delay; t < duration; t += Time.unscaledDeltaTime)
        {
            if (fill == null) yield break;
            if (t >= 0f) SetProgress(fill, EaseOutCubic(Mathf.Clamp01(t / duration)) * target);
            yield return null;
        }
        if (fill == null) yield break;
        SetProgress(fill, target);
    }

    internal static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

    internal static float EaseOutBack(float t)
    {
        const float c = 1.70158f;
        return 1f + (c + 1f) * Mathf.Pow(t - 1f, 3f) + c * Mathf.Pow(t - 1f, 2f);
    }

    internal static string FormatUnlockTime(DateTime utc) => utc.ToLocalTime().ToString("g");

    internal static void Prewarm()
    {
        _ = AchievementIcon;
        _ = SearchIcon;
        _ = SearchIconHover;
        _ = SearchClear;
        _ = SearchClearHover;
        _ = SearchGlyph;
        _ = SearchClearGlyph;
        _ = CloseButtonSprite;
        _ = ClickSound;
        _ = HoverSound;
    }

    // "BottomCenter" -> "Bottom Center"
    internal static string EnumDisplayName(Enum value)
        => System.Text.RegularExpressions.Regex.Replace(value.ToString(), "(\\B[A-Z])", " $1");

    // Re-parents `target` next to `source` and copies its anchors/pivot/offsets so it covers the same area.
    internal static void MatchRect(RectTransform target, RectTransform source)
    {
        target.SetParent(source.parent, false);
        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.pivot = source.pivot;
        target.offsetMin = source.offsetMin;
        target.offsetMax = source.offsetMax;
        // Draw over the lists
        target.SetAsLastSibling();
    }

    internal static Color RarityColor(int rarity) => AchievementsTheme.Current.RarityColor(rarity);

    // Progress-bar fills: the rarity color, but the teal AccentHover for
    // common so the fill does not sit gray-on-gray against the ring/track.
    internal static Color ProgressColor(int rarity) => rarity > 0 ? RarityColor(rarity) : (Color) AccentHover;

    // 0-100 completion percentage for a countable progress pair.
    internal static int Percent(int current, int required)
        => required > 0 ? Mathf.Clamp(Mathf.RoundToInt(100f * current / required), 0, 100) : 100;

    internal static string RarityName(int rarity)
        => AchievementsStrings.GetText(AchievementsStrings.RarityName(rarity));
}
