// here too, this wouldn't have existed without Pix!


using System.Reflection;
using AchievementsAPI.MainMenu;
using UnityEngine;

namespace AchievementsAPI.Patches.MainMenu;

public static class MainMenuButtons
{
    public static PassiveButton button;

    public static void SetUp(MainMenuManager menu)
    {
        var sprit = Assets.LoadSprite("AchievementsButtonActive");
        var sprit2 = Assets.LoadSprite("AchievementsButton");
        button = PassiveButtonStuff.CreatePassiveButton("AchievementsMenuButton", sprit2, sprit, new Vector2(1f, 1f), () => AchievementsMenuOpen.OpenMenu(menu));
        menu.StartCoroutine(Effects.ActionAfterDelay(0.1f, new System.Action(() =>
        {
            button.activeSprites.GetComponent<SpriteRenderer>().color = Color.white;
            button.inactiveSprites.GetComponent<SpriteRenderer>().color = Color.white;
        })));
        button.transform.SetParent(menu.transform.FindChild("MainUI/AspectScaler"));
        var aspect = button.gameObject.AddComponent<AspectPosition>();
        aspect.Alignment = AspectPosition.EdgeAlignments.RightTop;
        aspect.DistanceFromEdge = new Vector3(0.3f, 1.45f, 0.1f);
        aspect.updateAlways = true;
        aspect.AdjustPosition();
    }
}