# MetaProgressionHUD

> **Status**: Draft
> **Last Updated**: 2026-06-29
> **Implements Pillar**: Empowering — the teacher roster is the player's permanent achievement wall; every name on it represents a past run's success

## Summary

MetaProgressionHUD displays the player's accumulated teacher collection and run history between runs. It is accessible from the main menu and shows teacher cards (name, specialty, TrainingFocus, TrainingBuff), aggregate buff totals per stat type, and basic run history (total runs started, total wins). It is a pure read-only display — it never modifies TeacherRoster, SaveSystem, or any game state.

> **Quick reference** — Layer: `Presentation` · Priority: `MVP` · Key deps: `TeacherRoster`

---

## Overview

MetaProgressionHUD is a MonoBehaviour on a UIDocument, displayed as an overlay panel accessible from the main menu (a "View Teachers" or "Legacy" button). On open it calls `TeacherRoster.GetAll()` to build teacher cards and `TeacherRoster.GetTeacherCountByFocus(stat)` for each StatType to display aggregate buff summaries. It also reads basic run stats from `GameManager` (or from a `SaveData` struct via `SaveSystem`). The panel can be closed to return to the main menu. No events are subscribed to — it reads state once on open and does not update dynamically while open.

## Player Fantasy

The player opens this screen after a failed run and sees the teachers they earned. "I have 4 ATK teachers now — next run my students will hit harder from day 1." The named teachers feel like a hall of fame. Returning players with a full teacher roster feel genuinely powerful before the run even starts.

---

## Detailed Design

### Core Rules

1. MetaProgressionHUD is opened by a button in MainMenuController (not by a game event — it is player-triggered).
2. On open, it reads state once from TeacherRoster and displays it. It does **not** subscribe to events — the roster does not change while this panel is open (runs are not active during menu).
3. Teacher cards are displayed in acquisition order (most recently added last).
4. Aggregate buff summary: one row per StatType (HP, ATK, DEF, MG, MR, SPD, CRIT). Each row shows total buff from all teachers of that focus (e.g., "ATK buff: +8 (from 3 teachers)").
5. Run history reads from `SaveData` via `SaveSystem.GetSaveData()` — specifically `TotalRunsStarted` and `TotalRunsWon`.
6. "Close" button hides the panel and returns focus to the main menu.
7. MetaProgressionHUD never writes to TeacherRoster, SaveSystem, or any other system.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Closed` | Default; main menu visible | "View Teachers" button clicked | Panel hidden |
| `Open` | "View Teachers" clicked | "Close" button clicked | Panel visible; teacher cards + stat summary displayed |

### Teacher Card Layout

Each teacher card (one per `TeacherData`) displays:
- Teacher name
- Specialty trait icon (e.g., Fire icon)
- Training focus stat label (e.g., "ATK Teacher")
- Training buff value (e.g., "+3 ATK to future training")
- (Optional) Acquisition run number (future feature — not MVP)

### Aggregate Stat Summary

A summary table below (or beside) the teacher cards:

| Stat | Total Buff | Teachers |
|---|---|---|
| HP | +0 | 0 |
| ATK | +8 | 3 |
| DEF | +2 | 1 |
| MG | +5 | 2 |
| MR | +0 | 0 |
| SPD | +3 | 1 |
| CRIT | +10 | 2 |

These values are read from `TeacherRoster.GetTrainingBuff(StatType)` and `TeacherRoster.GetTeacherCountByFocus(StatType)`.

### Run History

A small stats section:
- "Runs started: N" — from `SaveData.TotalRunsStarted`
- "Runs won: N" — from `SaveData.TotalRunsWon`
- Win rate (computed: `TotalRunsWon / TotalRunsStarted`, displayed as %)

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `TeacherRoster` | Calls `GetAll()` for teacher cards; calls `GetTrainingBuff(stat)` and `GetTeacherCountByFocus(stat)` for summary |
| `SaveSystem` | Calls `GetSaveData()` to read `TotalRunsStarted` and `TotalRunsWon` for run history |
| `MainMenuController` | Receives open/close signal from MainMenuController's "View Teachers" button |

---

## Formulas

### Win Rate Display

```
winRate = TotalRunsWon / TotalRunsStarted × 100
```

Displayed as integer % (floored). If `TotalRunsStarted == 0`, display "—" instead of a division-by-zero.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| TeacherRoster is empty (first run, no teachers yet) | Show "No teachers yet — promote students to build your roster" placeholder | Expected on very first session |
| `TotalRunsStarted == 0` | Win rate shows "—" | Division by zero guard |
| More than 20 teachers | Scroll the teacher card list | Roster is uncapped in v1; scrollable container handles growth |
| Save file missing (first launch) | Show empty state | SaveSystem returns a default `SaveData` with all zeroes |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `TeacherRoster` | This depends on it | Data dependency — reads teacher list and buff totals |
| `SaveSystem` | This depends on it | Data dependency — reads run history from SaveData |
| `MainMenuController` | It depends on this | UI ownership — MainMenuController opens/closes this panel |

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| Teacher card grid columns | 3 | 2–5 | More cards per row; each card is smaller | Fewer per row; cards are larger |
| Panel slide-in duration | 0.3s | 0–0.6s | Smoother panel appearance | Near-instant |

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Panel opens | Cards fade in; stat summary appears | Soft UI whoosh | MVP |
| "View Teachers" hovered | Button highlights | Hover SFX | Alpha |
| Panel closes | Panel slides out | UI dismiss sound | Alpha |
| Empty roster state | Placeholder text with subtle animation | None | MVP |

---

## Game Feel

### Feel Reference

> "Should feel like a trophy room or achievement wall — quiet, reflective, and satisfying. The player sees their history represented by named individuals. NOT a raw data dump or a spreadsheet."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| "View Teachers" click → panel visible | 1 frame + animation | ≤ 2 frames + 0.3s animation |
| "Close" click → panel hidden | 1 frame + animation | ≤ 2 frames |

### Animation Feel Targets

| Animation | Startup Frames | Active Frames | Recovery Frames | Feel Goal |
|---|---|---|---|---|
| Panel slide-in | 0 | 18 (0.3s) | 0 | Smooth, not jarring |
| Teacher card fade-in | 0 | 12 (staggered) | 0 | "Unveiling" the roster |

### Weight and Responsiveness

- **Weight**: This screen is reflective and calm — not high-energy
- **Player control**: Read-only; only action is Close
- **Snap quality**: N/A — display only

### Feel Acceptance Criteria

- [ ] A player can read all teacher names, specialties, and buffs within 10 seconds of opening the panel
- [ ] The aggregate stat summary makes it immediately clear which stats are well-buffed
- [ ] An empty roster communicates "you haven't promoted anyone yet" clearly

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Teacher cards (name, specialty, focus, buff) | Scrollable grid | On panel open (once) | When panel is open |
| Aggregate stat summary table | Panel footer or side | On panel open (once) | When panel is open |
| Run history (starts, wins, win rate) | Panel header | On panel open (once) | When panel is open |
| "Close" button | Panel top-right or footer | Static | When panel is open |
| Empty state placeholder | Card grid area | Static | When roster is empty |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| `TeacherRoster.GetAll()` | `TeacherRoster.md` | `GetAll()` API | Data dependency |
| `TeacherRoster.GetTrainingBuff(stat)` | `TeacherRoster.md` | `GetTrainingBuff(StatType)` | Data dependency |
| `TeacherRoster.GetTeacherCountByFocus(stat)` | `TeacherRoster.md` | `GetTeacherCountByFocus(StatType)` | Data dependency |
| `SaveSystem.GetSaveData()` | `SaveSystem.md` | `TotalRunsStarted`, `TotalRunsWon` in `SaveData` | Data dependency |
| `MainMenuController` "View Teachers" button | `MainMenuController.md` | Button that opens this panel | Rule dependency |
| HUDs are read/listen only | `best-practices.md` | "HUDs are read/listen only" rule | Rule dependency |

---

## Acceptance Criteria

- [ ] All teachers from `TeacherRoster.GetAll()` are displayed as cards with correct name, specialty, focus, and buff
- [ ] Aggregate stat summary matches `TeacherRoster.GetTrainingBuff(stat)` for all 7 StatTypes
- [ ] Run history matches `SaveData.TotalRunsStarted` and `TotalRunsWon`
- [ ] Win rate shows "—" when `TotalRunsStarted == 0`
- [ ] Empty roster shows a clear placeholder message
- [ ] Panel is scrollable when more than N teachers overflow the visible area
- [ ] No `FindObjectOfType` in `MetaProgressionHUD.cs`
- [ ] Panel never writes to `TeacherRoster`, `SaveSystem`, or any game system

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should teachers be sorted by acquisition order, or grouped by StatType focus? | Designer | Before Alpha | Pending — recommend grouped by focus (ATK teachers together, etc.) for readability |
| Should run history show a per-run log (Run 1: Year 2 loss, Run 2: Win, etc.)? | Designer | Before Phase 3 | Pending — no for MVP; full history is a Phase 3 feature |
| Should there be a "dismiss teacher" option (to remove old weak teachers)? | Designer | Before Phase 3 | Pending — no for v1; teachers are permanent |
| Should the panel be accessible during a run (pause menu), or only from the main menu? | Designer | Before Alpha | Pending — recommend main menu only for MVP |
