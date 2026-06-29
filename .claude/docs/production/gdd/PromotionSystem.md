# PromotionSystem

> **Status**: Draft
> **Last Updated**: 2026-06-29
> **Implements Pillar**: Empowering — the player watches a student they trained grow into a teacher who will buff future classes; every promoted teacher is a permanent piece of their legacy

## Summary

PromotionSystem drives the end-of-year transition: it presents surviving students as promotion candidates, lets the player choose which ones to elevate to teachers, converts selected students into `TeacherData` objects and hands them to TeacherRoster, removes all students from StudentRoster, then fires `OnPromotionComplete` for RunManager to advance the run. Promotions are the game's primary meta-progression mechanism — teachers persist across runs, and their quality depends directly on how much the player trained the promoted students.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `StudentRoster, TeacherRoster`

---

## Overview

PromotionSystem is a MonoBehaviour in the YearEnd scene, active for the duration of the YearEnd phase. When RunManager loads the YearEnd scene and transitions to `RunPhase.YearEnd`, PromotionSystem queries `StudentRoster.GetAll()` to build the promotion candidate list. YearEndHUD presents these candidates. The player selects any subset to promote (no minimum, no maximum in v1). When the player confirms, PromotionSystem converts each selected student to a `TeacherData`, adds them to `TeacherRoster`, removes them from `StudentRoster` (all students are then cleared), and fires `OnPromotionComplete`. RunManager subscribes to advance the year or end the run.

## Player Fantasy

The player looks at their graduating class and feels the weight of the choice: "She has 4 training points in Attack — she'll make a great Attack teacher. But should I promote him too? His Speed training will help future teams." Every teacher kept is a gift to their future self.

---

## Detailed Design

### TeacherData Structure

*(Defined here as the conversion result; TeacherRoster.md owns the storage contract.)*

```csharp
[Serializable]
public class TeacherData {
    public string TeacherId;         // new GUID assigned at promotion
    public string Name;              // inherited from StudentData.Name
    public TraitType Specialty;      // derived from the student's first trait (Traits[0])
    public StatType TrainingFocus;   // which stat this teacher buffs (derived from specialty — see table below)
    public int TrainingBuff;         // flat bonus added to AllocateTraining() for the matching stat
}
```

**Specialty → TrainingFocus mapping:**

| Specialty (TraitType) | TrainingFocus (StatType) |
|---|---|
| `Fire` | `Attack` |
| `Healer` | `MaxHP` |
| `Shield` | `MaxHP` |
| `Arcane` | `Attack` |
| `Storm` | `Speed` |
| `Shadow` | `Speed` |

### TeacherData Conversion Formula

When a student is promoted, `TeacherData` is created from `StudentData`:

```
teacher.TeacherId = GUID.NewGuid().ToString()
teacher.Name = student.Name
teacher.Specialty = student.Traits[0]
teacher.TrainingFocus = TraitToStatMap[student.Traits[0]]
teacher.TrainingBuff = max(MinTeacherBuff, floor(student.TrainingPointsSpent × BuffPerPoint))
```

| Variable | Type | Default | Source | Description |
|---|---|---|---|---|
| `MinTeacherBuff` | int | 1 | `PromotionConfig` SO | Floor — even an untrained student gives a +1 buff as a teacher |
| `BuffPerPoint` | float | 0.5 | `PromotionConfig` SO | Multiplier: training points spent → teacher buff. 4 points spent = +2 buff |
| `MaxTeacherBuff` | int | 5 | `PromotionConfig` SO | Cap per teacher — prevents one teacher from dominating |

**Expected output**: a student with `TrainingPointsSpent = 0` → TeacherBuff = 1. A student with `TrainingPointsSpent = 4` → TeacherBuff = 3. A student with `TrainingPointsSpent = 10` → TeacherBuff = 5 (capped).

*Note: `BuffPerPoint` is a float used for the formula only — `TrainingBuff` on `TeacherData` is always stored as an integer (`floor`).*

### Core Rules

1. PromotionSystem is only active during `RunPhase.YearEnd`. Any call to its public API outside this phase is rejected with a logged error.
2. Promotion candidates are built from `StudentRoster.GetAll()` at the start of the YearEnd phase. The list does not update dynamically after that.
3. The player may promote **any number** of candidates, including zero (promoting none is valid — all students graduate without entering TeacherRoster).
4. The player may not promote the same student twice (UI prevents duplicate selection; PromotionSystem guards against it in code too).
5. When `ConfirmPromotions()` is called, PromotionSystem processes promotions in the order the player selected them (no reordering by stats or traits).
6. After promotions are processed, `StudentRoster.RemoveStudent(id)` is called for each promoted student, then `StudentRoster.Clear()` clears the remaining (non-promoted) students.
7. `OnPromotionComplete` fires exactly once per `ConfirmPromotions()` call.
8. Promoted students become part of TeacherRoster immediately — `TeacherRoster.AddTeacher()` is called synchronously before `OnPromotionComplete` fires.
9. **No student data is carried forward** beyond what is encoded in `TeacherData`. The `StudentData` objects are abandoned after this phase.
10. **TeacherRoster changes are persisted immediately after promotion** — `SaveSystem.Save()` is called by TeacherRoster on `AddTeacher()`, not by PromotionSystem. PromotionSystem does not touch SaveSystem directly.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Inactive` | Outside YearEnd phase | `OnPhaseChanged(YearEnd)` | Rejects all API calls |
| `SelectionOpen` | `OnPhaseChanged(YearEnd)` | `ConfirmPromotions()` called | Accepts selection/deselection calls; shows promotion UI |
| `Processing` | `ConfirmPromotions()` called | `OnPromotionComplete` fires | Converts students; writes to TeacherRoster; clears StudentRoster |

### Public API

```
// Read-only: list of students available for promotion this year
List<StudentData> GetCandidates()

// Called by YearEndHUD when the player clicks a student card to promote or deselect
// Returns: false if student is not in the candidate list or already confirmed
bool ToggleSelection(string studentId)

// Called by YearEndHUD "Confirm" button
// Processes all selected promotions, clears the roster, fires OnPromotionComplete
void ConfirmPromotions()

// Read-only: which student IDs are currently selected for promotion
IReadOnlyList<string> SelectedIds { get; }
```

### Events

```
event Action OnPromotionComplete   // fired after all promotions are processed and StudentRoster is cleared
```

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `RunManager` | Subscribes to `OnPromotionComplete` to advance the year or end the run; does not call PromotionSystem directly |
| `StudentRoster` | PromotionSystem calls `RemoveStudent(id)` for promoted students, then `Clear()` for the rest |
| `TeacherRoster` | PromotionSystem calls `TeacherRoster.AddTeacher(teacherData)` for each promoted student |
| `YearEndHUD` | Calls `GetCandidates()`, `ToggleSelection(id)`, `ConfirmPromotions()`; subscribes to track `SelectedIds` |

---

## Formulas

### TeacherBuff from TrainingPointsSpent

```
trainingBuff = max(MinTeacherBuff, min(MaxTeacherBuff, floor(TrainingPointsSpent × BuffPerPoint)))
```

| Variable | Default | Range | Description |
|---|---|---|---|
| `MinTeacherBuff` | 1 | 1–3 | Minimum buff even from an untrained student |
| `MaxTeacherBuff` | 5 | 3–10 | Maximum buff from one teacher (cap) |
| `BuffPerPoint` | 0.5 | 0.1–1.0 | How much each training point spent converts to teacher buff |
| `TrainingPointsSpent` | 0–∞ | depends on `TrainingActionsPerSemester` | Accumulated on `StudentData` by TrainingSystem |

**Expected output table** (with defaults):

| TrainingPointsSpent | Raw Calc | TeacherBuff |
|---|---|---|
| 0 | max(1, 0) | 1 |
| 2 | max(1, 1) | 1 |
| 4 | max(1, 2) | 2 |
| 6 | max(1, 3) | 3 |
| 8 | max(1, 4) | 4 |
| 10+ | max(1, 5) → capped | 5 |

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `ConfirmPromotions()` with zero students selected | `StudentRoster.Clear()` called; `OnPromotionComplete` fires; no teacher added | Valid choice — player forfeits teacher acquisition this year |
| `ConfirmPromotions()` with all 5 students selected | All 5 promoted to TeacherRoster; `StudentRoster` ends empty | Valid; player forfeits no run benefit (students are gone regardless) |
| `ToggleSelection()` called after `ConfirmPromotions()` | Rejected (state = Processing → Inactive); log warning | No UI change possible mid-confirmation |
| Student with only 1 trait: `Traits[0]` used for specialty | Use `Traits[0]` (always valid per StudentData rules: min 1 trait) | No special case needed |
| Student with 2 traits | `Traits[0]` determines specialty; second trait is ignored for promotion | Simple rule; avoids ambiguity |
| `ConfirmPromotions()` called while `RunPhase != YearEnd` | Log error, no-op | Phase guard |
| `TeacherRoster.AddTeacher()` fails (e.g., roster at capacity — future feature) | Log warning; teacher not added; continue with other promotions | Non-fatal; promotion is best-effort |
| `SaveSystem` write fails on `TeacherRoster.AddTeacher()` | TeacherRoster handles this (not PromotionSystem's responsibility) | Responsibility boundary |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `StudentRoster` | This depends on it | Data dependency — reads candidates; calls `RemoveStudent()` and `Clear()` |
| `TeacherRoster` | This depends on it | Ownership handoff — calls `AddTeacher()` for each promoted student |
| `RunManager` | It depends on this | State trigger — subscribes to `OnPromotionComplete` |
| `YearEndHUD` | It depends on this | Rule dependency — calls API to manage selection; renders candidates |

---

## Tuning Knobs

All knobs in `PromotionConfig.asset` (ScriptableObject):

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `MinTeacherBuff` | 1 | 1–3 | Even weak teachers contribute; promotion feels more universally rewarding | Untrained students are worthless as teachers |
| `MaxTeacherBuff` | 5 | 3–10 | Super-trained students produce very powerful teachers | Diminishing returns kick in earlier |
| `BuffPerPoint` | 0.5 | 0.1–1.0 | Training investment converts more directly to teacher quality | Teacher buff growth is slower; more points needed for high buff |

> There is no cap on how many teachers can be in the TeacherRoster (v1). If this causes balance issues, a cap and bump-out mechanic should be added in Phase 3 (`beta-task`).

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| YearEnd phase entered | Student cards animate in; "Choose your teachers" prompt appears | Year-end musical sting | MVP |
| `ToggleSelection()` → selected | Student card highlights (glowing border); selected indicator appears | Selection click | MVP |
| `ToggleSelection()` → deselected | Card returns to normal state | Deselect click | MVP |
| `ConfirmPromotions()` — promoting a student | Card "flies" to teacher roster panel; TeacherData card appears | Promotion fanfare (per teacher) | MVP |
| `ConfirmPromotions()` — discarding a student | Card fades and shrinks | Soft dismiss | MVP |
| All students resolved; `OnPromotionComplete` | YearEnd scene fades to next phase | None (RunManager handles scene music) | MVP |

---

## Game Feel

### Feel Reference

> "Should feel like FTL's crew promotion — bittersweet and deliberate. You're saying goodbye to individual students while investing in the future. NOT a button to mash — each card should be considered."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Toggle selection on a card | 1 frame | 1 frame |
| Confirm promotions → first teacher appears | 1 frame + animation | ≤ 2 frames logic + animation |

### Animation Feel Targets

| Animation | Startup Frames | Active Frames | Recovery Frames | Feel Goal |
|---|---|---|---|---|
| Student card select highlight | 0 | 6 | 0 | Snappy, tactile |
| Promotion "fly to teacher" | 0 | 30 | 0 | Satisfying arc; feels earned |
| Discard fade | 0 | 20 | 0 | Gentle — doesn't celebrate failure |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| A high-training student is promoted | 0.5s per teacher | Card transforms into teacher card; distinct animation |
| Player promotes no one | Instant on confirm | Students all fade; somber; no fanfare |
| All 5 students promoted | 2.5s | Five consecutive promotion animations; feels triumphant |

### Weight and Responsiveness

- **Weight**: Each promotion should feel meaningful — not a click-through
- **Player control**: Full agency — any combination is valid
- **Snap quality**: Cards respond instantly to clicks; no ambiguous hover states
- **Failure texture**: "Discarded" students leave quietly; the player is not punished visually for not promoting

### Feel Acceptance Criteria

- [ ] Player can select/deselect any student with a single click
- [ ] Selected state is visually unambiguous (glowing border or equivalent)
- [ ] Confirm button is clearly labeled and triggers within 1 frame
- [ ] Each promotion animation completes before the next begins (sequential, not simultaneous)
- [ ] All animations complete before `OnPromotionComplete` fires (scene should not transition mid-animation)

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Student name | YearEndHUD candidate card | Static | During YearEnd phase |
| Student traits | Candidate card (icons) | Static | During YearEnd phase |
| Student TrainingPointsSpent | Candidate card | Static | During YearEnd phase |
| Projected TeacherBuff | Candidate card (e.g. "Will teach: +3 Attack") | Static (derived from formula at phase start) | During YearEnd phase |
| Selected count | YearEndHUD header (e.g. "Promoting 2 of 5") | On `ToggleSelection()` | During YearEnd phase |
| Confirm button | YearEndHUD action bar | Always visible | During YearEnd phase |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| TeacherRoster stores and persists TeacherData | `TeacherRoster.md` | `AddTeacher(TeacherData)`, `SaveSystem` integration | Data dependency |
| StudentData.Traits[0] determines specialty | `StudentRoster.md` | `Traits` list format | Data dependency |
| StudentData.TrainingPointsSpent used in buff formula | `TrainingSystem.md` | `TrainingPointsSpent` accumulation | Data dependency |
| TraitType → StatType mapping is fixed | `TraitSystem.md` | `TraitType` enum values | Data type dependency |
| RunManager advances after OnPromotionComplete | `RunManager.md` | Phase advancement logic | Rule dependency |

---

## Acceptance Criteria

- [ ] `ConfirmPromotions()` outside `RunPhase.YearEnd` is rejected with a logged error
- [ ] Each selected student produces a `TeacherData` with the correct `Name`, `Specialty`, `TrainingFocus`, and `TeacherBuff`
- [ ] `TeacherBuff` formula: 0 points → buff=1; 4 points → buff=2; 10 points → buff=5 (capped at `MaxTeacherBuff`)
- [ ] `StudentRoster.Clear()` is called after all promotions — `GetAll()` returns empty
- [ ] `OnPromotionComplete` fires exactly once per `ConfirmPromotions()` call
- [ ] Promoting zero students: StudentRoster still clears; OnPromotionComplete still fires; no teacher added
- [ ] `TrainingBuff` on `TeacherData` is always an integer (no float stored)
- [ ] Unit test: student with `TrainingPointsSpent=6`, `Traits[0]=TraitType.Fire` → TeacherData has `Specialty=Fire`, `TrainingFocus=Attack`, `TrainingBuff=3`

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should there be a minimum number of promotions required per year (to prevent a "no-teacher" run)? | Designer | Before code-system | Pending — recommend no minimum ("Challenging" pillar; consequences of promoting no one = weaker future runs) |
| If a student has 2 traits and both map to the same `StatType` (e.g. Fire + Arcane → both Attack), should they count as a double-Attack teacher? | Designer | Before code-system | Pending — recommend single stat focus (Traits[0] only) for v1 simplicity |
| Should the player be able to see how many teachers they already have in TeacherRoster during the YearEnd promotion screen? | Designer | Before code-system | Pending — recommend yes (context for decisions) |
| Is there a cap on total teachers in the roster across all runs? | Designer | Before Phase 3 | Pending — v1 has no cap; may need one if the buff accumulates to game-breaking levels |
| Should the "projected teacher buff" shown in YearEndHUD account for diminishing returns if the player already has many Attack teachers? | Designer | Before code-system | Pending — recommend show raw buff (no diminishing returns in v1); keep the UI simple |
