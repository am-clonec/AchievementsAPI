using UnityEngine;

namespace AchievementsAPI.API;

/// <summary>
/// A read-only snapshot of one registered achievement and its current state, resolved for
/// display: names in the current language, hidden achievements masked the way the menu masks
/// them, and progress filled in for <see cref="CountAchievement"/>s.
/// </summary>
public sealed record AchievementInfo(
    BaseAchievement Achievement,
    AchievementsTab Tab,
    AchievementCategory Category,
    string ModName,
    string Id,
    string Name,
    string Description,
    Sprite? Icon,
    int Rarity,
    bool RarityVisible,
    int Points,
    bool Hidden,
    bool Unlocked,
    DateTime? UnlockedAt,
    int? Progress,
    int? Required)
{
    /// <summary>0-1 completion; 1 when unlocked, 0 for locked non-count achievements.</summary>
    public float Fraction => Unlocked ? 1f : Required is > 0 && Progress is { } p ? Mathf.Clamp01((float) p / Required.Value) : 0f;
}

/// <summary>Aggregate totals over a set of achievements, as shown on the menu's stats card.</summary>
public sealed record AchievementStats(int Total, int Unlocked, int EarnedPoints, int TotalPoints, IReadOnlyDictionary<int, (int Unlocked, int Total)> ByRarity);
