# BattleHUD

> **Status**: Draft
> **Last Updated**: 2026-07-03
> **Implements Pillar**: Empowering — the battle screen is the payoff; the player watches their trained team fight and feels the direct consequence of every training decision

## Summary

BattleHUD renders the Battle scene UI during the auto-battle simulation. It subscribes to AutoBattleResolver's per-tick events (`OnCombatantActed`, `OnCombatantDefeated`, `OnBattleComplete`) to animate attacks, display HP changes, show damage numbers, and reveal the outcome. It is purely a visual listener — it never calls any method that changes battle state, and it never reads from StudentRoster or EnemyDatabase directly after initialization.

> **Quick reference** — Layer: `Presentation` · Priority: `MVP` · Key deps: `AutoBattleResolver`

---

## Overview

BattleHUD is a MonoBehaviour on a UIDocument in the Battle scene. On scene load it reads the initial combatant states (students from StudentRoster, enemies from the first `OnCombatantActed` or from a pre-battle snapshot provided by AutoBattleResolver), builds combatant cards for each unit, then hands control to AutoBattleResolver. From that point on, all updates are event-driven: each `OnCombatantActed` triggers an attack animation + damage number on the target; each `OnCombatantDefeated` collapses the unit's card; `OnBattleComplete` shows the outcome screen. A speed-up button (held) reduces tick delay by calling InputHandler's speed-up action — BattleHUD does not control this directly.

## Player Fantasy

Watching the battle should feel like watching an exciting sports match — the player is invested, not bored. Each hit registers with visible weight. The outcome is readable moment-to-moment: "the enemy is almost dead" or "my team is being demolished." The speed-up option means the player is never trapped watching a slow battle.

---

## Detailed Design

### Core Rules

1. BattleHUD subscribes to AutoBattleResolver events in `OnEnable` and unsubscribes in `OnDisable`:
   - `AutoBattleResolver.OnCombatantActed` → trigger attack animation + damage number on target
   - `AutoBattleResolver.OnCombatantDefeated` → collapse unit card
   - `AutoBattleResolver.OnBattleComplete` → show outcome screen
2. **BattleHUD never writes game state.** It is a pure visual consumer of AutoBattleResolver events.
3. **Combatant card initialization**: BattleHUD subscribes to `AutoBattleResolver.OnCombatantsSet` in `OnEnable` (alongside its other event subscriptions — see Core Rule 1). When it fires, BattleHUD calls `GetCombatantSnapshots()` to read the now-current data and (re)builds the card layout. `OnCombatantsSet` may fire more than once before battle starts (once for the full roster, once more if `RunManager.ConfirmSquadPlacement()` narrows it to the fielded squad — see `RunManager.md`), so the build routine must be safe to call repeatedly (clear-and-rebuild, not append). This replaces any `Start()`-time frame-count guess — do not reintroduce a hardcoded delay. Once the first battle tick fires, cards stop rebuilding and are only updated in-place via event callbacks.
4. Student cards are displayed on the left side of the screen; enemy cards on the right.
5. Each combatant card shows: DisplayName, current HP bar, max HP, active BattleBehaviorFlags as icons.
6. Damage numbers float above the target on each `OnCombatantActed`. The number color distinguishes physical (white) from magic (purple) and crit (yellow/gold).
7. When `OnCombatantDefeated` fires, the card animates out (fade + shrink) and is removed from the layout.
8. The speed-up indicator (">> FAST") is visible when InputHandler's speed-up action is held. BattleHUD reads this from the InputHandler event, not from AutoBattleResolver.
9. On `OnBattleComplete(result)`:
   - Win: show "Victory!" overlay; list surviving students
   - Lose: show "Defeated" overlay; somber animation
   - `TimedOut = true`: add "(Time Limit)" note to outcome
   - Outcome screen has a "Continue" button that calls `RunManager.Instance.CompleteCurrentPhase()` (or equivalent) — but this is implemented as a post-battle prompt, not mid-battle interaction.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Initializing` | Scene loads | `OnCombatantsSet` fires (may recur before battle starts) | Building/rebuilding card layout; no per-tick events yet |
| `WatchingBattle` | Battle starts (first tick fires) | `OnBattleComplete` received | Animating events in real time |
| `ShowingOutcome` | `OnBattleComplete` received | Player clicks "Continue" | Outcome overlay visible; "Continue" button active |

### Combatant Card Layout

Each card (one per combatant) displays:
- DisplayName
- HP bar (fills proportionally to CurrentHP / MaxHP)
- HP label ("35 / 80")
- Active flag icons (e.g., fire icon for `AOEAttack`, skull for `FirstHitDouble`)
- Defeat animation slot (plays on `OnCombatantDefeated`)

### Hero Info Panel (Pre-Battle / In-Battle Inspector)

A right-docked panel that shows a single selected hero's stats, skill, and traits. Implemented as its own script, `HeroInfoPanel.cs` — not part of the future `BattleHUD.cs` monolith — because it must be usable during Placement Phase (before any `AutoBattleResolver` events exist), not only during the battle-watching phase this doc otherwise covers.

**Trigger**: clicking a bench card (Placement Phase) or a placed `BattleUnit` on the board (either phase) selects that hero. There is no hover trigger. Clicking a different hero swaps the panel's content; there is no close/dismiss button in this pass — the panel simply holds the last-selected hero's info.

**Fields shown**:
- Header: `DisplayName`, `Role`, `Cost`
- Stats: `MaxHP, ATK, DEF, MG, MR, AttackSpeed, CRIT, Range, MaxMana, StartingMana` — read directly from `ChampionData`
- Skill: the hero's `BattleBehaviorFlag`(s), each rendered through a static one-line description (e.g. "Magic Attack — uses MG/MR instead of ATK/DEF"). Empty list → "No active skill". **This is a placeholder mapping, not a real Skill/Ability system** — no dedicated Skill data model exists yet (`MaxMana`/`StartingMana` on `ChampionData` hint at one for the future, but it isn't designed). Do not treat this section as a spec for a future ability system.
- Traits: `VerticalTrait` and `HorizontalTrait` (skipped if `None`), each shown with its live count and active-breakpoint status pulled from `TraitTracker`

**Enemy units are inspectable too**, via an adapter: `HeroInfoPanel` holds its own `EnemyDatabaseStub` reference (same self-resolved-reference pattern as `ChampionRoster`/`TraitTracker`) and falls back to it when a clicked ID isn't a player champion. Enemies render through the same panel layout (Header/Stats/Skill/Traits) but with no Role/Cost in the header and a hardcoded "Traits: None" (enemies have no `VerticalTrait`/`HorizontalTrait` — `EnemyDatabaseStub` stays the source of truth for enemy stats; this is not a data-model unification with `ChampionRoster`).

**Still out of scope for this pass**: any close/dismiss control.

**Architecture — decoupled from `BattleBoardManager`, like `TraitHUDController`**: `HeroInfoPanel` is never referenced by `BattleBoardManager`, and never references it back — consistent with this doc's existing peer relationship with `BattleBoardManager` (see Interactions below) and with `best-practices.md`'s "Game systems never reference HUDs" rule. Selection travels through a small static event class, `HeroSelection` (`OnHeroSelected(string championId)` / `Select(string championId)`), which `BenchCardDrag` and `BattleUnit` call into directly on click. `HeroInfoPanel` subscribes to `HeroSelection.OnHeroSelected` in `OnEnable`/unsubscribes in `OnDisable`, then resolves the `ChampionData` itself via its own `ChampionRoster` reference and reads trait progress via its own `TraitTracker` reference — the same self-sufficient pattern `TraitHUDController` already uses for `TraitTracker`. `HeroSelection` is a lightweight decoupling utility, not a new game system.

### Damage Number Variants

| Damage Type | Color | Size Modifier |
|---|---|---|
| Physical (ATK vs DEF) | White | 1× |
| Magic (MG vs MR) | Purple | 1× |
| Critical hit (either type) | Gold | 1.5× |
| FirstHitDouble + Crit | Gold + glow | 2× |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `AutoBattleResolver` | Subscribes to `OnCombatantsSet` (rebuilds cards when it fires), `OnCombatantActed`, `OnCombatantDefeated`, `OnBattleComplete`; calls `GetCombatantSnapshots()` when `OnCombatantsSet` fires to get the data to build from |
| `BattleBoardManager` | BattleHUD overlays HP bars and damage numbers over the hex board; BattleHUD does NOT own the board — it is a peer that shares `AutoBattleResolver` events |
| `InputHandler` | Subscribes to `OnSpeedUpStarted` / `OnSpeedUpCancelled` to toggle the speed indicator UI |
| `RunManager` | "Continue" button on outcome screen calls `RunManager` to advance phase (read-only access to confirm battle is done) |
| `HeroInfoPanel` → `ChampionRoster` | `HeroInfoPanel` reads `ChampionData` directly via `ChampionRoster.GetChampionById()` — same self-sufficient read pattern as `TraitHUDController` → `TraitTracker` |
| `HeroInfoPanel` → `TraitTracker` | Reads `GetTraitCounts()` / `GetActiveBreakpoints()` to show the selected hero's live trait progress |
| `BenchCardDrag` / `BattleUnit` → `HeroSelection` (static event) | Both call `HeroSelection.Select(championId)` on click; `HeroInfoPanel` subscribes to `HeroSelection.OnHeroSelected` — no direct reference between `BattleBoardManager` and `HeroInfoPanel` in either direction |

---

## Formulas

No formulas — BattleHUD only reads values provided by AutoBattleResolver events. HP bar fill ratio:

```
hpBarFill = currentHP / maxHP   (clamped 0–1, computed in UI Toolkit binding)
```

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `OnCombatantActed` fires for an ID not in the card list | Log warning, no animation | Guard against stale event IDs |
| `OnCombatantDefeated` fires for an already-defeated unit | No-op | Idempotent — unit card already removed |
| `OnBattleComplete` fires before all defeat animations finish | Outcome overlay waits for active animations to complete (max 0.5s grace period) | Clean visual ordering |
| Speed-up held during outcome screen | Speed indicator visible; "Continue" button still clickable | Speed-up only affects tick delay in AutoBattleResolver; no HUD impact |
| Battle ends in timeout | `TimedOut = true` in result; outcome overlay shows "(Time Limit)" note | Inform player of edge case |
| `OnCombatantsSet` never fires (e.g. `RunManager` errors before calling `SetCombatants()`) | Cards never build; BattleHUD stays empty | Acceptable failure mode — matches the project's "log and no-op on upstream failure" pattern elsewhere; no defensive workaround needed here |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `AutoBattleResolver` | This depends on it | State trigger — subscribes to `OnCombatantsSet` (card build/rebuild trigger), per-tick, and outcome events; reads snapshot data via `GetCombatantSnapshots()` when `OnCombatantsSet` fires |
| `BattleBoardManager` | Peer (shared events) | Both subscribe to `AutoBattleResolver`; BattleHUD renders UI overlays over the board |
| `InputHandler` | This depends on it | State trigger — subscribes to speed-up events for indicator |
| `RunManager` | This depends on it (outcome only) | Rule dependency — "Continue" button on outcome triggers phase advance |

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| Damage number display duration | 0.8s | 0.3–2.0s | Numbers linger longer | Numbers disappear faster |
| Defeat card animation duration | 0.4s | 0.2–1.0s | More dramatic unit defeat | Snappier cleanup |
| Outcome overlay fade-in duration | 0.5s | 0.2–1.0s | Slower, more dramatic reveal | Immediate |
| HP bar update speed | Instant | Instant/0.2s tween | Smooth bar drain (cinematic) | Immediate bar jump (readable) |

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Battle scene loads | Combatant cards appear; brief "battle start" flash | Battle music begins (AudioSystem) | MVP |
| `OnCombatantActed` (physical hit) | Attacker lunges; damage number (white) floats on target; HP bar drains | Physical hit SFX | MVP |
| `OnCombatantActed` (magic hit) | Spell FX on target; damage number (purple) | Magic SFX | MVP |
| CRIT hit | Larger impact flash; gold damage number | Distinct crit SFX | MVP |
| `FirstHitDouble` triggered | Oversized first-hit animation; gold + glow number | First-hit impact SFX | Alpha |
| `AOEAttack` | Lines radiate from attacker to all enemies; damage numbers on each | AoE SFX (larger) | MVP |
| `OnCombatantDefeated` (enemy) | Enemy card shrinks + fades; brief flash | Enemy defeat SFX | MVP |
| `OnCombatantDefeated` (student) | Student card dims + collapses; solemn animation | Student defeat SFX | MVP |
| `OnBattleComplete` (win) | Victory overlay fades in; living students celebrate | Victory fanfare (AudioSystem) | MVP |
| `OnBattleComplete` (lose) | Defeat overlay fades in; screen dims | Defeat sting (AudioSystem) | MVP |
| Speed-up active | ">> FAST" indicator visible in corner | None | MVP |

---

## Game Feel

### Feel Reference

> "Should feel like TFT's battle phase — each unit is a character, each hit lands with readable weight, and the outcome builds from moment to moment. NOT a blur of numbers that resolves before the player can process it."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Speed-up key held → FAST indicator appears | 1 frame | 1 frame |
| "Continue" click → phase advances | 1 frame | 1 frame |

### Animation Feel Targets

| Animation | Startup Frames | Active Frames | Recovery Frames | Feel Goal |
|---|---|---|---|---|
| Attack lunge | 3 | 8 | 4 | Snappy, committed |
| Damage number float | 0 | 48 (0.8s) | 0 | Clear, readable |
| HP bar drain | 0 | 12 (0.2s tween) | 0 | Smooth, not jarring |
| Unit defeat collapse | 0 | 24 (0.4s) | 0 | Legible, slightly dramatic |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| FirstHitDouble + Crit combo | 3-frame hit-stop | Both units freeze briefly; oversized gold number |
| Final enemy defeated | 0.5s | Flash; all living students "celebrate" pose |
| Last student falls (run-ending loss) | 0.5s | Screen desaturates; defeat sting plays |

### Weight and Responsiveness

- **Weight**: Hits should feel physically impactful — not weightless damage ticks
- **Player control**: Zero during battle — the player watches
- **Snap quality**: Each event resolves discretely and visually
- **Failure texture**: When a student falls, the cause is visible — the enemy that attacked them is highlighted for 1 second

### Feel Acceptance Criteria

- [ ] A full Year-1 battle (5 students vs 3 enemies) plays out in under 20 seconds at normal speed
- [ ] Speed-up reduces perceived battle time to under 3 seconds
- [ ] Every hit is accompanied by a visible damage number and SFX
- [ ] Win/lose outcome is visually unambiguous within 1 second of the final action
- [ ] Physical and magic damage numbers are visually distinct

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Student HP bars and labels | Left column cards | On `OnCombatantActed` (student targeted) | During battle |
| Enemy HP bars and labels | Right column cards | On `OnCombatantActed` (enemy targeted) | During battle |
| Active flag icons per unit | Card header | Set once at battle start | During battle |
| Damage numbers | Floating above target card | Per `OnCombatantActed` | During battle |
| Speed-up indicator (">> FAST") | Top-right corner | On `OnSpeedUpStarted` / `OnSpeedUpCancelled` | When held |
| Outcome overlay (Victory / Defeated) | Full-screen overlay | On `OnBattleComplete` | After battle |
| "Continue" button | Outcome overlay footer | On `OnBattleComplete` | After battle |
| Hero Info Panel (stats/skill/traits) | Right-docked panel | On `HeroSelection.OnHeroSelected` | Placement Phase and during battle |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| `AutoBattleResolver.OnCombatantsSet` | `AutoBattleResolver.md` | Setup event signature | State trigger |
| `AutoBattleResolver.OnCombatantActed` | `AutoBattleResolver.md` | Event signature | State trigger |
| `AutoBattleResolver.OnCombatantDefeated` | `AutoBattleResolver.md` | Event signature | State trigger |
| `AutoBattleResolver.OnBattleComplete` | `AutoBattleResolver.md` | `BattleResult` struct | State trigger |
| `DamageType.Physical` / `DamageType.Magic` | `TraitSystem.md` | `DamageType` enum | Data dependency |
| `InputHandler.OnSpeedUpStarted` | `InputHandler.md` | Speed-up event | Rule dependency |
| HUDs are read/listen only | `best-practices.md` | "HUDs are read/listen only" rule | Rule dependency |
| `ChampionData`, `ChampionRoster` | `BattleBoardManager.md` | Champion stat/trait data source | Data dependency |
| `TraitTracker.GetTraitCounts()` / `GetActiveBreakpoints()` | `TraitSystem.md` | Live trait progress | Data dependency |

---

## Acceptance Criteria

- [ ] Combatant cards are built correctly from `GetCombatantSnapshots()`, triggered by `OnCombatantsSet`, before the first tick — never by a hardcoded frame/time delay
- [ ] If `OnCombatantsSet` fires more than once before battle starts (roster narrowed to fielded squad), cards rebuild cleanly with no duplicates or stale entries
- [ ] HP bar and label update correctly on every `OnCombatantActed` event targeting that unit
- [ ] Damage number color is white for physical, purple for magic, gold for crits
- [ ] `OnCombatantDefeated` removes the unit card (or collapses it) within 1 frame + animation duration
- [ ] `OnBattleComplete` shows the correct outcome overlay (Victory vs Defeated)
- [ ] "Continue" button is only clickable after `OnBattleComplete` fires
- [ ] Speed-up indicator appears within 1 frame of `OnSpeedUpStarted`
- [ ] No `FindObjectOfType` in `BattleHUD.cs`
- [ ] No direct reads from `StudentRoster` or `EnemyDatabase` after initialization
- [ ] Clicking a bench card or a placed `BattleUnit` shows that hero's stats, skill (from `BattleBehaviorFlag`s), and trait progress in the Hero Info Panel
- [ ] `HeroInfoPanel` holds no reference to `BattleBoardManager` and is held by no reference from it — selection travels only through the `HeroSelection` static event
- [ ] Clicking an enemy unit shows its stats and skill in the Hero Info Panel, with "Traits: None" (enemies have no trait data)

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should the BattleHUD show a "battle log" text panel (e.g., "Goblin Scout attacks Aria for 8 damage")? | Designer | Before Alpha | Pending — recommend yes (collapsible); helps players understand outcomes |
| Should student cards show their active trait flags (e.g., "AOEAttack") before the battle starts? | Designer | Before Alpha | Pending — recommend yes; sets expectations |
| Should the outcome screen show a summary (total damage dealt, crits landed, etc.)? | Designer | Before Alpha | Pending — recommend minimal summary for MVP; detailed stats in Phase 3 |
| Should the speed-up be a toggle (click once to enable, click again to disable) instead of hold? | Designer | Before Alpha | Pending — recommend hold for v1 (same as TFT) |
