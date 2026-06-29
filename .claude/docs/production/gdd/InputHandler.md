# InputHandler

> **Status**: Draft
> **Last Updated**: 2026-06-29
> **Implements Pillar**: Empowering — responsive hotkeys make the management loop feel fluid, not sluggish

## Summary

InputHandler wraps the Unity Input System and exposes typed C# events for non-UI gameplay inputs (Cancel/Back, SpeedUp battle, Confirm). UI click interactions are handled natively by UI Toolkit and do not route through InputHandler. InputHandler's primary value is (a) blocking all gameplay input during scene transitions and (b) providing a single place to rebind keys in the future.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `None`

---

## Overview

InputHandler lives in the Bootstrap scene and persists for the application's lifetime. It reads from a Unity Input Action Asset (`MagicTutorInputActions.inputactions`) and fires typed events that other systems subscribe to. Because this is a management game with auto-battle combat, there is no real-time gameplay input — all player actions flow through UI Toolkit's built-in click/select events. InputHandler covers only the keyboard shortcuts that sit outside the UI layer: ESC to cancel/pause, Space to fast-forward the battle simulation, and Enter to confirm a selection.

## Player Fantasy

The player never has to hunt for the right button. Pressing ESC always does the sensible thing (go back one step); holding Space in battle gives immediate gratification by speeding up the fight. Hotkeys feel like power-user affordances, not requirements.

---

## Detailed Design

### Input Action Asset

Asset path: `Assets/Input/MagicTutorInputActions.inputactions`

Action maps:

| Map | Active When | Actions |
|---|---|---|
| `Gameplay` | Always (when `IsInputEnabled = true`) | `Cancel`, `Confirm`, `SpeedUp` |

> UI navigation (tab, arrows, click) is handled by UI Toolkit's built-in event system — no custom action map needed for that.

### Actions

| Action Name | Default Binding (PC) | Type | Description |
|---|---|---|---|
| `Cancel` | Escape | Button | Go back / open pause |
| `Confirm` | Enter / Return | Button | Confirm highlighted selection |
| `SpeedUp` | Space | Hold (value axis) | Fast-forward battle simulation while held |

### Core Rules

1. InputHandler reads actions via the generated C# wrapper class (`MagicTutorInputActions`), not via `InputSystem.actions["name"]` string lookups.
2. InputHandler exposes C# events: `OnCancelPressed`, `OnConfirmPressed`, `OnSpeedUpStarted`, `OnSpeedUpCancelled`.
3. InputHandler owns the `IsInputEnabled` bool. When `false`, all event callbacks are silently swallowed — no events fire. Subscribers do not need to guard against this.
4. `DisableGameplayInput()` is called by SceneLoader at transition start; `EnableGameplayInput()` is called after the new scene is active.
5. InputHandler never reads UI Toolkit click events — those flow directly to the UI layer.
6. No `Input.GetKey` or legacy Input class usage anywhere.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Enabled` | `EnableGameplayInput()` called, or startup default | `DisableGameplayInput()` called | Actions fire events normally |
| `Disabled` | `DisableGameplayInput()` called | `EnableGameplayInput()` called | All callbacks swallowed; no events fire |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `SceneLoader` | Calls `DisableGameplayInput()` before a transition, `EnableGameplayInput()` after the new scene is active |
| `RunManager` | Subscribes to `OnCancelPressed` during `InRun` to open the pause/quit prompt |
| `AutoBattleResolver` | Subscribes to `OnSpeedUpStarted` / `OnSpeedUpCancelled` to toggle fast-forward mode |
| `MainMenuController` | Subscribes to `OnConfirmPressed` to activate the highlighted menu button |
| `YearEndHUD` | Subscribes to `OnConfirmPressed` to confirm the current promotion selection |

---

## Formulas

No formulas. InputHandler is a pure event dispatcher.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `Cancel` pressed during a scene transition (`IsInputEnabled = false`) | Swallowed — no event fires | Transition state must not be interrupted |
| `SpeedUp` held when no battle is in progress | Event fires; AutoBattleResolver simply ignores it (it is not subscribed outside Battle phase) | InputHandler does not need to know which phase is active |
| `Cancel` pressed on the MainMenu with nothing to cancel | `OnCancelPressed` fires; MainMenuController handles the no-op | InputHandler has no knowledge of context |
| Multiple `Confirm` presses in one frame | Only the first fires (Input System button performed fires once per press) | Unity Input System handles this natively |
| Input Action Asset missing from build | Log error on Awake, stay disabled | Non-fatal — game launches but hotkeys won't work |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| None | — | InputHandler has no system dependencies (Tier 1 foundation) |
| `SceneLoader` | It depends on this | State trigger — calls `DisableGameplayInput` / `EnableGameplayInput` |
| `RunManager` | It depends on this | Rule dependency — subscribes to `OnCancelPressed` |
| `AutoBattleResolver` | It depends on this | Rule dependency — subscribes to `OnSpeedUpStarted` / `OnSpeedUpCancelled` |
| `MainMenuController` | It depends on this | Rule dependency — subscribes to `OnConfirmPressed` |
| `YearEndHUD` | It depends on this | Rule dependency — subscribes to `OnConfirmPressed` |

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `SpeedUp` binding | Space | Any key | N/A (rebind) | N/A (rebind) |
| `Cancel` binding | Escape | Any key | N/A (rebind) | N/A (rebind) |
| `Confirm` binding | Enter | Any key | N/A (rebind) | N/A (rebind) |

> Key bindings are defined in the Input Action Asset and are not exposed as runtime tuning knobs in v1. Rebinding support is out of scope.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Any hotkey press | None — visual response is the subscriber's responsibility | None | MVP |

---

## Game Feel

### Feel Reference

> "Should feel like Slay the Spire's keyboard shortcuts — every hotkey does exactly what you'd intuit, works instantly, and never requires the player to leave the mouse. NOT a game where hotkeys conflict with text fields or require focus."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| `Cancel` pressed → event fires | 1 frame (16.6ms) | 1 frame |
| `SpeedUp` held → battle fast-forward begins | 1 frame (16.6ms) | 1 frame |
| `Confirm` pressed → event fires | 1 frame (16.6ms) | 1 frame |

### Animation Feel Targets

Not applicable — InputHandler has no animations.

### Impact Moments

Not applicable — InputHandler dispatches events; impact moments belong to subscribers.

### Weight and Responsiveness

- **Weight**: Zero — input events must fire in the same frame as the physical input
- **Player control**: Always player-initiated; no auto-inputs
- **Snap quality**: Binary (button performed / cancelled) — no analog blending in v1
- **Failure texture**: If a hotkey does nothing, the subscriber is responsible for feedback — InputHandler itself never shows an error

### Feel Acceptance Criteria

- [ ] Pressing ESC in any scene triggers the expected Cancel behavior within 1 frame
- [ ] Holding Space during a battle visibly speeds up the simulation within 1 frame
- [ ] No hotkey fires during a scene transition

---

## UI Requirements

InputHandler has no UI of its own. UI click events are handled by UI Toolkit directly.

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Hotkey hints (e.g. "[ESC] Back") | Each screen's HUD (SchoolHUD, BattleHUD, etc.) | Static | Always visible |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| SceneLoader disables input during transitions | `SceneLoader.md` | Transition sequence steps | State trigger |
| AutoBattleResolver toggles fast-forward on SpeedUp | `AutoBattleResolver.md` | `OnSpeedUpStarted` / `OnSpeedUpCancelled` | Rule dependency |

---

## Acceptance Criteria

- [ ] `InputHandler.Instance` accessible from any scene
- [ ] `OnCancelPressed` fires within 1 frame of ESC key press when `IsInputEnabled = true`
- [ ] No events fire when `IsInputEnabled = false`
- [ ] `DisableGameplayInput()` / `EnableGameplayInput()` toggle the enabled state correctly
- [ ] No `Input.GetKey`, `Input.GetButton`, or any legacy `UnityEngine.Input` usage in InputHandler
- [ ] Input Action Asset (`MagicTutorInputActions.inputactions`) exists at `Assets/Input/` before first compile
- [ ] Performance: InputHandler uses no `Update()` polling — purely callback-driven

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should `Cancel` during the YearEnd promotion screen open a "quit run?" confirmation, or immediately quit? | Designer | Before code-system | Pending |
| Should `SpeedUp` be a toggle (press once to fast-forward, press again to resume normal speed) or a hold? | Designer | Before code-system | Pending — recommend hold, consistent with genre conventions (Hades, Slay the Spire) |
| Should `Confirm` work on UI Toolkit buttons, or does UI Toolkit handle that natively with its own navigation? | Engineer | Before code-system | Pending — likely UI Toolkit handles Enter natively; `OnConfirmPressed` may only be needed for non-button selections |
