# RunManager

> **Status**: Draft
> **Last Updated**: 2026-06-29
> **Implements Pillar**: Challenging — the 3-year permadeath arc and the ordered phase sequence define the game's core tension

## Summary

RunManager drives the semester loop: Recruit → Train → Battle → YearEnd, repeated across 3 years. It owns the `RunPhase` enum and `CurrentYear` counter, fires phase and year events, triggers scene transitions via SceneLoader, and orchestrates the battle sequence by calling TraitSystem then AutoBattleResolver in the correct order. It is the only system permitted to transition `RunPhase`.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `GameManager, SceneLoader`

---

## Overview

RunManager is instantiated when `GameManager.StartNewRun()` is called and lives for the duration of one run. It does not persist between runs — a new RunManager is created for each run. It maintains `CurrentYear` (1–3) and `CurrentPhase` (RunPhase enum). Each phase transition fires `OnPhaseChanged`, and each year increment fires `OnYearChanged`. RunManager calls `SceneLoader.Instance.LoadScene()` at phase boundaries to switch scenes, then directly calls `TraitSystem.ResolveBattleBuffs()` and `AutoBattleResolver.Resolve()` at the start of the Battle phase. It subscribes to `AutoBattleResolver.OnBattleComplete` to receive the win/lose result, then either advances to YearEnd or calls `GameManager.Instance.EndRun(false, CurrentYear)`.

## Player Fantasy

The player feels a clear drumbeat of progression: each semester has a distinct mood (anticipation during Recruit, strategic pressure during Train, excitement during Battle, anguish during YearEnd promotion). The 3-year countdown creates a satisfying arc — the player always knows where they are in the run.

---

## Detailed Design

### RunPhase Enum

```
enum RunPhase {
    None,       // before a run starts
    Recruit,    // new students drawn; player reviews the incoming class
    Train,      // player allocates training sessions to students
    Battle,     // auto-battle simulation runs; no player input
    YearEnd     // player chooses which students to promote to teachers
}
```

### Core Rules

1. **RunManager is the only system that may transition `RunPhase`.** Other systems call `CompletePhase()` (or phase-specific completion methods); they never write `CurrentPhase` directly.
2. **Phase order is fixed:** Recruit → Train → Battle → YearEnd → (Year++ → Recruit, or Run End).
3. **Year increments only at YearEnd → Recruit transition.** No other transition changes `CurrentYear`.
4. **Battle phase ordering invariant:** RunManager calls `TraitSystem.ResolveBattleBuffs()` synchronously **before** calling `AutoBattleResolver.Resolve()`. These two calls must never be reordered or merged.
5. **Run ends in exactly two ways:** (a) team loses in Battle → `GameManager.EndRun(false, CurrentYear)`; (b) Year 3 YearEnd completes → `GameManager.EndRun(true, 3)`.
6. **RunManager fires `OnRunStarted` once** in its `Start()`, after all dependent systems (StudentRoster, TraitSystem) have initialized.
7. **RunManager is not a persistent singleton.** It is a MonoBehaviour on a GameObject spawned into the School scene when a run begins. It is destroyed when the School scene is unloaded.

### State Machine

```
(GameManager.StartNewRun called)
        ↓
   [None] → Start() → Year=1, Phase=Recruit
        ↓
   [Recruit]   ← OnPhaseChanged(Recruit) + LoadScene(School)
        ↓  CompleteRecruitPhase()
   [Train]     ← OnPhaseChanged(Train) [same scene, no load]
        ↓  CompleteTrainPhase()
   [Battle]    ← OnPhaseChanged(Battle) + LoadScene(Battle)
        ↓  TraitSystem.ResolveBattleBuffs()
        ↓  AutoBattleResolver.Resolve()
        ↓  ← OnBattleComplete(result)
        ├── result.Won = false → GameManager.EndRun(false, CurrentYear) → [Run ends]
        └── result.Won = true →
   [YearEnd]   ← OnPhaseChanged(YearEnd) + LoadScene(YearEnd)
        ↓  PromotionSystem.OnPromotionComplete fires
        ├── CurrentYear < 3 → CurrentYear++ → OnYearChanged → Phase=Recruit → LoadScene(School)
        └── CurrentYear == 3 → GameManager.EndRun(true, 3) → [Run ends]
```

### States and Transitions

| Phase | Entry Trigger | Exit Trigger | Scene Loaded | Key Behavior |
|---|---|---|---|---|
| `Recruit` | Run start, or previous YearEnd complete (year < 3) | `CompleteRecruitPhase()` called | School | StudentRoster draws new random students |
| `Train` | `CompleteRecruitPhase()` | `CompleteTrainPhase()` | School (no reload) | Player allocates training actions; TrainingSystem is active |
| `Battle` | `CompleteTrainPhase()` | `OnBattleComplete` received | Battle | TraitSystem resolves buffs, AutoBattleResolver simulates; no player input |
| `YearEnd` | Battle won | `OnPromotionComplete` received | YearEnd | PromotionSystem presents promotion choices; player selects teachers |

### Public API

```
// Called by GameManager at run start
void StartRun()

// Called by SchoolHUD / Recruit UI when player is ready to begin training
void CompleteRecruitPhase()

// Called by SchoolHUD / Train UI when player confirms all training is done
void CompleteTrainPhase()

// Not called externally — RunManager auto-advances after OnBattleComplete
// (internal, triggered by AutoBattleResolver event)

// Not called externally — RunManager auto-advances after OnPromotionComplete
// (internal, triggered by PromotionSystem event)
```

### Pause Support

RunManager exposes `PauseRun()` / `ResumeRun()`. When paused:
- `IsPaused = true` fires `OnPauseChanged(true)`
- `CompleteRecruitPhase()` and `CompleteTrainPhase()` are ignored (guarded by `IsPaused` check)
- Battle simulation pausing is delegated to `AutoBattleResolver.Pause()` / `.Resume()`
- `Time.timeScale` is **not** used — each system handles its own pause internally (avoids breaking unscaled timers)

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `GameManager` | RunManager reads `GameManager.Instance.TotalRunsStarted` at init; calls `GameManager.Instance.EndRun()` on run completion |
| `SceneLoader` | Calls `SceneLoader.Instance.LoadScene()` at Recruit (School), Battle (Battle), and YearEnd (YearEnd) phase entries |
| `StudentRoster` | Subscribes to `OnPhaseChanged`; draws students on Recruit, clears on run end |
| `TraitSystem` | RunManager calls `TraitSystem.ResolveBattleBuffs()` directly before AutoBattleResolver runs |
| `AutoBattleResolver` | RunManager calls `AutoBattleResolver.Resolve()` to start simulation; subscribes to `OnBattleComplete` |
| `PromotionSystem` | RunManager subscribes to `PromotionSystem.OnPromotionComplete` to advance to the next year or end the run |
| `EnemyDatabase` | Subscribes to `OnYearChanged` to scale enemy stats for the new year |
| `InputHandler` | Subscribes to `OnCancelPressed` to open pause/quit prompt during Train phase |

---

## Formulas

### Year Progression

```
isRunComplete = (CurrentYear == MaxYears) AND (yearEndComplete == true)
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `CurrentYear` | int | 1–3 | RunManager | Current semester number |
| `MaxYears` | int | 3 | Inspector (ScriptableObject) | Total semesters per run |

**Expected output**: Run ends after year 3 YearEnd completes (win) or when any battle is lost (lose).

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `CompleteRecruitPhase()` called during `Train` phase | Log error, no-op | Phases must be called in order |
| `CompleteTrainPhase()` called before `CompleteRecruitPhase()` | Log error, no-op | Same |
| `OnBattleComplete` fires twice | Only the first is processed; RunManager unsubscribes after the first result | AutoBattleResolver must not fire the event twice, but guard against it |
| `OnPromotionComplete` fires before entering `YearEnd` phase | Log warning, discard | Ordering violation; PromotionSystem should only fire during `YearEnd` |
| `StartRun()` called while a run is already active | Log error, no-op | GameManager should guard against this |
| Application quit during Train phase | `OnApplicationQuit` on GameManager triggers `EndRun(false, CurrentYear)`; TeacherRoster saves any teachers earned in prior years | Mid-run quit = run loss |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `GameManager` | This depends on it | State trigger — reads `TotalRunsStarted`; calls `EndRun()` |
| `SceneLoader` | This depends on it | State trigger — calls `LoadScene()` at phase boundaries |
| `TraitSystem` | This depends on it | Rule dependency — calls `ResolveBattleBuffs()` before battle |
| `AutoBattleResolver` | This depends on it | State trigger — calls `Resolve()`; subscribes to `OnBattleComplete` |
| `PromotionSystem` | It depends on this | Ownership handoff — subscribes to `OnPromotionComplete` |
| `EnemyDatabase` | It depends on this | State trigger — subscribes to `OnYearChanged` |
| `StudentRoster` | It depends on this | State trigger — subscribes to `OnPhaseChanged`, `OnRunStarted` |
| `SchoolHUD` | It depends on this | State trigger — subscribes to `OnPhaseChanged`, `OnYearChanged` |
| `BattleHUD` | It depends on this | State trigger — subscribes to `OnPhaseChanged` |

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `MaxYears` | 3 | 2–5 | Longer runs, more teacher accumulation per run | Shorter, more intense runs |
| `TrainingActionsPerSemester` | 5 | 3–8 | More student improvement per semester; reduces tension | Harder to fully develop students |

> `MaxYears` and `TrainingActionsPerSemester` are defined on a `RunConfig` ScriptableObject asset, not hardcoded.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Phase → Recruit | Scene transition (School); "New Semester" header shown by SchoolHUD | Music switches to school/management theme | MVP |
| Phase → Train | SchoolHUD shows training UI panel | Subtle UI transition sound | MVP |
| Phase → Battle | Scene transition (Battle); BattleHUD appears | Music switches to battle theme | MVP |
| Phase → YearEnd | Scene transition (YearEnd); YearEndHUD appears | Dramatic sting (win) or silence (if run ends in loss) | MVP |
| Year increments | Year counter updates in SchoolHUD | Year-start chime | MVP |
| Run ends (win) | Transition to RunEnd scene; win animation | Victory fanfare | MVP |
| Run ends (lose) | Transition to RunEnd scene; loss animation | Defeat sting | MVP |

---

## Game Feel

### Feel Reference

> "Should feel like Hades' run structure — each 'floor' (semester) has a distinct rhythm, the arc builds cleanly, and the run end (win or loss) lands with emotional weight. NOT a game where the semester transitions feel arbitrary or invisible."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| `CompleteRecruitPhase()` → Train phase begins | 1 frame + scene-less | 1 frame |
| `CompleteTrainPhase()` → Battle scene loaded | SceneLoader fade (~0.6s) | 36 frames (fade) |

### Animation Feel Targets

Not applicable to RunManager directly — scene transitions are SceneLoader's concern.

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Battle lost — run ends | Immediate (< 1 frame) | `OnRunEnded(false)` fires; BattleHUD plays defeat animation; scene transitions to RunEnd |
| Year 3 complete — run won | Immediate | `OnRunEnded(true)` fires; YearEndHUD plays victory animation |

### Weight and Responsiveness

- **Weight**: Each phase transition should feel deliberate and meaningful, not accidental
- **Player control**: Player explicitly confirms each phase exit (`CompleteRecruitPhase`, `CompleteTrainPhase`)
- **Snap quality**: Phase transitions are instant (state) + faded (scene); no ambiguous in-between state
- **Failure texture**: When the team loses, the defeat is immediate and legible — no confusion about what happened

### Feel Acceptance Criteria

- [ ] The current year and phase are always visible in the HUD
- [ ] A player pressing "Go to Battle" always transitions within 1 second (fade + load)
- [ ] A run loss triggers the defeat screen within 1 second of `OnBattleComplete(false)`
- [ ] The player is never stuck in a phase with no way to proceed

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| `CurrentYear` | SchoolHUD header (e.g. "Year 2 of 3") | On `OnYearChanged` | Always during run |
| `CurrentPhase` | SchoolHUD (active panel) | On `OnPhaseChanged` | Always during run |
| `IsPaused` | Pause overlay (SchoolHUD) | On `OnPauseChanged` | During Train phase only |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| TraitSystem must resolve before AutoBattleResolver | `TraitSystem.md`, `AutoBattleResolver.md` | `ResolveBattleBuffs()` ordering | Rule dependency |
| Best-practices ordering invariant | `best-practices.md` | "TraitSystem resolves before AutoBattleResolver" rule | Rule dependency |
| SceneLoader scene names | `SceneLoader.md` | `SceneName` enum values (School, Battle, YearEnd) | Data dependency |
| RunConfig tuning knobs | `EnemyDatabase.md` | `MaxYears`, `TrainingActionsPerSemester` | Data dependency |

---

## Acceptance Criteria

- [ ] A full 3-year run completes end-to-end: Recruit → Train → Battle (win) → YearEnd → (×3) → RunEnd screen
- [ ] A battle loss immediately ends the run: `GameManager.EndRun(false, year)` is called within 1 frame of `OnBattleComplete(won: false)`
- [ ] `OnPhaseChanged` fires exactly once per phase transition with the correct new phase
- [ ] `OnYearChanged` fires exactly once per year increment with the correct year number
- [ ] `TraitSystem.ResolveBattleBuffs()` is always called before `AutoBattleResolver.Resolve()` — verified by unit test
- [ ] No system other than RunManager writes `CurrentPhase`
- [ ] `MaxYears` and `TrainingActionsPerSemester` are read from a `RunConfig` ScriptableObject — no hardcoded values
- [ ] Pausing during Train phase prevents `CompleteTrainPhase()` from advancing

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should `MaxYears = 3` be configurable in a `RunConfig` ScriptableObject, or is 3 a hard constant? | Designer | Before code-system | Pending — recommend ScriptableObject for tuning flexibility |
| How many training actions does the player get per semester? (5 is the default above) | Designer | Before code-system | Pending |
| Is there a visual countdown between Train and Battle (e.g. "Combat starts in 3…2…1…") or is it immediate on player confirm? | Designer | Before code-system | Pending — recommend immediate for snappiness |
| Can the player skip the YearEnd animation and confirm promotions immediately, or is there a minimum wait? | Designer | Before code-system | Pending |
