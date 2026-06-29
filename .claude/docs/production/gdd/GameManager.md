# GameManager

> **Status**: Draft
> **Last Updated**: 2026-06-29
> **Implements Pillar**: Challenging — owns the permadeath run boundary; losing a run is a hard, legible state change

## Summary

GameManager is the global singleton that owns the `GameState` enum and the active run snapshot. It is the single source of truth for whether the player is in the main menu, mid-run, or viewing run results. All other systems read game state from it — nothing writes state except through its own transition methods.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `None`

---

## Overview

GameManager lives in the Bootstrap scene and persists for the application's lifetime. It exposes the current `GameState` (MainMenu, InRun, RunEnd) and a small `RunSnapshot` struct that records the result of the most recently completed run (year reached, win/lose). It fires `OnGameStateChanged` whenever the state transitions so that all systems — SceneLoader, AudioSystem, HUDs — can react without polling. It does not manage in-run phases (that is RunManager's responsibility); it only knows whether a run is happening at all.

## Player Fantasy

The player never consciously thinks about GameManager. Its job is invisible: state transitions feel clean, the game never gets stuck between states, and a lost run leads immediately to the results screen without ambiguity.

---

## Detailed Design

### Core Rules

1. GameManager is a singleton; only one instance may exist. If a second is created (e.g. scene reload), destroy the duplicate immediately in `Awake`.
2. GameManager lives in the Bootstrap scene. **Never call `DontDestroyOnLoad`** — the Bootstrap scene is never unloaded.
3. Only GameManager may write `CurrentState`. All other systems read it via `GameManager.Instance.CurrentState`.
4. Every state transition goes through a named method (`StartNewRun`, `EndRun`, `ReturnToMainMenu`). Direct assignment to `CurrentState` is forbidden.
5. Every state transition fires `OnGameStateChanged` with the new state before returning.
6. `StartNewRun` increments `TotalRunsStarted` before firing the event, so listeners see the updated run count.
7. `EndRun` stores the result in `LastRunResult` before transitioning to `RunEnd`, so the results screen can read it.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `MainMenu` | Game launch, or `ReturnToMainMenu()` called | `StartNewRun()` called | Idle; AudioSystem plays menu music |
| `InRun` | `StartNewRun()` called | `EndRun(won, year)` called by RunManager | RunManager owns all in-run phase logic |
| `RunEnd` | `EndRun()` called | `ReturnToMainMenu()` or `StartNewRun()` called | Results screen shown; TeacherRoster may be updated |

Valid transitions:
- `MainMenu` → `InRun` (player starts a run)
- `InRun` → `RunEnd` (run ends, win or lose)
- `RunEnd` → `MainMenu` (player returns to menu)
- `RunEnd` → `InRun` (player starts another run immediately)

Invalid transitions (must assert/log error and no-op):
- `MainMenu` → `RunEnd`
- `InRun` → `InRun`
- `RunEnd` → `RunEnd`

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `SceneLoader` | Reads `CurrentState` on scene load to decide which scene to show; GameManager calls `SceneLoader` inside each transition method |
| `RunManager` | Calls `EndRun(won, yearReached)` when a run concludes; reads `TotalRunsStarted` to initialize run context |
| `AudioSystem` | Subscribes to `OnGameStateChanged`; switches music track based on new state |
| `MainMenuController` | Calls `StartNewRun()` when the player presses New Game |
| `MetaProgressionHUD` | Reads `LastRunResult` on `RunEnd` state to populate the results screen |

---

## Formulas

No formulas. GameManager performs no calculations — it is a state container and event broadcaster.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `StartNewRun()` called while already `InRun` | Log error, no-op | Prevents double-init of RunManager |
| `EndRun()` called while in `MainMenu` | Log error, no-op | Guard against event ordering bugs |
| Bootstrap scene unloaded | Must never happen — assert in GameManager Awake | Bootstrap scene is the persistence anchor |
| Application quit during `InRun` | `OnApplicationQuit` fires `EndRun(won: false, yearReached: currentYear)` so TeacherRoster saves progress | Prevents mid-run progress loss |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| None | — | GameManager has no dependencies (Tier 1 foundation) |
| `SceneLoader` | It depends on this | State trigger — `SceneLoader` reads `CurrentState` and is called inside transition methods |
| `RunManager` | It depends on this | State trigger — `RunManager` calls `EndRun()` and reads state |
| `AudioSystem` | It depends on this | State trigger — subscribes to `OnGameStateChanged` |
| `MainMenuController` | It depends on this | State trigger — calls `StartNewRun()` |
| `MetaProgressionHUD` | It depends on this | Data dependency — reads `LastRunResult` |

---

## Tuning Knobs

GameManager has no tunable balance values. It is infrastructure.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| `GameState` → `MainMenu` | SceneLoader transitions to MainMenu scene | AudioSystem switches to menu music | MVP |
| `GameState` → `InRun` | SceneLoader transitions to School scene | AudioSystem switches to school/management music | MVP |
| `GameState` → `RunEnd` | SceneLoader transitions to RunEnd/results scene | AudioSystem plays run-end sting (win or lose variant) | MVP |

---

## Game Feel

### Feel Reference

> "Should feel like any well-shipped Unity game's scene manager — state changes are instant, reliable, and never leave the player in a limbo screen. NOT a game where you press New Game and wait 3 seconds with a black screen for no reason."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| State transition initiation | 1 frame (16.6ms) | 1 frame — state fires synchronously |

### Animation Feel Targets

Not applicable — GameManager has no animations.

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Run-end transition | < 500ms total (SceneLoader handles) | State change is instant; visual fade is SceneLoader's concern |

### Weight and Responsiveness

- **Weight**: N/A — infrastructure system
- **Player control**: State transitions are always player-initiated (except `OnApplicationQuit`)
- **Snap quality**: Binary — either in a state or transitioning; no blended states
- **Failure texture**: A lost run transitions cleanly to RunEnd; the player immediately sees what happened

### Feel Acceptance Criteria

- [ ] State transitions complete within 1 frame of the triggering call
- [ ] No state limbo: `CurrentState` always reflects a valid `GameState` value
- [ ] Results screen is visible within 500ms of `EndRun()` being called

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| `CurrentState` | Not displayed directly — drives scene selection | On change | Always |
| `LastRunResult.Won` | Results screen (MetaProgressionHUD) | Once on `RunEnd` entry | Only in `RunEnd` state |
| `LastRunResult.YearReached` | Results screen (MetaProgressionHUD) | Once on `RunEnd` entry | Only in `RunEnd` state |
| `TotalRunsStarted` | Optional stats display | On change | Optional — not MVP |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| SceneLoader drives scene loads on state change | `SceneLoader.md` | `LoadScene(sceneName)` method | State trigger |
| RunManager calls EndRun | `RunManager.md` | `EndRun(bool won, int yearReached)` | Ownership handoff |
| AudioSystem reacts to state changes | `AudioSystem.md` | `OnGameStateChanged` subscription | State trigger |

---

## Acceptance Criteria

- [ ] `GameManager.Instance` is accessible from any scene without `FindObjectOfType`
- [ ] Invalid state transitions (e.g. `InRun` → `InRun`) log a warning and no-op
- [ ] `OnGameStateChanged` fires exactly once per transition with the correct new state
- [ ] `LastRunResult` is populated before `RunEnd` state is entered
- [ ] `TotalRunsStarted` increments by 1 on every `StartNewRun()` call
- [ ] Application quit during `InRun` triggers `OnApplicationQuit` save path
- [ ] No hardcoded values — all configuration via Inspector or ScriptableObject
- [ ] Performance: `Update()` not used — GameManager is purely event-driven

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should there be a `Paused` state, or is pause handled entirely within RunManager without a GameState change? | Designer | Before code-system | Pending — recommend keeping pause within RunManager scope to keep GameManager minimal |
| Does `ReturnToMainMenu` from `RunEnd` require user confirmation (e.g. "Are you sure?") or is it immediate? | Designer | Before code-system | Pending |
