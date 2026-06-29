# TraitSystem

> **Status**: Draft
> **Last Updated**: 2026-06-29
> **Implements Pillar**: Strategic & Empowering — trait synergies are the primary build-diversity engine; hitting a threshold unlocks abilities that make the team dramatically stronger

## Summary

TraitSystem counts how many students on the active team share each `TraitType`, determines which trait thresholds are active, and resolves their effects in two stages: (1) `ResolveBattleBuffs()` applies stat multipliers directly to `StudentData` before the battle simulation begins, and (2) `GetActiveBattleBehaviors()` returns behavior flags that `AutoBattleResolver` queries during the simulation. Fires `OnTraitThresholdReached` when the trait count crosses a breakpoint so SchoolHUD can show synergy progress.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `StudentRoster`

---

## Overview

TraitSystem is a MonoBehaviour in the School scene. It subscribes to `StudentRoster.OnRosterChanged` to recount trait totals whenever the roster changes. It stores a `Dictionary<TraitType, int>` of team trait counts and exposes this for SchoolHUD display. At the start of every battle, RunManager calls `ResolveBattleBuffs()` — TraitSystem applies stat multipliers from all active thresholds directly to `StudentData` objects in StudentRoster, then caches the active `BattleBehaviorFlag` set for AutoBattleResolver to query. Trait abilities come in two forms: **StatMultipliers** (applied before battle) and **BattleBehaviorFlags** (queried during simulation). All 6 traits are defined in a `TraitDatabase` ScriptableObject.

## Player Fantasy

The player sees their trait counts building in the SchoolHUD ("Fire: 1/2 — 1 more for synergy!") and feels the pull of completing a threshold. When the threshold pops, there is a visible "synergy activated" moment. Watching two different synergies stack — e.g., Fire 2 + Healer 2 — feels like assembling a machine. A team with three active thresholds feels markedly more powerful than a team with none.

---

## Detailed Design

### TraitType Enum

```csharp
public enum TraitType {
    Fire,    // Offensive attack multiplier
    Healer,  // Team HP sustain
    Shield,  // Defensive survivability
    Arcane,  // Speed and initiative
    Storm,   // Area-of-effect attacks
    Shadow   // Burst damage / first-hit
}
```

### Ability Type System

Trait abilities resolve in two passes:

**StatMultiplier** — applied in `ResolveBattleBuffs()` before battle:
```csharp
struct StatMultiplierEffect {
    bool appliesToTrait;  // true: only students WITH this trait; false: all students
    StatType stat;        // Attack, MaxHP, or Speed
    float multiplier;     // applied as: bonusStat = (int)(totalStat * (multiplier - 1))
    int flatBonus;        // added after multiplier (integer; may be 0)
}
```

> **Rounding rule**: all float intermediate results are truncated (floor) to integer before being written to `StudentData`. Final applied stat is always ≥ 1.

**BattleBehaviorFlag** — cached in `_activeBehaviors` for AutoBattleResolver to query:
```csharp
struct BattleBehaviorFlag {
    string flagName;       // e.g. "AOEAttack", "TakesReducedDamage", "FirstHitDouble"
    bool appliesToTrait;   // true: only students WITH this trait receive the flag
}
```

### TraitDatabase ScriptableObject

`Assets/Config/TraitDatabase.asset` — all trait definitions:

| Trait | Threshold | Type | Effect |
|---|---|---|---|
| **Fire** | 2 | StatMultiplier | Fire students: Attack × 1.30 |
| **Fire** | 4 | StatMultiplier | Fire students: Attack × 1.70 (replaces 2-piece) |
| **Healer** | 2 | StatMultiplier | All students: MaxHP × 1.20 |
| **Healer** | 4 | StatMultiplier | All students: MaxHP × 1.50 (replaces 2-piece) |
| **Shield** | 2 | StatMultiplier | Shield students: MaxHP × 1.30 |
| **Shield** | 4 | StatMultiplier + BehaviorFlag | Shield students: MaxHP × 1.70 + `TakesReducedDamage` flag (20% damage reduction in AutoBattleResolver) |
| **Arcane** | 2 | StatMultiplier | All students: Speed +2 (flat) |
| **Arcane** | 4 | StatMultiplier | All students: Speed +5 (flat, replaces 2-piece) |
| **Storm** | 2 | BehaviorFlag | Storm students: `AOEAttack` flag (attacks deal 50% damage to all enemies) |
| **Storm** | 3 | BehaviorFlag | Storm students: `AOEAttack` flag (attacks deal 100% damage to all enemies, replaces 2-piece) |
| **Shadow** | 2 | BehaviorFlag | Shadow students: `FirstHitDouble` flag (first attack deals ×2 damage) |
| **Shadow** | 3 | BehaviorFlag | Shadow students: `FirstHitDouble` + `ShadowSurge` flag (Speed ×1.5 for the first battle round) |

> **Threshold replacement rule**: activating a higher tier of the same trait fully replaces the lower tier — effects do not stack additively.

> **Note**: all numeric values above are starting defaults for tuning. Final balance will be determined during the tune-pass.

### Core Rules

1. TraitSystem subscribes to `StudentRoster.OnRosterChanged` and immediately recounts all trait totals.
2. `_traitCounts` (Dictionary<TraitType, int>) is always kept synchronized with the current roster. It is rebuilt from scratch on every `OnRosterChanged` — no incremental updates.
3. `OnTraitThresholdReached(TraitType, int tier)` fires when a trait count crosses a threshold boundary (upward only). It does **not** fire on roster init — only on count increase.
4. `OnTraitThresholdLost(TraitType, int tier)` fires when a count drops below a threshold (downward). (Useful for future mechanic where removing a student affects the trait count — e.g., injuries.)
5. `ResolveBattleBuffs()` is called by RunManager before `AutoBattleResolver.Resolve()`. It iterates all active trait thresholds, applies StatMultiplierEffects to `StudentData.BonusAttack/MaxHP/Speed`, and builds `_activeBehaviors`.
6. Stat multipliers applied by `ResolveBattleBuffs()` are **additive to bonus stats, not base stats** — they are computed as: `bonusDelta = (int)(student.TotalStat * (multiplier - 1.0f))`, then `bonusStat += bonusDelta`. This preserves base stats for display.
7. `ResolveBattleBuffs()` must be idempotent within a single battle: calling it twice on the same roster before a battle must not double-apply. A `_battleBuffsApplied` bool guards this.
8. After a battle ends, `ResetBattleBuffs()` reverses all StatMultiplierEffects applied in step 6, restoring bonus stats to their pre-battle values. `_battleBuffsApplied` is reset to false.
9. `GetActiveBattleBehaviors()` returns the cached `_activeBehaviors` set. It must only be called after `ResolveBattleBuffs()`.
10. No student can have two copies of the same trait (enforced by StudentRoster at generation). Trait counts reflect unique students, not trait instances.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Counting` | Default during Train/Recruit phases | `ResolveBattleBuffs()` called | Trait counts update on every `OnRosterChanged` |
| `BuffsApplied` | `ResolveBattleBuffs()` returns | `ResetBattleBuffs()` called | Stat buffs are applied; `GetActiveBattleBehaviors()` returns valid data |
| `Reset` | `ResetBattleBuffs()` returns | `ResolveBattleBuffs()` called again | Stat buffs removed; back to pre-battle bonus values |

### Public API

```
// Subscribed internally to StudentRoster.OnRosterChanged
void OnRosterChanged()   // (private callback — recounts all traits)

// Returns current team trait count for each type — for SchoolHUD display
Dictionary<TraitType, int> GetTeamTraitCounts()

// Returns which threshold tier is active for a given trait (0 = none, 1 = lowest, 2 = highest)
int GetActiveThresholdTier(TraitType trait)

// Called by RunManager before AutoBattleResolver.Resolve()
// Applies stat multipliers and populates _activeBehaviors
void ResolveBattleBuffs()

// Called by RunManager after battle ends — reverses stat multipliers
void ResetBattleBuffs()

// Called by AutoBattleResolver during simulation setup
// Returns flags active for a given student
List<BattleBehaviorFlag> GetActiveBattleBehaviors(string studentId)
```

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `StudentRoster` | Subscribes to `OnRosterChanged`; reads `GetAll()` to recount traits; writes bonus stats via `NotifyStatChanged()` during `ResolveBattleBuffs()` |
| `RunManager` | Calls `ResolveBattleBuffs()` before battle; calls `ResetBattleBuffs()` after battle |
| `AutoBattleResolver` | Reads `GetActiveBattleBehaviors(studentId)` during simulation setup; reads stat values (already modified by `ResolveBattleBuffs()`) from StudentData |
| `SchoolHUD` | Subscribes to `OnTraitThresholdReached` and `OnTraitThresholdLost`; reads `GetTeamTraitCounts()` and `GetActiveThresholdTier()` to render trait progress UI |

---

## Formulas

### Stat Multiplier Application

```
bonusDelta = floor(student.TotalStat(stat) * (multiplier - 1.0))
student.BonusStat(stat) += bonusDelta
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `student.TotalStat(stat)` | int | 1–∞ | StudentRoster | Total stat before buff (Base + prior Bonus) |
| `multiplier` | float | 1.0–3.0 | `TraitDatabase` | Trait ability multiplier |
| `bonusDelta` | int | 0–∞ | calculated | Integer bonus added to BonusStat |

**Expected output range**: A Fire student with TotalAttack = 10 and Fire-2 active (×1.3): delta = floor(10 × 0.3) = 3 → TotalAttack becomes 13.

**Reversal on ResetBattleBuffs**: the same `bonusDelta` is subtracted. `_appliedDeltas` tracks these values per student per stat.

### Trait Count

```
traitCount[trait] = count of students in GetAll() where student.Traits.Contains(trait)
```

**Expected output**: 0 ≤ count ≤ `RecruitCountPerSemester` (default 5).

### Active Threshold Tier

```
activeThresholdTier = highest tier T such that thresholds[T].RequiredCount ≤ traitCount[trait]
(0 if no threshold is met)
```

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `ResolveBattleBuffs()` called twice before a battle | Second call is no-op (`_battleBuffsApplied` guard) | Prevents double-stacking multipliers |
| `ResetBattleBuffs()` called with `_battleBuffsApplied = false` | No-op | Idempotent reset |
| `GetActiveBattleBehaviors()` called before `ResolveBattleBuffs()` | Log error; return empty list | Behavior flags are undefined before resolution |
| Trait count drops below a threshold mid-run (future mechanic) | `OnTraitThresholdLost` fires; buffs will be lower in next battle | Only affects next `ResolveBattleBuffs()` call |
| Roster has 0 students with a trait | `traitCount[trait] = 0`; no threshold active | No effect |
| All students removed (after YearEnd, before next Recruit) | All trait counts = 0; no thresholds active | Clean state for next semester |
| Float precision: `floor(10 × 0.3)` = `floor(2.9999...)` = 2, not 3 | Use `Mathf.FloorToInt(totalStat * (multiplier - 1f))` with Unity's float; acceptable rounding | Integer floor is the rule; tune values to compensate |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `StudentRoster` | This depends on it | Data dependency — reads student list and trait assignments; writes bonus stat deltas |
| `RunManager` | It depends on this | Rule dependency — calls `ResolveBattleBuffs()` and `ResetBattleBuffs()` |
| `AutoBattleResolver` | It depends on this | Data dependency — reads behavior flags and modified stats |
| `SchoolHUD` | It depends on this | State trigger — subscribes to threshold events; reads trait counts for display |

---

## Tuning Knobs

All values in `TraitDatabase.asset` (ScriptableObject):

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| Fire 2-piece multiplier | 1.30 | 1.1–2.0 | Easier to build offensive teams | Less payoff for trait investment |
| Fire 4-piece multiplier | 1.70 | 1.4–3.0 | Strong late-game offense | Weaker 4-piece incentive |
| Healer 2-piece multiplier | 1.20 | 1.05–1.5 | More team sustain | Less survivability from Healer |
| Shield TakesReducedDamage | 20% | 10–40% | Shield build becomes much tankier | Weaker defensive incentive |
| Storm AOEAttack % (2-piece) | 50% | 25–100% | AoE is more punishing to enemies | AoE is less worth building |
| Storm AOEAttack % (3-piece) | 100% | 50–150% | Full AoE damage at high threshold | Weaker Storm 3-piece |
| Shadow FirstHitDouble | ×2.0 | ×1.5–×4.0 | Massive burst; one-shots possible | Less first-hit advantage |
| Arcane Speed flat bonus (2-piece) | +2 | +1–+5 | More actions per battle; fast teams dominant | Speed synergy less impactful |

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Trait count reaches a threshold (School phase) | Trait icon in SchoolHUD pulses; threshold badge lights up | "Synergy unlocked" chime | MVP |
| Trait count drops below a threshold | Threshold badge dims | Soft "synergy lost" sound | MVP |
| `ResolveBattleBuffs()` completes (Battle phase start) | Brief "synergy active" flash on each buffed student in BattleHUD | Battle intro sting includes trait activation sounds | MVP |
| Shadow `FirstHitDouble` triggers | First hit animation is larger/flashier | Distinct impact SFX on first hit | Alpha |

---

## Game Feel

### Feel Reference

> "Should feel like TFT's synergy panel — the player can glance at the trait bar and immediately know how far they are from the next threshold, and the 'pop' of hitting a new tier is always satisfying. NOT a system where the synergies are invisible or only legible in a tooltip."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Roster changes → trait counts update | 1 frame (synchronous recount) | 1 frame |
| Threshold crossed → `OnTraitThresholdReached` fires + SchoolHUD updates | 1 frame | 1 frame |

### Animation Feel Targets

| Animation | Startup Frames | Active Frames | Recovery Frames | Feel Goal |
|---|---|---|---|---|
| Trait threshold pop | 0 | 30 (glow + scale) | 0 | "Power up" moment |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| New threshold activated | 0.5s | Badge lights up; trait icon animates; chime plays |
| All trait buffs applied at battle start | 1.5s (staggered per student) | Each buffed student flashes their trait color |

### Weight and Responsiveness

- **Weight**: Trait synergies feel like a reward for planning, not a random bonus
- **Player control**: Fully player-controlled — traits are assigned at generation, but the player chooses which students to train and how to compose the team
- **Snap quality**: Threshold states are binary per tier — either active or not
- **Failure texture**: Being 1 student short of a threshold is legible and motivating ("so close!")

### Feel Acceptance Criteria

- [ ] A new player can read the current trait synergy progress without opening a help screen
- [ ] The "threshold activated" animation is noticed without looking for it
- [ ] The stat difference between a team with 0 thresholds and a team with 2 active thresholds is perceptible in battle

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Per-trait count (e.g. "Fire: 2") | SchoolHUD trait panel | On `OnRosterChanged` | During Train/Recruit phases |
| Active threshold tier indicator | Trait icon badge (0/1/2 stars) | On `OnTraitThresholdReached` / `Lost` | During run |
| Next threshold requirement | Trait panel tooltip (e.g. "+2 for Fire 4") | Static | On hover |
| Active behavior flags per student | BattleHUD student card (icon overlay) | Once at battle start | During Battle phase |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| RunManager calls ResolveBattleBuffs before AutoBattleResolver | `RunManager.md`, `AutoBattleResolver.md` | Ordering invariant | Rule dependency |
| AutoBattleResolver reads behavior flags | `AutoBattleResolver.md` | `GetActiveBattleBehaviors()` | Data dependency |
| Shield TakesReducedDamage handled in AutoBattleResolver | `AutoBattleResolver.md` | Damage calculation | Data dependency |
| StudentData bonus stat write pattern | `StudentRoster.md` | `NotifyStatChanged()` | Data dependency |

---

## Acceptance Criteria

- [ ] `GetTeamTraitCounts()` returns correct counts for every trait after every roster change
- [ ] `OnTraitThresholdReached(Fire, 1)` fires when the second Fire student joins the roster
- [ ] `OnTraitThresholdReached` does NOT fire on initial roster population — only on count increases
- [ ] `ResolveBattleBuffs()` applies correct stat deltas for all active thresholds (verified by unit test)
- [ ] `ResolveBattleBuffs()` is idempotent — calling it twice produces the same result as calling it once
- [ ] `ResetBattleBuffs()` restores all bonus stats to their pre-`ResolveBattleBuffs()` values exactly
- [ ] `GetActiveBattleBehaviors(studentId)` returns only the flags relevant to that student's traits and the active thresholds
- [ ] Fire 2 threshold is active when team has exactly 2 Fire students; inactive at 1; Fire 4 replaces Fire 2 at 4
- [ ] All trait definitions are loaded from `TraitDatabase.asset` — no hardcoded trait data in code
- [ ] Unit test: Fire 2 active, student TotalAttack = 10 → `ResolveBattleBuffs()` → BonusAttack increases by 3 (floor(10 × 0.3))

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Are 6 traits the right number for v1, or should we start with 4 (Fire, Healer, Shield, Arcane) and add Storm/Shadow later? | Designer | Before code-system | Pending — 4 traits is safer for v1 balance |
| Should the trait database allow mid-run dynamic traits (e.g., a teacher unlocks a 7th trait type)? | Designer | Before code-system | Pending — recommend no for v1 |
| How should traits be displayed on student cards — text label, colored icon, or both? | Designer/Artist | Before code-system | Pending |
| Should `ResetBattleBuffs()` be called automatically by RunManager, or does AutoBattleResolver call it when the battle ends? | Engineer | Before code-system | Pending — recommend RunManager calls it (only RunManager may advance phases) |
| Can two different thresholds of the same trait stack (Fire 2 AND Fire 4 both active simultaneously)? | Designer | Before code-system | Resolution: NO — higher tier fully replaces lower tier (documented in Core Rules) |
