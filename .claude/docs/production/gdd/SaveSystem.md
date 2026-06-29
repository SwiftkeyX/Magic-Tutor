# SaveSystem

> **Status**: Draft
> **Last Updated**: 2026-06-29
> **Implements Pillar**: Empowering — the teacher meta-progression ratchet only works if saves are reliable; losing teachers to a crash would shatter trust

## Summary

SaveSystem owns serialization and deserialization of all persistent cross-run data to disk. It saves exactly one file: `save.json`, containing the teacher roster and run statistics. Mid-run state (students, current year, run phase) is intentionally not saved — runs are either completed or abandoned fresh. SaveSystem exposes `Save()` and `Load()` and is always called synchronously per the project best-practices rule.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `None`

---

## Overview

SaveSystem is a singleton in the Bootstrap scene. It uses `UnityEngine.JsonUtility` to serialize a `SaveData` object to `Application.persistentDataPath/save.json`. Every time TeacherRoster mutates, it calls `SaveSystem.Instance.Save()` immediately and synchronously — there is no deferred or batched saving. On startup, `Load()` is called during Awake (before any other system reads teacher data) and the loaded `SaveData` is cached in memory. A backup file (`save.bak`) is maintained: on every successful save, the previous `save.json` is copied to `save.bak` before overwriting, enabling one-step recovery from corruption.

## Player Fantasy

The player never worries about losing their teachers. They can quit anytime — their teacher roster is always exactly as they left it. A crash never resets months of meta-progress.

---

## Detailed Design

### Save Data Structure

```
SaveData
├── List<TeacherSaveData> Teachers
├── int TotalRunsStarted
├── int TotalRunsCompleted
└── string SaveVersion        // for future migration

TeacherSaveData
├── string Name
├── List<string> Traits       // serialized as strings (TraitType.ToString())
├── int BuffValue             // the flat stat bonus this teacher provides
├── int YearGraduated         // 1–3
└── int RunGraduated          // which run number this teacher came from
```

> `JsonUtility` requires all serialized types to be plain C# classes with `[Serializable]` attribute. No Unity objects, no generics at the top level.

### Core Rules

1. `Load()` must be called in `Awake`, before `TeacherRoster.Awake` reads teacher data. Script execution order in Project Settings must enforce this.
2. `Save()` is always synchronous — no coroutines, no `async/await`, no deferred invocations.
3. Before overwriting `save.json`, copy the existing file to `save.bak` (one backup generation only).
4. If `Load()` finds no file, return a default `SaveData` (empty teacher list, all counters at zero). Never throw.
5. If `Load()` finds a file but deserialization fails (corrupt JSON), log an error, attempt to load `save.bak`, and if that also fails, return default `SaveData`. Never throw.
6. `SaveVersion` is checked on load. If the version is unrecognized, log a warning but attempt to load anyway (forward-compatible parsing via JsonUtility's lenient field matching).
7. SaveSystem never knows about the game rules — it only reads and writes the `SaveData` struct. TeacherRoster owns the interpretation of that data.

### Save File Location

```
Application.persistentDataPath/MagicTutor/save.json
Application.persistentDataPath/MagicTutor/save.bak
```

`Application.persistentDataPath` on Windows resolves to `%APPDATA%/../LocalLow/<CompanyName>/<ProductName>`.

### States and Transitions

SaveSystem is stateless — it has no internal state machine. `Load()` and `Save()` are pure I/O operations that execute and return.

| Method | Precondition | Postcondition |
|---|---|---|
| `Load()` | File may or may not exist | Returns a valid `SaveData` (never null) |
| `Save(SaveData data)` | Called with current roster data | `save.json` written; `save.bak` updated |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `TeacherRoster` | Calls `SaveSystem.Instance.Save(rosterData)` after every teacher mutation |
| `GameManager` | Reads `TotalRunsStarted` / `TotalRunsCompleted` from the loaded `SaveData` on startup via TeacherRoster |
| No other systems | SaveSystem has no other callers |

---

## Formulas

No formulas. SaveSystem performs no calculations.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| `save.json` does not exist (first launch) | `Load()` returns default `SaveData` | New player; no error |
| `save.json` is empty or corrupt | Log error, try `save.bak`; if also corrupt, return default | One backup generation prevents total data loss from a partial write |
| `Save()` called during `Load()` | Not possible — both are synchronous, single-threaded; no guard needed | Unity main thread only |
| Disk full during `Save()` | Log error; `save.json` may be partially written; `save.bak` still has the previous valid state | On next load, fallback to `.bak` |
| `Application.persistentDataPath` subdirectory does not exist | Create it (`Directory.CreateDirectory`) in `Awake` before first `Load()` | First launch on a clean system |
| `SaveVersion` mismatch | Log warning; attempt load; unknown fields are ignored by JsonUtility | Forward-compatible |
| Application quit mid-`Save()` | OS may leave a partial file; `save.bak` is the recovery path | Acceptable risk — backup covers this |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| None | — | SaveSystem has no system dependencies (Tier 1 foundation) |
| `TeacherRoster` | It depends on this | Data dependency — calls `Save()` on every teacher mutation, calls `Load()` on startup |

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `SaveVersion` | `"1.0"` | N/A (string) | Increment when save format changes | N/A |
| Subdirectory name | `"MagicTutor"` | Any valid directory name | N/A | N/A |

> No runtime-tunable knobs — SaveSystem is pure infrastructure.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Save completed | None — saving is invisible to the player | None | MVP |
| Load failure (corrupt save) | Optional: show a one-time warning toast on startup | None | Alpha |

---

## Game Feel

### Feel Reference

> "Should feel like Hades — the player never thinks about saving because it just always works. NOT a game that shows a 'Saving...' spinner every time a teacher is added."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| `Save()` call → file written | < 5ms on typical PC SSD | < 1 frame (synchronous) |

### Animation Feel Targets

Not applicable — saving is invisible.

### Impact Moments

Not applicable.

### Weight and Responsiveness

- **Weight**: Zero — saving should be imperceptible
- **Player control**: Automatic — player never triggers a save manually
- **Snap quality**: Binary — either saved or not; no partial states visible to the player
- **Failure texture**: On load failure, a warning is shown once; the player starts fresh with no teachers (worse-case but not catastrophic)

### Feel Acceptance Criteria

- [ ] Player never sees a "Saving..." indicator or frame hitch during normal gameplay
- [ ] Teachers are present after quitting and re-launching the game
- [ ] A corrupt `save.json` does not crash the game on next launch

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Save corruption warning | Startup screen / MainMenu | Once per launch if triggered | Only when both `save.json` and `save.bak` fail to load |

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| TeacherRoster calls Save on every mutation | `TeacherRoster.md` | Save-on-mutation rule | Data dependency |
| Best-practices: TeacherRoster saves immediately on every mutation | `best-practices.md` | "TeacherRoster saves immediately" rule | Rule dependency |

---

## Acceptance Criteria

- [ ] `SaveSystem.Instance.Load()` returns a non-null `SaveData` in all cases (no file, corrupt file, valid file)
- [ ] Quitting and relaunching the game restores the exact teacher roster that was present before quit
- [ ] A manually corrupted `save.json` causes fallback to `save.bak`, not a crash
- [ ] `save.bak` is updated on every successful `Save()` call
- [ ] The save subdirectory is created automatically if it does not exist
- [ ] `Save()` completes in under 5ms on a standard PC SSD (verified in Play Mode profiler)
- [ ] No `async/await`, `Coroutine`, or deferred invocation in the save path
- [ ] `SaveVersion` is written into every save file

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should settings (volume, display prefs) be stored in `save.json` or a separate `settings.json`? | Engineer | Before code-system | Pending — recommend separate file so a save reset doesn't wipe settings |
| Should there be a "Delete Save" option in the settings/main menu for testing/QA purposes? | Designer | Before code-system | Pending — strongly recommended for QA; not visible to players |
| Is `JsonUtility` sufficient, or should we use `Newtonsoft.Json` for better null handling and migration support? | Engineer | Before code-system | Pending — recommend `JsonUtility` for v1 (no extra dependency); migrate to Newtonsoft if save versioning becomes complex |
