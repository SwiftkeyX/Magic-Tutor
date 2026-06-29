# Architecture Contract

> The authoritative record of how scripts in this project communicate. Claude reads this before touching any existing script.
>
> For system responsibilities, dependencies, and tier assignments, see `systems-design.md`.
> For coding conventions and anti-patterns, see `best-practices.md`.

## Script Registry

Every system from `systems-design.md` mapped to its script and responsibility.

| System | Script | Responsibility | Pattern |
|---|---|---|---|
| GameManager | `GameManager.cs` | Singleton; owns `GameState` enum and active run snapshot | Singleton |
| SceneLoader | `SceneLoader.cs` | All scene loads/unloads; no scene changes happen outside this class | Singleton |
| InputHandler | `InputHandler.cs` | Wraps Unity Input System; exposes typed input events to consumers | Component |
| SaveSystem | `SaveSystem.cs` | JSON serialization/deserialization of persistent data to disk | Singleton |
| RunManager | `RunManager.cs` | Drives semester loop (recruit→train→battle→year-end, 3 years); owns `RunPhase` enum | Singleton |
| StudentRoster | `StudentRoster.cs` | Active student list; random recruitment draw, stat storage, removal | Component |
| TrainingSystem | `TrainingSystem.cs` | Allocates training sessions to students; applies stat deltas | Component |
| TraitSystem | `TraitSystem.cs` | Assigns traits to students; tracks team trait counts; activates threshold abilities | Component |
| AutoBattleResolver | `AutoBattleResolver.cs` | Simulates auto-battle round-by-round; produces `BattleResult` | Component |
| EnemyDatabase | `EnemyDatabase.cs` | ScriptableObject asset; defines enemy types and per-year stat scaling | ScriptableObject |
| PromotionSystem | `PromotionSystem.cs` | Year-end flow; accepts player promotion choices; writes to TeacherRoster, clears StudentRoster | Component |
| TeacherRoster | `TeacherRoster.cs` | Persistent teacher collection; computes aggregate buff profile; saves on change | Singleton |
| AudioSystem | `AudioSystem.cs` | AudioSource pool; plays music and SFX by clip reference | Singleton |
| MainMenuController | `MainMenuController.cs` | Main menu screen logic (new run, settings, quit buttons) | Component |
| SchoolHUD | `SchoolHUD.cs` | School management phase UI; reads from StudentRoster, TrainingSystem, TraitSystem via events | Component |
| BattleHUD | `BattleHUD.cs` | Battle view UI; listens to AutoBattleResolver events to animate and display outcome | Component |
| YearEndHUD | `YearEndHUD.cs` | Promotion/discard screen UI; presents PromotionSystem choices to the player | Component |
| MetaProgressionHUD | `MetaProgressionHUD.cs` | Teacher collection display; reads TeacherRoster on open | Component |

---

## Communication Patterns

**Rule:** only the communication methods listed below are permitted. No ad-hoc `FindObjectOfType`, `Find`, or `GetComponent` chains outside of `Awake` initialization.

### C# Events — loose coupling (broadcast / listen)

Game systems fire events; listeners react. Systems never hold references to their listeners.

| Broadcaster | Event | Payload | Listeners |
|---|---|---|---|
| `RunManager` | `OnRunStarted` | — | StudentRoster, SchoolHUD |
| `RunManager` | `OnPhaseChanged` | `RunPhase phase` | StudentRoster, SchoolHUD, BattleHUD, YearEndHUD |
| `RunManager` | `OnYearChanged` | `int year` | EnemyDatabase (scales stats), SchoolHUD |
| `RunManager` | `OnRunEnded` | `bool won` | GameManager, MetaProgressionHUD |
| `StudentRoster` | `OnRosterChanged` | — | TraitSystem, SchoolHUD |
| `StudentRoster` | `OnStudentStatChanged` | `StudentData student` | SchoolHUD |
| `TraitSystem` | `OnTraitThresholdReached` | `TraitType, int tier` | SchoolHUD (display), AutoBattleResolver (apply buff) |
| `AutoBattleResolver` | `OnBattleComplete` | `BattleResult result` | RunManager (advance phase), BattleHUD |
| `PromotionSystem` | `OnPromotionComplete` | `List<StudentData> promoted` | RunManager (advance year), MetaProgressionHUD |
| `TeacherRoster` | `OnTeacherAdded` | `TeacherData teacher` | MetaProgressionHUD |

### Direct References — tight coupling (explicitly allowed)

| Caller | Callee | Why direct |
|---|---|---|
| `RunManager` | `GameManager.Instance` | Reads/writes game state — orchestrator-level coupling is intentional |
| `RunManager` | `SceneLoader` | Triggers scene transitions at phase boundaries |
| `TrainingSystem` | `StudentRoster` | Writes stat deltas directly — same bounded context |
| `AutoBattleResolver` | `TraitSystem` | Reads active trait buffs synchronously during simulation tick |
| `AutoBattleResolver` | `EnemyDatabase` | Reads enemy definitions — pure data dependency |
| `PromotionSystem` | `StudentRoster` | Removes promoted/discarded students |
| `PromotionSystem` | `TeacherRoster` | Creates teacher records from promoted students |
| `TeacherRoster` | `SaveSystem` | Persists data on every teacher change |
| `AudioSystem` | `GameManager.Instance` | Reads game state to select correct music track |

### Forbidden

- **HUDs → game logic writes:** HUD scripts may read public data from systems and listen to events, but must never call methods that mutate game state.
- **Game systems → HUDs:** No game system holds a reference to any HUD script.
- **`FindObjectOfType` / `Find`:** Banned project-wide. Use singleton `.Instance` or serialized Inspector references assigned at edit-time.
- **`DontDestroyOnLoad`:** Banned. Persistent objects live in the `Bootstrap` scene instead.
- **`SceneManager.LoadScene` (direct):** Banned. All scene transitions route through `SceneLoader`.

---

## Singleton Initialization Order

Singletons live in the `Bootstrap` scene and persist for the application lifetime. Load order is enforced by script execution order in Project Settings.

1. `GameManager` — first; everything reads from it
2. `SaveSystem` — second; TeacherRoster reads from it on Awake
3. `TeacherRoster` — third; loads persistent teacher data
4. `SceneLoader` — fourth; can now trigger the first scene transition
5. `AudioSystem` — fifth; begins playing menu music after SceneLoader is ready
6. `RunManager` — spawned when a run begins, not at startup
