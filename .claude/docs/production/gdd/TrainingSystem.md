# TrainingSystem

> **Status**: Draft
> **Last Updated**: 2026-06-29
> **Implements Pillar**: Strategic — the limited training budget forces deliberate trade-offs: which student gets developed, and in which stat?

## Summary

TrainingSystem implements the core player action: allocating a limited pool of training sessions to students to raise their stats. The player chooses a student and a stat type (Attack, MaxHP, or Speed); TrainingSystem applies a stat bonus directly to `StudentData`, queries TeacherRoster for teacher-provided training buffs, then fires events so SchoolHUD reflects the change. It tracks remaining training actions for the semester and fires `OnTrainingExhausted` when the budget is spent.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `StudentRoster`

---

## Overview

TrainingSystem is a MonoBehaviour in the School scene, alive for one run. It is only active during the `Train` phase of a semester. At the start of each Train phase, RunManager initializes TrainingSystem with the semester's training action budget (`TrainingActionsPerSemester` from `RunConfig`). The player then repeatedly calls `AllocateTraining(studentId, statType)` through the SchoolHUD. For each call, TrainingSystem computes the gain (base + teacher buff), applies it to the target student's bonus stats, decrements remaining actions, and notifies StudentRoster. When the budget reaches zero, `OnTrainingExhausted` fires and the "Proceed to Battle" button becomes available.

## Player Fantasy

Every training session feels like a meaningful investment. The player agonizes: "Do I max out my strongest student's Attack, or spread the training to hit a new trait threshold?" The limited budget makes each choice matter, and the payoff comes in the Battle phase when the trained student visibly outperforms their starting stats.

---

## Detailed Design

### StatType Enum

```csharp
public enum StatType { HP, ATK, DEF, MG, MR, SPD, CRIT }
```

### TrainingConfig ScriptableObject

`Assets/Config/TrainingConfig.asset` — all tunable training parameters:

| Field | Type | Default | Description |
|---|---|---|---|
| `HPTrainingGain` | int | 12 | Base HP bonus per training action |
| `ATKTrainingGain` | int | 3 | Base physical attack bonus per training action |
| `DEFTrainingGain` | int | 2 | Base armor bonus per training action |
| `MGTrainingGain` | int | 3 | Base magic power bonus per training action |
| `MRTrainingGain` | int | 2 | Base magic resistance bonus per training action |
| `SPDTrainingGain` | int | 1 | Base speed bonus per training action |
| `CRITTrainingGain` | int | 5 | Base crit chance bonus (%) per training action |

### Core Rules

1. TrainingSystem is only active during `RunPhase.Train`. Calls to `AllocateTraining()` outside this phase are rejected with a logged error.
2. The training action budget (`RemainingActions`) is set at the start of each Train phase by RunManager. It is not cumulative across semesters.
3. Each call to `AllocateTraining(studentId, statType)` costs exactly 1 action, regardless of the gain amount.
4. A student may receive multiple training actions in the same semester (no per-student limit).
5. Training gain is computed as: `baseGain + teacherBuff`, where `baseGain` comes from `TrainingConfig` and `teacherBuff` comes from `TeacherRoster.GetTrainingBuff(statType)`.
6. TrainingSystem writes the computed gain directly to the target `StudentData`'s matching bonus stat (`BonusHP`, `BonusATK`, `BonusDEF`, `BonusMG`, `BonusMR`, `BonusSPD`, or `BonusCRIT`) and increments `TrainingPointsSpent`. `BonusCRIT` is clamped so `TotalCRIT` never exceeds 100.
7. TrainingSystem calls `StudentRoster.NotifyStatChanged(student)` after every write.
8. When `RemainingActions` reaches 0, `OnTrainingExhausted` fires. Further calls to `AllocateTraining()` are rejected (no-op with a warning — the player must proceed to Battle).
9. The player may proceed to Battle before the budget is fully spent (`CompleteTrainPhase()` is available at any time during Train phase, not only on exhaustion).
10. No randomness in training outcomes in v1 — gain is deterministic (base + teacher buff).

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Inactive` | Outside Train phase | `OnPhaseChanged(Train)` | Rejects all training calls |
| `Active` | `OnPhaseChanged(Train)` + `RemainingActions > 0` | `RemainingActions == 0` or `CompleteTrainPhase()` | Accepts training calls |
| `Exhausted` | `RemainingActions == 0` | `CompleteTrainPhase()` called | Rejects further training; fires `OnTrainingExhausted` |

### Public API

```
// Called by RunManager at the start of each Train phase
void InitializeSemester(int actionBudget)

// Called by SchoolHUD when the player selects a student + stat
// Returns: actual gain applied (for HUD to display a "+N" feedback)
int AllocateTraining(string studentId, StatType stat)

// Read-only: remaining actions this semester
int RemainingActions { get; }

// Called by SchoolHUD "Proceed to Battle" button
// Delegates to RunManager.CompleteTrainPhase()
void ConfirmTrainingComplete()
```

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `RunManager` | Calls `InitializeSemester(budget)` at `OnPhaseChanged(Train)`; receives `CompleteTrainPhase()` call via `ConfirmTrainingComplete()` |
| `StudentRoster` | Reads student by ID via `GetById()`; writes bonus stats directly; calls `NotifyStatChanged()` |
| `TeacherRoster` | Reads `GetTrainingBuff(StatType)` to add teacher bonuses to the gain |
| `SchoolHUD` | Calls `AllocateTraining()` on player input; subscribes to `OnTrainingActionUsed`, `OnTrainingExhausted` to update the UI |

---

## Formulas

### Training Gain

```
gain = baseGain(statType) + teacherBuff(statType)
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `baseGain(statType)` | int | 1–20 | `TrainingConfig` ScriptableObject | Base stat increase per training action for the chosen stat |
| `teacherBuff(statType)` | int | 0–10 | `TeacherRoster.GetTrainingBuff(statType)` | Flat bonus from accumulated teachers matching the stat type |

**Expected output range with defaults** (base gain + up to +10 from teachers):
- HP gain: 12–22
- ATK gain: 3–13
- DEF gain: 2–12
- MG gain: 3–13
- MR gain: 2–12
- SPD gain: 1–11
- CRIT gain: 5–15 (% points; clamped so TotalCRIT ≤ 100)

**Edge cases**: Gain is always ≥ 1 (clamped at minimum 1 even if formula produces 0 or negative). CRIT gain is additionally clamped so the student's TotalCRIT never exceeds 100%.

### Total Training Budget

```
RemainingActions = TrainingActionsPerSemester (from RunConfig)
RemainingActions decrements by 1 per AllocateTraining() call
```

**Expected output**: 0 ≤ `RemainingActions` ≤ `TrainingActionsPerSemester` at all times.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `AllocateTraining()` called in `Inactive` state | Log error, return 0 | Phase guard |
| `AllocateTraining()` called with unknown `studentId` | Log error, return 0 | StudentRoster.GetById returns null; TrainingSystem guards before writing |
| `AllocateTraining()` called with `RemainingActions == 0` | Log warning, return 0 | Budget exhausted; HUD should disable training input before this happens |
| All 5 training actions used on one student | Valid and allowed | No per-student cap — specialization is a valid strategy |
| `ConfirmTrainingComplete()` called with `RemainingActions > 0` | Valid — advances to Battle without spending the full budget | Player may choose to save time or accept suboptimal training |
| `TeacherRoster.GetTrainingBuff()` returns a negative value | Clamp final gain to minimum 1 | Prevents a debuff from making training harmful |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `StudentRoster` | This depends on it | Data dependency — reads student data; writes bonus stats |
| `TeacherRoster` | This depends on it | Data dependency — reads training buff values per stat type |
| `RunManager` | It depends on this | State trigger — calls `InitializeSemester()`; receives `CompleteTrainPhase()` |
| `SchoolHUD` | It depends on this | Rule dependency — calls `AllocateTraining()`; subscribes to training events |

---

## Tuning Knobs

All knobs are on `TrainingConfig.asset` (ScriptableObject):

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `HPTrainingGain` | 12 | 5–40 | Students are much more durable | Students die faster; harder difficulty |
| `ATKTrainingGain` | 3 | 1–15 | Students deal more physical damage; battles end faster | Less physical damage; battles last longer |
| `DEFTrainingGain` | 2 | 1–10 | Students tank physical hits much better | Less defensive value from armor training |
| `MGTrainingGain` | 3 | 1–15 | Trait magic abilities hit harder | Less magic damage payoff |
| `MRTrainingGain` | 2 | 1–10 | Students resist magic trait abilities better | More vulnerable to magic builds |
| `SPDTrainingGain` | 1 | 1–5 | Students act more frequently; fast build viability increases | Slower pacing; speed training less impactful |
| `CRITTrainingGain` | 5 | 2–15 | Crit builds reach high chance faster (more variance) | Slower crit scaling; crit builds need more investment |

> `TrainingActionsPerSemester` (default: 5) is on `RunConfig.asset`, not `TrainingConfig.asset`.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| `AllocateTraining()` succeeds | Student card flashes highlight; stat number animates to new value; "+N" floats upward | Satisfying "train" SFX (stat-up ping) | MVP |
| `RemainingActions` decrements | Training budget counter ticks down in SchoolHUD | Subtle click/tick | MVP |
| `OnTrainingExhausted` | "Proceed to Battle" button activates; training controls dim | Distinct chime ("training complete") | MVP |
| `AllocateTraining()` rejected (budget empty) | Input is blocked by UI (button disabled before call) | Error buzz | MVP |

---

## Game Feel

### Feel Reference

> "Should feel like Slay the Spire's card upgrade — each choice is permanent, the cost is visible (remaining actions count), and the payoff (stat number goes up) is immediate and satisfying. NOT a system where the stat changes are invisible or feel random."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Player selects student + stat → gain applied | 1 frame | 1 frame |
| "+N" float animation starts | 1 frame after stat applied | 1 frame |

### Animation Feel Targets

| Animation | Startup Frames | Active Frames | Recovery Frames | Feel Goal |
|---|---|---|---|---|
| Stat number update | 0 | 10 (count-up) | 0 | Snappy number rollup |
| "+N" float | 0 | 30 | 0 | Clear, readable upward drift |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Training budget exhausted | 0.5s | "Training complete" chime + button activates |
| High-gain training action (teacher buff active) | 0.3s | Larger "+N" text; slightly warmer flash |

### Weight and Responsiveness

- **Weight**: Moderate — each training action should feel considered and irreversible
- **Player control**: Fully player-controlled during Train phase; no auto-training
- **Snap quality**: Gain is applied instantly (deterministic, no roll animation)
- **Failure texture**: Spending training on a weak student is a recoverable mistake (they still contribute to trait thresholds)

### Feel Acceptance Criteria

- [ ] The stat change is visible within 1 frame of clicking the train button
- [ ] The "+N" feedback makes it unambiguous which stat was raised and by how much
- [ ] The training budget counter is always visible and updates on every action
- [ ] A player with 0 remaining actions cannot accidentally consume an extra action

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Remaining training actions | SchoolHUD header (e.g. "Training: 3/5") | On `OnTrainingActionUsed` | During Train phase |
| Per-stat gain preview | Tooltip on stat training button | On hover | During Train phase |
| Teacher buff amount | Tooltip / stat training button label (e.g. "Attack +3 (+1 from teachers)") | Static per semester | During Train phase |
| Student TrainingPointsSpent | Student card progress pip/bar | On `OnStudentStatChanged` | During Train phase |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| TeacherRoster provides training buff per stat type | `TeacherRoster.md` | `GetTrainingBuff(StatType)` | Data dependency |
| StudentRoster NotifyStatChanged | `StudentRoster.md` | `NotifyStatChanged(StudentData)` | Data dependency |
| RunConfig.TrainingActionsPerSemester | `RunManager.md` | `TrainingActionsPerSemester` tuning knob | Data dependency |

---

## Acceptance Criteria

- [ ] `AllocateTraining()` outside `RunPhase.Train` is rejected with a logged error
- [ ] Stat bonus is applied correctly for all 7 stat types: e.g. `BonusATK += ATKTrainingGain + teacherBuff(ATK)` for ATK training
- [ ] `TrainingPointsSpent` increments by 1 on every successful training call
- [ ] `RemainingActions` decrements by exactly 1 per successful call; never goes below 0
- [ ] `OnTrainingExhausted` fires exactly once when `RemainingActions` reaches 0
- [ ] `AllocateTraining()` with a spent budget returns 0 and logs a warning
- [ ] Teacher buff is read from `TeacherRoster.GetTrainingBuff(statType)` — never hardcoded
- [ ] Final gain is always ≥ 1 (minimum clamp enforced)
- [ ] `BonusCRIT` is clamped so `TotalCRIT` never exceeds 100 after any training action
- [ ] All gain values are integers — no float arithmetic in the training path
- [ ] Unit test: `AllocateTraining(id, StatType.ATK)` with `ATKTrainingGain=3` and `teacherBuff=2` applies `BonusATK += 5`
- [ ] Unit test: CRIT training on a student with `TotalCRIT=95` and `CRITTrainingGain=5` → `BonusCRIT` increases by only 5, `TotalCRIT` clamped to 100

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should training be undoable within the same semester (undo last action), or is it always permanent once confirmed? | Designer | Before code-system | Pending — recommend no undo (permanent, as per "Challenging" pillar) |
| Should there be a visual indicator of which students have not yet received any training? (Risk of neglecting a student.) | Designer | Before code-system | Pending — recommend yes: a "0 training" badge on the student card |
| Should the teacher training buff apply per stat, or as a flat bonus to all stats? (Current design: per stat.) | Designer | Before code-system | Pending — per-stat is more strategic; recommend keeping it |
| Is there a maximum number of training actions that can be applied to a single student per semester? | Designer | Before code-system | Pending — recommend no cap (allows specialization strategy) |
