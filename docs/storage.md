# Storage

Achievement state survives game restarts in an encrypted save file owned by the API. `Unlock()`, `SetValue()` and `Reset()` write through automatically, so you rarely touch the file directly. The layout still matters if you're debugging a save issue or shipping a save migration.

## Location

`AchievementsAPIData/achievements.json` under:

- Windows/other: the game's persistent data path (`Application.persistentDataPath`)
- Android: `STAR_DATA_PATH` (Star's mod loader data directory)

Despite the extension, the file on disk is not plain JSON (anymore; see migration below).

## File format

`AchievementCrypto` wraps the JSON payload:

```
5 bytes   magic "ACHV1"
16 bytes  AES IV (random per save)
32 bytes  HMAC-SHA256 over IV + ciphertext
n bytes   AES-256-CBC (PKCS7) ciphertext of the UTF-8 JSON
```

The keys are fixed and compiled into the DLL. The point is tamper-evidence, not secrecy: the file can't be edited by hand without failing the integrity check, but don't treat it as protection against a determined reader.

The decrypted payload is a list of `AchievementData` records:

```json
[
  {
    "Name": "First Win",
    "Id": "MyMod_FirstWin",
    "Unlocked": true,
    "Progress": 0,
    "UnlockedAt": "2026-10-15T21:30:00Z"
  }
]
```

Achievements are matched by `Id`; `Name` is informational only (kept for save compatibility and readable debugging dumps). `Progress` is clamped into `[0, RequiredValue]`, and unfinished progress is written as 0 for achievements whose `ProgressPersists` isn't `ThroughoutSessions`, so the file on disk already reflects the next session's starting state.

## Reads and writes

- All state lives in memory (`AchievementStorage.BaseAchievements`, indexed by id) after one load at startup; lookups never re-read the file.
- `Unlock()` saves immediately. An unlock is never left in memory long enough to lose.
- Progress updates are batched: `SetValue`/`Increment` schedule a save roughly half a second out, so rapid increments (e.g. per-task completions) coalesce into one write instead of causing stutter.
- `AchievementStorage.Save()` forces a write; `AchievementDataReset(...)` and the [lifecycle resets](game-events.md) use it after batching.
- `AchievementStorage.ResetAll()` clears the whole store (including entries for achievements that are no longer registered), runs `Reset(false)` on every registered achievement and saves once. This is what the menu's [Reset all achievements](menu-and-toasts.md#resetting-all-achievements) button calls after the player confirms.

## Failure handling

On load, anything unreadable (bad HMAC, undecryptable content, invalid JSON) is treated as corruption: the file is copied next to itself as `achievements.json.corrupt`, a warning is logged, and the store starts fresh. Nothing is dropped silently without a backup.

Legacy plaintext saves (from before encryption was added) are detected by the missing magic header, loaded as-is, and immediately re-saved in the encrypted format.

## Keeping ids stable

Because ids are the only join key between the file and live achievements:

- Don't change the assembly name of a shipping mod; it changes every id.
- Pass your `Assembly` explicitly when registering through [helpers](getting-started.md#soft-dependency-optional-achievements-api).
- Register `StringNames` deterministically (see [localization](localization.md#ids-are-language-independent)).
