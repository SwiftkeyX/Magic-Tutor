# BattleHUD

> **Status**: Draft
> **Last Updated**: 2026-07-04 (Hero Info Panel Traits line simplified to plain names, no count/breakpoint)
> **Implements Pillar**: Empowering — the battle screen is the payoff; the player watches their trained team fight on the hex board and feels the direct consequence of every training decision. BattleHUD's role is narrow: deliver the outcome clearly and let the player continue.

## Summary

BattleHUD renders the Battle scene's outcome UI. It subscribes to `AutoBattleResolver.OnBattleComplete` to reveal the win/lose overlay, and to `InputHandler`'s speed-up events to show a speed indicator. **All in-battle visual feedback (unit attacks, HP, defeats) is owned entirely by `BattleBoardManager` on the hex board — BattleHUD does not duplicate it with cards, HP bars, or damage numbers.** It is purely a visual listener — it never calls any method that changes battle state, and it never reads from StudentRoster or EnemyDatabase directly.

> **Quick reference** — Layer: `Presentation` · Priority: `MVP` · Key deps: `AutoBattleResolver` (outcome only), `InputHandler`, `RunManager`

---

## Overview

BattleHUD is a MonoBehaviour on a UIDocument in the Battle scene. It shows no combat visuals of its own during Placement Phase or the battle itself — the hex board is the sole visual surface for combat — beyond the speed-up indicator, which can appear at any point in the scene while the speed-up input is held. Once `AutoBattleResolver` fires `OnBattleComplete`, BattleHUD reveals the outcome overlay and enables the "Continue" button.

## Player Fantasy

The payoff of training is watching the hex board fight play out — student and enemy units clash directly on the grid. When the dust settles, the outcome is immediate and unambiguous: a clear Victory or Defeat screen, with no waiting or confusion about what happened. The speed-up option means the player is never trapped watching a slow battle.

---

## Detailed Design

### Core Rules

1. BattleHUD subscribes to `AutoBattleResolver.OnBattleComplete` in `OnEnable` and unsubscribes in `OnDisable`. It separately subscribes to `InputHandler.OnSpeedUpStarted` / `OnSpeedUpCancelled`.
2. **BattleHUD never writes game state.** It is a pure visual consumer of the outcome and speed-up events. The only outbound call is `RunManager.Instance.CompleteBattlePhase()` from the "Continue" button — the handshake that lets `RunManager` advance the phase once the player has seen the outcome.
3. The speed-up indicator (">> FAST") is visible when InputHandler's speed-up action is held. BattleHUD reads this from the InputHandler event, not from AutoBattleResolver.
4. On `OnBattleComplete(result)`:
   - Win: show "Victory!" overlay; list surviving students
   - Lose: show "Defeated" overlay; somber animation
   - `TimedOut = true`: add "(Time Limit)" note to outcome
   - Outcome screen has a "Continue" button, enabled only after `OnBattleComplete` fires, that calls `RunManager.Instance.CompleteBattlePhase()`
5. **BattleHUD has no per-tick subscriptions.** It does not subscribe to `OnCombatantsSet`, `OnCombatantActed`, or `OnCombatantDefeated`, and never reads per-combatant snapshot/HP data from `AutoBattleResolver`. All of that visual feedback is `BattleBoardManager`'s responsibility, rendered directly on the hex board.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Battle` | Scene loads / battle in progress | `OnBattleComplete` received | No BattleHUD UI besides the speed-up indicator; the hex board shows all combat |
| `ShowingOutcome` | `OnBattleComplete` received | Player clicks "Continue" | Outcome overlay visible; "Continue" button active |

### Hero Info Panel (Pre-Battle / In-Battle Inspector)

A right-docked panel that shows a single selected hero's stats, skill, and traits. Implemented as its own script, `HeroInfoPanel.cs` — not part of `BattleHUD.cs` — because it must be usable during Placement Phase (before any `AutoBattleResolver` events exist), not only during battle.

**Trigger**: clicking a bench card (Placement Phase) or a placed `BattleUnit` on the board (either phase) selects that hero. There is no hover trigger. Clicking a different hero swaps the panel's content; there is no close/dismiss button in this pass — the panel simply holds the last-selected hero's info.

**Fields shown**:
- Header: `DisplayName`, `Role`, `Cost`
- Stats: `MaxHP, ATK, DEF, MG, MR, AttackSpeed, CRIT, Range, MaxMana, StartingMana` — read directly from `ChampionData`
- Skill: the hero's `BattleBehaviorFlag`(s), each rendered through a static one-line description (e.g. "Magic Attack — uses MG/MR instead of ATK/DEF"). Empty list → "No active skill". **This is a placeholder mapping, not a real Skill/Ability system** — no dedicated Skill data model exists yet. Do not treat this section as a spec for a future ability system.
- Traits: `VerticalTrait` and `HorizontalTrait` (skipped if `None`), shown as plain trait names only — no live count or breakpoint status. `TraitHUDController`'s dedicated panel remains the sole place for live trait counts/breakpoints; Hero Info Panel intentionally does not duplicate that numeric detail.

**Enemy units are inspectable too**, via an adapter: `HeroInfoPanel` holds its own `EnemyDatabaseStub` reference (same self-resolved-reference pattern as `ChampionRoster`/`TraitTracker`) and falls back to it when a clicked ID isn't a player champion. Enemies render through the same panel layout (Header/Stats/Skill/Traits) but with no Role/Cost in the header and a hardcoded "Traits: None" (enemies have no `VerticalTrait`/`HorizontalTrait` — `EnemyDatabaseStub` stays the source of truth for enemy stats; this is not a data-model unification with `ChampionRoster`).

**On a genuine miss** (the selected ID resolves to neither a player champion nor a known enemy), `HeroInfoPanel` calls `ShowPlaceholder()` and logs a `Debug.LogWarning` — it does not silently leave stale content on screen. This matters because the ID passed into `HeroSelection.Select()` for a player unit is resolved by `BattleBoardManager.OnCardClicked()`/placement code (see `BattleBoardManager.md` Core Rule 6), not the raw board/combat identity — a genuine miss here is a real upstream bug, not a normal empty state, and should be visible during development.

**Still out of scope for this pass**: any close/dismiss control.

**Architecture — decoupled from `BattleBoardManager`, like `TraitHUDController`**: `HeroInfoPanel` is never referenced by `BattleBoardManager`, and never references it back — consistent with `best-practices.md`'s "Game systems never reference HUDs" rule. Selection travels through a small static event class, `HeroSelection` (`OnHeroSelected(string championId)` / `Select(string championId)`), which `BenchCardDrag` and `BattleUnit` call into directly on click. `HeroInfoPanel` subscribes to `HeroSelection.OnHeroSelected` in `OnEnable`/unsubscribes in `OnDisable`, then resolves the `ChampionData` itself via its own `ChampionRoster` reference — the same self-sufficient pattern `TraitHUDController` uses for its own `TraitTracker` reference. `HeroInfoPanel` holds no `TraitTracker` reference (removed once the Traits line dropped live count/breakpoint display). `HeroSelection` is a lightweight decoupling utility, not a new game system.

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `AutoBattleResolver` | Subscribes to `OnBattleComplete` only; reads the `BattleResult` struct to show the outcome. No other subscription. |
| `BattleBoardManager` | Owns all in-battle visual feedback (unit attack/HP/defeat animations rendered directly on the hex board). BattleHUD does not overlay or duplicate this — a decoupled peer, no reference in either direction. |
| `InputHandler` | Subscribes to `OnSpeedUpStarted` / `OnSpeedUpCancelled` to toggle the speed indicator UI |
| `RunManager` | "Continue" button on outcome screen calls `RunManager` to advance phase (read-only access to confirm battle is done) |
| `HeroInfoPanel` → `ChampionRoster` | `HeroInfoPanel` reads `ChampionData` directly via `ChampionRoster.GetChampionById()` — same self-sufficient read pattern as `TraitHUDController` → `TraitTracker` |
| `BenchCardDrag` / `BattleUnit` → `HeroSelection` (static event) | Both call `HeroSelection.Select(championId)` on click; `HeroInfoPanel` subscribes to `HeroSelection.OnHeroSelected` — no direct reference between `BattleBoardManager` and `HeroInfoPanel` in either direction |

---

## Formulas

No formulas — BattleHUD only reads the `BattleResult` struct provided by `OnBattleComplete`.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Speed-up held during outcome screen | Speed indicator visible; "Continue" button still clickable | Speed-up only affects tick delay in AutoBattleResolver; no HUD impact |
| Battle ends in timeout | `TimedOut = true` in result; outcome overlay shows "(Time Limit)" note | Inform player of edge case |
| `OnBattleComplete` never fires (e.g. `AutoBattleResolver` errors) | Outcome overlay never shows; player stuck on the board | Acceptable failure mode — matches the project's "log and no-op on upstream failure" pattern elsewhere; no defensive workaround needed here |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `AutoBattleResolver` | This depends on it (outcome only) | State trigger — subscribes to `OnBattleComplete`, reads `BattleResult` |
| `InputHandler` | This depends on it | State trigger — subscribes to speed-up events for indicator |
| `RunManager` | This depends on it (outcome only) | Rule dependency — "Continue" button triggers phase advance |

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| Outcome overlay fade-in duration | 0.5s | 0.2–1.0s | Slower, more dramatic reveal | Immediate |

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| `OnBattleComplete` (win) | Victory overlay fades in; living students celebrate | Victory fanfare (AudioSystem) | MVP |
| `OnBattleComplete` (lose) | Defeat overlay fades in; screen dims | Defeat sting (AudioSystem) | MVP |
| Speed-up active | ">> FAST" indicator visible in corner | None | MVP |

---

## Game Feel

### Feel Reference

> "The outcome should feel decisive — the moment the fight resolves, the result is unmistakable. NOT a vague fade that leaves the player wondering who won."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Speed-up key held → FAST indicator appears | 1 frame | 1 frame |
| "Continue" click → phase advances | 1 frame | 1 frame |

### Animation Feel Targets

| Animation | Startup Frames | Active Frames | Recovery Frames | Feel Goal |
|---|---|---|---|---|
| Outcome overlay fade-in | 0 | 30 (0.5s) | 0 | Clear, decisive reveal |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Battle resolves (win) | 0.5s | Overlay fades in; fanfare plays |
| Battle resolves (loss / run-ending) | 0.5s | Screen desaturates; defeat sting plays |

### Weight and Responsiveness

- **Weight**: The outcome reveal should feel conclusive, not tentative
- **Player control**: Zero during battle — the player watches the hex board; control returns only at the "Continue" button
- **Snap quality**: The outcome resolves discretely and visually via the overlay
- **Failure texture**: When the run ends in defeat, the outcome overlay itself communicates it clearly (desaturation + sting) — no ambiguity about the result

### Feel Acceptance Criteria

- [ ] Win/lose outcome is visually unambiguous within 1 second of the final action

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Speed-up indicator (">> FAST") | Top-right corner | On `OnSpeedUpStarted` / `OnSpeedUpCancelled` | When held |
| Outcome overlay (Victory / Defeated) | Full-screen overlay | On `OnBattleComplete` | After battle |
| "Continue" button | Outcome overlay footer | On `OnBattleComplete` | After battle |
| Hero Info Panel (stats/skill/traits) | Right-docked panel | On `HeroSelection.OnHeroSelected` | Placement Phase and during battle |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| `AutoBattleResolver.OnBattleComplete` | `AutoBattleResolver.md` | `BattleResult` struct | State trigger |
| `InputHandler.OnSpeedUpStarted` | `InputHandler.md` | Speed-up event | Rule dependency |
| HUDs are read/listen only | `best-practices.md` | "HUDs are read/listen only" rule | Rule dependency |
| `ChampionData`, `ChampionRoster` | `BattleBoardManager.md` | Champion stat/trait data source | Data dependency |

---

## Acceptance Criteria

- [ ] `OnBattleComplete` shows the correct outcome overlay (Victory vs Defeated)
- [ ] "Continue" button is only clickable after `OnBattleComplete` fires
- [ ] Speed-up indicator appears within 1 frame of `OnSpeedUpStarted`
- [ ] No `FindObjectOfType` in `BattleHUD.cs`
- [ ] `BattleHUD` does not subscribe to or read any per-combatant data (`OnCombatantsSet` / `OnCombatantActed` / `OnCombatantDefeated` / `GetCombatantSnapshots()`) — all in-battle visual feedback is owned by `BattleBoardManager`
- [ ] Clicking a bench card or a placed `BattleUnit` shows that hero's stats, skill (from `BattleBehaviorFlag`s), and traits (plain names, no count/breakpoint) in the Hero Info Panel
- [ ] `HeroInfoPanel` holds no reference to `BattleBoardManager` and is held by no reference from it — selection travels only through the `HeroSelection` static event
- [ ] Clicking an enemy unit shows its stats and skill in the Hero Info Panel, with "Traits: None" (enemies have no trait data)

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should the speed-up be a toggle (click once to enable, click again to disable) instead of hold? | Designer | Before Alpha | Pending — recommend hold for v1 (same as TFT) |
