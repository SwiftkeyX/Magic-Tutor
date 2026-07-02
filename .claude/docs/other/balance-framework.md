# Balance Framework

> **Status**: Active  
> **Scope**: Cross-phase — governs all champion stat and trait tuning decisions  
> **Tools**: `Assets/Editor/BalanceValidator.cs` (simulation runner)

---

## Overview

Three-layer process for balancing champions:
1. **Layer 1 — Accumulator & Mana**: float accumulator replaces integer timer; universal mana system
2. **Layer 2 — Stat Budget Score**: spreadsheet math to set per-tier targets
3. **Layer 3 — Simulation**: `BalanceValidator.cs` runs 200 battles per matchup and reports win rates

No champion stats should change without running Layer 2 first, and Layer 3 after.

---

## Layer 1 — Attack Speed Accumulator

### Tick Rate

The resolver runs at **0.1s per tick** (10 ticks/second), matching TFT's resolution.  
`MaxBattleTicks = 1200` (120s ceiling).

### Accumulator Logic

Each unit has a float `ActionProgress` that starts at `0.0f`. Each tick:

```
ActionProgress += AttackSpeed × TickDelay   (= AttackSpeed × 0.1)
```

When `ActionProgress >= 1.0f`, the unit acts and we subtract 1.0 (overflow carries forward):

```
ActionProgress -= 1.0f
```

This eliminates the dead-zones and breakpoints of the old integer `ActionInterval` system. A +5% AS buff now always results in exactly +5% more attacks over time — no rounding cliff.

### Trait Speed Bonuses

Trait bonuses multiply `AttackSpeed` directly before the battle loop (in `ApplyPreBattleTraitModifiers`). Because accumulation uses `AttackSpeed × TickDelay` each tick, the effect is immediate and linear.

| Trait effect | AS multiplier | Effective DPS change |
|---|---|---|
| Ranger BP2 / Warden BP3 | ×1.176 | +17.6% |
| Warden BP4 | ×1.429 | +42.9% |
| Striker max-stack (inline) | ÷0.70 on `gain` | +42.9% while at 8 stacks |

---

## Layer 1b — Mana & Ability Casting

All units gain mana through combat. When `Mana >= MaxMana`, the unit casts its ability (currently a placeholder bonus attack) and mana resets to 0.

### Mana Sources

| Source | Amount |
|---|---|
| Per auto-attack (attacker) | +10 flat |
| Per hit received (defender) | +`min(42, round(rawOffense × 0.07))` |
| Kinetic trait tick (every 3s) | +5 or +15 depending on BP |
| Kinetic BP4 starting bonus | +30 starting mana at battle start |

> **Why pre-mitigation for on-hit mana?** A tank with 300 DEF would otherwise gain 4× less mana per hit than a squishy carry struck by the same attack. Pre-mitigation keeps mana gain fair across all archetypes.

### Per-Champion Mana Pools

| Champion | MaxMana | StartingMana | Profile |
|---|---|---|---|
| Ironclad | 100 | 0 | Utility tank |
| Bloodhound | 50 | 0 | Spam carry |
| Pyromancer | 50 | 0 | Spam AP carry |
| Windrunner | 50 | 0 | Spam carry |
| Grove Keeper | 80 | 0 | Support |
| Shadowblade | 40 | 0 | Hyper-spam carry |
| Phalanx | 120 | 30 | CC tank (casts once early) |
| Stormbringer | 80 | 0 | Support |
| Phantom Assassin | 60 | 0 | Burst carry |
| Dread Overlord | 150 | 50 | Hyper-tank (long windup) |

---

## Layer 2 — Stat Budget Score

### Formula

```
Score = Offense × AttackSpeed × sqrt( HP × (100 + DEF) / 100 )

Offense = ATK  (physical units)
Offense = MG   (Elementalists and Supports where MG > ATK)
```

**Offense × AS** = raw DPS at 0 armor.  
**sqrt(EHP)** = square root of effective physical HP (reduces tank weight to reflect team-fight realism).

> **Why √EHP?** Pure DPS × EHP measures a unit's theoretical duel power in a vacuum, which over-scores tanks because of their massive EHP. In team combat, a tank's damage output is rarely focused on carries, and tanks are prone to being CC'd or ignored, while carries deal damage from safety. Taking the square root of EHP dampens the weight of raw health and armor so the score reflects a unit's true utility in a dynamic team fight.

> CRIT, traits, shields, omnivamp, Striker stacks excluded — base stats only.

### Target Score Bands (by cost tier, unified across roles)

| Tier | Score range |
|---|---|
| 1c | 900 – 1100 |
| 2c | 950 – 1450 |
| 3c | 1200 – 1350 |
| 4c | ~2200 |
| 5c | ~2100 |

### Current Scores (TFT-aligned roster)

| Champion | Cost | Role | Score | In band? |
|---|---|---|---|---|
| Ironclad | 1c | Tank | 921 | ✅ |
| Bloodhound | 1c | Carry | 1010 | ✅ |
| Pyromancer | 1c | Carry | 964 | ✅ |
| Windrunner | 2c | Carry | 1393 | ✅ |
| Grove Keeper | 2c | Support | 979 | ✅ |
| Shadowblade | 2c | Carry | 1391 | ✅ |
| Phalanx | 3c | Tank | 1244 | ✅ |
| Stormbringer | 3c | Support | 1345 | ✅ |
| Phantom Assassin | 4c | Carry | 2246 | ✅ |
| Dread Overlord | 5c | Tank | 2080 | ✅ |

---

## Layer 3 — Simulation Validation

### Tool

`Assets/Editor/BalanceValidator.cs` — Unity menu: **Magic School → Validate Balance**

Runs 200 1v1 battles for all 10×10 champion pairs, synchronous (no coroutines).
Prints a win-rate matrix and flags matchups outside the expected band.

### Expected Win-Rate Bands

| Cost difference | Expected win % for the higher-cost unit |
|---|---|
| +1 (e.g. 2c vs 1c) | 55 – 80% |
| +2 (e.g. 3c vs 1c) | 70 – 92% |
| +3 (e.g. 4c vs 1c) | 80 – 96% |
| +4 (e.g. 5c vs 1c) | 88 – 98% |

> Units at the same cost tier should win ~45–55% of their mirror matchup.

### Simulation Rules

- Base stats only (no traits, no shields, no Striker stacks, no mana/abilities)
- `IncludeCrit = false` by default — set to `true` to check CRIT variance impact
- Movement ignored — both units start in range (pure 1v1 DPS race)
- Float accumulator mirrors the resolver exactly
- Timeout at 1200 ticks: winner is the unit with more HP remaining

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
| AS accumulator | `AutoBattleResolver.cs` | `ActionProgress` field, BattleLoop accumulation block |
| Simulation tool | `Assets/Editor/BalanceValidator.cs` | Implementation |
