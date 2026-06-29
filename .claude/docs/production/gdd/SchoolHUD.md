# SchoolHUD

> **Status**: Draft
> **Last Updated**: 2026-06-29
> **Implements Pillar**: Strategic — the school management screen is where all player decisions happen; it must make every stat, trait, and training option legible at a glance

## Summary

SchoolHUD renders the School scene UI for the Recruit and Train phases. It displays the active student roster with all 7 stats and traits, shows the training budget and trait synergy panel, and provides the controls for allocating training actions and advancing to the next phase. It is read/listen-only — it never mutates game state directly; all player actions route through TrainingSystem and RunManager.

> **Quick reference** — Layer: `Presentation` · Priority: `MVP` · Key deps: `StudentRoster, TrainingSystem, TraitSystem`

---

## Overview

SchoolHUD is a MonoBehaviour on a UIDocument (UI Toolkit) in the School scene. It subscribes to events from StudentRoster, TrainingSystem, and TraitSystem to stay in sync with game state. During the Recruit phase it shows student cards in read-only mode. During the Train phase it activates training controls. The HUD reads `RunManager.CurrentPhase` and `RunManager.CurrentYear` to show the correct context. All player input (selecting a student, choosing a stat to train, clicking Proceed) is forwarded to TrainingSystem or RunManager — the HUD never writes data directly.

## Player Fantasy

The player sits in front of a magical class register — each student card shows their face, name, traits as glowing icons, and 7 stats as a clean readout. The training budget ticks down satisfyingly. The trait synergy panel shows progress bars building toward threshold pops. It feels like managing a fantasy sports team with visible, meaningful feedback on every click.

---

## Detailed Design

### Core Rules

1. SchoolHUD subscribes to events in `OnEnable` and unsubscribes in `OnDisable`:
   - `RunManager.OnPhaseChanged` → switch between Recruit/Train UI modes
   - `RunManager.OnYearChanged` → update year display
   - `StudentRoster.OnRosterChanged` → rebuild student card list
   - `StudentRoster.OnStudentStatChanged` → update the affected student card's stat display
   - `TrainingSystem.OnTrainingActionUsed` → decrement remaining-actions counter
   - `TrainingSystem.OnTrainingExhausted` → enable "Proceed to Battle" button
   - `TraitSystem.OnTraitThresholdReached` → animate trait synergy badge
   - `TraitSystem.OnTraitThresholdLost` → dim trait synergy badge
2. **HUDs never write game state.** Player inputs are forwarded:
   - Training stat button click → `TrainingSystem.AllocateTraining(selectedStudentId, statType)`
   - "Proceed to Battle" button → `TrainingSystem.ConfirmTrainingComplete()`
   - "Begin Training" button (end of Recruit phase) → `RunManager.CompleteRecruitPhase()`
3. A student card is selected by clicking it. The selected student's stat buttons become active. Only one student can be selected at a time.
4. During Recruit phase, stat training buttons are hidden (or disabled). Student cards are displayed in read-only mode.
5. During Train phase, each student card shows all 7 stats (HP, ATK, DEF, MG, MR, SPD, CRIT) with their total values (Base + Bonus). A "+N" float animates on a stat that was just trained.
6. The trait synergy panel shows each trait type (Fire, Healer, Shield, Arcane, Storm, Shadow), the current team count, and which threshold tier is active (0/1/2). It updates synchronously on `OnRosterChanged`.
7. "Proceed to Battle" button is **disabled** until `TrainingSystem.ConfirmTrainingComplete()` is callable (i.e., always enabled during Train phase — the player may proceed early). It becomes visually highlighted when `OnTrainingExhausted` fires.
8. The remaining-actions counter (e.g., "Actions: 3 / 5") is always visible during Train phase and updates on every `OnTrainingActionUsed`.

### States and Transitions

| State | Entry Condition | Exit Condition | Active Controls |
|---|---|---|---|
| `RecruitMode` | `OnPhaseChanged(Recruit)` | `OnPhaseChanged(Train)` | Student cards (read-only); "Begin Training" button |
| `TrainMode` | `OnPhaseChanged(Train)` | Player clicks "Proceed to Battle" | Student cards + stat buttons; actions counter; trait panel; "Proceed" button |
| `Inactive` | Outside School scene | School scene loaded | All UI hidden |

### Student Card Layout

Each student card (rendered per `StudentData`) displays:
- Name
- Trait icons (1–2 icons, colored by TraitType)
- 7 stat rows: HP / ATK / DEF / MG / MR / SPD / CRIT — each shows Total value with Bonus contribution as "+N" in a secondary color
- Training points spent (pip indicators — one pip per point spent)
- Selection highlight border (when selected during Train phase)

### Trait Synergy Panel

One row per TraitType (6 rows). Each row shows:
- Trait icon + name
- Count label (e.g., "Fire: 2")
- Threshold progress indicator (e.g., filled/unfilled stars — 0/1/2 tier)
- Threshold effect tooltip on hover (e.g., "Fire 2: ATK ×1.30 for Fire students")

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `RunManager` | Reads `CurrentPhase` and `CurrentYear` on scene load; subscribes to `OnPhaseChanged` and `OnYearChanged` |
| `StudentRoster` | Subscribes to `OnRosterChanged` to rebuild cards; subscribes to `OnStudentStatChanged` to refresh one card |
| `TrainingSystem` | Forwards training button clicks to `AllocateTraining()`; reads `RemainingActions` for counter; subscribes to `OnTrainingActionUsed` and `OnTrainingExhausted` |
| `TraitSystem` | Reads `GetTeamTraitCounts()` and `GetActiveThresholdTier()` on load; subscribes to `OnTraitThresholdReached` and `OnTraitThresholdLost` |

---

## Formulas

No formulas — SchoolHUD only reads computed values from game systems. Stat totals (TotalHP = BaseHP + BonusHP, etc.) are computed by `StudentData` properties, not by the HUD.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Roster has 0 students (pre-generation) | Student card area shows a placeholder ("Waiting for students...") | Should only occur briefly before `GenerateStudents()` runs |
| `AllocateTraining()` returns 0 (student not found or phase wrong) | HUD shows no "+N" animation; no stat change | Error is logged by TrainingSystem; HUD ignores return value of 0 |
| Training budget exhausted; player clicks a stat button | Button is disabled by HUD before the call is made (checks `TrainingSystem.RemainingActions == 0`) | Prevents rejected calls reaching TrainingSystem |
| Phase changes to Battle while SchoolHUD is visible | Unsubscribe all events; HUD hides itself; SceneLoader handles scene transition | HUD must not render stale state in the Battle scene |
| `OnStudentStatChanged` fires for a student not currently rendered | No-op — find the card by StudentId; if not found, log warning | Guard against stale event deliveries |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `StudentRoster` | This depends on it | Data dependency + state trigger — reads student data; subscribes to roster/stat events |
| `TrainingSystem` | This depends on it | Rule dependency + state trigger — forwards training calls; subscribes to training events |
| `TraitSystem` | This depends on it | Data dependency + state trigger — reads trait counts; subscribes to threshold events |
| `RunManager` | This depends on it | State trigger — subscribes to phase/year changes |

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| Student card width | 160px | 120–220px | More stat detail visible; fewer cards fit without scroll | More cards visible; less detail |
| "+N" float animation duration | 0.5s | 0.2–1.0s | Slower, more satisfying stat-up feedback | Snappier, less noticeable |
| Trait threshold badge pulse duration | 0.5s | 0.2–1.0s | Longer celebration of threshold unlock | Subtler |

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Recruit phase: students arrive | Cards slide in sequentially (staggered) | Recruit chime (AudioSystem) | MVP |
| Student selected | Card border highlights | Soft selection click | MVP |
| Training action allocated | Stat number ticks up; "+N" floats above stat | Stat-up ping | MVP |
| Remaining actions counter decrements | Counter number ticks down | Subtle tick | MVP |
| Trait threshold reached | Badge lights up and pulses | Synergy unlock chime | MVP |
| Training budget exhausted | "Proceed" button glows; training buttons dim | "Training complete" chime | MVP |
| Phase → Train | Training controls fade in | Subtle transition sound | MVP |

---

## Game Feel

### Feel Reference

> "Should feel like Teamfight Tactics' bench + synergy panel — every stat is readable in 2 seconds, every synergy shows its progress without a tooltip, and training actions produce immediate satisfying feedback. NOT a spreadsheet."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Student card click → selection highlight | 1 frame | 1 frame |
| Stat button click → stat updates | 1 frame (TrainingSystem is sync) | 1 frame |
| "+N" animation start | 1 frame after stat applied | 1 frame |

### Animation Feel Targets

| Animation | Startup Frames | Active Frames | Recovery Frames | Feel Goal |
|---|---|---|---|---|
| Student card slide-in (per card) | 0 | 20 | 0 | Satisfying staggered arrival |
| Stat number count-up | 0 | 8 | 0 | Snappy number rollup |
| "+N" float | 0 | 30 | 0 | Clear, readable upward drift |
| Threshold badge pulse | 0 | 30 | 0 | "Power up" moment |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| First trait threshold unlocked this run | 0.5s | Badge lights up + chime — the first synergy is a milestone |
| Training budget hits zero | 0.3s | "Proceed" button glows — clear call to action |

### Weight and Responsiveness

- **Weight**: Training actions feel deliberate and irreversible
- **Player control**: Full control — player chooses which student and which stat each action
- **Snap quality**: Stats update instantly; animations are additive (never block interaction)
- **Failure texture**: Running out of training with an underpowered team is readable ("this student has 0 training pips — they'll be weak")

### Feel Acceptance Criteria

- [ ] A player can read all student stats within 5 seconds of the Recruit phase starting
- [ ] Stat changes are visible within 1 frame of clicking a training button
- [ ] The training budget counter is always visible and updates on every action
- [ ] Trait synergy progress is visible without opening a tooltip

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Current year ("Year 2 of 3") | HUD header | On `OnYearChanged` | Always during run |
| Current phase label | HUD header | On `OnPhaseChanged` | Always during run |
| Student cards (name, traits, 7 stats) | Main card grid | On `OnRosterChanged` / `OnStudentStatChanged` | Always during School scene |
| Training actions remaining ("Actions: 3/5") | Training controls bar | On `OnTrainingActionUsed` | During Train phase |
| Trait synergy panel | Right side panel | On `OnRosterChanged` / `OnTraitThresholdReached` | Always during School scene |
| "Begin Training" button | Bottom action bar | Static | During Recruit phase |
| Stat training buttons (HP/ATK/DEF/MG/MR/SPD/CRIT) | Below selected student card | On student select | During Train phase, when student selected |
| "Proceed to Battle" button | Bottom action bar | Static (enabled after Recruit complete) | During Train phase |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| `TrainingSystem.AllocateTraining()` | `TrainingSystem.md` | `AllocateTraining(studentId, StatType)` | Rule dependency |
| `TrainingSystem.ConfirmTrainingComplete()` | `TrainingSystem.md` | `ConfirmTrainingComplete()` | Rule dependency |
| `RunManager.CompleteRecruitPhase()` | `RunManager.md` | `CompleteRecruitPhase()` | Rule dependency |
| `TraitSystem.GetTeamTraitCounts()` | `TraitSystem.md` | `GetTeamTraitCounts()` | Data dependency |
| `StudentData` 7-stat structure | `StudentRoster.md` | `TotalHP/ATK/DEF/MG/MR/SPD/CRIT` properties | Data dependency |
| HUDs are read/listen only | `best-practices.md` | "HUDs are read/listen only" rule | Rule dependency |

---

## Acceptance Criteria

- [ ] All 7 stats (HP, ATK, DEF, MG, MR, SPD, CRIT) are displayed per student card with correct total values
- [ ] Training button click calls `TrainingSystem.AllocateTraining()` — never writes `StudentData` directly
- [ ] "+N" animation fires correctly for the stat that was trained
- [ ] Trait synergy panel correctly reflects team trait counts after every roster change
- [ ] `OnTraitThresholdReached` triggers badge animation within 1 frame
- [ ] "Proceed to Battle" button is accessible at any point during Train phase (not locked behind budget exhaustion)
- [ ] No `FindObjectOfType` in `SchoolHUD.cs`
- [ ] No direct mutation of `StudentData` from `SchoolHUD.cs`
- [ ] SchoolHUD unsubscribes all events on `OnDisable`

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should the 7 stat buttons always be visible on the selected card, or collapsed behind an "expand" chevron? | Designer | Before Alpha | Pending — recommend always visible; 7 stats is manageable in a vertical list |
| Should there be a "training history" log showing what was trained this semester? | Designer | Before Alpha | Pending — recommend no for MVP; adds complexity |
| Should the trait panel show enemy traits (what Year N enemies will use) as a hint? | Designer | Before Alpha | Pending — recommend no for MVP; enemy info is discovered in Battle |
| Should CRIT display as a percentage (e.g. "CRIT: 15%") or as a raw integer? | Designer | Before Alpha | Pending — recommend "%" suffix for clarity |
