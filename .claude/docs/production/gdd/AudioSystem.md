# AudioSystem

> **Status**: Draft
> **Last Updated**: 2026-06-29
> **Implements Pillar**: Tactile School Fantasy — audio reinforces every meaningful moment in the school loop without interrupting focus

## Summary

AudioSystem is the singleton audio manager that handles all music and SFX playback for the game. It owns an AudioSource pool for overlapping sound effects and a dedicated music AudioSource that cross-fades tracks when the game state changes. It exists to give every game event — student recruited, trait unlocked, battle resolved — an audio response without any other system needing to know about Unity's audio API.

> **Quick reference** — Layer: `Presentation` · Priority: `Alpha` · Key deps: `GameManager`

---

## Overview

AudioSystem is a persistent singleton that lives in the Bootstrap scene. It listens to C# events from game systems (RunManager, TraitSystem, AutoBattleResolver, PromotionSystem) and reacts by playing the appropriate clip from its clip library ScriptableObject. Music selection is driven by `GameManager.GameState` and, within a run, by `RunPhase`; when either changes, AudioSystem cross-fades from the current track to the next. SFX are played by pulling an idle AudioSource from the pool, assigning the clip, and returning it automatically when the clip finishes.

## Player Fantasy

The player should feel immersed in the rhythms of school life — calm, studious music during training, tension building into battle, triumphant or somber resolution. Every UI action should feel satisfying and crisp. The audio layer should feel invisible (never intrusive) but its absence should be noticed immediately.

---

## Detailed Design

### Core Rules

1. One dedicated `AudioSource` (the music channel) plays looping background music at all times while the game is in focus; it is never used for SFX.
2. A pool of `SfxPoolSize` AudioSources (default 12) handles all SFX. When a clip is requested, the first idle (not playing) source is used. If all sources are busy, the oldest playing source is interrupted.
3. Music tracks are selected by state priority: `GameState.MainMenu` → menu track; `GameState.RunEnd (won)` → victory track; `GameState.RunEnd (lost)` → defeat track; `GameState.InRun` → track determined by current `RunPhase` (Training, Battle, YearEnd map to their respective tracks).
4. Music transitions use a linear cross-fade over `MusicFadeDuration` seconds — the outgoing track fades out while the incoming track fades in simultaneously.
5. `AudioSystem` subscribes to events in `OnEnable` and unsubscribes in `OnDisable`. It never holds references to event-broadcasting game systems (subscribe via the static event, not via an instance reference).
6. All clip assignments are made through a single `AudioClipLibrary` ScriptableObject serialized in the Inspector — no clip references are hardcoded in script.
7. Master volume, music volume, and SFX volume are each a separate float (0–1) exposed in the Inspector and read at playback time (not baked in at clip load).
8. If a requested SFX clip is `null`, log a warning and return silently — never throw or crash.

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| `Idle` | AudioSystem initializes before GameManager fires first state | Any music track assigned | Silence; pool initialized but empty |
| `PlayingMusic` | A valid music track is selected for current game state | Game state changes to a different track | Loop current music track at music volume |
| `CrossFading` | Game state changes while music is playing | Fade completes (`MusicFadeDuration` elapsed) | Outgoing track fades to 0, incoming track fades to 1; reverts to `PlayingMusic` |

### Interactions with Other Systems

| System | Interaction |
|---|---|
| `GameManager` | AudioSystem reads `GameManager.Instance.CurrentState` on each state change to select the correct music track; listens to `GameManager.OnStateChanged` event |
| `RunManager` | Subscribes to `OnPhaseChanged` to switch music between Training, Battle, and YearEnd tracks; subscribes to `OnRunEnded` to play victory or defeat track |
| `TraitSystem` | Subscribes to `OnTraitThresholdReached` to play trait-unlock SFX |
| `AutoBattleResolver` | Subscribes to `OnBattleComplete` to play battle-result sting (win or lose SFX) |
| `PromotionSystem` | Subscribes to `OnPromotionComplete` to play promotion fanfare SFX |
| `StudentRoster` | Subscribes to `OnRosterChanged` to play student-recruit SFX when a new student is added |

---

## Formulas

### Cross-Fade Volume Ramp

```
outgoingVolume = musicVolume * (1 - t)
incomingVolume = musicVolume * t
t = elapsedTime / MusicFadeDuration   (clamped 0–1)
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `musicVolume` | float | 0–1 | Inspector | Master music volume setting |
| `t` | float | 0–1 | Calculated each frame | Linear interpolation progress |
| `MusicFadeDuration` | float | 0.1–3.0 | Inspector / ScriptableObject | Seconds to complete the cross-fade |

**Expected output range**: both volumes 0–1, always summing to `musicVolume` at any given `t`.
**Edge cases**: if `MusicFadeDuration` is 0, snap to new track instantly (no coroutine).

### SFX Pool Claim

```
source = firstIdle ?? oldestPlaying
```

No numeric formula — selection is index-scan order over the pool array; oldest-playing is determined by `AudioSource.time` (time since clip start).

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Music track for a state is null in the clip library | Stop music (silence) — do not loop previous track | Null track is intentional silence; crashing or looping stale audio is worse |
| SFX pool exhausted (all 12 sources busy) | Interrupt source with smallest `remainingTime`; play new clip on it | Prevents SFX backlog; less-important nearly-finished clips yield to new events |
| `PlayClip` called during cross-fade | Queue has no queue — play immediately on an available pool source | Cross-fade only affects the music channel; SFX pool is independent |
| AudioSystem Awake fires before GameManager | Read state lazily on `Start` (not `Awake`); `Start` runs after all `Awake` calls | Singleton initialization order: GameManager is #1, AudioSystem is #5 |
| Scene reload (never happens — Bootstrap persists) | N/A — Bootstrap scene is never unloaded | AudioSystem persists for application lifetime per architecture contract |
| Application loses focus (mobile/background) | Unity's `OnApplicationPause` — reduce music volume to 0, restore on resume | Prevents audio bleed when switching apps |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `GameManager` | This depends on it | State trigger — reads `CurrentState` to select music track |
| `RunManager` | It triggers this | State trigger — `OnPhaseChanged` / `OnRunEnded` events drive music switches |
| `TraitSystem` | It triggers this | State trigger — `OnTraitThresholdReached` fires SFX |
| `AutoBattleResolver` | It triggers this | State trigger — `OnBattleComplete` fires outcome SFX |
| `PromotionSystem` | It triggers this | State trigger — `OnPromotionComplete` fires fanfare SFX |
| `StudentRoster` | It triggers this | State trigger — `OnRosterChanged` fires recruit SFX |

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `SfxPoolSize` | 12 | 4–24 | More simultaneous sounds, higher memory | Fewer simultaneous sounds, clips interrupt more |
| `MusicFadeDuration` | 1.0s | 0–3.0s | Smoother transitions, longer blending | Snappier transitions, potential pop |
| `MusicVolume` | 0.7 | 0–1 | Louder music | Quieter music |
| `SfxVolume` | 1.0 | 0–1 | Louder SFX | Quieter SFX |
| `OldestInterruptThreshold` | 0.15s remaining | 0–0.5s | More sources eligible for interrupt | Only very-end clips eligible |

All parameters serialized on `AudioClipLibrary` ScriptableObject or directly on the `AudioSystem` component in the Bootstrap scene.

---

## Visual / Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|---|---|---|---|
| Game boots → Main Menu | — | Menu music begins (fade in) | MVP |
| New run starts | — | Training music fades in | MVP |
| Phase changes to Battle | — | Battle music cross-fades in | MVP |
| Phase changes to YearEnd | — | Year-end music cross-fades in | MVP |
| Run won | — | Victory sting, then victory music | MVP |
| Run lost | — | Defeat sting, then defeat music | MVP |
| Student recruited | Brief roster flash (SchoolHUD) | Recruit chime SFX | Alpha |
| Trait threshold reached | Trait icon pulse (SchoolHUD) | Trait-unlock SFX | Alpha |
| Battle hit (per round) | Unit flash (BattleHUD) | Hit impact SFX | Alpha |
| Promotion confirmed | Card-flip animation (YearEndHUD) | Promotion fanfare SFX | Alpha |
| Student discarded | Card-discard animation (YearEndHUD) | Discard whoosh SFX | Alpha |
| UI button click | — | Click SFX | Alpha |

---

## Game Feel

### Feel Reference

> "Should feel like *Hades*'s contextual music system — music shifts in real time to match gameplay stakes and rewards moments of intensity with audio punctuation. NOT ambient wallpaper that ignores what's happening on screen."

### Input Responsiveness

| Action | Max Input-to-Response Latency | Frame Budget (60fps) |
|---|---|---|
| SFX triggered by event | 50ms | 3 frames |
| Music track switch (cross-fade start) | 100ms | 6 frames |

### Animation Feel Targets

N/A — AudioSystem has no animations. SFX durations are clip-dependent; keep UI SFX under 300ms to feel responsive.

### Impact Moments

| Impact Type | Duration | Effect |
|---|---|---|
| Battle hit SFX | 150–300ms | Short, punchy — supports BattleHUD hit animation |
| Trait-unlock SFX | 400–600ms | Rising tone — celebrates a threshold moment |
| Promotion fanfare | 800–1200ms | Triumphant sting — marks year-end achievement |

### Weight and Responsiveness

- **Weight**: Music is atmospheric and weighty; SFX are crisp and immediate.
- **Player control**: SFX are fire-and-forget; player cannot interrupt them.
- **Snap quality**: SFX snap; music transitions are smooth (cross-fade).
- **Failure texture**: Defeat music should feel somber, not punishing — the roguelike loop rewards trying again.

### Feel Acceptance Criteria

- [ ] Music transitions complete without audible pop or click
- [ ] No two identical SFX clips overlap in a way that sounds like a doubled echo (pool handles this by not sharing the same source)
- [ ] Playtesters describe the audio as "fitting" without being asked

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|---|---|---|---|
| Volume sliders (music, SFX) | Settings screen (MainMenuController) | On slider change | Settings menu open |

AudioSystem exposes `SetMusicVolume(float)` and `SetSfxVolume(float)` public methods for the settings UI to call. These are the only public mutators.

---

## Cross-References

| This Doc References | Target Doc | Element Referenced | Nature |
|---|---|---|---|
| GameState enum | `GameManager.md` | `GameState` (MainMenu, InRun, RunEnd) | State trigger |
| RunPhase enum | `RunManager.md` | `RunPhase` (Training, Battle, YearEnd) | State trigger |
| `OnPhaseChanged` event | `RunManager.md` | `OnPhaseChanged(RunPhase)` | State trigger |
| `OnRunEnded` event | `RunManager.md` | `OnRunEnded(bool won)` | State trigger |
| `OnRosterChanged` event | `StudentRoster.md` | `OnRosterChanged` | State trigger |
| `OnTraitThresholdReached` event | `TraitSystem.md` | `OnTraitThresholdReached(TraitType, int tier)` | State trigger |
| `OnBattleComplete` event | `AutoBattleResolver.md` | `OnBattleComplete(BattleResult)` | State trigger |
| `OnPromotionComplete` event | `PromotionSystem.md` | `OnPromotionComplete(List<StudentData>)` | State trigger |
| Bootstrap scene singleton pattern | `architecture.md` | Singleton initialization order #5 | Rule dependency |

---

## Acceptance Criteria

- [ ] Music plays on boot and matches the current `GameState` without manual trigger
- [ ] Music cross-fades correctly when `GameState` or `RunPhase` changes — no pop, no silence gap longer than `MusicFadeDuration`
- [ ] All SFX events listed in Visual/Audio Requirements fire the correct clip
- [ ] SFX pool handles 12 simultaneous clips without exceptions; pool-full case interrupts correctly
- [ ] `AudioSystem.PlayClip(null)` logs a warning and returns — no NullReferenceException
- [ ] Volume settings (music / SFX) applied in real time when sliders change
- [ ] No `FindObjectOfType` or `DontDestroyOnLoad` anywhere in `AudioSystem.cs`
- [ ] All clip references live in `AudioClipLibrary` ScriptableObject — zero hardcoded strings or direct clip fields on the MonoBehaviour
- [ ] Performance: pool scan completes in under 1ms per SFX call (profiler verified)
- [ ] No hardcoded values in implementation — all tuning knobs exposed via Inspector

---

## Open Questions

| Question | Owner | Deadline | Resolution |
|---|---|---|---|
| Should music volume duck (lower) while SFX play, or run at full volume always? | Designer | Before Alpha | Pending |
| Do we need per-system SFX volume categories (e.g. UI vs gameplay vs music)? | Designer | Before Alpha | Pending |
| Should `AudioClipLibrary` be one ScriptableObject or split per system (UI clips, battle clips, etc.)? | Programmer | Before implementation | Pending — single library preferred for simplicity unless count exceeds 30 clips |
| What happens to audio when the Settings screen is opened mid-run — pause music? | Designer | Before Alpha | Pending |
