using System.Collections;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static AchievementsAPI.MenuUi;

namespace AchievementsAPI;

/// <summary>
/// Modal card over a dimmed scrim for destructive confirmations, styled like
/// the rest of the achievements menu. Clicking the scrim or the cancel button
/// dismisses it; the confirm callback runs before it closes.
/// </summary>
public static class AchievementsConfirmDialog
{
    /// <summary>
    /// Shows the dialog under <paramref name="host"/>. <paramref name="close"/> receives the
    /// single close path (destroys the dialog and runs <paramref name="onClose"/>).
    /// </summary>
    public static GameObject Show(Transform host, string title, string body,
        string confirmLabel, string cancelLabel, System.Action onConfirm,
        System.Action onClose, out System.Action close)
    {
        var root = new GameObject("ConfirmDialog");
        var rootRect = root.AddComponent<RectTransform>();
        rootRect.SetParent(host, false);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        rootRect.SetAsLastSibling();

        void Close()
        {
            root.Destroy();
            onClose();
        }
        close = Close;

        // Click-outside cancels; navigation mode None keeps the scrim out of
        // controller focus while still accepting pointer clicks.
        var scrim = CreateImage(rootRect, "Scrim", null, new Color(0f, 0f, 0f, 0f));
        var scrimRect = scrim.rectTransform;
        scrimRect.anchorMin = Vector2.zero;
        scrimRect.anchorMax = Vector2.one;
        scrimRect.offsetMin = Vector2.zero;
        scrimRect.offsetMax = Vector2.zero;
        var scrimButton = scrim.gameObject.AddComponent<Button>();
        scrimButton.transition = Selectable.Transition.None;
        scrimButton.navigation = new Navigation { mode = Navigation.Mode.None };
        scrimButton.onClick.AddListener((UnityAction)(() => Close()));

        var card = CreateCard(rootRect, "Card");
        var cardRect = card.rectTransform;
        cardRect.anchorMin = cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.sizeDelta = new Vector2(680f, 340f);
        cardRect.localScale = new Vector3(StartScale, StartScale, 1f);
        var group = card.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        Coroutines.Start(CoPopIn(cardRect, group, scrim));

        var titleText = CreateText(cardRect, "Title", title, 34f, Color.white,
            FontStyles.Bold, TextAlignmentOptions.Center);
        titleText.overflowMode = TextOverflowModes.Overflow;
        var titleRect = titleText.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -36f);
        titleRect.sizeDelta = new Vector2(-80f, 48f);

        var bodyText = CreateText(cardRect, "Body", body, 22f, ValueColor,
            FontStyles.Normal, TextAlignmentOptions.Center);
        bodyText.enableWordWrapping = true;
        var bodyRect = bodyText.rectTransform;
        bodyRect.anchorMin = new Vector2(0f, 1f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.pivot = new Vector2(0.5f, 1f);
        bodyRect.anchoredPosition = new Vector2(0f, -100f);
        bodyRect.sizeDelta = new Vector2(-96f, 120f);

        var cancel = CreateButton(cardRect, cancelLabel, new Vector2(-110f, 64f), "Back");
        var confirm = CreateButton(cardRect, confirmLabel, new Vector2(110f, 64f), "Confirm", danger: true);
        AttachGameSounds(cancel.gameObject);
        AttachGameSounds(confirm.gameObject);

        cancel.navigation = new Navigation
        {
            mode = Navigation.Mode.Explicit,
            selectOnLeft = confirm,
            selectOnRight = confirm
        };
        confirm.navigation = new Navigation
        {
            mode = Navigation.Mode.Explicit,
            selectOnLeft = cancel,
            selectOnRight = cancel
        };

        cancel.onClick.AddListener((UnityAction)(() => Close()));
        confirm.onClick.AddListener((UnityAction)(() =>
        {
            onConfirm();
            Close();
        }));

        return root;
    }

    private const float StartScale = 0.85f;
    private const float ScrimAlpha = 0.55f;

    // Short and front-loaded: the fade is eased out so the card is nearly
    // opaque by the halfway point and only the scale overshoot is left to read.
    private static IEnumerator CoPopIn(RectTransform rect, CanvasGroup group, Image scrim)
    {
        const float duration = 0.12f;
        for (var t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            if (rect == null) yield break;
            var k = Mathf.Clamp01(t / duration);
            var s = Mathf.Lerp(StartScale, 1f, EaseOutBack(k));
            rect.localScale = new Vector3(s, s, 1f);
            var fade = EaseOutCubic(k);
            group.alpha = fade;
            scrim.color = new Color(0f, 0f, 0f, ScrimAlpha * fade);
            yield return null;
        }
        if (rect == null) yield break;
        rect.localScale = Vector3.one;
        group.alpha = 1f;
        scrim.color = new Color(0f, 0f, 0f, ScrimAlpha);
    }
}
