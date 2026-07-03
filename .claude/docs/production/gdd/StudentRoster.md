# StudentRoster

> **Status**: Draft
> **Last Updated**: 2026-07-03
> **Implements Pillar**: Strategic — the randomly drawn student roster each semester is the primary source of roguelite variance; players adapt their training strategy to whoever shows up

## Summary

StudentRoster manages the active list of students for the current semester. It generates a random set of `StudentData` objects at the Recruit phase, provides read/write access to student stats (via TrainingSystem), and clears the entire roster at year end after PromotionSystem has moved promoted students to TeacherRoster. It fires `OnRosterChanged` and `OnStudentStatChanged` so HUDs and TraitSystem can react.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `RunManager`

---

## Overview

StudentRoster is a MonoBehaviour in the School scene, alive for the duration of one run. When RunManager fires `OnPhaseChanged(Recruit)`, StudentRoster calls `GenerateStudents()` to populate the active roster with N randomly generated `StudentData` objects, each assigned random base stats and 1–2 traits from the available trait pool (sourced from a `StudentConfig` ScriptableObject). TrainingSystem writes stat bonuses directly into `StudentData`. PromotionSystem calls `RemoveStudent(id)` for each student it processes. At run end, `Clear()` removes any remaining students. StudentRoster never persists beyond the current run.

The full roster is the **selection pool**, not the fielded squad — not every recruited student fights every battle. `GetStudentsForBattle()` converts the entire pool to `StudentCombatData` for `RunManager` to hand to `AutoBattleResolver.SetCombatants()`; the player then chooses up to `MaxSquadSize` of them to actually place on the board during `BattleBoardManager`'s Placement Phase (see `BattleBoardManager.md`, `RunManager.md`). StudentRoster itself has no concept of "fielded" vs "benched" — that distinction lives entirely in the Placement Phase hand-off.

## Player Fantasy

The player looks at each new class of students and immediately starts theorycrafting: "I have two Fire traits and one Healer — if I train the Fire students hard I can hit the 2-trait threshold." The randomness is the puzzle; the player's training decisions are the solution.

---

## Detailed Design

### StudentData Structure

```csharp
[Serializable]
public class StudentData {
    public string StudentId;        // GUID assigned at generation
    public string Name;             // drawn from name pool
    public List<TraitType> Traits;  // 1–2 traits assigned at generation

    // Base stats — randomly generated at recruitment
    public int BaseHP;    // health points
    public int BaseATK;   // physical attack damage
    public int BaseDEF;   // armor (physical damage reduction)
    public int BaseMG;    // magic power (used by trait magic abilities)
    public int BaseMR;    // magic resistance
    public int BaseSPD;   // speed (action frequency in battle)
    public int BaseCRIT;  // critical strike chance (integer %, e.g. 5 = 5%)

    // Bonus stats — accumulated from TrainingSystem (all start at 0)
    public int BonusHP;
    public int BonusATK;
    public int BonusDEF;
    public int BonusMG;
    public int BonusMR;
    public int BonusSPD;
    public int BonusCRIT;

    public int TrainingPointsSpent; // total training actions used on this student
}

// Derived (computed in C# properties, not stored):
// TotalHP   = BaseHP   + BonusHP
// TotalATK  = BaseATK  + BonusATK
// TotalDEF  = BaseDEF  + BonusDEF
// TotalMG   = BaseMG   + BonusMG
// TotalMR   = BaseMR   + BonusMR
// TotalSPD  = BaseSPD  + BonusSPD
// TotalCRIT = BaseCRIT + BonusCRIT   (clamped to 0–100)
```

All stat values are **integers** — no floats. This eliminates floating-point drift in formulas.

### StudentConfig ScriptableObject

`Assets/Config/StudentConfig.asset` — all tunable generation parameters:

| Field | Type | Default | Description |
|---|---|---|---|
| `RecruitCountPerSemester` | int | 5 | How many students are generated each Recruit phase |
| `MaxSquadSize` | int | 3 | Max students the player may field in a single battle (of the `RecruitCountPerSemester` pool) — enforced by `BattleBoardManager` during Placement Phase, not by StudentRoster itself |
| `BaseHPRange` | Vector2Int | (40, 80) | Min/max base HP on generation |
| `BaseATKRange` | Vector2Int | (5, 15) | Min/max base physical attack on generation |
| `BaseDEFRange` | Vector2Int | (0, 10) | Min/max base armor on generation |
| `BaseMGRange` | Vector2Int | (0, 10) | Min/max base magic power on generation |
| `BaseMRRange` | Vector2Int | (0, 10) | Min/max base magic resistance on generation |
| `BaseSPDRange` | Vector2Int | (3, 8) | Min/max base speed on generation |
| `BaseCRITRange` | Vector2Int | (0, 10) | Min/max base crit chance (%) on generation |
| `TraitsPerStudent` | Vector2Int | (1, 2) | Min/max traits assigned per student |
| `AvailableTraits` | List\<TraitType\> | [all traits] | Which traits can appear in the random pool |
| `NamePool` | List\<string\> | [50+ names] | Student name list for random draw |

### Core Rules

1. `GenerateStudents()` is called exactly once per Recruit phase, triggered by `OnPhaseChanged(Recruit)`.
2. Each generated student receives a unique GUID `StudentId`.
3. Base stats are drawn uniformly at random within their configured ranges (`StudentConfig`).
4. Traits are drawn without replacement within a single student: one student cannot hold two copies of the same trait. Across students, duplicates are allowed.
5. Names are drawn with replacement from the name pool (duplicates possible in a run — acceptable).
6. TrainingSystem is the only system permitted to write `BonusHP`, `BonusATK`, `BonusDEF`, `BonusMG`, `BonusMR`, `BonusSPD`, `BonusCRIT`, and `TrainingPointsSpent` on a `StudentData`.
7. `OnRosterChanged` fires after any add or remove operation.
8. `OnStudentStatChanged(StudentData)` fires after any stat modification (called by TrainingSystem after it writes).
9. `Clear()` removes all remaining students without firing individual remove events — fires one `OnRosterChanged` at the end.
10. StudentRoster never persists beyond the run — it is not saved to disk.

### States and Transitions

| State | Entry Condition | Exit Condition | Roster Contents |
|---|---|---|---|
| `Empty` | Run start, or after `Clear()` | `GenerateStudents()` called | 0 students |
| `Populated` | `GenerateStudents()` completes | `Clear()` called | N students (N = `RecruitCountPerSemester`) |
| `PartiallyCleared` | PromotionSystem removes students mid-YearEnd | `Clear()` called | 1 to N-1 students (promotions in progress) |

### Public API

```
// Called by RunManager on OnPhaseChanged(Recruit)
void GenerateStudents()

// Read access for TraitSystem, AutoBattleResolver, HUDs
List<StudentData> GetAll()
StudentData GetById(string studentId)

// Called by TrainingSystem after modifying a student's bonus stats
void NotifyStatChanged(StudentData student)   // triggers OnStudentStatChanged

// Called by PromotionSystem when a student is promoted or discarded
void RemoveStudent(string studentId)

// Called by RunManager at run end (after PromotionSystem completes)
void Clear()

// Called by RunManager when entering the Battle phase — converts the full
// roster (the selection pool) to combat-ready data. NOT the fielded squad;
// see MaxSquadSize / BattleBoardManager Placement Phase.
List<StudentCombatData> GetStudentsForBattle()
```

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `RunManager` | Subscribes to `OnPhaseChanged(Recruit)` to trigger `GenerateStudents()`; calls `Clear()` at run end |
| `TrainingSystem` | Reads `GetAll()` / `GetById()`; writes `BonusAttack`, `BonusMaxHP`, `BonusSpeed`, `TrainingPointsSpent`; calls `NotifyStatChanged()` |
| `TraitSystem` | Subscribes to `OnRosterChanged` to recount team trait totals |
| `AutoBattleResolver` | Reads `GetAll()` during battle simulation; never writes |
| `PromotionSystem` | Calls `RemoveStudent(id)` for each promoted/discarded student |
| `SchoolHUD` | Subscribes to `OnRosterChanged` and `OnStudentStatChanged` to refresh the student list UI |

---

## Formulas

### Stat Generation

```
baseStat = Random.Range(statRange.x, statRange.y + 1)   // inclusive on both ends
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `statRange.x` | int | ≥ 0 | `StudentConfig` | Minimum base stat value (DEF/MG/MR/CRIT may start at 0) |
| `statRange.y` | int | ≥ `statRange.x` | `StudentConfig` | Maximum base stat value |

**Expected output with defaults**: `BaseHP` ∈ [40, 80], `BaseATK` ∈ [5, 15], `BaseDEF` ∈ [0, 10], `BaseMG` ∈ [0, 10], `BaseMR` ∈ [0, 10], `BaseSPD` ∈ [3, 8], `BaseCRIT` ∈ [0, 10].

### Trait Count

```
traitCount = Random.Range(TraitsPerStudent.x, TraitsPerStudent.y + 1)
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `TraitsPerStudent.x` | int | 1 | `StudentConfig` | Minimum traits per student |
| `TraitsPerStudent.y` | int | 2 | `StudentConfig` | Maximum traits per student |

**Expected output**: each student has 1 or 2 traits (default).

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `GenerateStudents()` called while roster is non-empty | Log error, no-op | Should only be called once per Recruit phase; double-call would duplicate students |
| `GetById()` called with an unknown ID | Return null, log warning | Caller must guard against null |
| `RemoveStudent()` called with an unknown ID | Log warning, no-op | PromotionSystem should not call with stale IDs |
| `StudentConfig.NamePool` is empty | Log error; use a fallback name ("Student {N}") | Non-fatal; must not crash generation |
| `AvailableTraits` list has fewer traits than `TraitsPerStudent.y` | Draw as many as available without replacement (clamp) | Avoids infinite loop; log warning |
| `Clear()` called on an already-empty roster | No-op (no event fired, no error) | Idempotent |
| Roster size after all promotions > 0 at `Clear()` | Remaining students silently cleared; `OnRosterChanged` fires once | Discarded students are intentionally not promoted |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `RunManager` | This depends on it | State trigger — subscribes to `OnPhaseChanged(Recruit)`; `Clear()` called on run end |
| `TrainingSystem` | It depends on this | Data dependency — reads and writes student stats |
| `TraitSystem` | It depends on this | State trigger — subscribes to `OnRosterChanged` |
| `AutoBattleResolver` | It depends on this | Data dependency — reads student stats during battle |
| `PromotionSystem` | It depends on this | Ownership handoff — calls `RemoveStudent()` |
| `SchoolHUD` | It depends on this | State trigger — subscribes to `OnRosterChanged`, `OnStudentStatChanged` |

---

## Tuning Knobs

All knobs are on `StudentConfig.asset` (ScriptableObject) — no hardcoded values.

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `RecruitCountPerSemester` | 5 | 3–8 | Larger team; more trait synergy options; harder to fully train all | Smaller team; easier to train deeply; fewer trait combos |
| `MaxSquadSize` | 3 | 2–5 | More trait breakpoints reachable at once; less benching tension | Sharper squad-selection decisions; more recruits sit out each battle |
| `BaseHPRange` | (40, 80) | (10, 200) | Students survive longer; battles last more ticks | Squishier; high ATK one-shots more often |
| `BaseATKRange` | (5, 15) | (1, 50) | Higher physical damage output | Lower physical damage; battles last longer |
| `BaseDEFRange` | (0, 10) | (0, 50) | Students tank more physical hits at base | Students are fragile before DEF training |
| `BaseMGRange` | (0, 10) | (0, 50) | Magic trait abilities hit harder at base | Magic builds need more training investment |
| `BaseMRRange` | (0, 10) | (0, 50) | Students resist magic damage at base | Magic enemies (with MG flags) deal more damage |
| `BaseSPDRange` | (3, 8) | (1, 20) | More actions per battle round | Fewer actions; battles are slower-paced |
| `BaseCRITRange` | (0, 10) | (0, 30) | Higher starting crit chance; more variance in battle | Predictable damage with less burst |
| `TraitsPerStudent` | (1, 2) | (1, 3) | More synergy potential; more complex builds | Simpler builds; fewer combos |

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| `GenerateStudents()` completes | SchoolHUD animates new student cards sliding in | "New students arrive" chime | MVP |
| `OnStudentStatChanged` | Stat number on student card updates (flash highlight) | Subtle stat-up tick | MVP |
| `RemoveStudent()` (promotion) | Student card flies toward the teacher section | Promotion fanfare (from YearEndHUD) | MVP |
| `RemoveStudent()` (discard) | Student card fades/shrinks away | Soft dismiss sound | MVP |

---

## Game Feel

### Feel Reference

> "Should feel like Teamfight Tactics' bench — each unit card is a legible identity (name + traits + stats) that the player can scan in 2 seconds. NOT a wall of numbers with no visual hierarchy."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Recruit phase → students appear | SceneLoader fade + `GenerateStudents()` | ≤ 1 frame after scene load |

### Animation Feel Targets

| Animation | Startup Frames | Active Frames | Recovery Frames | Feel Goal |
|---|---|---|---|---|
| Student card slide-in | 0 | 20 (staggered, 4 per card) | 0 | Satisfying "class arriving" cadence |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| A student is discarded at year end | 0.5s | Card shrinks and fades; feels like a small loss |
| A student is promoted | 0.5s | Card flies to teacher area; feels rewarding |

### Weight and Responsiveness

- **Weight**: Student generation is deliberate (not instant) — the arriving cards create anticipation
- **Player control**: Player cannot choose which students arrive — the draw is forced
- **Snap quality**: Stats are immediately legible; traits are displayed as icons
- **Failure texture**: A bad draw (weak stats, mismatched traits) is immediately readable — the player knows they have a hard semester ahead

### Feel Acceptance Criteria

- [ ] A player can read all students' names, traits, and stats within 5 seconds of the Recruit phase starting
- [ ] Stat changes from training are visually reflected within 1 frame of `OnStudentStatChanged`
- [ ] Student removal animations complete before the next phase begins

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Student name | Student card in SchoolHUD | Static (set on generation) | Always visible during run |
| Student traits (icons) | Student card | Static | Always visible during run |
| Base stats | Student card | Static | Always visible |
| Bonus stats (from training) | Student card (as "+N" overlay) | On `OnStudentStatChanged` | During Train phase |
| Total stats | Student card | On `OnStudentStatChanged` | Always visible |
| TrainingPointsSpent | Student card progress indicator | On `OnStudentStatChanged` | During Train phase |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| TrainingSystem writes BonusStats | `TrainingSystem.md` | Stat delta application | Data dependency |
| TraitSystem reads roster to count traits | `TraitSystem.md` | `OnRosterChanged` subscription | State trigger |
| PromotionSystem calls RemoveStudent | `PromotionSystem.md` | `RemoveStudent(id)` | Ownership handoff |
| StudentConfig ScriptableObject | `EnemyDatabase.md` | `RunConfig` pattern (same SO convention) | Rule dependency |

---

## Acceptance Criteria

- [ ] `GenerateStudents()` produces exactly `RecruitCountPerSemester` students with non-null, non-empty names and traits
- [ ] No student has duplicate traits (e.g. [Fire, Fire] is invalid)
- [ ] All 7 base stats (HP, ATK, DEF, MG, MR, SPD, CRIT) are within their configured ranges (`StudentConfig`)
- [ ] `TotalCRIT` is always clamped to 0–100 regardless of bonuses
- [ ] `OnRosterChanged` fires exactly once per `GenerateStudents()` call
- [ ] `OnStudentStatChanged` fires exactly once per stat modification
- [ ] `Clear()` leaves `GetAll()` returning an empty list
- [ ] After a full YearEnd, `GetAll()` returns empty before the next Recruit phase
- [ ] No `StudentData` object is saved to disk (`SaveSystem` never receives student data)
- [ ] All stat values are integers — no float fields on `StudentData`
- [ ] Performance: `GenerateStudents(5)` completes within 1ms

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| How many students are on the roster vs. how many fight in battle? (Are all recruited students fielded, or is there a bench?) | Designer | Before code-system | **Resolved (2026-07-03)** — real bench: up to `MaxSquadSize` (default 3) of the `RecruitCountPerSemester` (default 5) roster fight each battle; the rest sit out. Selected via `BattleBoardManager`'s Placement Phase. |
| Should the player see the incoming students before confirming the Recruit phase, or are they revealed all at once? | Designer | Before code-system | Pending |
| Should the name pool be hard-coded in `StudentConfig`, or loaded from a separate text file for easier editing? | Engineer | Before code-system | Pending — recommend hard-coded list for v1 |
| Can a student have 0 traits if `TraitsPerStudent.x = 0`? (Recommend: minimum 1 always.) | Designer | Before code-system | Pending — recommend minimum 1 trait always |
