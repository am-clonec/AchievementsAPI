using System.Reflection;
using AchievementsAPI.Patches.MainMenu;
using HarmonyLib;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace AchievementsAPI;

[HarmonyPatch(typeof(OptionsMenuBehaviour))]
public static class OptionsMenuPatches
{
    internal static OptionsMenuBehaviour? Instance { get; private set; }
    internal static BoxCollider2D MaskCollider = null!;
    private static SpriteRenderer? background;

    private static int currentPage = 1;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(OptionsMenuBehaviour.Start))]
    public static void StartPostfix(OptionsMenuBehaviour __instance)
    {
        Instance = __instance;
        background = __instance.Background;
        // Fix for tabs not being clickable in the main menu
        if (!AmongUsClient.Instance.IsInGame)
        {
            return;
        }

        var maskObj = new GameObject
        {
            layer = 5,
            name = "SpriteMask",
        };
        maskObj.transform.SetParent(__instance.transform);
        maskObj.transform.localPosition = new Vector3(0, -0.3f, 0);
        maskObj.transform.localScale = new Vector3(500, 120, 1);

        var blank = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
            new Vector2(0.5f, 0.5f));

        var mask = maskObj.AddComponent<SpriteMask>();
        mask.sprite = blank;
        mask.isCustomRangeActive = true;
        mask.sortingLayerName = "Default";
        mask.backSortingOrder = 100;
        mask.frontSortingOrder = 500;

        MaskCollider = maskObj.AddComponent<BoxCollider2D>();
        MaskCollider.size = new Vector2(0.01f, 0.1f);
        MaskCollider.isTrigger = true;
        MaskCollider.enabled = true;

        currentPage = 1;
        float yOffset = 0;
        int i = 0;
        int tabIdx = 0;
        int page = 1;
        var button = CreateTabButton(__instance, ref tabIdx, ref yOffset);
    }

    /// <summary>
    /// Creates the tab button.
    /// </summary>
    /// <param name="instance">The <see cref="OptionsMenuBehaviour"/> instance.</param>
    /// <param name="tabIdx">The tab index.</param>
    /// <param name="offset">The current button offset.</param>
    /// <returns>The created button <see cref="GameObject"/>.</returns>
    public static GameObject CreateTabButton(OptionsMenuBehaviour instance, ref int tabIdx, ref float offset)
    {
        var tabButtonObject = Object.Instantiate(instance.Tabs[0], instance.transform);
        tabButtonObject.name = $"Achievements Button";
        tabButtonObject.transform.localPosition = new Vector3(-2.4f, 2.1f - offset, 5.5f);
        tabButtonObject.transform.localScale = new Vector3(1.25f, 1.25f, 1);
        var color = new Color32(150, 150, 50, 255);
        var colorHover = new Color32(200, 200, 100, 255);
        tabButtonObject.Button.color = color;

        var tabButtonText = tabButtonObject.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>();
        tabButtonText.gameObject.Destroy();

        var tabButton = tabButtonObject.GetComponent<PassiveButton>();
        var rollover = tabButtonObject.Rollover;
        rollover.OverColor = colorHover;
        rollover.OutColor = color;

        var tabButtonRend = new GameObject("sprite").AddComponent<SpriteRenderer>();
        tabButtonRend.gameObject.layer = 5; // ui layer
        tabButtonRend.transform.SetParent(tabButtonObject.transform);
        tabButtonRend.transform.localPosition = new Vector3(-0.5f, 0, -2);
        tabButtonRend.transform.localScale = new Vector3(0.2f, 0.2f, 1);
        tabButtonRend.sprite = Assets.LoadSprite("AchievementsIcon");

        tabButton.OnClick.AddListener((UnityAction)(() => { AchievementsMenuOpen.OpenMenu(instance); }));

        return tabButtonObject.gameObject;
    }
}