# SceneLoader

> **Status**: Draft
> **Last Updated**: 2026-06-29
> **Implements Pillar**: Challenging — clean scene boundaries enforce that game state is never in a limbo between phases

## Summary

SceneLoader is the singleton that owns all scene transitions. No system may call `SceneManager.LoadScene` directly — every scene change is routed through SceneLoader. It uses an additive loading model: the Bootstrap scene is always resident, and exactly one gameplay scene is active at a time. Loading is async with an optional fade to prevent single-frame black flashes.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `GameManager`

---

## Overview

SceneLoader lives in the Bootstrap scene and persists for the application's lifetime. It exposes a single public entry point: `LoadScene(SceneName scene)`. Internally it loads the requested scene additively via `LoadSceneAsync`, waits for load completion, sets it active, then unloads the previously active scene. This keeps the Bootstrap scene always loaded (and all its singletons alive) while exactly one gameplay scene is mounted. SceneLoader fires `OnSceneChanged` after each transition so that AudioSystem and HUDs can react.

## Player Fantasy

The player never sees a loading hitch or black flash between phases. Scene transitions feel like flipping between chapters of the same experience — one screen fades out, another fades in, and the game state is always coherent.

---

## Detailed Design

### Scene Names (Enum)

All valid scene names are defined in a `SceneName` enum to prevent string-typo bugs:

```
Bootstrap, MainMenu, School, Battle, YearEnd
```

`Bootstrap` is listed but is never passed to `LoadScene` — it is always resident and must never be unloaded.

### Core Rules

1. **Bootstrap is immortal.** SceneLoader must never unload `Bootstrap`. If `Bootstrap` is passed to `LoadScene`, log an error and no-op.
2. **One active gameplay scene at a time.** When a new scene is requested, the previously active scene is unloaded after the new one finishes loading.
3. **Additive loading only.** All scenes are loaded with `LoadSceneMode.Additive`. Directly using `LoadSceneMode.Single` is forbidden because it would unload Bootstrap.
4. **Async only.** `LoadSceneAsync` is used; synchronous scene loads are forbidden.
5. **No concurrent transitions.** If `LoadScene` is called while a transition is already in progress, log a warning and queue the new request (FIFO). Do not drop it silently.
6. **Optional fade.** SceneLoader owns a canvas overlay used for fade-in/fade-out. Fade duration is configurable via Inspector.
7. **`OnSceneChanged` fires once** per completed transition, after the new scene is active and the old one is unloaded.
8. **Startup auto-load.** On `Start`, SceneLoader automatically loads `MainMenu` (the initial active scene). Bootstrap itself is never "the active scene."

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Idle` | Default; startup after initial MainMenu load | `LoadScene()` called | Accepts new load requests |
| `Loading` | `LoadScene()` called while `Idle` | Load + unload complete | Rejects new requests (queues them) |
| `Fading` | Fade is enabled and transition begins | Fade animation completes | Runs overlay coroutine; blocks scene swap until fade-out is done |

### Active Scene Tracking

SceneLoader maintains `_activeScene` (a `Scene` handle) pointing to the currently mounted gameplay scene. This is set to the loaded scene after each successful transition and is used to identify which scene to unload next.

### Transition Sequence

```
1. LoadScene(target) called
2. If already Loading → queue request, return
3. If fade enabled → fade-out overlay (coroutine)
4. LoadSceneAsync(target, Additive) → await completion
5. SetActiveScene(target)
6. If fade enabled → fade-in (optional, or fade in after unload)
7. UnloadSceneAsync(_activeScene) → await completion
8. _activeScene = target
9. Fire OnSceneChanged(target)
10. If queued requests exist → dequeue and repeat from step 2
```

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `GameManager` | Calls `LoadScene()` inside `StartNewRun`, `EndRun`, `ReturnToMainMenu` |
| `RunManager` | Calls `LoadScene()` at phase boundaries (School → Battle → YearEnd → School) |
| `AudioSystem` | Subscribes to `OnSceneChanged`; switches music track after each transition |
| All systems | Must never call `SceneManager` directly — always go through `SceneLoader.Instance.LoadScene()` |

---

## Formulas

### Fade Timing

```
totalTransitionTime = fadeOutDuration + loadTime + fadeInDuration
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `fadeOutDuration` | float | 0–1.0s | Inspector | How long the screen takes to go black |
| `loadTime` | float | varies | runtime | Async scene load (not tunable) |
| `fadeInDuration` | float | 0–1.0s | Inspector | How long the new scene takes to appear |

**Expected output range**: 0.2s (instant with no fade) to ~3s (slow async load + long fades)
**Edge cases**: If `fadeOutDuration = 0` and `fadeInDuration = 0`, skip the overlay entirely — no black frame.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `LoadScene(Bootstrap)` called | Log error, no-op | Bootstrap must never be unloaded |
| `LoadScene` called during an active transition | Queue the request; process after current completes | Prevents scene stack corruption |
| Async load fails (scene not in build settings) | Log error, remain in current scene, fire `OnSceneLoadFailed` event | Graceful degradation; don't leave the game in a broken state |
| Fade canvas missing from Bootstrap scene | Skip fade, proceed with instant transition; log warning | Non-fatal; game still functions |
| `_activeScene` is invalid on first call | On startup `_activeScene` is unset; skip the unload step for the first load | Bootstrap is the only scene at startup; there is no prior gameplay scene to unload |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `GameManager` | This depends on it | State trigger — GameManager calls `LoadScene()` inside state transition methods |
| `RunManager` | It depends on this | State trigger — RunManager calls `LoadScene()` at phase boundaries |
| `AudioSystem` | It depends on this | State trigger — subscribes to `OnSceneChanged` |

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `FadeOutDuration` | 0.3s | 0–0.8s | Slower, more dramatic exit | Faster, snappier; 0 = instant |
| `FadeInDuration` | 0.3s | 0–0.8s | Slower reveal of new scene | Faster appearance |
| `FadeColor` | Black (0,0,0,1) | Any color | Aesthetic choice | Aesthetic choice |

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Fade-out begins | Screen darkens over `FadeOutDuration` | None (audio handled by AudioSystem on `OnSceneChanged`) | MVP |
| New scene active | Screen brightens over `FadeInDuration` | None | MVP |
| Load failure | No visual change (stay on current scene) | None | MVP |

---

## Game Feel

### Feel Reference

> "Should feel like Hades' room transitions — fast, clean fade that never makes you wait. NOT a long black screen with no feedback."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Transition initiation (call to fade-out start) | 1 frame (16.6ms) | 1 frame |

### Animation Feel Targets

| Animation | Startup Frames | Active Frames | Recovery Frames | Feel Goal |
|---|---|---|---|---|
| Fade overlay | 0 | `FadeOutDuration × 60` | 0 | Smooth, linear alpha |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Scene transition | 0.6s total (0.3 + 0.3) default | Black screen briefly separates scenes — clean, intentional |

### Weight and Responsiveness

- **Weight**: Light — transitions should feel like turning a page, not waiting for a loading bar
- **Player control**: Player-initiated via buttons; no input during transition
- **Snap quality**: Smooth fade, not a hard cut (unless `fadeOutDuration = 0`)
- **Failure texture**: On load failure, the player stays in their current scene — no black screen

### Feel Acceptance Criteria

- [ ] Scene transitions complete (new scene visible) within 1 second on a local build
- [ ] No single-frame black flash without a fade
- [ ] No frame freeze during async load (load runs in background)

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Fade overlay | Full-screen canvas in Bootstrap scene | Per-frame during transition | During `Fading` state only |
| Loading progress | Not displayed (load is fast for local scenes) | N/A | Not MVP |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| GameManager calls LoadScene on state change | `GameManager.md` | `StartNewRun`, `EndRun`, `ReturnToMainMenu` | State trigger |
| RunManager calls LoadScene at phase boundaries | `RunManager.md` | Phase transition points | State trigger |
| AudioSystem reacts to OnSceneChanged | `AudioSystem.md` | `OnSceneChanged` subscription | State trigger |

---

## Acceptance Criteria

- [ ] `SceneLoader.Instance.LoadScene(SceneName.School)` navigates to the School scene from any other scene
- [ ] Bootstrap scene remains loaded (visible in hierarchy) after every transition
- [ ] Attempting `LoadScene(SceneName.Bootstrap)` logs an error and does nothing
- [ ] Calling `LoadScene` during an active transition queues the request and processes it after completion
- [ ] `OnSceneChanged` fires exactly once per completed transition
- [ ] All scene names used by the project are entries in the `SceneName` enum (no raw strings)
- [ ] Fade overlay does not linger after transition completes
- [ ] Performance: async load does not spike frame time above 2ms in any frame

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should Battle and YearEnd be separate Unity scenes, or tabs/panels within the School scene? Separate scenes give cleaner state isolation; panels are lighter-weight. | Designer | Before code-system | Pending — recommend separate scenes for clean separation |
| Should the queue hold more than 1 deferred request, or just the single most-recent? | Engineer | Before code-system | Pending — FIFO queue of 1 is simplest; expand if needed |
