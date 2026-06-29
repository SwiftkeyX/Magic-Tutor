# AutoBattleResolver

> **Status**: Draft
> **Last Updated**: 2026-06-29
> **Implements Pillar**: Empowering — the payoff moment; the player watches the team they built and trained dismantle the enemy

## Summary

AutoBattleResolver runs a tick-based combat simulation between the trained student team and the year's enemy squad. It runs as a coroutine with configurable tick delays (so the player can watch the battle unfold), fires per-tick events for BattleHUD to animate, and fires `OnBattleComplete(BattleResult)` when the fight ends. No player input occurs during battle. It reads student stats from StudentRoster (already modified by `TraitSystem.ResolveBattleBuffs()`), enemy data from EnemyDatabase, and behavior flags from TraitSystem.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `TraitSystem, EnemyDatabase`

---

## Overview

AutoBattleResolver is a MonoBehaviour in the Battle scene. RunManager calls `Resolve()` after `TraitSystem.ResolveBattleBuffs()` completes. `Resolve()` is a coroutine: each tick it advances all combatants' action timers, resolves actions for any unit whose timer hits zero (in Speed order), applies damage and behavior flags, removes defeated units, and checks the win condition. Between ticks it yields for `TickDelay` seconds (default 0.5s) so the player can see each step. When `OnSpeedUpStarted` fires from InputHandler, `TickDelay` is reduced to `FastTickDelay` (default 0.05s). When the fight resolves, `OnBattleComplete(BattleResult)` fires and the coroutine ends.

## Player Fantasy

The player watches their carefully trained team face off against the year's enemies and feels either growing confidence ("the Fire synergy is shredding them") or mounting dread ("we're going to lose"). The simulation is readable — each hit is visible, each defeat is dramatized — and the outcome feels earned, not arbitrary.

---

## Detailed Design

### BattleCombatant (Internal Data)

AutoBattleResolver builds this from StudentData/EnemyData at the start of `Resolve()`. Never persisted.

```csharp
class BattleCombatant {
    string Id;                         // matches StudentId or EnemyId
    string DisplayName;
    bool IsStudent;                    // false = enemy unit
    int MaxHP;
    int CurrentHP;
    int Attack;
    int Speed;
    int ActionTimer;                   // ticks remaining until this unit acts; initialized to ActionInterval
    int ActionInterval;                // = max(1, BaseActionInterval - Speed)
    List<BattleBehaviorFlag> Flags;    // from TraitSystem (students) or EnemyDatabase (enemies)
    bool HasActedThisBattle;           // used by FirstHitDouble flag
    bool IsDefeated => CurrentHP <= 0;
}
```

### BattleResult

```csharp
struct BattleResult {
    bool Won;
    int TicksElapsed;
    bool TimedOut;                     // true if MaxBattleTicks was reached
}
```

### Core Rules

1. **Phase guard**: `Resolve()` checks that `RunManager.Instance.CurrentPhase == RunPhase.Battle`. If not, logs an error and returns without starting.
2. **Combatant initialization**: students are built from `StudentRoster.GetAll()` (stats already buffed by TraitSystem); enemies are built from `EnemyDatabase.GetEnemiesForYear(currentYear)`.
3. **Action interval**: `ActionInterval = max(1, BaseActionInterval - combatant.Speed)`. Higher Speed → shorter interval → acts more often.
4. **Tick loop**: each tick, all combatants' `ActionTimer` decrements by 1. Any unit with `ActionTimer == 0` acts this tick. After acting, `ActionTimer` resets to `ActionInterval`.
5. **Action order within a tick**: if multiple units act on the same tick, they are ordered by Speed descending (higher Speed goes first). Ties broken by array insertion order.
6. **Targeting**: each attacking unit targets the living opponent with the lowest `CurrentHP`. Tie broken randomly (Unity `Random.Range`).
7. **Base damage**: `damage = attacker.Attack` (flat; no defense stat in v1).
8. **Behavior flag processing** (applied per-attack, in order):
   - `FirstHitDouble`: if `!combatant.HasActedThisBattle`, multiply damage by 2. Set `HasActedThisBattle = true`.
   - `TakesReducedDamage` (on target): multiply damage by `(1 - DamageReductionFraction)` before applying.
   - `AOEAttack` (on attacker): damage is applied to **all living enemy units** at the attacker's base damage. (Other flags still apply per-target.)
9. **ShadowSurge** (if active): on the first tick only, Shadow-flagged students with this behavior have their `ActionInterval` reduced by `ShadowSurgeIntervalReduction` for that tick only (effectively acting earlier).
10. **Defeat**: a combatant with `CurrentHP ≤ 0` is immediately marked defeated and removed from the active list. Its `OnCombatantDefeated` event fires.
11. **Win condition check** happens after every action: if all enemies are defeated → `Won = true` → stop loop. If all students are defeated → `Won = false` → stop loop.
12. **Timeout**: if `TicksElapsed ≥ MaxBattleTicks` and no win condition is met, the battle times out. Outcome: if more students are alive than enemies (by count), students win; otherwise students lose. `BattleResult.TimedOut = true`.
13. **`OnBattleComplete`** fires once, after the coroutine resolves. RunManager subscribes to it to advance the phase.

### Battle Simulation Flow

```
Resolve() coroutine:
  1. Guard: check CurrentPhase == Battle
  2. Build BattleCombatant list from StudentRoster + EnemyDatabase
  3. Apply BattleBehaviorFlags from TraitSystem.GetActiveBattleBehaviors()
  4. Initialize ActionTimers to each unit's ActionInterval
  5. TICK LOOP:
     a. Decrement all ActionTimers by 1
     b. Collect units with ActionTimer == 0, sort by Speed DESC
     c. For each acting unit:
        i.  Select target (lowest CurrentHP among opponents)
        ii. Compute damage (base + behavior flags)
        iii.Apply damage to target (and all targets if AOEAttack)
        iv. Fire OnCombatantActed event
        v.  If target.CurrentHP <= 0: mark defeated, fire OnCombatantDefeated
        vi. Reset actor's ActionTimer to ActionInterval
     d. Check win condition → if met, break loop
     e. TicksElapsed++
     f. If TicksElapsed >= MaxBattleTicks → timeout, determine outcome, break
     g. yield return new WaitForSeconds(TickDelay)   ← observes _currentTickDelay
  6. Fire OnBattleComplete(BattleResult)
```

### Speed-Up Mode

InputHandler fires `OnSpeedUpStarted` / `OnSpeedUpCancelled`. AutoBattleResolver maintains `_currentTickDelay`:
- Normal: `_currentTickDelay = NormalTickDelay` (default 0.5s)
- Held Space: `_currentTickDelay = FastTickDelay` (default 0.05s)

The coroutine reads `_currentTickDelay` on every `yield` — the change takes effect immediately on the next tick.

### Pause Support

When RunManager calls `Pause()`, AutoBattleResolver sets `_paused = true`. The coroutine inserts a `while (_paused) yield return null;` before the `yield WaitForSeconds`, effectively halting the simulation until `Resume()` clears the flag. No tick processing occurs while paused.

### Per-Tick Events (for BattleHUD)

```
event Action<string actorId, string targetId, int damage, List<string> flagsTriggered>
    OnCombatantActed

event Action<string combatantId>
    OnCombatantDefeated

event Action<BattleResult>
    OnBattleComplete
```

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `RunManager` | Calls `Resolve()` after `TraitSystem.ResolveBattleBuffs()`; subscribes to `OnBattleComplete` to advance phase |
| `TraitSystem` | Reads modified student stats from StudentData (already applied by `ResolveBattleBuffs()`); reads `GetActiveBattleBehaviors(studentId)` for behavior flags |
| `EnemyDatabase` | Reads `GetEnemiesForYear(currentYear)` to build enemy combatants |
| `InputHandler` | Subscribes to `OnSpeedUpStarted` / `OnSpeedUpCancelled` to toggle `_currentTickDelay` |
| `BattleHUD` | Subscribes to `OnCombatantActed`, `OnCombatantDefeated`, `OnBattleComplete` for animation |
| `StudentRoster` | Reads `GetAll()` at the start of `Resolve()` — never writes |

---

## Formulas

### Action Interval

```
ActionInterval = max(1, BaseActionInterval - combatant.Speed)
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `BaseActionInterval` | int | 10–15 | `BattleConfig` ScriptableObject | Ticks at Speed 0; higher = slower combat pace |
| `combatant.Speed` | int | 1–∞ | StudentData / EnemyData | Higher = shorter interval = more frequent actions |

**Expected output**: Speed 3 → interval 7; Speed 8 → interval 2; Speed 10 → interval 1 (minimum).

### Damage Calculation

```
damage = attacker.Attack

// FirstHitDouble (if flag active and first action):
damage = damage * 2

// TakesReducedDamage (if flag on target):
damage = floor(damage * (1 - DamageReductionFraction))

// Clamp:
damage = max(1, damage)
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `attacker.Attack` | int | 1–∞ | StudentData (post-buff) / EnemyData | Base damage per hit |
| `DamageReductionFraction` | float | 0.0–0.5 | `BattleConfig` / TraitDatabase | Shield trait damage reduction fraction |

**Expected output range**: minimum 1 damage per hit (floor clamp).

### AOEAttack Damage

```
// Applied to each enemy independently at the same base damage:
foreach enemy in livingEnemies:
    enemy.CurrentHP -= max(1, damage)
```

AOE applies the same damage to all targets (no split damage). Each target's `TakesReducedDamage` flag is evaluated independently.

### Timeout Tiebreaker

```
if (livingStudentCount > livingEnemyCount) → Won = true
else → Won = false
```

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `Resolve()` called outside Battle phase | Log error, no-op | Phase guard — best-practices rule |
| `Resolve()` called while already running | Log error, no-op | Only one simulation at a time |
| All students are already defeated before first tick | `OnBattleComplete(Won: false)` fires immediately | Edge case on Year 3 with heavily debuffed team |
| AOEAttack kills multiple enemies in one action | Each enemy's `OnCombatantDefeated` fires in sequence; win condition checked after all are processed | Correct; avoid checking mid-AOE resolution |
| `FirstHitDouble` flag on an enemy | Enemy uses the same flag logic (HasActedThisBattle). Valid if EnemyDatabase assigns it. | Symmetric flag handling |
| Battle timeout with equal survivor counts | Students lose (tie goes to enemy) | Harsh but consistent with "Challenging" pillar |
| `_paused = true` when coroutine is between ticks | Coroutine halts at the `while (_paused)` guard on next iteration | No partial tick processing while paused |
| TickDelay changed to FastTickDelay mid-battle | Takes effect on the next `yield` | Immediate switch within 1 tick |
| `ShadowSurge` flag: reduces ActionInterval below 1 | Clamped to 1 | Minimum interval rule |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `TraitSystem` | This depends on it | Data dependency — reads behavior flags; stats already buffed by ResolveBattleBuffs() |
| `EnemyDatabase` | This depends on it | Data dependency — reads enemy definitions per year |
| `StudentRoster` | This depends on it | Data dependency — reads student stats at simulation start (read-only) |
| `RunManager` | It depends on this | State trigger — calls `Resolve()`; subscribes to `OnBattleComplete` |
| `InputHandler` | It depends on this | Rule dependency — fires speed-up events that AutoBattleResolver acts on |
| `BattleHUD` | It depends on this | State trigger — subscribes to per-tick events for animation |

---

## Tuning Knobs

All knobs in `BattleConfig.asset` (ScriptableObject):

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `BaseActionInterval` | 10 | 6–20 | Slower combat; more ticks per battle | Faster combat; Speed has less relative impact |
| `NormalTickDelay` | 0.5s | 0.2–2.0s | Battle plays slower; more dramatic | Faster default speed; less time to observe |
| `FastTickDelay` | 0.05s | 0.01–0.2s | Speed-up is less extreme | Almost instant speed-up |
| `MaxBattleTicks` | 200 | 100–500 | Longer battles before timeout | Earlier timeout; more draws |
| `DamageReductionFraction` | 0.20 | 0.0–0.50 | Shield trait is much tankier | Shield trait has less defensive value |
| `ShadowSurgeIntervalReduction`| 3 | 1–5 | Shadow acts much earlier on tick 1 | Shadow speed advantage is smaller |

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| `OnCombatantActed` (attack) | Attacker lunges toward target; damage number floats | Attack SFX (varies by trait) | MVP |
| `OnCombatantActed` (AOEAttack) | Attack lines radiate to all enemies; damage numbers on each | AoE SFX (bigger) | MVP |
| `OnCombatantActed` (FirstHitDouble) | Larger impact flash; bigger damage number | Distinct "first hit" SFX | Alpha |
| `OnCombatantDefeated` (enemy) | Enemy unit fades/collapses | Enemy defeat SFX | MVP |
| `OnCombatantDefeated` (student) | Student unit falls; dimmed | Student defeat SFX (solemn) | MVP |
| `OnBattleComplete` (win) | Victory flash; all living students animate | Victory fanfare | MVP |
| `OnBattleComplete` (lose) | Screen dims; all students defeated | Defeat sting | MVP |
| Speed-up active | Tick delay indicator in BattleHUD | None | MVP |

---

## Game Feel

### Feel Reference

> "Should feel like TFT's battle phase — readable, unit-by-unit action that plays out in real time. Each hit lands with weight; the outcome builds tension. NOT a screen of numbers that resolves in 1 second with no visual narrative."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| `OnSpeedUpStarted` → tick rate changes | 1 frame + current tick remainder | ≤ 1 tick delay |

### Animation Feel Targets

| Animation | Startup Frames | Active Frames | Recovery Frames | Feel Goal |
|---|---|---|---|---|
| Attack lunge | 3 | 8 | 4 | Snappy, committed |
| Damage number float | 0 | 30 | 0 | Clear, readable |
| Unit defeat | 0 | 20 | 0 | Legible, slightly sad for students |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| First Shadow hit (FirstHitDouble) | 0.1s hit-stop | Freeze both units for 3 frames; large number |
| Enemy wave cleared (all enemies dead) | 0.5s | Brief flash; victory sound; students "celebrate" |
| Student defeated | 0.3s | Student card dims; sound of disappointment |

### Weight and Responsiveness

- **Weight**: Hits should feel physically impactful — not weightless damage ticks
- **Player control**: Zero during battle — the player watches and reacts
- **Snap quality**: Each tick resolves discretely and cleanly; no blurred state between ticks
- **Failure texture**: When a student falls, it is immediately visible and attributable (the enemy that hit them is still active)

### Feel Acceptance Criteria

- [ ] A full battle (5 students vs Year-1 enemies) plays out in under 30 seconds at normal speed
- [ ] Speed-up reduces perceived battle time to under 5 seconds
- [ ] Every hit is accompanied by a visible damage number and SFX
- [ ] The win/lose outcome is visually unambiguous within 2 seconds of the final action

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Student HP bars | BattleHUD student cards | On `OnCombatantActed` (when student is target) | During battle |
| Enemy HP bars | BattleHUD enemy display | On `OnCombatantActed` (when enemy is target) | During battle |
| Active behavior flags | Icon overlay on student/enemy card | Set once at battle start | During battle |
| Ticks elapsed | Debug display only (hidden in release) | Every tick | Dev builds |
| Speed-up indicator | BattleHUD corner (e.g. ">> FAST") | On speed-up toggle | When held |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| TraitSystem must resolve before AutoBattleResolver | `TraitSystem.md`, `RunManager.md` | `ResolveBattleBuffs()` ordering invariant | Rule dependency |
| BattleBehaviorFlags: TakesReducedDamage, AOEAttack, FirstHitDouble, ShadowSurge | `TraitSystem.md` | Flag definitions | Data dependency |
| EnemyDatabase provides year-scaled enemies | `EnemyDatabase.md` | `GetEnemiesForYear(year)` | Data dependency |
| InputHandler fires speed-up events | `InputHandler.md` | `OnSpeedUpStarted` / `OnSpeedUpCancelled` | Rule dependency |
| Best-practices: AutoBattleResolver only runs during Battle phase | `best-practices.md` | Phase guard rule | Rule dependency |

---

## Acceptance Criteria

- [ ] `Resolve()` called outside `RunPhase.Battle` logs an error and does not start
- [ ] A battle with students who have higher combined stats than enemies results in a student win (statistical expectation across 10 runs — not a deterministic guarantee, as Speed ordering introduces variance)
- [ ] `OnBattleComplete` fires exactly once per `Resolve()` call
- [ ] Speed-up reduces `_currentTickDelay` to `FastTickDelay` within 1 tick of `OnSpeedUpStarted`
- [ ] `FirstHitDouble` applies to only the first action of a flagged unit per battle
- [ ] `AOEAttack` applies damage to all living enemies independently
- [ ] `TakesReducedDamage` reduces incoming damage by `DamageReductionFraction` (floored to int, minimum 1)
- [ ] `MaxBattleTicks` prevents an infinite simulation (verified: simulation always terminates)
- [ ] Pause halts tick advancement without losing any state
- [ ] Unit test: 1 student (Attack=10, MaxHP=50, Speed=5) vs 1 enemy (Attack=5, MaxHP=20, Speed=3) → student wins in ≤ 20 ticks

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should enemies have behavior flags (e.g., a Year-3 enemy with `FirstHitDouble`)? | Designer | Before code-system | Pending — recommend yes for Year 3 to increase difficulty surprise |
| Should the timeout tiebreaker use HP totals (remaining HP%) instead of unit counts? | Designer | Before code-system | Pending — HP-based is more nuanced; count-based is simpler |
| Should students target the lowest-HP enemy, or the highest-HP (to eliminate threats faster)? | Designer | Before code-system | Pending — lowest HP (focus-fire) is the standard auto-battler behavior |
| Is the `ShadowSurge` flag for the first round only, or does Shadow always get a Speed boost? | Designer | Before code-system | Pending — first round only (as designed); confirm |
| Should the battle simulation be fully deterministic (no randomness in targeting ties)? | Engineer | Before code-system | Pending — recommend deterministic (seed-based or sorted ID tiebreaker) for reproducibility |
