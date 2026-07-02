# AutoBattleResolver

> **Status**: Draft
> **Last Updated**: 2026-06-30
> **Implements Pillar**: Empowering — the payoff moment; the player watches the team they built and trained dismantle the enemy

## Summary

AutoBattleResolver runs a tick-based combat simulation between the placed champion team and the enemy squad. It runs as a coroutine with configurable tick delays (so the player can watch the battle unfold), fires per-tick events for BattleBoardManager and BattleHUD to animate, and fires `OnBattleComplete(BattleResult)` when the fight ends. No player input occurs during battle. It receives pre-applied trait stat bonuses via `ApplyPreBattleTraitModifiers()` (called by `TraitEffectApplier` before `BeginBattle()`), and resolves all trait combat mechanics (shields, omnivamp, stacks, dashes, DoTs, explosions, mana) during the tick loop.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `TraitSystem (via TraitEffectApplier), BattleBoardManager`

---

## Overview

AutoBattleResolver is a MonoBehaviour in the Battle scene. In the **Prototype phase**, `BattleBoardManager` calls `ApplyPreBattleTraitModifiers()`, then `BeginBattle()` — there is no RunManager. `BeginBattle()` starts the coroutine: each tick it runs 6 pre-action phases (DoT, Dreadknight shield check, Trickster dash trigger, untargetable countdown, Kinetic mana tick, bonus actions), then the standard action-timer loop (decrement → find ready actors → attack or move → win check). Between ticks it yields for `TickDelay` seconds (default 0.6s). When the fight resolves, `OnBattleComplete(BattleResult)` fires and the coroutine ends.

## Player Fantasy

The player watches their carefully trained team face off against the year's enemies and feels either growing confidence ("the Fire synergy is shredding them") or mounting dread ("we're going to lose"). The simulation is readable — each hit is visible, each defeat is dramatized — and the outcome feels earned, not arbitrary.

---

## Detailed Design

### BattleCombatant (Internal Data)

AutoBattleResolver builds this from StudentData/EnemyData at the start of `Resolve()`. Never persisted.

```csharp
class BattleCombatant {
    // --- Core identity ---
    string Id;                              // matches ChampionData.Id or EnemyId
    string DisplayName;
    bool   IsPlayer;                        // false = enemy unit
    ChampionRole Role;                      // Tank / Carry / Support
    bool   IsFrontRow;                      // player row >= 2

    // --- Base stats ---
    int MaxHP;
    int CurrentHP;
    int ATK;                                // physical attack damage
    int DEF;                                // armor — reduces incoming physical damage
    int MG;                                 // magic power — used when MagicAttack flag active
    int MR;                                 // magic resistance — reduces incoming magic damage
    int SPD;                                // speed — determines ActionInterval
    int CRIT;                               // critical strike chance (0–100 integer %)
    int Range;                              // attack range in hex distance (1 = melee, 2+ = ranged)
    HexCoord Position;                      // current cell
    int ActionTimer;                        // ticks until next action; initialized to ActionInterval
    int ActionInterval;                     // = max(1, BaseActionInterval - SPD)
    List<BattleBehaviorFlag> Flags;         // MagicAttack (and future flags)
    bool HasActedThisBattle;                // FirstHitDouble guard
    bool IsDefeated => CurrentHP <= 0;

    // --- Shield (Warden / Dreadknight 4) ---
    int  Shield;                            // absorbs damage before HP; set pre-battle by Warden trait

    // --- Omnivamp (Dreadknight) ---
    float OmnivampPct;                      // % of damage dealt healed to attacker
    bool  DreadknightShieldEnabled;         // BP4: trigger low-HP shield
    bool  DreadknightShieldGranted;         // one-time guard per combat

    // --- Striker stacks ---
    int   StrikerStacks;                    // current stack count (0–8)
    float StrikerPctPerStack;               // 0 = trait not active on this unit
    bool  StrikerBypassArmor;               // BP8: attacks ignore 40% of DEF
    bool  StrikerMaxStackSpeedBonus;        // BP6 + Carry: ActionInterval −30% at max stacks

    // --- Mana (Kinetic) ---
    int   Mana;                             // current mana; bonus attack at 100

    // --- Trickster ---
    bool  TricksterDashEnabled;             // BP2
    bool  TricksterBleedEnabled;            // BP4
    bool  TricksterDashTriggered;           // one-time per combat
    bool  TricksterUntargetable;            // true during 2-tick post-dash window
    int   TricksterUntargetableTicks;       // countdown
    bool  TricksterBleedNextAttack;         // next attack applies bleed

    // --- Bleed DoT (lives on the victim) ---
    int   BleedDamagePerTick;
    int   BleedTicksRemaining;

    // --- Elementalist ---
    float ElementalistExplosionPct;         // 0 = not active; 0.10 (BP6) or 0.20 (BP8)
    bool  ElementalistTrueDamage;           // BP8: explosion ignores MR

    // --- Ranger ---
    float RangerBonusDmgPerHex;             // 0 = not active; 0.05 (BP4) or 0.12 (BP6)
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
2. **Combatant initialization**: students are built from `StudentRoster.GetAll()` (stats already buffed by TraitSystem); enemies are built from `EnemyDatabase.GetEnemiesForYear(currentYear)`. All 7 stats (HP, ATK, DEF, MG, MR, SPD, CRIT) are copied.
3. **Action interval**: `ActionInterval = max(1, BaseActionInterval - combatant.SPD)`. Higher SPD → shorter interval → acts more often.
4. **Tick loop**: each tick, all combatants' `ActionTimer` decrements by 1. Any unit with `ActionTimer == 0` acts this tick. After acting, `ActionTimer` resets to `ActionInterval`.
5. **Action order within a tick**: if multiple units act on the same tick, they are ordered by SPD descending (higher SPD goes first). Ties broken by array insertion order.
6. **Positioning and targeting (proximity-based)**: each unit checks whether any enemy is within `Range` hex distance. If yes, it attacks the nearest enemy (by `HexGrid.Distance`; ties broken randomly). If no enemy is in range, the unit moves one hex along the shortest path toward the nearest enemy (by hex distance) instead of attacking. Movement fires `OnCombatantMoved(id, fromCoord, toCoord)` and does **not** deal damage or reset `ActionTimer`. The unit's `Position` in `HexGrid` is updated immediately.
7. **Default attack (physical)**: `rawDamage = attacker.ATK`. Apply DEF mitigation: `damage = max(1, floor(rawDamage × (100 / (100 + target.DEF))))`. Roll CRIT before mitigation (see rule 7a). DEF never fully blocks — minimum 1 damage always dealt.
   - **7a. CRIT roll**: before mitigation, roll `Random.Range(0, 100) < attacker.CRIT`. On crit: `rawDamage × 2`. CRIT is rolled once per attack action.
8. **Magic attack (flag-triggered)**: when the active `BattleBehaviorFlag` specifies `DamageType.Magic`, use `rawDamage = attacker.MG` and mitigate with `target.MR` instead of `target.DEF` using the same formula: `damage = max(1, floor(rawDamage × (100 / (100 + target.MR))))`. CRIT still applies.
9. **Behavior flag processing** (applied per-attack, in order after damage type is resolved):
   - `FirstHitDouble`: if `!combatant.HasActedThisBattle`, multiply `rawDamage` by 2 before mitigation. Set `HasActedThisBattle = true`.
   - `AOEAttack` (on attacker): the computed damage is applied to **all living enemy units** independently. Each target's DEF or MR is evaluated separately.
10. **ShadowSurge** (if active): on the first tick only, Shadow-flagged students with this behavior have their `ActionInterval` reduced by `ShadowSurgeIntervalReduction` for that tick only (effectively acting earlier).
11. **Defeat**: a combatant with `CurrentHP ≤ 0` is immediately marked defeated and removed from the active list. Its `OnCombatantDefeated` event fires.
12. **Win condition check** happens after every action: if all enemies are defeated → `Won = true` → stop loop. If all students are defeated → `Won = false` → stop loop.
13. **Timeout**: if `TicksElapsed ≥ MaxBattleTicks` and no win condition is met, the battle times out. Outcome: if more students are alive than enemies (by count), students win; otherwise students lose. `BattleResult.TimedOut = true`.
14. **`OnBattleComplete`** fires once, after the coroutine resolves. RunManager subscribes to it to advance the phase.

### Pre-Battle API

**`ApplyPreBattleTraitModifiers(perUnitMods, globalSettings)`** — called by `TraitEffectApplier` before `BeginBattle()`. Applies all trait-derived stat deltas (DEF, MR, MG, Range, Shield, Mana, ActionInterval) and sets all runtime flags (OmnivampPct, StrikerPctPerStack, TricksterDashEnabled, etc.) on the Combatant objects. Logs an error and no-ops if called after battle has started.

### Battle Simulation Flow

```
BeginBattle():
  1. Apply player positions from SetUnitPositions(); auto-place enemies on enemy front row
  2. Start BattleLoop coroutine

BattleLoop coroutine per tick:
  --- Pre-action phases (run before timer decrement) ---
  Phase 1 — Bleed DoT:
    For each combatant with BleedTicksRemaining > 0:
      ApplyDamage(target, BleedDamagePerTick)  [bypasses mitigation — true DoT]
      BleedTicksRemaining--
      if IsDefeated → HandleKill(null, target)

  Phase 2 — Dreadknight low-HP shield:
    For each combatant with DreadknightShieldEnabled and !DreadknightShieldGranted:
      if CurrentHP < 40% MaxHP → Shield += 25% MaxHP; DreadknightShieldGranted = true

  Phase 3 — Trickster dash trigger:
    For each combatant with TricksterDashEnabled and !TricksterDashTriggered:
      if CurrentHP < 50% MaxHP → ExecuteTricksterDash(c)

  Phase 4 — Untargetable countdown:
    For each combatant with TricksterUntargetable:
      TricksterUntargetableTicks--
      if TricksterUntargetableTicks <= 0 → TricksterUntargetable = false

  Phase 5 — Kinetic mana tick (every 5 ticks):
    For each living player combatant:
      Mana += KineticManaPerInterval (+ 10 if Support and KineticSupportExtraBonus)
      if Mana >= 100 → Mana -= 100; add to pendingBonusActions

  Phase 6 — Kinetic bonus actions:
    For each unit in pendingBonusActions:
      if not defeated: find target in range → Attack(); else skip
    Clear pendingBonusActions

  --- Standard action loop ---
  a. Decrement all ActionTimers by 1
  b. Collect units with ActionTimer <= 0, sort by SPD DESC
  c. For each acting unit:
     i.  Skip untargetable check on potential targets (FindInRange filters TricksterUntargetable)
     ii. Find nearest enemy in range (HexGrid.Distance <= unit.Range)
     iii.If enemy in range → Attack(actor, target)
                           → fire OnCombatantActed
                           → if target defeated: HandleKill(actor, target)
                           → reset ActionTimer to ActionInterval
                             (if Striker max-stacks + Carry: ActionInterval × 0.70 this reset)
     iv. If no enemy in range → move one hex toward nearest enemy (BFS next step)
                              → fire OnCombatantMoved(id, from, to)
                              → do NOT reset ActionTimer
  d. Check win condition → if met, break loop
  e. TicksElapsed++
  f. If TicksElapsed >= MaxBattleTicks → timeout, determine outcome, break
  g. yield return new WaitForSeconds(TickDelay)
  Fire OnBattleComplete(BattleResult)
```

### New Methods

**`Attack(actor, target)`** — full attack resolution:
1. Determine type: `MagicAttack` flag → use MG vs MR; else ATK vs DEF
2. Apply Striker stack multiplier to rawOffense; if `StrikerBypassArmor`: `effectiveDEF = DEF × 0.6`
3. Apply Ranger per-hex bonus: `damage × (1 + RangerBonusDmgPerHex × hexDistance)` (physical only)
4. Apply base LoL formula: `Max(1, floor(rawOffense × 100 / (100 + effectiveDEF)))`
5. Increment Striker stacks (after calc, before applying damage)
6. Apply shield: `shieldAbsorbed = Min(Shield, damage); Shield -= absorbed; damage -= absorbed`
7. Apply HP damage: `target.CurrentHP -= damage`
8. Apply Omnivamp: `actor.CurrentHP = Min(MaxHP, CurrentHP + floor((damage+absorbed) × OmnivampPct))`
9. Trickster bleed trigger: if `TricksterBleedNextAttack` → set bleed on target; clear flag
10. If `target.IsDefeated` → `HandleKill(actor, target)`

**`HandleKill(actor, target)`** — clears grid occupant, fires `OnCombatantDefeated`, triggers `TriggerElementalistExplosion` if actor has `ElementalistExplosionPct > 0`. `actor` may be null (DoT kill) — no explosion for DoT kills.

**`TriggerElementalistExplosion(actor, killed)`** — deals explosion damage to all adjacent enemy hexes. BP8: true damage (no MR), chains on secondary kills (one level of chaining only).

**`ExecuteTricksterDash(combatant)`** — sets `TricksterDashTriggered = true`, `TricksterUntargetable = true`, `TricksterUntargetableTicks = 2`, `TricksterBleedNextAttack = TricksterBleedEnabled`. Finds the furthest enemy backliner, teleports to an adjacent unoccupied hex, fires `OnCombatantMoved`.

**`ApplyDamage(target, damage)`** — shared helper: absorbs shield first, then HP. Used by DoT and explosion to avoid duplicating shield logic.

**`FindInRange(actor, opponents)`** — filters out `TricksterUntargetable` units before range check.

### Speed-Up Mode

InputHandler fires `OnSpeedUpStarted` / `OnSpeedUpCancelled`. AutoBattleResolver maintains `_currentTickDelay`:
- Normal: `_currentTickDelay = NormalTickDelay` (default 0.5s)
- Held Space: `_currentTickDelay = FastTickDelay` (default 0.05s)

The coroutine reads `_currentTickDelay` on every `yield` — the change takes effect immediately on the next tick.

### Pause Support

When RunManager calls `Pause()`, AutoBattleResolver sets `_paused = true`. The coroutine inserts a `while (_paused) yield return null;` before the `yield WaitForSeconds`, effectively halting the simulation until `Resume()` clears the flag. No tick processing occurs while paused.

### SetUnitPositions (pre-battle injection)

`void SetUnitPositions(Dictionary<string, HexCoord> placements)` — called by `BattleBoardManager` after the player confirms placement. Injects the player's chosen positions into the simulation before `Resolve()` is called. Enemy positions are auto-assigned: enemies fill row 4 of the enemy side (staggered left-to-right by insertion order). Must be called before `Resolve()`; calling it after logs an error and no-ops.

### GetCombatantSnapshots (bench population API)

`List<CombatantSnapshot> GetCombatantSnapshots()` — returns a read-only list of all combatants as lightweight snapshots. Each `CombatantSnapshot` carries: `id`, `displayName`, `isStudent`, `maxHP`, `currentHP`. Called by `BattleBoardManager` during Placement Phase to populate the bench panel without reading `StudentRoster` or `EnemyDatabase` directly. Before `Resolve()` is called the list reflects the full pre-battle roster; once the simulation is running it reflects live battle state (HP updates as combatants take damage). `CombatantSnapshot` is defined in `BattleData.cs`.

### Per-Tick Events (for BattleHUD and BattleBoardManager)

```
event Action<string actorId, string targetId, int damage, List<string> flagsTriggered>
    OnCombatantActed

event Action<string id, HexCoord from, HexCoord to>
    OnCombatantMoved

event Action<string combatantId>
    OnCombatantDefeated

event Action<BattleResult>
    OnBattleComplete
```

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `BattleBoardManager` | Calls `SetUnitPositions()` then `ApplyPreBattleTraitModifiers()` then `BeginBattle()`; subscribes to all events for visual updates |
| `TraitEffectApplier` | Calls `ApplyPreBattleTraitModifiers(perUnitMods, globalSettings)` — sole writer of trait-derived Combatant fields |
| `ChampionRoster` | Provides champion stat data via `GetStudents()` in the lazy-init path (replaces `StudentRosterStub`) |
| `EnemyDatabaseStub` | Provides enemy data until Phase 2 `EnemyDatabase` is built |
| `BattleHUD` | Subscribes to `OnCombatantActed`, `OnCombatantDefeated`, `OnBattleComplete` for animation |
| `HexGrid` | Queried for occupancy during movement and Trickster dash; `GetInRange` used for Elementalist explosion |

---

## Formulas

### Action Interval

```
ActionInterval = max(2, round(1.0 / (combatant.AttackSpeed × TickDelay)))
// TickDelay = 0.6f (seconds per tick, constant in AutoBattleResolver)
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `combatant.AttackSpeed` | float | 0.1–2.0 | `ChampionData` / `EnemyData` | Attacks per second. Linear scale: doubling AS doubles frequency. |
| `TickDelay` | float | 0.6 | `AutoBattleResolver` constant | Seconds per simulation tick |

**Expected output**: AS 0.21 → interval 8; AS 0.33 → interval 5; AS 0.56 → interval 3 (minimum 2).

**Trait speed bonuses** multiply `AttackSpeed` directly (e.g. ×1.176 for Ranger), then re-derive interval. This keeps trait scaling linear and equal across all units regardless of base speed.

> **Why not `BaseActionInterval − SPD`?** The old subtraction formula was non-linear: going from SPD 6→7 gave +33% more attacks, while SPD 2→3 gave only +14%. The same +1 stat had 2.4× more impact at the high end. The AS formula eliminates this imbalance. See `.claude/docs/balance/balance-framework.md` for the full derivation.

### Physical Damage Calculation (default)

```
rawDamage = attacker.ATK

// CRIT roll (before mitigation):
if Random.Range(0, 100) < attacker.CRIT:
    rawDamage = rawDamage * 2

// FirstHitDouble (if flag active and first action):
if !attacker.HasActedThisBattle:
    rawDamage = rawDamage * 2     // stacks multiplicatively with CRIT

// DEF mitigation (LoL formula):
damage = max(1, floor(rawDamage * (100.0 / (100 + target.DEF))))
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `attacker.ATK` | int | 1–∞ | StudentData (post-buff) / EnemyData | Physical attack power |
| `attacker.CRIT` | int | 0–100 | StudentData (post-buff) / EnemyData | Crit chance as integer % |
| `target.DEF` | int | 0–∞ | StudentData (post-buff) / EnemyData | Armor — higher = more physical damage absorbed |

**Expected output**: ATK=10, DEF=0 → 10 dmg; ATK=10, DEF=25 → 8 dmg; ATK=10, DEF=100 → 5 dmg. Minimum 1 always.

### Magic Damage Calculation (flag-triggered)

When a `BattleBehaviorFlag` sets `DamageType.Magic`, MG and MR replace ATK and DEF:

```
rawDamage = attacker.MG

// CRIT and FirstHitDouble apply identically (same roll, same rules)

// MR mitigation (same formula):
damage = max(1, floor(rawDamage * (100.0 / (100 + target.MR))))
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `attacker.MG` | int | 0–∞ | StudentData (post-buff) / EnemyData | Magic power |
| `target.MR` | int | 0–∞ | StudentData (post-buff) / EnemyData | Magic resistance |

### AOEAttack Damage

```
// Applied to each living enemy independently:
foreach enemy in livingEnemies:
    // Each enemy's DEF (or MR) evaluated separately
    enemy.CurrentHP -= computeDamage(attacker, enemy)
```

AOE applies the full computed damage to every target (no split). Each target's DEF/MR is evaluated independently.

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
