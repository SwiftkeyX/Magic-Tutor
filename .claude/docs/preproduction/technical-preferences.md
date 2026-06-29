# Technical Preferences

## Engine & Language

| Field | Value |
|---|---|
| **Engine** | Unity 6 LTS (6000.4.9f1) |
| **Language** | C# |
| **Rendering** | URP 2D (Universal Render Pipeline) |
| **Physics** | Unity 2D Physics (Box2D) |

## Input & Platform

| Field | Value |
|---|---|
| **Target Platforms** | PC (Windows) |
| **Input Methods** | Keyboard / Mouse |
| **Primary Input** | Keyboard / Mouse |
| **Gamepad Support** | None |
| **Touch Support** | None |

**Platform Notes**

Keyboard/Mouse is the only supported input method. No gameplay-critical actions may require gamepad or touch. Controller support is explicitly out of scope for v1.

## Performance Budgets

| Budget | Target | Notes |
|---|---|---|
| **Target Framerate** | 60 fps | Hard target for PC |
| **Frame Budget** | 16.6 ms | Derived from 60 fps |
| **Draw Calls** | TBD — set after first profiling pass | 2D sprite game; expect 50–150 range |
| **Memory Ceiling** | TBD — set after first profiling pass | |
| **GC Alloc / Frame** | Zero in steady state | Allocations cause frame spikes in auto-battle loops |

## Testing

| Field | Value |
|---|---|
| **Framework** | NUnit (Unity Test Runner) |
| **Test types** | Edit Mode (pure logic), Play Mode (scene/runtime) |
| **Minimum Coverage** | All gameplay systems, all state machine transitions, all formulas |

**Required Tests** — must pass before shipping:

- Trait threshold activation logic (correct ability triggered at each breakpoint)
- Battle win/lose state transitions (all paths through the combat outcome state machine)
- Student stat formulas (training calculations produce correct values)
- Year-end promotion/discard logic (correct students selected as teachers)
- Meta-progression accumulation (teacher buffs stack correctly across runs)

## Forbidden Patterns

- `Resources.Load` — use Addressables for all runtime asset loading; Resources causes synchronous stalls
- `DontDestroyOnLoad` — use Bootstrap scene for persistent objects instead; see `unity-editor.md`
- `SceneManager.LoadScene` (direct call) — always route through SceneLoader; direct loads bypass transition logic
- Legacy `Input` class — use Unity Input System package only; legacy Input is disabled

## Allowed Libraries / Addons

- Unity Input System — player input handling (pre-installed)
- Universal Render Pipeline (URP) — 2D rendering (pre-installed)
- Coplay MCP — Unity editor automation via Claude Code (pre-installed)

## Architecture Decisions Log

- No ADRs yet — create `.claude/docs/other/adr/adr-001-*.md` for the first significant technical decision

## Agent / Specialist Routing

| Task Type | Agent / Skill | Notes |
|---|---|---|
| General C# scripts, scene wiring | `gameplay-programmer` | Default for most Unity work |
| Architecture review, code audit | `technical-director` | Read-only — advises, does not implement |
| UI implementation | `ui-programmer` | Canvas, HUD, menus |
| Audio (SFX, music, AudioSource) | `audio-engineer` | Wires audio to game events |
| Level / scene composition | `level-designer` | Scene layout, GameObject placement |
| Numeric tuning (stats, thresholds) | `systems-designer` | Value changes only, no architecture |
| Security review | `/security-review` skill | |

### File Extension Routing

| File Type | Agent to Use |
|---|---|
| `.cs` game scripts | `gameplay-programmer` |
| `.shader`, `.shadergraph`, `.mat` | `gameplay-programmer` (simple) / manual (complex) |
| `.uxml`, `.uss`, Canvas prefabs | `ui-programmer` |
| `.unity`, `.prefab` | `gameplay-programmer` (via coplay MCP) |
| Architecture review | `technical-director` |
