# Game events

Three systems hang off the game's lifecycle: automatic resets, progress persistence, and deferred toasts. They're all driven by a handful of Harmony patches in `AchievementLifecycle`, so the events line up with real game moments.

## Events

| Event | Fires on |
| --- | --- |
| Round start | `IntroCutscene.OnDestroy`, `ExileController.ReEnableGameplay` |
| Meeting start | `MeetingHud.Start` |
| Meeting end | `MeetingHud.Close` |
| Game end | `PlayerControl.OnGameEnd`, `AchievementManager.OnMatchExited`, `LobbyBehaviour.Start` (covers finishing, leaving, and returning to the lobby) |
| Match start | `AchievementManager.OnMatchStart`, `TutorialManager.Awake` |

## Reset triggers

`BaseAchievement.ResetTrigger` (`AchResetTrigger`) resets the achievement when its event fires. Resetting clears `Unlocked`, zeroes a count achievement's `CurrentValue`, drops any queued toast, and saves.

| Trigger | Resets at |
| --- | --- |
| `Never` (default) | |
| `OnGameEnd` | Game end |
| `OnMeetingStart` | Meeting start |
| `OnMeetingEnd` | Meeting end |
| `OnRoundStart` | Round start |

Use these for per-round or per-meeting challenges ("win without doing tasks") instead of wiring your own patches. You can also call `achievement.Reset()` by hand; pass `save: false` when resetting many achievements in a batch and save once afterwards.

## Progress persistence

`CountAchievement.ProgressPersists` (`AchPersistence`) controls how long unfinished progress lives. It is enforced by the lifecycle events:

| Value | Unfinished progress resets | Enforced at |
| --- | --- | --- |
| `ThroughoutSessions` (default) | Never | |
| `ThroughoutRounds` | End of match / start of next match | Game end, match start |
| `ResetOnRoundStart` | New round | Round start |
| `ResetOnMeetingStart` | Meeting called | Meeting start |

Unlocked achievements are exempt: persistence only applies while `CurrentValue < RequiredValue`. On disk, unfinished progress for non-`ThroughoutSessions` achievements is written as 0; see [storage](storage.md).

## Deferred toasts

`BaseAchievement.ToastTiming` (`AchToastTiming`) queues the unlock/progress toast instead of showing it immediately:

| Timing | Shown at |
| --- | --- |
| `Immediate` (default) | Right away |
| `OnRoundStart` | Round start |
| `OnMeetingStart` | Meeting start |
| `OnMeetingEnd` | Meeting end |
| `OnGameEnd` | Game end |

Useful when a toast mid-meeting or mid-cutscene would be distracting or spoilery: queue it and let it land at a quiet moment.

Queue semantics (in `AchievementToastQueue`):

- Deferred toasts flush one bucket at a time when their event fires, shown in the order they were earned.
- A pending toast for the same achievement is replaced, so repeated progress ticks before the flush collapse into one toast with the final value.
- `Reset()` dequeues the achievement's pending toasts everywhere.
- At game end the `OnGameEnd` bucket flushes first, then everything still queued (`FlushAll`), so nothing earned in a match is lost because the lobby loaded before its timing fired.
- Toasts are snapshot at enqueue time (title, name, icon, rarity), so a deferred toast shows the values it was earned with even if the achievement resets or the language changes before it displays.
