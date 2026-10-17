# Menu and toasts

What players actually see. None of this needs any work from a consuming mod; it's documented so you know what your achievements land in and which knobs exist.

## The menu

The achievements button on the main menu opens a full-screen menu listing every registered achievement, grouped by mod inside [categories](tabs-and-categories.md#categories). It has:

- filters for All, Completed, In Progress and Locked, plus a search box and an "all mods" selector;
- a category column on the left, with paging for setups that register many categories;
- a details page per achievement (description, rarity, progress, unlock time);
- a stats card with Skeld Points and per-rarity completion;
- a settings page for toasts with a live preview, and a **Reset all achievements** button at the bottom of it.

### Resetting all achievements

The Reset button on the settings page opens a confirmation ("Wipe everything?" with **Nevermind** / **I'm ready**). Confirming wipes every achievement from every mod back to locked with zero progress, drops stale entries left by mods that are no longer installed, and rewrites the save file once; the menu's stats and list refresh immediately. There is no undo. Escape/B, clicking outside the dialog, or **Nevermind** cancels. The same operation is available to code as `AchievementStorage.ResetAll()`; see [Storage](storage.md#reads-and-writes).

While the menu or a toast has the pointer, clicks don't fall through to the game UI behind it.

Tabs with `IsSelectable == false` (e.g. the API's own Developer tab) don't appear, and `Hidden` achievements render as "Hidden Achievement" until unlocked.

### Controller and keyboard

The menu is fully drivable without a mouse. A focus ring marks the selection and the list scrolls to keep it visible; moving the mouse switches back to pointer mode.

| Action | Controller | Keyboard |
| --- | --- | --- |
| Move | Left stick / D-pad | Arrow keys |
| Select | A | Enter / Space |
| Back / Close | B | Escape |
| Previous / next category | LB / RB | Q / E |
| Previous / next filter | LT / RT | Shift+Tab / Tab |

## Toasts

Toasts slide in from the configured screen edge, play a chime (a rarer one for rarity above common) and can be swiped away sideways. Middle-clicking a toast opens the achievements menu to that achievement; on a controller, hold Y while it's visible. Trackpad users can turn on `TrackpadMode` to open it with a normal click instead.

Multiple simultaneous toasts queue and play one at a time rather than overlapping. An unlock toast drops progress toasts still queued for the same achievement, so a quick `Increment` burst that finishes the achievement doesn't trail stale progress popups behind the unlock. [Deferred updates](game-events.md#deferred-toasts) additionally collapse into one toast carrying the final value.

### Configuration

Toast settings live in the `Toasts` section of the plugin config (`AchievementsConfig`) and can also be changed from the Settings page in the menu:

| Setting | Default | Description |
| --- | --- | --- |
| `ToastsEnabled` | `true` | Show toast popups. Unlocking still happens when disabled |
| `ToastDuration` | `4.4` | Seconds a toast stays fully visible (range 1-10) |
| `RareToastDuration` | `6.0` | Same, for rarity above common (range 1-10) |
| `ToastSoundEnabled` | `true` | Play the unlock chime |
| `ToastVolume` | `0.5` | Chime volume (range 0-1) |
| `Position` | `BottomCenter` | Corner/edge: `TopLeft`, `TopCenter`, `TopRight`, `BottomLeft`, `BottomCenter`, `BottomRight` |
| `TrackpadMode` | `false` | Open the menu from a toast with a normal click instead of middle-click |

The volume slider plays a short preview tick at the chosen volume as it moves. These entries are also readable and writable from code via `AchievementsConfig`; see [Integrating with the API](integration.md).

When an achievement's toast appears relative to game events is controlled per achievement via `ToastTiming`; see [game events](game-events.md#deferred-toasts).
