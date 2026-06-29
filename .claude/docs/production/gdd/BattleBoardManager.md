# BattleBoardManager

> **Status**: Draft
> **Last Updated**: 2026-06-29
> **Implements Pillar**: Empowering — the hex board is the visual stage for the battle payoff; spatial positioning makes every training decision tangible

## Summary

BattleBoardManager owns the hex grid visual layer in the Battle scene. It renders a 4×7 tile board (one side per team, 8 rows total), manages the player's pre-battle drag-and-drop unit placement, and drives `BattleUnit` visuals in response to `AutoBattleResolver` events. It never touches battle logic — it is a pure visual and input layer on top of the simulation.

> **Quick reference** — Layer: `Presentation` · Priority: `MVP` · Key deps: `AutoBattleResolver`

---

## Overview

The full board is 8 rows × 7 columns of hexagonal tiles. Rows 0–3 are the player's side; rows 4–7 are the enemy's side (mirrored). Columns are 0–6 (A–G in the reference). Row 0 of the player side is the Front Line (closest to enemies); row 3 is the Back Line.

Before battle starts, BattleBoardManager enters **Placement Phase**: it shows a bench panel listing unplaced students. The player drags student cards onto player-side tiles. When the player confirms, BattleBoardManager calls `AutoBattleResolver.SetUnitPositions()` and signals readiness.

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
  where `HEX_WIDTH = 1.0f`, `HEX_OFFSET = 0.5f`, `HEX_HEIGHT = 0.866f` (≈ √3/2)

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
   - Reads `AutoBattleResolver.GetCombatantSnapshots()` to populate the bench panel.
   - Player drags a student card using `IBeginDragHandler` / `IDragHandler` / `IEndDragHandler`.
   - On drag-enter to a player-side tile: highlight the tile (valid drop zone).
   - On drop: instantiate a `BattleUnit` at that tile's position; record placement in `_pendingPlacements`.
   - On drag back to bench (drop outside any tile): remove unit and clear tile.
   - "Start Battle" button becomes interactable when ≥1 student is placed.
   - On confirm: call `AutoBattleResolver.SetUnitPositions(_pendingPlacements)` then invoke `AutoBattleResolver.BeginBattle()` (or equivalent RunManager call).
3. **Battle Phase** (event-driven):
   - `OnCombatantMoved(id, from, to)` → start `BattleUnit.MoveTo(to)` coroutine; update `HexGrid` occupancy.
   - `OnCombatantActed(actorId, targetId, ...)` → call `BattleUnit.PlayAttackAnim(targetTileWorldPos)`.
   - `OnCombatantDefeated(id)` → call `BattleUnit.PlayDeathAnim()` then `Destroy(go)`.
4. BattleBoardManager **never calls `Resolve()`** — that is RunManager's responsibility.
5. BattleBoardManager **never reads student stats** — it only reads IDs and display names from snapshots.

### States

| State | Entry | Exit | Behavior |
|---|---|---|---|
| `Initializing` | Scene load | Snapshots loaded | Building tile grid; bench panel populating |
| `Placement` | Snapshots loaded | Player confirms | Drag-drop active; board highlights valid tiles |
| `Battle` | `OnCombatantMoved` or `OnCombatantActed` first fires | `OnBattleComplete` | Animating units; no drag-drop |
| `Outcome` | `OnBattleComplete` | Scene unloads | Board frozen; outcome overlay visible |

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
| `OnCombatantMoved` fires for an unknown unit ID | Log warning, no-op |
| Two `MoveTo` coroutines start on the same unit | Cancel the previous coroutine before starting the new one |
| `OnBattleComplete` fires while a `MoveTo` animation is in progress | Stop all unit coroutines immediately; freeze board |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `AutoBattleResolver` | This depends on it | Reads snapshots; subscribes to movement/action/defeat/complete events; calls `SetUnitPositions()` |
| `HexGrid` | This depends on it | Grid adjacency, distance, BFS pathing |

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect |
|---|---|---|---|
| `HEX_WIDTH` | 1.0 | 0.8–1.5 | Tile spacing horizontal |
| `HEX_HEIGHT` | 0.866 | 0.7–1.3 | Tile spacing vertical |
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

---

## Acceptance Criteria

- [ ] All 56 tiles spawn at correct world positions with correct hex spacing
- [ ] Player-side tiles highlight on drag-over; enemy-side tiles do not respond to drops
- [ ] Placed student units appear at the correct tile position
- [ ] `SetUnitPositions()` is called before `Resolve()` starts
- [ ] `OnCombatantMoved` drives unit lerp animation to correct world position
- [ ] `OnCombatantDefeated` removes unit from scene
- [ ] No `FindObjectOfType` anywhere in `BattleBoardManager.cs`
- [ ] No direct reads from `StudentRoster` or `EnemyDatabase`

---

## Cross-References

| This Doc References | Target Doc | Element |
|---|---|---|
| `AutoBattleResolver.SetUnitPositions()` | `AutoBattleResolver.md` | Pre-battle injection method |
| `OnCombatantMoved` event | `AutoBattleResolver.md` | Movement event signature |
| `HexCoord`, `HexGrid` | (implemented in `HexCoord.cs`, `HexGrid.cs`) | Grid data types |
