using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AchievementsAPI.API;

[Serializable]
public class AchievementData
{
    /// <summary>
    /// The achievement's name as it was last displayed. Informational only (kept for save file
    /// compatibility); achievements are identified by <see cref="Id"/>, which never changes
    /// with the display language.
    /// </summary>
    public string Name { get; set; }
    public bool Unlocked { get; set; }
    public int Progress { get; set; }
    public string Id { get; set; }
    public DateTime? UnlockedAt { get; set; }
}


public class AchievementStorage
{
    public static List<AchievementData> BaseAchievements = new List<AchievementData>();

    private static readonly Dictionary<string, AchievementData> _index = new();
    private static bool _saveScheduled;
    private static bool _dirty;

    private static void EnsureIndex()
    {
        if (_index.Count == BaseAchievements.Count) return;
        _index.Clear();
        foreach (var data in BaseAchievements) _index[data.Id] = data;
    }

    private static AchievementData FindData(string id)
    {
        EnsureIndex();
        return _index.TryGetValue(id, out var data) ? data : null;
    }

    private static void AddData(AchievementData data)
    {
        BaseAchievements.Add(data);
        EnsureIndex();
        _index[data.Id] = data;
    }

    private static void RequestSave()
    {
        _dirty = true;
        if (_saveScheduled) return;
        _saveScheduled = true;
        Coroutines.Start(CoSave());
    }

    /// <summary>
    /// Flushes any pending debounced save to disk immediately.
    /// </summary>
    public static void FlushPendingSave()
    {
        if (!_saveScheduled && !_dirty) return;
        _saveScheduled = false;
        Save();
    }

    private static System.Collections.IEnumerator CoSave()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        if (!_saveScheduled) yield break;
        _saveScheduled = false;
        Save();
    }
    public static string JsonPath => OperatingSystem.IsAndroid() ? Path.Combine(Environment.GetEnvironmentVariable("STAR_DATA_PATH"), "AchievementsAPIData/achievements.json") : Path.Combine(Application.persistentDataPath, "AchievementsAPIData/achievements.json");

    public static void AchievementStorageUpdate(BaseAchievement achievement, bool unlocked)
    {
        var data = GetData(achievement);
        SetUnlocked(data, unlocked);

        RequestSave();
    }

    // Records the UTC time when an achievement transitions to unlocked, and
    // clears it when it goes back to locked.
    private static void SetUnlocked(AchievementData data, bool unlocked)
    {
        if (unlocked && !data.Unlocked) data.UnlockedAt = DateTime.UtcNow;
        else if (!unlocked) data.UnlockedAt = null;
        data.Unlocked = unlocked;
    }

    public static void AchievementStorageUpdate(CountAchievement achievement, int progress, bool unlocked)
    {
        var data = GetData(achievement);
        var newProgress = Math.Clamp(progress, 0, achievement.RequiredValue);
        SetUnlocked(data, unlocked);
        // Runtime storage always tracks progress; Save() filters transient
        // progress for disk based on ProgressPersists.
        data.Progress = newProgress;

        RequestSave();
    }
    public static void AchievementDataReset(BaseAchievement achievement, bool? unlocked = false)
    {
        AchievementDataReset(achievement, unlocked, true);
    }

    public static void AchievementDataReset(BaseAchievement achievement, bool? unlocked, bool save)
    {
        var data = GetData(achievement);
        if (unlocked != null)
        {
            achievement.Unlocked = unlocked.Value;
            SetUnlocked(data, unlocked.Value);
        }

        if (save) Save();
    }

    public static void AchievementDataReset(CountAchievement achievement, int progress = 0, bool? unlocked = null)
    {
        AchievementDataReset(achievement, progress, unlocked, true);
    }

    public static void AchievementDataReset(CountAchievement achievement, int progress, bool? unlocked, bool save)
    {
        var data = GetData(achievement);
        data.Progress = Math.Clamp(progress, 0, achievement.RequiredValue);
        if (unlocked != null)
        {
            achievement.Unlocked = unlocked.Value;
            SetUnlocked(data, unlocked.Value);
        }
        else
        {
            SetUnlocked(data, data.Progress == achievement.RequiredValue);
            achievement.Unlocked = data.Unlocked;
        }

        if (save) Save();
    }

    public static AchievementData GetData(BaseAchievement achievement)
    {
        var data = FindData(achievement.Id);
        if (data == null)
        {
            data = new AchievementData { Id = achievement.Id, Name = achievement.GetName(), Unlocked = false };
            AddData(data);
        }
        return data;
    }

    public static AchievementData GetData(CountAchievement achievement)
    {
        var data = FindData(achievement.Id);
        if (data == null)
        {
            // Bug 1 fixed: Id was missing, causing Find() to never match on reload
            data = new AchievementData
            {
                Id = achievement.Id,
                Name = achievement.GetName(),
                Unlocked = false,
                Progress = achievement.CurrentValue
            };
            AddData(data);
        }
        return data;
    }

    public static void AchievementStorageGet(AchievementsTab tab)
    {
        foreach (var achievement in tab.GetAchievements())
        {
            AchievementStorageGet(achievement);
        }
    }

    public static void AchievementStorageGet(BaseAchievement achievement)
    {
        var data = FindData(achievement.Id);
        if (data == null) return;
        achievement.Unlocked = data.Unlocked;
        if (achievement is CountAchievement countAchievement)
        {
            countAchievement.CurrentValue = data.Progress;
        }
    }

    /// <summary>
    /// Wipes every achievement (registered or stale) back to locked with zero progress and rewrites the save file.
    /// </summary>
    public static void ResetAll()
    {
        _saveScheduled = false;
        BaseAchievements.Clear();
        _index.Clear();
        foreach (var tab in AchievementsManager.Tabs)
            foreach (var achievement in tab.GetAchievements())
                achievement.Reset(false);
        Save();
    }

    public static void Save()
    {
        var lookup = new Dictionary<string, BaseAchievement>();
        foreach (var achievement in AchievementsManager.Tabs.SelectMany(t => t.GetAchievements()))
        {
            lookup.TryAdd(achievement.Id, achievement);
        }

        var persisted = BaseAchievements.Select(data =>
        {
            lookup.TryGetValue(data.Id, out var achievement);
            var progress = achievement is CountAchievement count &&
                           count.ProgressPersists != AchPersistence.ThroughoutSessions &&
                           !data.Unlocked
                ? 0
                : data.Progress;
            return new AchievementData
            {
                Name = data.Name,
                Id = data.Id,
                Unlocked = data.Unlocked,
                Progress = progress,
                UnlockedAt = data.UnlockedAt
            };
        }).ToList();

        byte[] bytes;
        try
        {
            bytes = AchievementCrypto.Encrypt(JsonSerializer.Serialize(persisted));
        }
        catch (Exception e)
        {
            AchievementsAPIPlugin.Instance.Log.LogWarning($"Failed to serialize achievements save: {e}");
            return;
        }

        var delays = new[] { 50, 100, 200, 400, 800 };
        for (var attempt = 0; attempt < delays.Length; attempt++)
        {
            try
            {
                var directory = Path.GetDirectoryName(JsonPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var tmp = JsonPath + ".tmp";
                File.WriteAllBytes(tmp, bytes);
                File.Move(tmp, JsonPath, true);
                _dirty = false;
                return;
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                if (attempt + 1 < delays.Length) Thread.Sleep(delays[attempt]);
            }
        }

        AchievementsAPIPlugin.Instance.Log.LogWarning($"Failed to write achievements save to {JsonPath}; will retry.");
        _dirty = true;
        RequestSave();
    }

    // Corrupt files (bad HMAC, undecryptable, or invalid JSON) are backed up
    // next to the original and replaced with a fresh store.
    private static void HandleCorruptFile(string reason)
    {
        try
        {
            File.Copy(JsonPath, JsonPath + ".corrupt", true);
        }
        catch (Exception e)
        {
            AchievementsAPIPlugin.Instance.Log.LogWarning($"Failed to back up corrupt achievements file: {e}");
        }
        AchievementsAPIPlugin.Instance.Log.LogWarning($"Achievements file unreadable ({reason}); backed up to achievements.json.corrupt and starting fresh.");
        BaseAchievements = new List<AchievementData>();
    }

    public static void Load()
    {
        if (!File.Exists(JsonPath))
        {
            BaseAchievements = new List<AchievementData>();
            return;
        }

        var bytes = File.ReadAllBytes(JsonPath);
        if (bytes.Length == 0)
        {
            BaseAchievements = new List<AchievementData>();
            return;
        }

        var encrypted = AchievementCrypto.IsEncrypted(bytes);
        string json;
        if (encrypted)
        {
            if (!AchievementCrypto.TryDecrypt(bytes, out json))
            {
                HandleCorruptFile("decryption or integrity check failed");
                return;
            }
        }
        else
        {
            json = Encoding.UTF8.GetString(bytes);
            if (string.IsNullOrWhiteSpace(json))
            {
                BaseAchievements = new List<AchievementData>();
                return;
            }
        }

        try
        {
            BaseAchievements =
                JsonSerializer.Deserialize<List<AchievementData>>(json)
                ?? new List<AchievementData>();
        }
        catch (Exception e)
        {
            HandleCorruptFile(e.Message);
            return;
        }

        // Migrate legacy plaintext saves to the encrypted format.
        if (!encrypted) Save();
    }
}
