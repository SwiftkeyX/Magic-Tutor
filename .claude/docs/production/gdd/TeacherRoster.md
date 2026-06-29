# TeacherRoster

> **Status**: Draft
> **Last Updated**: 2026-06-29
> **Implements Pillar**: Empowering — the player's past runs contribute directly to future power; every promoted teacher is a permanent upgrade

## Summary

TeacherRoster is the game's meta-progression store. It maintains the persistent list of teachers accumulated across all runs, provides training buff totals to TrainingSystem on demand, and persists the roster to disk via SaveSystem after every addition. It is a permanent singleton in the Bootstrap scene, loaded before every run begins. Teachers are never removed from the roster (no dismissal mechanic in v1) — the roster only grows.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `SaveSystem`

---

## Overview

TeacherRoster lives in Bootstrap scene and is therefore present for the entire application lifetime. In `Awake()`, after SaveSystem has initialized, TeacherRoster calls `SaveSystem.Load()` to restore the `Teachers` list from disk. PromotionSystem calls `AddTeacher(TeacherData)` at the end of each YearEnd phase; TeacherRoster immediately writes the updated roster to disk via `SaveSystem.Save()`. TrainingSystem calls `GetTrainingBuff(StatType)` once at the start of each training allocation to determine how much bonus to add. Because SaveSystem is synchronous, no coroutines or callbacks are needed.

## Player Fantasy

Before a run even starts, the player can see their accumulated teacher roster and feel the weight of past victories: "I've built up four Attack teachers — this next run's students will hit hard from the start." The power of good teachers is visible and felt in training, not hidden behind a tooltip.

---

## Detailed Design

### TeacherSaveData Structure

*(Used for JsonUtility serialization in SaveSystem. Mirrors TeacherData but is JsonUtility-safe.)*

```csharp
[Serializable]
public class TeacherSaveData {
    public string TeacherId;     // GUID
    public string Name;
    public string Specialty;     // TraitType enum value, stored as string
    public string TrainingFocus; // StatType enum value, stored as string
    public int TrainingBuff;     // pre-computed by PromotionSystem; always int
}
```

*String enum storage rationale*: `JsonUtility` does not serialize C# enums reliably when the underlying type is changed; string representation is explicit and forward-compatible.

### Runtime TeacherData

At runtime, TeacherRoster deserializes `List<TeacherSaveData>` from disk and stores it as `List<TeacherData>`. Conversion is one-to-one: the string enum fields are parsed back to their enum types in `Awake()`.

```csharp
// Conversion example (Awake path):
teacher.Specialty = Enum.Parse<TraitType>(saveData.Specialty);
teacher.TrainingFocus = Enum.Parse<StatType>(saveData.TrainingFocus);
```

If an enum parse fails (e.g., a future enum value removed), the teacher is logged as a warning and skipped — the roster continues loading without crashing.

### Singleton Contract

- TeacherRoster is a singleton MonoBehaviour on the `[Bootstrap]` GameObject in the `Bootstrap.unity` scene.
- It does **not** call `DontDestroyOnLoad()` — Bootstrap scene persistence is handled by never unloading Bootstrap (see `SceneLoader.md`).
- Initialization order: `GameManager.Awake()` → `SaveSystem.Awake()` → `TeacherRoster.Awake()` (enforced by Script Execution Order in Unity project settings).

### Core Rules

1. **TeacherRoster.Awake()** loads teacher data from SaveSystem. If no save exists, it starts with an empty roster (first run has no teachers — intended).
2. **`AddTeacher(TeacherData teacher)`** appends the teacher to the runtime list and immediately calls `SaveSystem.Save()`. The write is synchronous — no deferred save.
3. **`GetTrainingBuff(StatType stat)`** returns the sum of `TrainingBuff` for all teachers whose `TrainingFocus == stat`. Returns 0 if no matching teachers.
4. **Teachers are never removed** in v1. Once added, a teacher is permanent for the save file's lifetime.
5. **TeacherRoster never fires events** — it is a passive data store queried on demand. Systems that need roster data call its read API directly.
6. **MetaProgressionHUD reads from TeacherRoster** via `GetAll()` to display the accumulated roster between runs. Read-only.
7. **SaveSystem is the single source of truth** for teacher persistence. TeacherRoster's in-memory list must always reflect what is on disk; they are synchronized on every `AddTeacher()` call.

### Public API

```
// Called by PromotionSystem at the end of each YearEnd phase
void AddTeacher(TeacherData teacher)

// Called by TrainingSystem on each AllocateTraining() call
// Returns: total training buff from all teachers whose TrainingFocus matches 'stat'
int GetTrainingBuff(StatType stat)

// Called by MetaProgressionHUD (read-only, between runs and during runs)
IReadOnlyList<TeacherData> GetAll()

// Called by MetaProgressionHUD to show counts per stat focus
// Returns: number of teachers with TrainingFocus == stat
int GetTeacherCountByFocus(StatType stat)
```

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `SaveSystem` | TeacherRoster calls `SaveSystem.Load()` in `Awake()` and `SaveSystem.Save()` in `AddTeacher()` |
| `PromotionSystem` | Calls `AddTeacher(TeacherData)` for each promoted student |
| `TrainingSystem` | Calls `GetTrainingBuff(StatType)` on every `AllocateTraining()` call to compute gain |
| `MetaProgressionHUD` | Calls `GetAll()` and `GetTeacherCountByFocus()` to render the teacher roster between runs |
| `GameManager` | Reads `TotalRunsStarted` and `TotalRunsCompleted` from SaveData (not from TeacherRoster) — no direct TeacherRoster interaction |

---

## Formulas

### Total Training Buff for a Stat

```
GetTrainingBuff(stat) = sum(teacher.TrainingBuff for teacher in _teachers where teacher.TrainingFocus == stat)
```

| Variable | Type | Range | Source |
|---|---|---|---|
| `teacher.TrainingBuff` | int | 1–5 (per teacher) | Set by PromotionSystem formula at promotion |
| Result | int | 0–∞ | Grows with each run's promotions |

**Expected output examples**:
- No teachers → `GetTrainingBuff(ATK)` = 0
- 2 ATK teachers (Fire specialty) with buffs [2, 3] → `GetTrainingBuff(ATK)` = 5
- 3 CRIT teachers (Shadow specialty) with buffs [1, 3, 5] → `GetTrainingBuff(CRIT)` = 9 (applied as bonus % crit chance in TrainingSystem, clamped to 100)
- Mix: 4 DEF teachers (Shield) with buffs [1, 2, 3, 4] → `GetTrainingBuff(DEF)` = 10

There is no cap on total buff from all teachers. If this causes runaway power in late saves, a cap can be added in Phase 3 tuning (`beta-task`).

### TrainingGain in TrainingSystem (for reference)

```
gain = baseGain(statType) + TeacherRoster.GetTrainingBuff(statType)
```

*(Defined fully in `TrainingSystem.md`. Listed here as context for how TeacherRoster's output is consumed.)*

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| No save file exists (first run ever) | TeacherRoster initializes with empty list | First run has no meta-progression — intentional |
| Save file is corrupted | SaveSystem handles fallback to `.bak` — TeacherRoster loads whatever SaveSystem returns | Responsibility boundary |
| Enum parse fails on load (e.g., removed `TraitType`) | Log warning, skip that teacher, continue loading | Defensive load — roster is partially degraded rather than crashing |
| `GetTrainingBuff()` called with no teachers | Returns 0 | Training proceeds with base gain only |
| `AddTeacher()` called while save write fails | SaveSystem logs the error; TeacherRoster's in-memory list is already updated (teacher is not rolled back) | Accept save-write failures gracefully; in-memory list stays consistent with the teacher actually being "earned" |
| Multiple `AddTeacher()` calls in rapid succession (e.g. 5 promotions) | Each call writes to disk sequentially (synchronous); roster after 5 calls is correct | Synchronous saves are slower but correct |
| `GetAll()` called during `AddTeacher()` | Returns the snapshot before the addition (or after, depending on call order) — fine for read-only consumers | MetaProgressionHUD refreshes on scene load, not on every add |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `SaveSystem` | This depends on it | Data persistence — loads on Awake, saves on AddTeacher |
| `PromotionSystem` | It depends on this | Ownership handoff — calls AddTeacher to register promoted students |
| `TrainingSystem` | It depends on this | Data query — reads GetTrainingBuff per stat |
| `MetaProgressionHUD` | It depends on this | Data query — reads GetAll / GetTeacherCountByFocus for display |

---

## Tuning Knobs

No numeric tuning knobs live on TeacherRoster directly — the buff values per teacher are authored by PromotionSystem (via `PromotionConfig`). The only indirect tuning lever is the save file: clearing the save file resets the teacher roster to zero.

| Parameter | Owner | Effect |
|---|---|---|
| `MinTeacherBuff`, `MaxTeacherBuff`, `BuffPerPoint` | `PromotionConfig.asset` | Controls how much buff each teacher contributes |
| Promotion count per year | Player decision | Directly affects how fast TeacherRoster grows |

---

## Visual / Audio Requirements

TeacherRoster itself has no visual or audio requirements. MetaProgressionHUD renders its data.

| Information | Rendered By | Source API |
|---|---|---|
| Full teacher list | MetaProgressionHUD | `GetAll()` |
| Teacher count by stat | MetaProgressionHUD | `GetTeacherCountByFocus(stat)` |
| Teacher name, specialty, buff | MetaProgressionHUD teacher card | `TeacherData` fields |

---

## Game Feel

### Feel Reference

> "Between runs, the teacher roster should feel like a trophy case — each teacher is a named individual the player remembers from a past run. NOT a flat '+15 Attack bonus' number. The names matter."

### Impact Moments

| Impact Type | Who Creates It | Description |
|---|---|---|
| Teacher added (promotion) | YearEndHUD + PromotionSystem | Teacher card appears; name and buff displayed |
| Teacher roster viewed between runs | MetaProgressionHUD | Full named list with per-teacher buff; feels cumulative and earned |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| SaveData.Teachers persists the roster | `SaveSystem.md` | `TeacherSaveData` in `SaveData` struct | Data dependency |
| PromotionSystem authors TeacherBuff values | `PromotionSystem.md` | `BuffPerPoint`, `MinTeacherBuff`, `MaxTeacherBuff` | Data dependency |
| TrainingSystem consumes GetTrainingBuff | `TrainingSystem.md` | `gain = baseGain + GetTrainingBuff(stat)` | Data dependency |
| Bootstrap singleton pattern | `best-practices.md` | Bootstrap scene, no DontDestroyOnLoad | Architecture dependency |
| Script Execution Order: after SaveSystem | `architecture.md` | Initialization order table | Rule dependency |

---

## Acceptance Criteria

- [ ] TeacherRoster starts empty on a fresh save (no teachers) — first run has base training only
- [ ] After `AddTeacher(teacher)`, `GetAll()` includes the new teacher and `SaveSystem.Save()` has been called
- [ ] `GetTrainingBuff(StatType.Attack)` returns the sum of `TrainingBuff` for all Attack-focused teachers
- [ ] `GetTrainingBuff` for a stat with no matching teachers returns 0
- [ ] On `Awake()`, TeacherRoster loads the teacher list from SaveSystem and parses enum fields correctly
- [ ] A corrupted enum field on load logs a warning and skips that teacher (does not throw an exception)
- [ ] All `TeacherData.TrainingBuff` values are integers (no float fields in `TeacherSaveData`)
- [ ] Unit test: add 3 Attack teachers (buffs: 1, 2, 3) → `GetTrainingBuff(Attack)` = 6; `GetTeacherCountByFocus(Attack)` = 3

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should TeacherRoster have a maximum size (e.g., 20 teachers) to prevent an infinite-growing save file? | Designer | Before Phase 3 | Pending — no cap in v1; evaluate in beta if buffs become game-breaking |
| Should old teachers "retire" after N runs (periodic roster pruning)? | Designer | Before Phase 3 | Pending — adds complexity; recommend defer |
| Should the player be able to "dismiss" teachers from the roster? | Designer | Before Phase 3 | Pending — no dismiss in v1 (teachers are permanent) |
| Should there be diminishing returns on `GetTrainingBuff()` (e.g. buff curve, not sum)? | Designer | Before Phase 3 | Pending — linear sum for v1; curve if power creep is observed |
