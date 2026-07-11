# BattleBoardManager

> **Status**: Draft
> **Last Updated**: 2026-07-04 (UI Toolkit migration complete — Phase 3: Start Battle button + placement counter, redundant outcome panel/text removed; Phase 4: bench cards + drag-and-drop)
> **Implements Pillar**: Empowering — the hex board is the visual stage for the battle payoff; spatial positioning makes every training decision tangible

## Summary

BattleBoardManager owns the hex grid visual layer in the Battle scene. It renders a 4×7 tile board (one side per team, 8 rows total), manages the player's pre-battle drag-and-drop unit placement, and drives `BattleUnit` visuals in response to `AutoBattleResolver` events. It never touches battle logic — it is a pure visual and input layer on top of the simulation.

> **Quick reference** — Layer: `Presentation` · Priority: `MVP` · Key deps: `AutoBattleResolver`

---

## Overview

The full board is 8 rows × 7 columns of hexagonal tiles. Rows 0–3 are the player's side; rows 4–7 are the enemy's side (mirrored). Columns are 0–6 (A–G in the reference). Row 0 of the player side is the Front Line (closest to enemies); row 3 is the Back Line.

Before battle starts, BattleBoardManager enters **Placement Phase**: it shows a bench panel listing unplaced students. The player drags student cards onto player-side tiles. When the player confirms, BattleBoardManager hands off to `RunManager` (see Core Rule 2) rather than starting the battle itself.

**Two entry contexts** for Placement Phase, since this system serves both a standalone dev scene and the production game:
- **Prototype / standalone (`BattleTest.unity`, no `RunManager` present)**: Placement Phase auto-starts in `Start()`, reading `AutoBattleResolver.GetCombatantSnapshots()` immediately (its lazy-init fallback supplies dummy `ChampionRoster` data — see `AutoBattleResolver.md`). Unaffected by this change.
- **Production (`Battle.unity`, driven by `RunManager`)**: Placement Phase does **not** auto-start. `RunManager` calls `BeginPlacement(students, enemies, maxSquadSize)` explicitly, only after it has already called `AutoBattleResolver.SetCombatants()` with the real roster (see `RunManager.md`'s Battle-phase sequence). This ordering guarantee is why `BeginPlacement()` exists as a separate entry point instead of relying on `Start()` — it removes the race where `GetCombatantSnapshots()` could be read before real data exists.

During battle, BattleBoardManager subscribes to `AutoBattleResolver` events and updates `BattleUnit` GameObjects in response — moving them along the grid, playing attack animations, and destroying defeated units.

---

## Detailed Design

### Grid Coordinate System

- `HexCoord(col, row)` — col: 0–6 (A–G), row: 0–7 (0–3 player, 4–7 enemy)
- Offset hex grid: even rows are shifted right by half a tile (standard offset-row honeycomb)
- World position formula:
  ```
  x = col * HEX_WIDTH + (row % 2 == 0 ? 0 : HEX_OFFSET)
  y = row * HEX_HEIGHT
  ```
  where `HEX_WIDTH = 1.1f`, `HEX_OFFSET = 0.55f`, `HEX_HEIGHT = 0.95f` (tuned from √3/2 for visual spacing)

### Named Positions (Reference Image)

| Name | HexCoord | Player significance |
|---|---|---|
| Front Line | row 0, all cols | First line to engage enemies |
| Back Line | row 3, all cols | Safest starting position |
| Left Corner | (0, 3) | Back-left anchor |
| Right Corner | (6, 3) | Back-right anchor |
| Left God Hex | (1, 3) | Strong melee anchor |
| Right God Hex | (5, 3) | Strong melee anchor |
| Left Pocket | (0, 2) | Flanking position |
| Right Pocket | (6, 1) | Flanking position |
| D Column | col 3, all rows | Central axis (highlighted in reference) |

Enemy positions mirror these: enemy "Front Line" is row 7 (closest to player side).

### Core Rules

1. BattleBoardManager instantiates all 56 `HexTileView` objects (8×7) on `Awake()` from `HexTile` prefab. Player tiles (rows 0–3) use one color; enemy tiles (rows 4–7) use another.
2. **Placement Phase** (before battle):
   - Entry: either auto-started in `Start()` (Prototype/standalone) or via `BeginPlacement(students, enemies, maxSquadSize)` (Production — see Overview). Either way, once entered it reads `AutoBattleResolver.GetCombatantSnapshots()` to populate the bench panel.
   - Player drags a student card via `BenchCardDragManipulator` (a UI Toolkit `PointerManipulator` attached to each bench card `VisualElement` — bench cards are C# `VisualElement`s built in `BuildBench()`, not GameObjects, matching `SchoolHUD.cs`'s dynamic-card pattern). `PointerDownEvent` captures the pointer and records the down position; `PointerMoveEvent` promotes to a drag once movement exceeds a 10px threshold (mirrors the old uGUI `EventSystem.m_DragThreshold`), calling `BattleBoardManager.OnCardDragStart(studentId)` / `OnCardDrag(studentId, screenPos)`; `PointerUpEvent` calls `OnCardDragEnd(...)` if dragging, or `OnCardClicked(studentId)` otherwise — this click-vs-drag disambiguation was previously free via uGUI's `EventSystem`, now implemented explicitly in the manipulator. Coordinate conversion from UI Toolkit's panel-space pointer position to Unity screen-space uses `RuntimePanelUtils.PanelToScreen()`; after that, `Cam.ScreenToWorldPoint()` and the nearest-hex-tile search are unchanged from the original implementation.
   - On drag-enter to a player-side tile: highlight the tile (valid drop zone).
   - On drop: if `_pendingPlacements.Count >= maxSquadSize`, reject the drop (tile flashes red, same as an occupied-tile rejection) — the squad cap is enforced here, not just tile occupancy. Otherwise instantiate a `BattleUnit` at that tile's position; record placement in `_pendingPlacements`.
   - On drag back to bench (drop outside any tile): remove unit and clear tile.
   - "Start Battle" button becomes interactable when `1 <= _pendingPlacements.Count <= maxSquadSize` (in Production, `maxSquadSize` comes from `BeginPlacement()`'s parameter, sourced from `StudentConfig.MaxSquadSize`; in Prototype/standalone with no config passed, it defaults to unlimited/tile-capacity as before).
   - On confirm: call `AutoBattleResolver.SetUnitPositions(_pendingPlacements)`, then hand off — **Production**: call `RunManager.Instance.ConfirmSquadPlacement(fieldedStudentIds, _pendingPlacements)`, which itself finalizes combatants and starts the fight (see `RunManager.md`). **Prototype/standalone** (no `RunManager.Instance`): call `AutoBattleResolver.BeginBattle()` directly, as before.
3. **Battle Phase** (event-driven):
   - `OnCombatantMoved(id, from, to)` → start `BattleUnit.MoveTo(to)` coroutine; update `HexGrid` occupancy.
   - `OnCombatantActed(actorId, targetId, ...)` → call `BattleUnit.PlayAttackAnim(targetTileWorldPos)`.
   - `OnCombatantDefeated(id)` → call `BattleUnit.PlayDeathAnim()` then `Destroy(go)`.
4. BattleBoardManager **never calls `Resolve()`** — that is RunManager's responsibility.
5. BattleBoardManager may read `ChampionData` via `ChampionRoster` for trait bookkeeping only — registering/unregistering placements with `TraitTracker` (`RegisterPlacement`/`UnregisterPlacement`) and applying trait bonuses via `TraitEffectApplier` before battle start. It must still **never read `StudentRoster` or `EnemyDatabase` directly**, and it does not itself own or render any stat-display UI — that's `HeroInfoPanel`'s job (see `BattleHUD.md`), which resolves `ChampionData` independently and is never referenced by BattleBoardManager.

   **`_championDataLookup` keying (critical — this was a real bug):** `_championDataLookup` is a `Dictionary<string, ChampionData>` built **once per Placement Phase entry** (both the standalone `Start()` path and the production `BeginPlacement()` path), keyed by **each placed unit's own ID** (`CombatantSnapshot.Id` — a `StudentId` GUID in production, a champion slug in standalone) — **not** by the champion slug itself. It is built by resolving `ChampionRoster.GetChampionById(snapshot.ChampionId)` for every student snapshot and storing the result under `snapshot.Id`. This is what lets `RegisterPlacement`, `TraitEffectApplier.Apply`, and hero-selection (below) all do a plain `_championDataLookup.TryGetValue(placedUnitId, ...)` and succeed regardless of whether the ID space is GUIDs (production) or slugs (standalone, where `Id == ChampionId` so the distinction is moot). Keying this dictionary by the slug itself (via `ChampionRoster.GetChampionLookup()`) was the bug — every placement lookup in production missed silently, breaking trait-count updates, battle-start trait bonuses, and hero selection simultaneously.

6. **Hero selection on click**: `BenchCardDragManipulator`'s click path and a placed `BattleUnit`'s click do not call `HeroSelection.Select()` with the raw placed-unit ID directly. Instead, `BattleBoardManager.OnCardClicked(string studentId)` resolves the champion slug via `_championDataLookup` (falling back to the raw ID if unresolved) and calls `HeroSelection.Select(championId)` with that — this is what lets `HeroInfoPanel`'s `ChampionRoster.GetChampionById()` lookup succeed for a placed/benched player unit. Enemy units pass their own ID unchanged (enemy IDs already match `EnemyDatabaseStub`'s keys, no bridge needed).

7. **BattleBoardManager does not show its own outcome overlay or outcome text.** That responsibility belongs entirely to `BattleHUD` (see `BattleHUD.md`) — both systems subscribe to the same `AutoBattleResolver.OnBattleComplete` event, but only `BattleHUD`'s overlay has the "Continue" button that gates `RunManager.CompleteBattlePhase()`. An earlier pass had BattleBoardManager showing a redundant duplicate outcome panel/text with no functional purpose; it has been removed.

### States

| State | Entry | Exit | Behavior |
|---|---|---|---|
| `Initializing` | Scene load | Snapshots loaded | Building tile grid (`Awake()`, always immediate); bench panel stays empty until Placement Phase is entered |
| `Placement` | Snapshots loaded | Player confirms | Drag-drop active; board highlights valid tiles |
| `Battle` | `OnCombatantMoved` or `OnCombatantActed` first fires | `OnBattleComplete` | Animating units; no drag-drop |
| `Outcome` | `OnBattleComplete` | Scene unloads | Board frozen; `BattleHUD`'s outcome overlay (a peer system, not BattleBoardManager's own) becomes visible |

### HexTileView Component

Attached to each tile prefab instance:
- `HexCoord Coord` — set at instantiation
- `bool IsPlayerSide` — `Coord.Row < 4`
- `void SetHighlight(bool active)` — swaps sprite or color to indicate valid drop target
- `void SetOccupied(bool occupied)` — dims tile slightly when a unit is placed on it

### BattleUnit Component

One per combatant, spawned by BattleBoardManager:
- `string CombatantId`
- `HexCoord CurrentCell`
- `IEnumerator MoveTo(Vector3 worldPos)` — lerp over `MoveAnimDuration` (default 0.15s)
- `void PlayAttackAnim(Vector3 targetWorldPos)` — lunge 30% toward target over 3 frames, snap back over 5 frames
- `IEnumerator PlayDeathAnim()` — fade alpha to 0 over 0.4s, then `Destroy(gameObject)`

### Enemy Auto-Placement

Enemies are placed on rows 4–7 (enemy side). Default layout: all enemies start on row 4 (front line of enemy side), distributed evenly across columns starting from col 0. If more than 7 enemies exist, overflow to row 5.

---

## Formulas

### Hex Distance

```
distance = max(abs(a.Col - b.Col), abs(a.Row - b.Row),
               abs((a.Col - a.Row) - (b.Col - b.Row)))
```
(Offset-to-cube coordinate conversion used internally by `HexGrid.Distance`.)

### World Position

```
x = col * HEX_WIDTH + (row % 2 == 1 ? HEX_OFFSET : 0)
y = row * HEX_HEIGHT
```

---

## Edge Cases

| Scenario | Expected Behavior |
|---|---|
| Player clicks "Start Battle" with 0 students placed | Button is non-interactable; no battle starts |
| Player drags a student to an already-occupied tile | Highlight turns red; drop is rejected |
| Player drags a student when `_pendingPlacements.Count == maxSquadSize` | Highlight turns red; drop is rejected (squad is full) |
| `BeginPlacement()` called a second time before confirm | Log error, no-op — Placement Phase already active |
| `OnCombatantMoved` fires for an unknown unit ID | Log warning, no-op |
| Two `MoveTo` coroutines start on the same unit | Cancel the previous coroutine before starting the new one |
| `OnBattleComplete` fires while a `MoveTo` animation is in progress | Stop all unit coroutines immediately; freeze board |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `AutoBattleResolver` | This depends on it | Reads snapshots; subscribes to movement/action/defeat/complete events; calls `SetUnitPositions()` |
| `HexGrid` | This depends on it | Grid adjacency, distance, BFS pathing |
| `ChampionRoster` | This depends on it | Reads `ChampionData` for trait bookkeeping only (see Core Rule 5) — not for any UI display |
| `TraitTracker` | This depends on it | Registers/unregisters placements so trait breakpoints stay current — keyed via `_championDataLookup`, see Core Rule 5 |
| `HeroSelection` (static event) | This depends on it | `OnCardClicked()` resolves the champion slug via `_championDataLookup` and calls `HeroSelection.Select()` — see Core Rule 6 |
| `RunManager` | It depends on this (Production only) | State trigger — calls `BeginPlacement(students, enemies, maxSquadSize)` to start Placement Phase; receives `ConfirmSquadPlacement(fieldedStudentIds, positions)` callback on confirm. Not present in the Prototype/standalone context. |

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect |
|---|---|---|---|
| `HEX_WIDTH` | 1.1 | 0.8–1.5 | Tile spacing horizontal |
| `HEX_HEIGHT` | 0.95 | 0.7–1.3 | Tile spacing vertical |
| `MoveAnimDuration` | 0.15s | 0.05–0.4s | Unit movement speed between cells |
| `AttackLungePercent` | 0.3 | 0.1–0.6 | How far unit lurches toward target on attack |

---

## Visual / Audio Requirements

| Event | Visual | Priority |
|---|---|---|
| Grid spawns | Player tiles (warm color), enemy tiles (cool color) | MVP |
| Valid drop hover | Tile glows green | MVP |
| Invalid drop hover | Tile glows red | MVP |
| Unit placed on tile | Unit sprite appears; tile dims slightly | MVP |
| `OnCombatantMoved` | Unit lerp-slides to new tile | MVP |
| `OnCombatantActed` | Unit lunges toward target, snaps back | MVP |
| `OnCombatantDefeated` | Unit fades out | MVP |
| Placement Phase active | `_placementCountText` shows "`{placed}/{maxSquadSize} heroes placed`" | MVP |

`_placementCountText` (`[SerializeField] Label`, UI Toolkit) is updated at the same points `_startBattleButton.interactable` is recalculated (`PlaceStudent`, `UnplaceStudent`), plus an initial set when Placement Phase begins (`BeginPlacement()` / standalone `Start()`), so the player always has a live read on squad-cap progress without needing to count bench cards manually.

---

## Acceptance Criteria

- [ ] All 56 tiles spawn at correct world positions with correct hex spacing
- [ ] Player-side tiles highlight on drag-over; enemy-side tiles do not respond to drops
- [ ] Placed student units appear at the correct tile position
- [ ] `SetUnitPositions()` is called before `Resolve()` starts
- [ ] In Production, placements beyond `maxSquadSize` are rejected at drop time; "Start Battle" stays non-interactable above the cap or below 1 placement
- [ ] In Production, `GetCombatantSnapshots()` is never read before `RunManager` has called `AutoBattleResolver.SetCombatants()` with real data — `BeginPlacement()` is only called after that
- [ ] `OnCombatantMoved` drives unit lerp animation to correct world position
- [ ] `OnCombatantDefeated` removes unit from scene
- [ ] No `FindObjectOfType` anywhere in `BattleBoardManager.cs`
- [ ] No direct reads from `StudentRoster` or `EnemyDatabase`
- [ ] `_championDataLookup` resolves correctly for both production (`StudentId`-GUID-keyed) and standalone (champion-slug-keyed) placement flows — `RegisterPlacement`, `TraitEffectApplier.Apply`, and hero-selection all succeed in both contexts
- [ ] `_placementCountText` updates on every placement/unplacement to show current/max

---

## Cross-References

| This Doc References | Target Doc | Element |
|---|---|---|
| `AutoBattleResolver.SetUnitPositions()` | `AutoBattleResolver.md` | Pre-battle injection method |
| `OnCombatantMoved` event | `AutoBattleResolver.md` | Movement event signature |
| `HexCoord`, `HexGrid` | (implemented in `HexCoord.cs`, `HexGrid.cs`) | Grid data types |
| `BeginPlacement()` / `ConfirmSquadPlacement()` hand-off | `RunManager.md` | Battle-phase Placement sub-step | Rule dependency |
| `MaxSquadSize` | `StudentRoster.md` | `StudentConfig.MaxSquadSize` tuning knob | Data dependency |
