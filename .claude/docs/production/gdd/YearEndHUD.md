# YearEndHUD

> **Status**: Draft
> **Last Updated**: 2026-06-29
> **Implements Pillar**: Empowering & Strategic — the promotion screen is where the player converts this run's effort into permanent legacy; every choice here compounds into future power

## Summary

YearEndHUD renders the promotion/discard screen at the end of each year. It displays surviving students as promotion candidates showing all 7 stats, projected teacher buff, and trait specialty. The player selects any subset to promote; the rest are discarded. All selection and confirmation logic is forwarded to PromotionSystem — the HUD never writes TeacherData or modifies the roster directly.

> **Quick reference** — Layer: `Presentation` · Priority: `MVP` · Key deps: `PromotionSystem`

---

## Overview

YearEndHUD is a MonoBehaviour on a UIDocument in the YearEnd scene. On `Start()` it calls `PromotionSystem.GetCandidates()` to build the candidate card list. Each card shows the student's full stats and the projected TeacherBuff that would result from promoting them (`floor(TrainingPointsSpent × 0.5)`, clamped). The player clicks cards to toggle selection; a "Confirm Promotions" button calls `PromotionSystem.ConfirmPromotions()`. The HUD then plays per-student animations (promote or discard) and waits for `PromotionSystem.OnPromotionComplete` before handing control back to RunManager.

## Player Fantasy

This screen should feel like graduation day — bittersweet and weighty. Each student card is a face and a name the player recognizes from the semester. Choosing who becomes a teacher feels like a real decision: "She has the best ATK training — she'll boost future attackers. But he has Fire trait and I need more Fire teachers." The promoted cards animating into the teacher panel feel earned.

---

## Detailed Design

### Core Rules

1. YearEndHUD subscribes to `PromotionSystem.OnPromotionComplete` in `OnEnable` and unsubscribes in `OnDisable`.
2. On `Start()`, calls `PromotionSystem.GetCandidates()` to populate candidate cards. Cards are built once; not rebuilt dynamically.
3. Each card click calls `PromotionSystem.ToggleSelection(studentId)`. The HUD reads `PromotionSystem.SelectedIds` to update the selection highlight.
4. The "Confirm Promotions" button calls `PromotionSystem.ConfirmPromotions()`. The button is always enabled — no minimum promotion requirement.
5. After `ConfirmPromotions()` is called, all cards are locked (no further selection changes). The HUD runs promotion/discard animations in SelectedIds order.
6. On `OnPromotionComplete`, the HUD does not advance the scene — RunManager advances automatically on receiving that event. The HUD simply ensures all animations have completed first (max 2s grace period).
7. **HUD never writes to TeacherRoster, StudentRoster, or any game system** — all state changes happen inside PromotionSystem.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `SelectionOpen` | Scene loads + candidates built | `ConfirmPromotions()` called | Cards clickable; "Confirm" button active |
| `Animating` | `ConfirmPromotions()` called | All animations complete + `OnPromotionComplete` received | Cards locked; promotion/discard animations play |
| `Done` | `OnPromotionComplete` received | RunManager transitions scene | Blank screen; brief pause |

### Candidate Card Layout

Each candidate card (one per StudentData from `GetCandidates()`) displays:
- Student name
- Trait icons (1–2)
- All 7 stats: HP / ATK / DEF / MG / MR / SPD / CRIT (total values, with bonus annotated)
- Training points spent (pip count)
- **Projected teacher buff**: "Will teach: +N [StatType]" — computed from `floor(TrainingPointsSpent × BuffPerPoint)`, clamped to [MinTeacherBuff, MaxTeacherBuff]
- **Trait specialty**: which StatType their trait maps to (e.g., Fire → ATK Teacher)
- Selection glow border (when selected)

### Promotion Animation Sequence

When `ConfirmPromotions()` is confirmed, for each student in `SelectedIds`:
1. Card scales up slightly → flies toward a "Teacher Panel" on the right side of the screen
2. At the teacher panel, the card transforms (visual change: "student card" → "teacher card" styling)
3. Teacher card settles into the panel with a brief glow

For non-selected (discarded) students:
1. Card fades and shrinks away simultaneously with promotions
2. No sound effect — quiet exit

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `PromotionSystem` | Calls `GetCandidates()` on init; forwards card clicks to `ToggleSelection()`; reads `SelectedIds`; calls `ConfirmPromotions()`; subscribes to `OnPromotionComplete` |
| `RunManager` | Does not interact directly — RunManager advances on `PromotionSystem.OnPromotionComplete`, which fires after YearEndHUD's animations complete |

---

## Formulas

### Projected Teacher Buff (display only — mirrors PromotionSystem formula)

```
projectedBuff = clamp(floor(TrainingPointsSpent × BuffPerPoint), MinTeacherBuff, MaxTeacherBuff)
```

YearEndHUD reads `BuffPerPoint`, `MinTeacherBuff`, `MaxTeacherBuff` from a `PromotionConfig` ScriptableObject reference (same asset used by PromotionSystem) — does not recompute independently.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `GetCandidates()` returns empty list (all students died in battle) | Show "No candidates" message; "Confirm" button still present; clicking confirm advances (0 promotions) | Should not occur in normal play (loss = run end); defensive handling |
| Player clicks "Confirm" with 0 students selected | Valid — all students discarded; all cards fade; `OnPromotionComplete` still fires | No minimum requirement |
| Player clicks "Confirm" with all 5 selected | All 5 promoted; 5 promotion animations play sequentially | Valid — all cards become teachers |
| `ToggleSelection()` returns false (invalid ID) | Card does not highlight; no-op | Guard against stale card references |
| Player rapidly clicks confirm twice | Second click ignored (state = Animating → buttons locked) | Transition guard |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `PromotionSystem` | This depends on it | Rule dependency + ownership handoff — all selection/confirmation routes through PromotionSystem |

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| Promotion animation duration | 0.5s per teacher | 0.3–1.5s | More cinematic per promotion | Faster, less ceremonial |
| Discard animation duration | 0.3s | 0.1–0.6s | More visible discard | Near-instant discard |
| Sequential promotion delay | 0.1s between cards | 0–0.5s | Staggered, more dramatic | All cards fly simultaneously |

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| YearEnd scene loads | Candidate cards slide in | Year-end musical sting (AudioSystem) | MVP |
| Card selected (toggle on) | Card glows; selected indicator appears | Selection click SFX | MVP |
| Card deselected (toggle off) | Card returns to normal | Deselect click SFX | MVP |
| Confirm clicked | Cards lock; promotions begin | None (promotions have own SFX) | MVP |
| Student promoted | Card flies to teacher panel; transforms | Promotion fanfare per teacher | MVP |
| Student discarded | Card fades + shrinks | Soft dismiss sound | MVP |
| All animations done | Brief pause; scene transitions | None (RunManager/AudioSystem handles) | MVP |

---

## Game Feel

### Feel Reference

> "Should feel like FTL's crew augmentation screen — bittersweet, deliberate, consequential. Each student card is a person the player spent a semester with. NOT a checkbox list."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| Card click → selection toggle | 1 frame | 1 frame |
| Confirm click → first animation starts | 1 frame | 1 frame |

### Animation Feel Targets

| Animation | Startup Frames | Active Frames | Recovery Frames | Feel Goal |
|---|---|---|---|---|
| Card select highlight | 0 | 6 | 0 | Snappy, tactile |
| Promotion fly | 0 | 30 | 0 | Satisfying arc |
| Discard fade | 0 | 18 | 0 | Quiet, not punishing |

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| A heavily-trained student is promoted | 0.5s | Larger, more glowing promotion animation |
| All 5 students promoted | 2.5s | Five consecutive animations; triumphant |
| Zero students promoted | 0.5s | All cards fade simultaneously; somber |

### Weight and Responsiveness

- **Weight**: Each selection feels deliberate — promotion cards carry weight
- **Player control**: Full agency — any combination valid
- **Snap quality**: Selection toggle is instant; confirmation is irreversible
- **Failure texture**: Discarded students leave quietly — the player chose; no punishment animation

### Feel Acceptance Criteria

- [ ] Player can select/deselect any student with a single click
- [ ] Selected state is visually unambiguous without a tooltip
- [ ] Confirm button is clearly labeled and cannot be missed
- [ ] Promotion animations complete before scene transitions

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Student name | Candidate card | Static | YearEnd phase |
| Student traits | Candidate card (icons) | Static | YearEnd phase |
| All 7 stats | Candidate card | Static | YearEnd phase |
| Training points spent | Candidate card (pip indicator) | Static | YearEnd phase |
| Projected teacher buff ("Will teach: +N ATK") | Candidate card | Static | YearEnd phase |
| Selected count ("Promoting: 2 of 5") | HUD header | On `ToggleSelection()` | YearEnd phase |
| Teacher panel (animated destination) | Right side | On promotion animation | During promotion sequence |
| "Confirm Promotions" button | Bottom action bar | Always visible | YearEnd phase |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| `PromotionSystem.GetCandidates()` | `PromotionSystem.md` | `GetCandidates()` API | Data dependency |
| `PromotionSystem.ToggleSelection()` | `PromotionSystem.md` | `ToggleSelection(studentId)` | Rule dependency |
| `PromotionSystem.ConfirmPromotions()` | `PromotionSystem.md` | `ConfirmPromotions()` | Rule dependency |
| `PromotionSystem.OnPromotionComplete` | `PromotionSystem.md` | `OnPromotionComplete` event | State trigger |
| Projected buff formula | `PromotionSystem.md` | `BuffPerPoint`, `MinTeacherBuff`, `MaxTeacherBuff` | Data dependency |
| Specialty → StatType mapping | `PromotionSystem.md` | `TraitToStatMap` | Data dependency |
| HUDs are read/listen only | `best-practices.md` | "HUDs are read/listen only" rule | Rule dependency |

---

## Acceptance Criteria

- [ ] All candidate cards built from `PromotionSystem.GetCandidates()` — never from StudentRoster directly
- [ ] Each card correctly displays all 7 stats, projected teacher buff, and trait specialty
- [ ] Card click calls `PromotionSystem.ToggleSelection()` — never writes selection state directly
- [ ] "Confirm" button calls `PromotionSystem.ConfirmPromotions()` — never writes to TeacherRoster or StudentRoster
- [ ] Promotion and discard animations play for the correct cards
- [ ] Scene does not transition before `OnPromotionComplete` fires and all animations finish
- [ ] No `FindObjectOfType` in `YearEndHUD.cs`

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should the player see current teacher roster counts during YearEnd (e.g., "You have 3 ATK teachers already")? | Designer | Before Alpha | Pending — recommend yes; informs strategic choices |
| Should promoted teachers appear in a visible "teacher panel" during the promotion animation, or just disappear? | Designer | Before Alpha | Pending — recommend visible panel; reinforces permanence |
| Should discarded students have any lasting memorial (e.g., "In Memory" log)? | Designer | Before Phase 3 | Pending — no for MVP |
| Is it possible to undo a promotion selection after Confirm is clicked? | Designer | Before Alpha | Pending — recommend no (irreversible = weight) |
