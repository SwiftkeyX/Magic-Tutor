# MainMenuController

> **Status**: Draft
> **Last Updated**: 2026-06-29
> **Implements Pillar**: Empowering — the main menu is the player's gateway into and out of each run; it should feel welcoming and purposeful, never a blocker

## Summary

MainMenuController handles the main menu screen: new run, settings, and quit. It reads GameManager state to decide which options to show (e.g., a "Continue" button if a run is in progress) and delegates all scene transitions to SceneLoader. It owns no gameplay logic — it is a pure UI controller that fires commands toward GameManager and SceneLoader.

> **Quick reference** — Layer: `Presentation` · Priority: `MVP` · Key deps: `GameManager, SceneLoader`

---

## Overview

MainMenuController is a MonoBehaviour on a Canvas in the MainMenu scene. On `Start()`, it reads `GameManager.Instance.CurrentState` to configure which buttons are visible (e.g., "Continue Run" is hidden if no run is in progress). Button callbacks invoke `GameManager.Instance.StartNewRun()` or `SceneLoader.Instance.LoadScene()` directly — no intermediate events. Settings are presented as an in-scene overlay panel toggled by the Settings button, not a separate scene.

## Player Fantasy

Opening the game should feel like sitting down at a desk in a magical academy — calm, inviting, ready to begin. The menu is never a puzzle: the player sees exactly what to do next. Returning after a failed run, they see their teacher roster has grown and feel motivated to try again.

---

## Detailed Design

### Core Rules

1. MainMenuController is a `Component` (not a singleton) — it only exists while the MainMenu scene is active.
2. On `Start()`, it queries `GameManager.Instance.CurrentState` to determine button visibility:
   - `GameState.MainMenu` → show New Run button; hide Continue button.
   - `GameState.InRun` → show both New Run and Continue buttons (player may have quit mid-run via quit button during a run — future feature; for MVP, Continue is hidden since mid-run quit ends the run).
3. **New Run** button: calls `GameManager.Instance.StartNewRun()`. GameManager transitions state and calls `SceneLoader.Instance.LoadScene(School)`.
4. **Settings** button: toggles the Settings overlay panel (in-scene, not a new scene). The Settings panel hosts volume sliders that call `AudioSystem.Instance.SetMusicVolume()` and `AudioSystem.Instance.SetSfxVolume()`.
5. **Quit** button: calls `Application.Quit()`. In the Unity Editor, calls `UnityEditor.EditorApplication.isPlaying = false` (wrapped in `#if UNITY_EDITOR`).
6. MainMenuController never loads scenes directly — all navigation routes through `SceneLoader.Instance.LoadScene()` or `GameManager.Instance` methods.
7. No `FindObjectOfType` — references to GameManager and SceneLoader use their `.Instance` singleton accessors.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Idle` | MainMenu scene loads | Any button clicked | Menu visible; buttons interactive |
| `SettingsOpen` | Settings button clicked | Settings close button clicked | Settings panel visible; main buttons dimmed but still accessible |
| `Transitioning` | New Run (or Continue) clicked | SceneLoader fade completes | Buttons disabled; scene fade plays |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `GameManager` | Reads `CurrentState` on Start to configure buttons; calls `StartNewRun()` on New Run click |
| `SceneLoader` | GameManager internally calls `SceneLoader.LoadScene(School)` after `StartNewRun()` — MainMenuController does not call SceneLoader directly |
| `AudioSystem` | Settings panel calls `SetMusicVolume(float)` and `SetSfxVolume(float)` when sliders change |
| `TeacherRoster` | Reads `GetAll()` to display accumulated teacher count in the menu (e.g., "Teachers: 7") — read-only |

---

## Formulas

No formulas — MainMenuController is pure UI logic.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `GameManager.Instance` is null on Start | Log error; disable all buttons | Bootstrap scene failed to load — should never happen in production |
| New Run clicked while Settings panel is open | Close Settings panel first (1 frame), then transition | Prevents overlapping fade animations |
| Quit called in WebGL build | `Application.Quit()` is a no-op in WebGL; disable Quit button on WebGL platform | Platform-appropriate behavior |
| Player clicks New Run rapidly (double-click) | Button is disabled on first click (state = Transitioning); second click ignored | Transition guard |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `GameManager` | This depends on it | State trigger — reads game state; triggers run start |
| `SceneLoader` | This depends on it (indirectly via GameManager) | State trigger — scene transition |
| `AudioSystem` | This depends on it | Rule dependency — Settings panel calls volume setters |
| `TeacherRoster` | This depends on it | Data dependency — reads teacher count for display |

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| Button fade-in duration | 0.3s | 0–1s | Slower, more cinematic menu appearance | Snappier, more abrupt |
| Settings panel slide-in duration | 0.2s | 0–0.5s | Smoother panel animation | Near-instant toggle |

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Menu scene loads | Buttons fade in sequentially (staggered 0.05s) | Menu music begins (AudioSystem) | MVP |
| New Run clicked | Button highlights briefly; screen fades to black | UI click SFX | MVP |
| Settings opened | Panel slides in from right | UI whoosh SFX | MVP |
| Volume slider dragged | Immediate audio volume change | Live preview of music/SFX volume | Alpha |
| Quit clicked | Application exits immediately | None | MVP |

---

## Game Feel

### Feel Reference

> "Should feel like Hades' main menu — clean, readable, game-world-appropriate. The menu art and music establish the academy atmosphere before the player clicks anything. NOT a generic Unity default UI with white buttons on grey backgrounds."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Button click → visual feedback | 1 frame | 1 frame |
| New Run click → fade begins | 1 frame | 1 frame |

### Animation Feel Targets

| Animation | Startup Frames | Active Frames | Recovery Frames | Feel Goal |
|---|---|---|---|---|
| Button hover highlight | 0 | 6 | 4 | Responsive, not jumpy |
| Scene fade-out | 0 | 18 (0.3s) | 0 | Smooth, not abrupt |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Teacher count visible on menu | Static | "You have 7 teachers" — communicates progress even before clicking Play |

### Weight and Responsiveness

- **Weight**: Light — menus should never feel heavy or slow
- **Player control**: All actions are 1-click
- **Snap quality**: Buttons respond on pointer-down, not pointer-up (if supported by UI Toolkit)
- **Failure texture**: N/A — there is no failure state in the main menu

### Feel Acceptance Criteria

- [ ] A new player can identify the "New Run" button within 2 seconds of the menu loading
- [ ] Settings slider changes take effect on the same frame the slider moves
- [ ] No layout shift or pop-in after the initial fade-in

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| "New Run" button | Center of screen | Static | Always |
| "Settings" button | Below New Run | Static | Always |
| "Quit" button | Below Settings | Static | Always (hidden on WebGL) |
| Teacher count ("Teachers: N") | Menu footer or side panel | On scene load | Always |
| Volume sliders (Music, SFX) | Settings overlay panel | On open | When Settings open |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| `GameManager.StartNewRun()` | `GameManager.md` | `StartNewRun()` public method | State trigger |
| `SceneLoader.LoadScene(School)` | `SceneLoader.md` | `SceneName.School` | State trigger |
| `AudioSystem.SetMusicVolume()` / `SetSfxVolume()` | `AudioSystem.md` | Volume setter methods | Rule dependency |
| `TeacherRoster.GetAll()` | `TeacherRoster.md` | Teacher count read | Data dependency |

---

## Acceptance Criteria

- [ ] New Run button calls `GameManager.Instance.StartNewRun()` and transitions to the School scene
- [ ] Settings button toggles the Settings overlay panel (show/hide)
- [ ] Quit button exits the application (or stops play in the Editor)
- [ ] Volume sliders in Settings update AudioSystem volumes in real time
- [ ] No `FindObjectOfType` in `MainMenuController.cs`
- [ ] All scene transitions route through SceneLoader (never `SceneManager.LoadScene` directly)
- [ ] Teacher count displayed on menu matches `TeacherRoster.GetAll().Count`
- [ ] Buttons are disabled during the scene-transition fade (no double-click vulnerability)

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should there be a "Continue Run" button for mid-run saves in a future version? | Designer | Before Phase 3 | Pending — not in v1 (mid-run quit = run loss) |
| Does the main menu show a background scene/animation, or a static image? | Artist | Before Alpha | Pending — recommend animated background (school hallway) for polish; static for MVP |
| Should the teacher count on the menu be a full roster preview or just a number? | Designer | Before Alpha | Pending — number for MVP; full preview is MetaProgressionHUD's job |
