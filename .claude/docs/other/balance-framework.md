# Balance Framework

> **Status**: Active  
> **Scope**: Cross-phase — governs all champion stat and trait tuning decisions  
> **Tools**: `Assets/Editor/BalanceValidator.cs` (simulation runner)

---

## Overview

Three-layer process for balancing champions:
1. **Layer 1 — Attack Speed formula**: linear AS replaces old SPD (see below)
2. **Layer 2 — Stat Budget Score**: spreadsheet math to set per-tier targets
3. **Layer 3 — Simulation**: `BalanceValidator.cs` runs 200 battles per matchup and reports win rates

No champion stats should change without running Layer 2 first, and Layer 3 after.

---

## Layer 1 — Attack Speed (AS)

### Concept

`AttackSpeed` is a `float` (attacks per second), the same concept as TFT's AS stat.  
`ActionInterval` (ticks between actions) is derived from it:

```
ActionInterval = max(2, round(1.0 / (AttackSpeed × 0.6)))

// 0.6 = TickDelay in seconds (AutoBattleResolver.TickDelay constant)
```

### Why not `10 − SPD`

The old `ActionInterval = max(1, 10 − SPD)` formula was non-linear:
- SPD 6 → 7: +33% more attacks per tick
- SPD 2 → 3: +14% more attacks per tick

The same +1 stat point had 2.4× more impact at the high end. This is why Shadowblade (old SPD 7) was disproportionately dominant.

With AS, the scale is linear: doubling AS always doubles attack frequency.

### Trait speed bonuses

Trait bonuses multiply `AttackSpeed` (not the interval), then ActionInterval is re-derived:

```csharp
c.AttackSpeed    *= m.AttackSpeedMult;       // e.g. 1.176f = +17.6% AS
c.ActionInterval  = Mathf.Max(2, Mathf.RoundToInt(1f / (c.AttackSpeed * TickDelay)));
c.ActionTimer     = c.ActionInterval;
```

| Old interval mult | New AS mult | Effect |
|---|---|---|
| ×0.85 (Ranger, Warden BP3) | ×1.176 | +17.6% more attacks |
| ×0.70 (Warden BP4)         | ×1.429 | +42.9% more attacks |

### AS reference table (current roster)

| Champion | AS | Interval (ticks) | Attacks/10 ticks |
|---|---|---|---|
| Ironclad | 0.21 | 8 | 1.25 |
| Bloodhound | 0.33 | 5 | 2.00 |
| Pyromancer | 0.28 | 6 | 1.67 |
| Windrunner | 0.42 | 4 | 2.50 |
| Grove Keeper | 0.24 | 7 | 1.43 |
| Shadowblade | 0.56 | 3 | 3.33 |
| Phalanx | 0.21 | 8 | 1.25 |
| Stormbringer | 0.33 | 5 | 2.00 |
| Phantom Assassin | 0.42 | 4 | 2.50 |
| Dread Overlord | 0.24 | 7 | 1.43 |

---

## Layer 2 — Stat Budget Score

### Formula

```
Score = Offense × AttackSpeed × sqrt( HP × (100 + DEF) / 100 )

Offense = ATK  (physical units)
Offense = MG   (Elementalists and Supports where MG > ATK)
```

**Offense × AS** = raw DPS at 0 armor (attacks per second × damage per hit).  
**sqrt(EHP)** = square root of effective physical HP.

> CRIT, traits, shields, omnivamp, Striker stacks excluded — base stats only.

### Target score bands by tier

| Tier  | Tank     | Carry    | Support  |
|-------|----------|----------|----------|
| 1c    | 25 – 40  | 40 – 60  | —        |
| 2c    | —        | 65 – 95  | 50 – 70  |
| 3c    | 80 – 130 | —        | 85 – 125 |
| 4c    | —        | 140 – 220| —        |
| 5c    | 260 – 400| —        | —        |

### Current scores (as of last balance pass)

| Champion | Cost | Role | Score | In band? |
|---|---|---|---|---|
| Ironclad | 1c | Tank | 29.6 | ✅ |
| Bloodhound | 1c | Carry | 50.9 | ✅ |
| Pyromancer | 1c | Carry | 44.0 | ✅ |
| Windrunner | 2c | Carry | 74.5 | ✅ |
| Grove Keeper | 2c | Support | 59.8 | ✅ |
| Shadowblade | 2c | Carry | 110.5 | ❌ target 65–95 |
| Phalanx | 3c | Tank | 45.4 | ❌ target 80–130 |
| Stormbringer | 3c | Support | 100.3 | ✅ |
| Phantom Assassin | 4c | Carry | 167.4 | ✅ |
| Dread Overlord | 5c | Tank | 101.7 | ❌ target 260–400 |

---

## Layer 3 — Simulation Validation

### Tool

`Assets/Editor/BalanceValidator.cs` — Unity menu: **Magic School → Validate Balance**

Runs 200 1v1 battles for all 10×10 champion pairs, synchronous (no coroutines).
Prints a win-rate matrix and flags matchups outside the expected band.

### Expected win-rate bands

| Cost difference | Expected win % for the higher-cost unit |
|---|---|
| +1 (e.g. 2c vs 1c) | 55 – 80% |
| +2 (e.g. 3c vs 1c) | 70 – 92% |
| +3 (e.g. 4c vs 1c) | 80 – 96% |
| +4 (e.g. 5c vs 1c) | 88 – 98% |

> Units at the same cost tier should win ~45–55% of their mirror matchup.

### Simulation rules

- Base stats only (no traits, no shields, no Striker stacks, no omnivamp)
- `IncludeCrit = false` by default — set to `true` to check CRIT variance impact
- Movement ignored — both units start in range (pure 1v1 DPS race)
- Timeout at 200 ticks: winner is the unit with more HP remaining

---

## Workflow

When adding or tuning a champion:
1. Compute `Score = Offense × AS × sqrt(EHP)` and confirm it falls in the tier band.
2. Run **Magic School → Validate Balance** and check the printed outlier list.
3. Adjust stats until the Score is in band AND the simulation win rates are in band.
4. Update the "Current scores" table above.
5. Update the champion stat table in `TraitSystem.md`.

---

## Cross-References

| This doc references | Target | Nature |
|---|---|---|
| Champion stats | `TraitSystem.md` | Source of truth for all HP/ATK/DEF/MG/MR/AS/CRIT/Range values |
| AS formula | `AutoBattleResolver.md` | Action Interval section |
| Simulation tool | `Assets/Editor/BalanceValidator.cs` | Implementation |
