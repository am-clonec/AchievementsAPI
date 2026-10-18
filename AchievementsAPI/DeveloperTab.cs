using AchievementsAPI.API;
using UnityEngine;

namespace AchievementsAPI;

/// <summary>
/// Tab for developer-encounter achievements. Unlocking is driven by
/// <see cref="DeveloperPresence"/>: a developer-only plugin announces itself over
/// RPC, which unlocks <see cref="PlayedWithDeveloper"/> for everyone else. The tab
/// stays completely out of the menu and stats until the achievement is unlocked.
/// </summary>
public class DeveloperTab : AchievementsTab
{
    public override StringNames Name => AchievementsStrings.DeveloperTabName;
    public override bool IsSelectable => PlayedWithDeveloper.Unlocked;
    public override Sprite GetIcon() => Assets.LoadSprite("AchievementsIcon");

    public BaseAchievement PlayedWithDeveloper { get; set; } = new BaseAchievement(
        "Among Developers", "Play a game with a developer.", Assets.LoadSprite("AchievementsIcon"), rarity: 3, hidden: true);
}
