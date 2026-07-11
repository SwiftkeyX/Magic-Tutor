# Best Practices

<!-- Two sections: project-critical patterns (fill in), then Unity 6 current patterns (pre-filled, factual). -->
<!-- Agents read this before writing any code. Project-critical patterns override everything else. -->

---

## Project-Critical Patterns

> These are hard constraints for **Magic Tutor**. They override all other guidance. Every rule below was derived from the architecture contract in `architecture.md`.

---

### No runtime object lookup

**Rule**: Never call `FindObjectOfType`, `FindObjectsOfType`, `GameObject.Find`, or `GameObject.FindWithTag` at runtime.

**Why**: These are O(n) searches over all scene objects and produce invisible coupling. They are banned by the architecture contract.

**Instead**: Use singleton `.Instance` for global systems (GameManager, RunManager, etc.) or assign references via the Inspector in `Awake`.

---

### HUDs are read/listen only

**Rule**: HUD scripts (`SchoolHUD`, `BattleHUD`, `YearEndHUD`, `MetaProgressionHUD`) may never call methods that mutate game state.

**Why**: UI writing to game logic reverses the dependency direction, making game systems unpredictable and untestable.

**Instead**: HUDs subscribe to C# events and read public properties. Player input from HUDs fires an event or invokes a dedicated command method on the owning game system — never directly mutating data.

---

### Game systems never reference HUDs

**Rule**: No game system script (`RunManager`, `StudentRoster`, `TraitSystem`, etc.) may hold a reference to any HUD script.

**Why**: Game logic must be testable without the UI layer. A reference in the wrong direction forces the HUD to exist in tests and makes the logic scene-dependent.

**Instead**: Game systems broadcast C# events. HUDs subscribe.

---

### All scene transitions via SceneLoader

**Rule**: Never call `SceneManager.LoadScene`, `SceneManager.LoadSceneAsync`, or `SceneManager.UnloadSceneAsync` directly.

**Why**: Direct calls bypass transition guards, loading screens, and game state synchronization logic that `SceneLoader` enforces.

**Instead**: Always call `SceneLoader.Instance.LoadScene(sceneName)` (or the appropriate `SceneLoader` method).

---

### No DontDestroyOnLoad

**Rule**: Never call `DontDestroyOnLoad()` on any GameObject.

**Why**: `DontDestroyOnLoad` creates hidden scene state that is invisible in the hierarchy and impossible to reason about across scene loads.

**Instead**: Persistent singletons (GameManager, SaveSystem, TeacherRoster, AudioSystem, SceneLoader) live in the `Bootstrap` scene which is never unloaded.

---

### RunManager is the only phase gatekeeper

**Rule**: Only `RunManager` may read or write `RunPhase`. No other system may transition the run phase.

**Why**: Phase transitions control the semester loop. Multiple writers create race conditions and undefined ordering.

**Instead**: Other systems request transitions by invoking a `RunManager` method (e.g. `RunManager.Instance.CompletePhase()`) — never by setting phase state directly.

---

### AutoBattleResolver only runs during Battle phase

**Rule**: `AutoBattleResolver.Resolve()` (or equivalent entry point) must only be called when `RunManager.CurrentPhase == RunPhase.Battle`.

**Why**: Running battle simulation outside the Battle phase corrupts run state (student stats applied twice, trait buffs double-counted, etc.).

**Instead**: Guard the call: `if (RunManager.Instance.CurrentPhase != RunPhase.Battle) return;`

---

### TeacherRoster saves immediately on every mutation

**Rule**: Every method on `TeacherRoster` that adds, removes, or modifies a teacher must call `SaveSystem.Instance.Save()` before returning.

**Why**: Teacher data is the only cross-run persistent state. Losing it on crash or unexpected quit destroys player progress.

**Instead**: Call save synchronously inside the mutating method — not after, not in a coroutine, not deferred.

---

### Student data is ephemeral — clear on run end

**Rule**: `StudentRoster` must be fully cleared when a run ends (win or lose). No student data may leak into the next run.

**Why**: Students are intentionally reset each run. Leakage creates inconsistent rosters and breaks the roguelike contract.

**Instead**: `RunManager.OnRunEnded` triggers `StudentRoster.Clear()` before any new-run initialization begins.

---

### TraitSystem resolves before AutoBattleResolver runs

**Rule**: All trait threshold checks and buff applications must complete before `AutoBattleResolver.Resolve()` is called in any given battle.

**Why**: The battle simulation reads active trait buffs during each tick. Unresolved traits produce incorrect damage and healing values.

**Instead**: `RunManager` calls `TraitSystem.ResolveBattleBuffs()` explicitly before calling `AutoBattleResolver.Resolve()`. Never merge these into a single async flow.

---

## Review Heuristics

> Unlike Project-Critical Patterns above, these are **soft signals**, not hard constraints. They don't block anything on their own — they flag candidates for closer SRP review during `technical-director` / `architecture-reviewer` passes.

---

### Class length as an SRP signal

**Guideline**: A class exceeding ~400 lines is not automatically a violation, but is a trigger to re-check whether it still has a single responsibility.

**Why**: Line count alone is a blunt proxy — a cohesive class doing one complex job (e.g. a tick-engine) can legitimately run long, while a much shorter class can still mix unrelated concerns. The real signal is whether the class owns logic that another established pattern already owns (e.g. `AutoBattleResolver.cs` hardcoding trait-ability logic that `Skills/SkillArchetypeExecutor` already owns).

**Instead**: When a class crosses ~400 lines, check whether every method belongs to the class's stated responsibility and whether any logic duplicates or bypasses an existing pattern elsewhere in the codebase — then decide whether to split it.

---

## Unity 6 LTS — Current Patterns

**Last verified:** 2026-05-30

> These patterns differ from older Unity versions that may appear in LLM training data.
> Follow these. Do not revert to the legacy column.

### Input

| Use This | Not This | Why |
|---|---|---|
| `UnityEngine.InputSystem` package | `Input.GetKey()` / `Input.GetAxis()` | Rebindable, cross-platform, event-driven |

```csharp
// ✅ Correct
controls.Gameplay.Jump.performed += ctx => Jump();

// ❌ Legacy
if (Input.GetKeyDown(KeyCode.Space)) Jump();
```

---

### UI

| Use This | Not This | Why |
|---|---|---|
| UI Toolkit (`.uxml` + `.uss`) | UGUI Canvas + `Text`/`Image` components | Production-ready in Unity 6, HTML/CSS workflow |

```csharp
// ✅ Correct
var root = GetComponent<UIDocument>().rootVisualElement;
root.Q<Button>("play-button").clicked += StartGame;

// ❌ Legacy
GetComponent<Button>().onClick.AddListener(StartGame);
```

---

### Asset Loading

| Use This | Not This | Why |
|---|---|---|
| `Addressables` | `Resources.Load` | Async, memory-efficient, supports remote delivery |

```csharp
// ✅ Correct
var handle = Addressables.InstantiateAsync(enemyKey);
var enemy = await handle.Task;
Addressables.ReleaseInstance(enemy);

// ❌ Legacy
var enemy = Resources.Load<GameObject>("Enemies/Basic");
```

---

### Tunable Data

| Use This | Not This | Why |
|---|---|---|
| `ScriptableObject` assets | Hardcoded values or config files | Inspector-editable, designer-friendly, no recompile |

```csharp
// ✅ Correct
[CreateAssetMenu(menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject {
    public float MoveSpeed;
    public int MaxHealth;
}
```

---

### Timers (when timeScale may change)

| Use This | Not This | Why |
|---|---|---|
| `Time.unscaledDeltaTime` / `WaitForSecondsRealtime` | `Time.deltaTime` / `WaitForSeconds` | `Time.timeScale = 0` (pause) freezes scaled time |

```csharp
// ✅ Survives pause
_timer += Time.unscaledDeltaTime;
yield return new WaitForSecondsRealtime(duration);

// ❌ Freezes when timeScale = 0
_timer += Time.deltaTime;
yield return new WaitForSeconds(duration);
```

---

### Testing

| Use This | Not This | Why |
|---|---|---|
| NUnit + Unity Test Runner | Manual Play Mode testing only | Repeatable, automated, catches regressions |
| Edit Mode tests for pure logic | Play Mode tests for everything | Faster iteration |

```csharp
// ✅ Edit Mode test — no scene needed
[Test]
public void TraitThreshold_ActivatesAtCorrectCount() {
    Assert.IsTrue(TraitSystem.IsThresholdMet(TraitType.Fire, count: 2));
}

// ✅ Play Mode test — needs scene
[UnityTest]
public IEnumerator BattleResolver_WinCondition_TriggersRunAdvance() {
    // setup → resolve → yield → assert
    yield return null;
}
```

---

### Summary Reference

| Feature | Use (2026) | Avoid (Legacy) |
|---|---|---|
| Input | Input System package | `Input` class |
| UI | UI Toolkit | UGUI Canvas |
| Assets | Addressables | `Resources` |
| Tunable data | `ScriptableObject` | Hardcoded constants |
| Rendering | URP + RenderGraph | Built-in pipeline |
| Timers (pause-safe) | `unscaledDeltaTime` | `deltaTime` |
| Testing | NUnit + Test Runner | Manual only |
| Object lookup | Singleton `.Instance` / Inspector refs | `FindObjectOfType` |
