# EnemyDatabase

> **Status**: Draft
> **Last Updated**: 2026-06-29
> **Implements Pillar**: Challenging — three years of escalating threat; the player cannot survive Year 3 without a well-built, well-trained team

## Summary

EnemyDatabase is a ScriptableObject that stores all enemy definitions for the game. It provides the enemy squad for each year of the run via `GetEnemiesForYear(year)`. AutoBattleResolver reads from it at the start of each battle. Enemies are defined explicitly per year (no procedural generation in v1) — this gives designers direct control over the difficulty curve. Enemy data is read-only at runtime; the ScriptableObject is never modified during play.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `AutoBattleResolver`

---

## Overview

EnemyDatabase (`Assets/Config/EnemyDatabase.asset`) contains three fixed enemy squads — one per year. Each squad is a list of `EnemyData` objects with hand-authored stats and optional `BattleBehaviorFlag` assignments. There is no procedural stat scaling in v1: Year 1, 2, and 3 squads are authored explicitly by a designer and tuned to match expected student power at each year of a well-played run. `GetEnemiesForYear(int year)` returns a deep copy of the year's enemy list so AutoBattleResolver can freely mutate HP values during simulation without corrupting the source data.

## Player Fantasy

Each year's battle feels categorically different — Year 1 teaches the player how combat works (slow enemies, low threat), Year 2 pressures them (faster enemies, more HP, a behavior flag or two), and Year 3 is the real test (enemies that hit back hard, surprising behaviors). The escalation is felt, not read.

---

## Detailed Design

### EnemyData Structure

```csharp
[Serializable]
public class EnemyData {
    public string EnemyId;                      // unique identifier (no GUID — use designer-authored string IDs for readability)
    public string DisplayName;                  // shown in BattleHUD (e.g. "Dungeon Golem")
    public int BaseAttack;                      // flat damage per hit
    public int BaseMaxHP;                       // total HP at battle start
    public int BaseSpeed;                       // action interval = max(1, BaseActionInterval - BaseSpeed)
    public List<BattleBehaviorFlag> Flags;      // optional; same enum as TraitSystem behavior flags
    public Sprite Icon;                         // optional art ref; null falls back to default enemy sprite
}
```

All stat values are **integers** — no floats. Consistent with the project-wide integer-stats rule.

### EnemyDatabase ScriptableObject

`Assets/Config/EnemyDatabase.asset`:

| Field | Type | Description |
|---|---|---|
| `Year1Squad` | `List<EnemyData>` | Enemy team for Year 1 battle |
| `Year2Squad` | `List<EnemyData>` | Enemy team for Year 2 battle |
| `Year3Squad` | `List<EnemyData>` | Enemy team for Year 3 battle |

### Public API

```
// Called by AutoBattleResolver at the start of Resolve()
// Returns a deep copy of the year's enemy squad (year: 1, 2, or 3)
List<EnemyData> GetEnemiesForYear(int year)
```

EnemyDatabase has no other public methods. It is a data store — no logic, no events.

### Deep Copy Requirement

`GetEnemiesForYear()` must return new `EnemyData` instances, not references into the ScriptableObject arrays. AutoBattleResolver mutates `CurrentHP` during simulation; this must not corrupt the source data, which must survive across runs and be re-usable for every new battle.

**Enforcement**: serialize a copy in the return path:
```csharp
List<EnemyData> GetEnemiesForYear(int year) {
    var source = year switch { 1 => Year1Squad, 2 => Year2Squad, 3 => Year3Squad, _ => null };
    if (source == null) { Debug.LogError(...); return new List<EnemyData>(); }
    return source.ConvertAll(e => new EnemyData {
        EnemyId = e.EnemyId, DisplayName = e.DisplayName,
        BaseAttack = e.BaseAttack, BaseMaxHP = e.BaseMaxHP, BaseSpeed = e.BaseSpeed,
        Flags = new List<BattleBehaviorFlag>(e.Flags), Icon = e.Icon
    });
}
```

### Default Enemy Squads (Design Starting Point)

> These are tuning-starting values. Adjust on `EnemyDatabase.asset` in the Unity Inspector; never hardcode in scripts.

#### Year 1 Squad — "Beginner Creatures" (3 enemies)

| EnemyId | DisplayName | Attack | MaxHP | Speed | Flags |
|---|---|---|---|---|---|
| `goblin_scout` | Goblin Scout | 5 | 25 | 3 | — |
| `forest_slime` | Forest Slime | 4 | 35 | 2 | — |
| `bark_golem` | Bark Golem | 6 | 30 | 2 | — |

*Design intent*: Year 1 enemies have lower stats than a lightly trained 5-student team. The player should win Year 1 comfortably even with poor training choices — this is the tutorial battle.

#### Year 2 Squad — "Threatening Creatures" (4 enemies)

| EnemyId | DisplayName | Attack | MaxHP | Speed | Flags |
|---|---|---|---|---|---|
| `iron_hound` | Iron Hound | 9 | 40 | 6 | — |
| `shadow_sprite` | Shadow Sprite | 7 | 30 | 8 | `FirstHitDouble` |
| `stone_troll` | Stone Troll | 8 | 55 | 3 | `TakesReducedDamage` |
| `fire_imp` | Fire Imp | 10 | 28 | 5 | — |

*Design intent*: Year 2 is a real pressure test. A player who spread training evenly will struggle. The Stone Troll's `TakesReducedDamage` introduces the first enemy with a defensive flag, teaching the player that some targets are tank-like. Shadow Sprite's `FirstHitDouble` punishes slow students in the action queue.

#### Year 3 Squad — "Boss Tier Creatures" (5 enemies)

| EnemyId | DisplayName | Attack | MaxHP | Speed | Flags |
|---|---|---|---|---|---|
| `arcane_guardian` | Arcane Guardian | 14 | 65 | 5 | `AOEAttack` |
| `void_wraith` | Void Wraith | 12 | 45 | 9 | `FirstHitDouble` |
| `iron_colossus` | Iron Colossus | 10 | 90 | 2 | `TakesReducedDamage` |
| `ember_witch` | Ember Witch | 13 | 40 | 7 | — |
| `plague_lurker` | Plague Lurker | 11 | 50 | 6 | — |

*Design intent*: Year 3 is the final boss encounter. Arcane Guardian's `AOEAttack` punishes HP-weak teams (all students take damage each round). Void Wraith's high Speed + `FirstHitDouble` can one-shot a low-HP student before they ever act. Only a team with strong trait synergies and focused training should reliably win Year 3.

### Core Rules

1. EnemyDatabase is **read-only at runtime** — never write to it during play.
2. `GetEnemiesForYear()` with an invalid year (not 1, 2, or 3) logs an error and returns an empty list (battle will immediately resolve as a student win; this edge case should never reach production).
3. Enemy stats use the same units as student stats — `BaseAttack` deals flat damage, `BaseMaxHP` is the full HP pool, `BaseSpeed` feeds directly into the action interval formula.
4. Enemies may hold any `BattleBehaviorFlag` that AutoBattleResolver supports — the flags are not exclusive to students.
5. The `Icon` field is optional for MVP — null icons display a fallback sprite in BattleHUD, not a crash.
6. `EnemyId` must be unique across all three squads (used for BattleHUD tracking). Duplicate IDs log a validation error in the Unity editor (via a custom `OnValidate()` check).

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `AutoBattleResolver` | Calls `GetEnemiesForYear(RunManager.CurrentYear)` at the start of `Resolve()` to build the enemy combatant list |
| `RunManager` | Does not call EnemyDatabase directly — provides `CurrentYear` which AutoBattleResolver uses when querying |
| `BattleHUD` | Reads enemy `DisplayName` and `Icon` from the BattleCombatant data (not directly from EnemyDatabase) |

---

## Formulas

EnemyDatabase contains no formulas — it is a data store. All combat formulas that use enemy stats live in `AutoBattleResolver`. Stat interaction with combat uses the same formulas as student stats (see `AutoBattleResolver.md`).

### Enemy Action Interval (defined in AutoBattleResolver)

```
EnemyActionInterval = max(1, BaseActionInterval - enemy.BaseSpeed)
```

| Variable | Type | Source | Description |
|---|---|---|---|
| `BaseActionInterval` | int | `BattleConfig` SO | Shared constant used for all combatants |
| `enemy.BaseSpeed` | int | `EnemyData.BaseSpeed` | Read from the deep-copied EnemyData |

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `GetEnemiesForYear(0)` or `GetEnemiesForYear(4)` | Log error, return empty list | Invalid year; battle resolves as an instant student win to prevent a stuck game state |
| A squad list is empty in the inspector | Log error at battle start; return empty list | Authoring error — should be caught in QA; not a runtime crash |
| Duplicate `EnemyId` values across squads | Log editor validation error (OnValidate) | Prevents BattleHUD tracking ambiguity; designer must fix before shipping |
| `Icon` is null | BattleHUD uses fallback sprite; no crash | Art is not required for MVP |
| AutoBattleResolver mutates a returned enemy's HP to 0 | Does not affect the ScriptableObject source | Deep copy guarantees isolation |
| RunManager subscribes to `OnYearChanged` from EnemyDatabase | Not applicable — EnemyDatabase fires no events; RunManager passes the year into AutoBattleResolver at battle start | EnemyDatabase is a passive data store |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `AutoBattleResolver` | It depends on this | Data dependency — reads enemy squad for the current year |
| `TraitSystem` / `BattleBehaviorFlag` | This depends on it | Data type dependency — `EnemyData.Flags` uses `BattleBehaviorFlag` enum defined in TraitSystem |

---

## Tuning Knobs

All tuning is done in the Unity Inspector on `EnemyDatabase.asset`. No code changes required. There are no additional ScriptableObject configs — the database IS the config.

| Parameter | Location | Effect of Increase | Effect of Decrease |
|---|---|---|---|
| Enemy `BaseAttack` (any year) | `EnemyDatabase.asset` → squad | More damage per hit; faster student defeats | Less pressure; battles drag |
| Enemy `BaseMaxHP` (any year) | `EnemyDatabase.asset` → squad | Battles last longer; more training required to win | Shorter battles; underpowered teams can win |
| Enemy `BaseSpeed` (any year) | `EnemyDatabase.asset` → squad | Enemy acts more often; punishes slow students more | Enemy acts less often; speed advantage reduced |
| Squad size (count per year) | Add/remove enemies in the asset | Outnumber students → harder | Fewer enemies → easier; may reduce battle tension |
| Enemy `Flags` (add/remove) | `EnemyDatabase.asset` → individual enemy | More combat variance; unpredictability increases | Simpler, more readable battles |

**Difficulty curve target**: a team of 5 students with 5 training actions each (25 total) should:
- Win Year 1 comfortably (< 5 students lost)
- Win Year 2 with effort (1–2 students lost)
- Win Year 3 only with strong trait synergies (0 margin for poor builds)

---

## Visual / Audio Requirements

EnemyDatabase itself has no visual or audio requirements — it is a data store. BattleHUD reads `DisplayName` and `Icon` from the `EnemyData` structs that AutoBattleResolver passes back. Audio triggers are in AutoBattleResolver and BattleHUD.

| Asset | Source | Used By |
|---|---|---|
| `EnemyData.Icon` | Sprite assigned in Inspector | BattleHUD enemy cards |
| Enemy name strings | `EnemyData.DisplayName` | BattleHUD enemy name labels |

---

## Game Feel

### Feel Reference

> "Each enemy should feel like a distinct character, not a stat block. Year 1 enemies are goofy and approachable; Year 3 enemies are intimidating. The name and icon carry most of this weight — the stats are the mechanical body, the art is the soul."

### Impact Moments

| Impact Type | Whose Responsibility |
|---|---|
| Enemy appears in BattleHUD at battle start | BattleHUD (reads EnemyData.Icon and DisplayName) |
| Enemy defeated visual/audio | BattleHUD + AudioSystem (triggered by AutoBattleResolver's OnCombatantDefeated) |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| BattleBehaviorFlag enum shared with TraitSystem | `TraitSystem.md` | `BattleBehaviorFlag` enum | Data type dependency |
| Action interval formula applies to enemies | `AutoBattleResolver.md` | Action interval formula | Data dependency |
| RunConfig.MaxYears defines year range (1–3) | `RunManager.md` | `MaxYears = 3` | Constraint dependency |

---

## Acceptance Criteria

- [ ] `GetEnemiesForYear(1)`, `GetEnemiesForYear(2)`, `GetEnemiesForYear(3)` each return a non-empty list with the correct squads
- [ ] Returned lists are deep copies — mutating `CurrentHP` on a returned `EnemyData` does not change the ScriptableObject's data
- [ ] `GetEnemiesForYear(0)` and `GetEnemiesForYear(4)` log an error and return an empty list (no exception thrown)
- [ ] No `EnemyData` object in the database has duplicate `EnemyId` values (verified by `OnValidate`)
- [ ] All enemy stat values are positive integers (no zeroes or negatives — validated by `OnValidate`)
- [ ] Unit test: `GetEnemiesForYear(2)` returns exactly 4 enemies with the correct DisplayNames per the designer-authored default

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should enemy squads vary per run (random selection from a pool), or are Year 1/2/3 fixed squads for every run? | Designer | Before code-system | Pending — recommend fixed squads in v1 (predictable, more tunable); random pool is a future feature |
| Should Year 3 have a "boss" enemy with unique stats separate from the regular squad, or is the whole squad the challenge? | Designer | Before code-system | Pending — recommend whole-squad challenge (no single boss) to match the team-vs-team theme |
| How many enemies per year? (Above defaults: 3, 4, 5) | Designer | Before code-system | Pending — this directly affects difficulty; start with 3/4/5 and tune |
| Should enemies scale with the run number (e.g. Run 2 enemies are harder than Run 1 enemies), or are enemies always the same? | Designer | Before code-system | Pending — recommend same per run for v1; meta-progression tuning is a Phase 3 concern |
| Should enemy `Icon` be required at ship (blocking) or optional (non-blocking for MVP)? | Art lead | Before code-system | Pending — recommend optional for MVP (fallback sprite); art pass in Phase 3 |
