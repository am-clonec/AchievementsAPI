using System.Collections.Generic;
using AchievementsAPI.API;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static AchievementsAPI.MenuUi;

namespace AchievementsAPI;

/// <summary>
/// Procedurally built toast settings panel shown as a tab inside the
/// achievements menu, bound directly to the <see cref="AchievementsConfig"/>
/// entries.
/// </summary>
public static class AchievementsSettingsPanel
{
    /// <summary>
    /// Creates the settings panel (hidden until toggled by the caller).
    /// </summary>
    /// <param name="parent">The menu root transform.</param>
    /// <param name="onResetAll">Invoked when the reset-all row's button is clicked.</param>
    /// <returns>The created settings panel.</returns>
    public static GameObject Create(Transform parent, System.Action onResetAll)
    {
        var panel = CreatePanel(parent, onResetAll);
        panel.SetActive(false);
        return panel;
    }

    private static GameObject CreatePanel(Transform parent, System.Action onResetAll)
    {
        // Plain container: the rows are the same charcoal boxes as the menu's
        // buttons, so no card behind them. The menu positions it (MenuUi.MatchRect).
        var panel = new GameObject("ToastSettingsPanel");
        var rect = panel.AddComponent<RectTransform>();
        rect.SetParent(parent, false);

        // Full-width row strips, 76px pitch, first strip below the frame edge
        const float rowsTop = -4f;
        CreateToggleRow(CreateRowStrip(rect, rowsTop), AchievementsStrings.GetText(AchievementsStrings.SettingToasts), AchievementsConfig.ToastsEnabled);
        CreateToggleRow(CreateRowStrip(rect, rowsTop - 76f), AchievementsStrings.GetText(AchievementsStrings.SettingSound), AchievementsConfig.ToastSoundEnabled);
        CreateSliderRow(CreateRowStrip(rect, rowsTop - 152f), AchievementsStrings.GetText(AchievementsStrings.SettingVolume), AchievementsConfig.ToastVolume, 0f, 1f,
            v => $"{Mathf.RoundToInt(v * 100f)}%", onStep: AchievementToast.PlayVolumePreview);
        CreateSliderRow(CreateRowStrip(rect, rowsTop - 228f), AchievementsStrings.GetText(AchievementsStrings.SettingDuration), AchievementsConfig.ToastDuration, 1f, 10f,
            v => $"{v:0.0}s");
        CreateSliderRow(CreateRowStrip(rect, rowsTop - 304f), AchievementsStrings.GetText(AchievementsStrings.SettingRareDuration), AchievementsConfig.RareToastDuration, 1f, 10f,
            v => $"{v:0.0}s");
        CreateCycleRow(CreateRowStrip(rect, rowsTop - 380f), AchievementsStrings.GetText(AchievementsStrings.SettingPosition), AchievementsConfig.Position);
        CreateToggleRow(CreateRowStrip(rect, rowsTop - 456f), AchievementsStrings.GetText(AchievementsStrings.SettingTrackpadMode), AchievementsConfig.TrackpadMode);
        CreateActionRow(CreateRowStrip(rect, rowsTop - 532f), AchievementsStrings.GetText(AchievementsStrings.SettingResetAll),
            AchievementsStrings.GetText(AchievementsStrings.ResetAllButton), onResetAll);

        var preview = CreateButton(rect, AchievementsStrings.GetText(AchievementsStrings.Preview), Vector2.zero, "Preview");
        var previewRect = preview.GetComponent<RectTransform>();
        previewRect.anchorMin = previewRect.anchorMax = new Vector2(1f, 0f);
        previewRect.pivot = new Vector2(1f, 0.5f);
        previewRect.anchoredPosition = new Vector2(0f, 40f);
        preview.onClick.AddListener((UnityAction)(() => AchievementToast.ShowPreview()));
        AttachGameSounds(preview.gameObject);

        return panel;
    }

    private static RectTransform CreateRowStrip(Transform panel, float topY)
    {
        var strip = CreateImage(panel, "Row", ToastSprites.ButtonBox, Color.white);
        strip.type = Image.Type.Sliced;
        var rect = strip.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, topY);
        rect.sizeDelta = new Vector2(0f, 64f);
        return rect;
    }

    private static void CreateRowLabel(Transform strip, string text)
    {
        var label = CreateText(strip, "Label", text, 24f, TextColor);
        var rect = label.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(24f, 0f);
        rect.sizeDelta = new Vector2(320f, 32f);
    }

    private static void ApplySwitchState(RectTransform knob, Image track, bool on)
    {
        knob.anchorMin = knob.anchorMax = new Vector2(on ? 1f : 0f, 0.5f);
        knob.pivot = new Vector2(on ? 1f : 0f, 0.5f);
        knob.anchoredPosition = new Vector2(on ? -3f : 3f, 0f);
        track.color = on ? AccentColor : TrackOffColor;
    }

    private static void CreateToggleRow(Transform strip, string label, ConfigEntry<bool> entry)
    {
        CreateRowLabel(strip, label);

        var track = CreateImage(strip, $"{label}Track", ToastSprites.RoundedRectTiny, TrackOffColor);
        track.type = Image.Type.Sliced;
        var trackRect = track.rectTransform;
        trackRect.anchorMin = trackRect.anchorMax = new Vector2(1f, 0.5f);
        trackRect.pivot = new Vector2(1f, 0.5f);
        trackRect.anchoredPosition = new Vector2(-24f, 0f);
        trackRect.sizeDelta = new Vector2(64f, 32f);

        var knob = CreateImage(trackRect, "Knob", ToastSprites.RoundedRectTiny, Color.white);
        knob.type = Image.Type.Sliced;
        knob.raycastTarget = false;
        var knobRect = knob.rectTransform;
        knobRect.sizeDelta = new Vector2(34f, 24f);

        var toggle = track.gameObject.AddComponent<Toggle>();
        toggle.graphic = null;
        toggle.targetGraphic = track;
        toggle.transition = Selectable.Transition.ColorTint;
        var colors = toggle.colors;
        colors.normalColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = Color.white;
        colors.selectedColor = Color.white;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        toggle.colors = colors;
        toggle.isOn = entry.Value;
        ApplySwitchState(knobRect, track, entry.Value);
        toggle.onValueChanged.AddListener((UnityAction<bool>)(v =>
        {
            entry.Value = v;
            ApplySwitchState(knobRect, track, v);
        }));
        AttachGameSounds(track.gameObject);
    }

    private static void CreateCycleRow<T>(Transform strip, string label, ConfigEntry<T> entry) where T : struct, System.Enum
    {
        var values = System.Enum.GetValues(typeof(T));
        var names = new List<string>();
        foreach (var v in values) names.Add(EnumDisplayName((T) v));
        var initial = System.Array.IndexOf(values, entry.Value);
        CreateListCycleRow(strip, label, names, initial, i => entry.Value = (T) values.GetValue(i));
    }

    // Shared "< value >" chrome for a row that steps through a list of names.
    private static void CreateListCycleRow(Transform strip, string label, List<string> names,
        int initial, System.Action<int> onChanged)
    {
        CreateRowLabel(strip, label);

        Button Arrow(string name, string text, float x)
        {
            var bg = CreateImage(strip, name, ToastSprites.RoundedRectTiny, Color.white);
            bg.type = Image.Type.Sliced;
            var rect = bg.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, 0f);
            rect.sizeDelta = new Vector2(36f, 36f);

            var labelText = CreateText(rect, "Label", text, 22f, TextColor, FontStyles.Bold, TextAlignmentOptions.Center);
            labelText.rectTransform.anchorMin = Vector2.zero;
            labelText.rectTransform.anchorMax = Vector2.one;
            labelText.rectTransform.offsetMin = Vector2.zero;
            labelText.rectTransform.offsetMax = Vector2.zero;

            var button = bg.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = AccentColor;
            colors.highlightedColor = AccentHover;
            colors.pressedColor = PressedColor;
            colors.selectedColor = AccentColor;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            AttachGameSounds(bg.gameObject);
            return button;
        }

        var box = CreateImage(strip, $"{label}ValueBox", ToastSprites.RoundedRectTiny, AchievementsTheme.Current.CardFace);
        box.type = Image.Type.Sliced;
        var boxRect = box.rectTransform;
        boxRect.anchorMin = boxRect.anchorMax = new Vector2(1f, 0.5f);
        boxRect.pivot = new Vector2(0.5f, 0.5f);
        boxRect.anchoredPosition = new Vector2(-152f, 0f);
        boxRect.sizeDelta = new Vector2(200f, 36f);

        var index = Mathf.Clamp(initial, 0, Mathf.Max(0, names.Count - 1));
        var valueText = CreateText(boxRect, "Value", names.Count > 0 ? names[index] : "", 20f, TextColor,
            FontStyles.Normal, TextAlignmentOptions.Center);
        var valueRect = valueText.rectTransform;
        valueRect.anchorMin = Vector2.zero;
        valueRect.anchorMax = Vector2.one;
        valueRect.offsetMin = Vector2.zero;
        valueRect.offsetMax = Vector2.zero;

        void Step(int dir)
        {
            if (names.Count == 0) return;
            index = (index + dir + names.Count) % names.Count;
            valueText.text = names[index];
            onChanged(index);
        }

        Arrow($"{label}Left", "<", -278f).onClick.AddListener((UnityAction)(() => Step(-1)));
        Arrow($"{label}Right", ">", -26f).onClick.AddListener((UnityAction)(() => Step(1)));
    }

    // Label plus a danger-tinted pill button pinned to the strip's right edge.
    private static void CreateActionRow(Transform strip, string label, string buttonText, System.Action onClick)
    {
        CreateRowLabel(strip, label);

        var bg = CreateImage(strip, "ResetAllButton", ToastSprites.RoundedRectTiny, Color.white);
        bg.type = Image.Type.Sliced;
        var rect = bg.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = new Vector2(-24f, 0f);
        rect.sizeDelta = new Vector2(120f, 36f);

        var labelText = CreateText(rect, "Label", buttonText, 20f, TextColor, FontStyles.Bold, TextAlignmentOptions.Center);
        labelText.rectTransform.anchorMin = Vector2.zero;
        labelText.rectTransform.anchorMax = Vector2.one;
        labelText.rectTransform.offsetMin = Vector2.zero;
        labelText.rectTransform.offsetMax = Vector2.zero;

        var button = bg.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        var colors = button.colors;
        colors.normalColor = DangerColor;
        colors.highlightedColor = DangerHover;
        colors.pressedColor = DangerPressed;
        colors.selectedColor = DangerColor;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        button.colors = colors;
        button.onClick.AddListener((UnityAction)(() => onClick()));
        AttachGameSounds(bg.gameObject);
    }

    private static void CreateSliderRow(Transform strip, string label, ConfigEntry<float> entry,
        float min, float max, System.Func<float, string> format, System.Action<float>? onStep = null)
    {
        CreateRowLabel(strip, label);

        var valueLabel = CreateText(strip, $"{label}Value", format(entry.Value), 22f,
            ValueColor, FontStyles.Normal, TextAlignmentOptions.MidlineRight);
        var valueRect = valueLabel.rectTransform;
        valueRect.anchorMin = valueRect.anchorMax = new Vector2(1f, 0.5f);
        valueRect.pivot = new Vector2(1f, 0.5f);
        valueRect.anchoredPosition = new Vector2(-24f, 0f);
        valueRect.sizeDelta = new Vector2(72f, 28f);

        var bg = CreateImage(strip, $"{label}Slider", ToastSprites.RoundedRectTiny, TrackOffColor);
        bg.type = Image.Type.Sliced;
        var bgRect = bg.rectTransform;
        bgRect.anchorMin = bgRect.anchorMax = new Vector2(1f, 0.5f);
        bgRect.pivot = new Vector2(1f, 0.5f);
        bgRect.anchoredPosition = new Vector2(-112f, 0f);
        bgRect.sizeDelta = new Vector2(220f, 10f);

        var fill = CreateImage(bgRect, "Fill", ToastSprites.RoundedRectTiny, AccentColor);
        fill.type = Image.Type.Sliced;
        var fillRect = fill.rectTransform;
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(0f, 0f);

        var handle = CreateImage(bgRect, "Handle", ToastSprites.RoundedRectTiny, Color.white);
        handle.type = Image.Type.Sliced;
        var handleRect = handle.rectTransform;
        handleRect.anchorMin = handleRect.anchorMax = new Vector2(0f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.anchoredPosition = Vector2.zero;
        handleRect.sizeDelta = new Vector2(26f, 26f);

        var slider = bg.gameObject.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = false;
        slider.value = entry.Value;
        // The config file hits disk on every `entry.Value` set, so writes are
        // debounced to label-visible steps plus one final write on release.
        var lastText = format(entry.Value);
        slider.onValueChanged.AddListener((UnityAction<float>)(v =>
        {
            var text = format(v);
            if (text == lastText) return;
            lastText = text;
            valueLabel.text = text;
            entry.Value = v;
            onStep?.Invoke(v);
        }));

        var trigger = bg.gameObject.GetComponent<EventTrigger>() ?? bg.gameObject.AddComponent<EventTrigger>();
        var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((UnityAction<BaseEventData>)(_ => entry.Value = slider.value));
        trigger.triggers.Add(pointerUp);
    }
}
