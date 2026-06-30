# TraitSystem

> **Status**: Draft
> **Last Updated**: 2026-06-30
> **Implements Pillar**: Strategic & Empowering — dual-axis trait synergies are the primary build-diversity engine; hitting a breakpoint makes the team visibly and dramatically stronger

## Summary

TraitSystem counts how many champions on the active battle board share each trait, determines which breakpoints are active, and applies their effects in two passes: (1) `TraitEffectApplier.Apply()` modifies combatant stats directly on `AutoBattleResolver` before the simulation begins, and (2) new per-unit flags on the internal `Combatant` class govern runtime combat behaviors (shields, omnivamp, dashes, stacks, etc.) during the tick loop.

In the **Prototype phase**, trait counting is triggered at **placement time** by `BattleBoardManager` — there is no RunManager and no School scene yet. All 10 champions are defined in `ChampionRoster.cs`. Trait counts update in real time via `TraitTracker` as the player drags champions onto the hex board.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `AutoBattleResolver`, `BattleBoardManager`

---

## Champion Model

Each champion has **two traits** — one Vertical and one Horizontal — plus a Role. Every placed champion contributes to both its Vertical and Horizontal trait count simultaneously.

```csharp
public enum VerticalTrait   { None, Vanguard, Striker, Elementalist, Ranger }
public enum HorizontalTrait { None, Kinetic, Dreadknight, Warden, Trickster }
public enum ChampionRole    { Tank, Carry, Support }
```

### Full Champion Roster

| Champion | Cost | Vertical | Horizontal | Role | HP | ATK | DEF | MG | MR | AS | CRIT | Range |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Ironclad | 1 | Vanguard | Warden | Tank | 100 | 13 | 18 | 4 | 12 | 0.21 | 2 | 1 |
| Bloodhound | 1 | Striker | Dreadknight | Carry | 70 | 18 | 5 | 0 | 5 | 0.33 | 12 | 1 |
| Pyromancer | 1 | Elementalist | Kinetic | Carry | 60 | 8 | 3 | 20 | 8 | 0.28 | 8 | 2 |
| Windrunner | 2 | Ranger | Kinetic | Carry | 75 | 20 | 5 | 0 | 6 | 0.42 | 14 | 3 |
| Grove Keeper | 2 | Elementalist | Warden | Support | 85 | 7 | 8 | 26 | 14 | 0.24 | 4 | 2 |
| Shadowblade | 2 | Striker | Trickster | Carry | 65 | 24 | 4 | 0 | 6 | 0.56 | 20 | 1 |
| Phalanx | 3 | Vanguard | Dreadknight | Tank | 145 | 16 | 26 | 5 | 18 | 0.21 | 3 | 1 |
| Stormbringer | 3 | Ranger | Warden | Support | 95 | 18 | 8 | 30 | 12 | 0.33 | 7 | 3 |
| Phantom Assassin | 4 | Elementalist | Trickster | Carry | 85 | 15 | 6 | 42 | 10 | 0.42 | 22 | 2 |
| Dread Overlord | 5 | Vanguard | Trickster | Tank | 195 | 26 | 36 | 8 | 26 | 0.24 | 4 | 1 |

> **AS** = AttackSpeed (attacks/second). `ActionInterval = round(1 / (AS × 0.6))`. See `balance-framework.md` for the full formula rationale.

> **Magic users**: Elementalist champions and Support champions with MG > ATK receive `BattleBehaviorFlag.MagicAttack` automatically via `ChampionData.ToStudentCombatData()`. This switches their damage formula to MG vs MR.

---

## Trait Breakpoints & Effects

### Vertical Traits

#### Vanguard (Breakpoints: 2 / 4 / 6 / 8)

Affects: Vanguard-trait champions only (unless noted at 8).

| Breakpoint | DEF bonus | MR bonus | Extra |
|---|---|---|---|
| 2 | +25 | +25 | — |
| 4 | +60 | +60 | — |
| 6 | +120 | +120 | Tank-role Vanguards get +30 more DEF & MR |
| 8 | +250 | +250 | ALL placed allies (any trait) get +60 DEF & MR |

Applied as flat additive bonuses to `Combatant.DEF` and `Combatant.MR` before battle via `ApplyPreBattleTraitModifiers`.

#### Striker (Breakpoints: 2 / 4 / 6 / 8)

Affects: Striker-trait champions only. Stacks are accumulated **in combat**, not pre-battle.

| Breakpoint | ATK % per stack (max 8 stacks) | Extra at max stacks | Extra always |
|---|---|---|---|
| 2 | +6% | — | — |
| 4 | +12% | — | — |
| 6 | +20% | Carry-role Strikers: ActionInterval −30% | — |
| 8 | +35% | Carry-role Strikers: ActionInterval −30% | Attacks bypass 40% of target's DEF |

Runtime: `Combatant.StrikerPctPerStack` set pre-battle. Stack count increments each attack (capped at 8). Total ATK multiplier = `1 + stacks × StrikerPctPerStack`.

#### Elementalist (Breakpoints: 2 / 4 / 6 / 8)

Affects: Elementalist-trait champions only.

| Breakpoint | MG bonus | Kill explosion |
|---|---|---|
| 2 | +25 | — |
| 4 | +55 | — |
| 6 | +95 | On kill: 10% of killed unit's MaxHP as magic damage to all adjacent enemies |
| 8 | +150 | On kill: 20% of killed unit's MaxHP as **true damage** to adjacent enemies; chains to secondary kills |

True damage at BP8 ignores MR. Explosion damage is handled in `HandleKill` inside `AutoBattleResolver`.

#### Ranger (Breakpoints: 2 / 4 / 6)

Affects: Ranger-trait champions only.

| Breakpoint | Range bonus (cumulative) | ActionInterval | Damage bonus |
|---|---|---|---|
| 2 | +1 | −15% | — |
| 4 | +1 (total +2) | −15% | +5% dmg per hex distance to target |
| 6 | +2 (total +3) | −15% | +12% dmg per hex distance |

Range bonus applied pre-battle. `RangerBonusDmgPerHex` is set per unit and multiplied onto computed damage after the base formula.

---

### Horizontal Traits

#### Kinetic (Breakpoints: 2 / 4)

Affects: All placed player units (not just Kinetic-trait units) receive the mana restoration.

| Breakpoint | Mana per interval | Support bonus | Starting mana |
|---|---|---|---|
| 2 | 5 | +10 (Supports get 15 total) | 0 |
| 4 | 15 | +10 (Supports get 25 total) | +30 for ALL allies |

Mana interval = every 5 ticks (≈3 seconds at 0.6s/tick). When a unit's Mana reaches 100 it resets to 0 and triggers a **bonus attack** immediately. This is the prototype approximation of "ability cycle acceleration" before a full ability system exists.

#### Dreadknight (Breakpoints: 2 / 4)

Affects: Dreadknight-trait champions only.

| Breakpoint | Omnivamp | Low-HP shield |
|---|---|---|
| 2 | 15% | — |
| 4 | 30% | Below 40% HP: gain Shield = 25% MaxHP (triggers once per combat) |

Omnivamp: each attack, attacker heals `damage_dealt × OmnivampPct` (floored, capped at MaxHP). Applied after shield absorption.

#### Warden (Breakpoints: 2 / 3 / 4)

Affects: Warden-trait champions only. "Front rows" = player rows 2–3 (nearest to enemy). "Back rows" = player rows 0–1.

| Breakpoint | Shield (front rows) | MG bonus (back rows) | ActionInterval (all Wardens) |
|---|---|---|---|
| 2 | 250 | +20 | — |
| 3 | 450 | +35 | −15% |
| 4 | 750 | +60 | −30% |

Shield is pre-applied before battle via `InitialShield`. Absorbed by the damage formula before HP reduction. Front/back determined by `placements[id].Row >= 2`.

#### Trickster (Breakpoints: 2 / 4)

Affects: Trickster-trait champions only.

| Breakpoint | Effect |
|---|---|
| 2 | Below 50% HP: become untargetable for 2 ticks and dash to the hex adjacent to the furthest enemy backliner. Triggers once per combat. |
| 4 | First attack after dash applies bleed: 25% of target's MaxHP as magic damage over 7 ticks (~4.2s). |

Dash is handled in `AutoBattleResolver.ExecuteTricksterDash()`. Bleed damage lives on the victim (`BleedDamagePerTick`, `BleedTicksRemaining`).

---

## Architecture (Prototype Phase)

Four components collaborate — none of them is "the TraitSystem" singleton described in the Phase 2 GDD. The equivalent functionality is distributed:

| Component | Type | Responsibility |
|---|---|---|
| `ChampionRoster` | MonoBehaviour | Defines all 10 champions; exposes `GetStudents()` and `GetChampionLookup()` |
| `TraitTracker` | MonoBehaviour | Counts traits per placement/unplacement; fires `OnTraitCountsChanged`; computes active breakpoints |
| `TraitEffectApplier` | Static class | Translates active breakpoints → calls `resolver.ApplyPreBattleTraitModifiers()` |
| `TraitHUDController` | MonoBehaviour | Subscribes to `TraitTracker.OnTraitCountsChanged`; renders trait count labels in UGUI |

### Placement Flow

```
Player drags champion to hex tile
  → BattleBoardManager.PlaceStudent()
    → TraitTracker.RegisterPlacement(id, coord, championData)
      → Recalculate trait counts + breakpoints
        → OnTraitCountsChanged fires
          → TraitHUDController updates labels

Player clicks "Start Battle"
  → BattleBoardManager.OnStartBattle()
    → resolver.SetUnitPositions(placements)
    → TraitEffectApplier.Apply(breakpoints, championLookup, placements, resolver)
      → resolver.ApplyPreBattleTraitModifiers(perUnitMods, globalSettings)
    → resolver.BeginBattle()
```

### Data Contracts

```csharp
// Per-unit trait modifiers injected into AutoBattleResolver before battle
public struct CombatantTraitModifiers
{
    public ChampionRole Role;
    public bool         IsFrontRow;
    public int          BonusDEF, BonusMR, BonusMG, BonusATK, BonusRange;
    public float        ActionIntervalMult;   // 1.0 = no change; multiplicative chaining
    public int          InitialShield;
    public int          InitialMana;
    public float        OmnivampPct;
    public bool         DreadknightShieldOnLowHP;
    public float        StrikerPctPerStack;
    public bool         StrikerBypassArmor;
    public bool         StrikerMaxStackSpeedBonus;
    public bool         TricksterDashEnabled;
    public bool         TricksterBleedEnabled;
    public float        ElementalistExplosionPct;
    public bool         ElementalistTrueDamage;
    public float        RangerBonusDmgPerHex;
}

// Global resolver settings (Kinetic mana tick, etc.)
public struct ResolverTraitSettings
{
    public bool KineticEnabled;
    public int  KineticManaPerInterval;
    public bool KineticSupportExtraBonus;
}
```

### TraitTracker — Breakpoint Resolution

```
traitCount[trait] = count of placed champions where VerticalTrait == trait OR HorizontalTrait == trait

activeBreakpoint[trait] = highest threshold T such that thresholds[T] ≤ traitCount[trait]
                          (0 if no threshold met)
```

Breakpoints table:

| Trait | Thresholds |
|---|---|
| Vanguard | 2, 4, 6, 8 |
| Striker | 2, 4, 6, 8 |
| Elementalist | 2, 4, 6, 8 |
| Ranger | 2, 4, 6 |
| Kinetic | 2, 4 |
| Dreadknight | 2, 4 |
| Warden | 2, 3, 4 |
| Trickster | 2, 4 |

With 10 champions, max achievable breakpoints: Vanguard 2, Striker 2, Elementalist 2, Ranger 2, Kinetic 2, Dreadknight 2, Warden 3, Trickster 2. Higher breakpoints (4/6/8) are reserved for future champions.

---

## Core Rules

1. Each champion contributes to **exactly two** trait counts: its Vertical trait and its Horizontal trait.
2. Trait counts are rebuilt from scratch on every `RegisterPlacement` / `UnregisterPlacement` — no incremental updates.
3. `TraitEffectApplier.Apply()` must be called **after** `SetUnitPositions()` and **before** `BeginBattle()`. Calling it after battle starts logs an error and no-ops.
4. `ActionIntervalMult` chains multiplicatively across traits: if Ranger gives ×0.85 and Warden(3) gives ×0.85, a Ranger+Warden unit gets `×0.85 × 0.85 = ×0.7225`. Applied as: `newInterval = Max(1, (int)(baseInterval × mult))`.
5. Warden front/back split is determined by the player's chosen row at the moment `OnStartBattle` fires — changing position after that point has no effect.
6. Trickster dash is one-time per combat per unit (`TricksterDashTriggered` guards it). It resets between battles.
7. Dreadknight low-HP shield is one-time per combat per unit (`DreadknightShieldGranted` guards it).
8. Striker stacks persist for the full battle — they are never reset mid-combat. Reset occurs when the next battle initializes combatants.
9. Elementalist kill explosions at BP8 chain: a secondary kill from the explosion also triggers an explosion. This is capped to 1 level of chaining to avoid runaway cascades (secondary kills do not chain further).
10. Kinetic bonus actions fire in the same tick as the mana threshold is crossed — they are queued and resolved after the standard ready-actor loop.

---

## Interactions with Other Systems

| System | Interaction |
|---|---|
| `BattleBoardManager` | Calls `TraitTracker.RegisterPlacement` / `UnregisterPlacement` on every drag-drop; calls `TraitEffectApplier.Apply()` in `OnStartBattle()` |
| `AutoBattleResolver` | Receives `ApplyPreBattleTraitModifiers(perUnitMods, globalSettings)` before `BeginBattle()`; reads new Combatant fields (Shield, OmnivampPct, StrikerPctPerStack, etc.) during simulation |
| `ChampionRoster` | Provides `GetStudents()` as a drop-in replacement for `StudentRosterStub.GetStudents()` in the resolver's lazy-init path |
| `TraitHUDController` | Subscribes to `TraitTracker.OnTraitCountsChanged`; read-only; never calls trait-mutating methods |

---

## Formulas

### Damage with Striker Stacks

```
stackMult = 1 + (StrikerStacks × StrikerPctPerStack)
rawOffense = ATK × stackMult

if StrikerBypassArmor:
    effectiveDEF = target.DEF × 0.6

damage = Max(1, floor(rawOffense × 100 / (100 + effectiveDEF)))
```

### Ranger Distance Bonus

```
damage = baseDamage × (1 + RangerBonusDmgPerHex × hexDistance)
```

Applied after the base LoL formula, physical attacks only.

### Warden ActionInterval

```
newInterval = Max(1, floor(baseInterval × ActionIntervalMult))
```

ActionIntervalMult chains: each multiplying trait applies `mult *= (1 - reductionPct)`.

### Omnivamp Heal

```
healAmount = floor(rawDamageDealt × OmnivampPct)
attacker.CurrentHP = Min(MaxHP, CurrentHP + healAmount)
```

`rawDamageDealt` = shield-absorbed + HP-damage combined.

### Trickster Bleed

```
BleedDamagePerTick = Max(1, floor(target.MaxHP × 0.25 / 7))
BleedTicksRemaining = 7
```

Applied to victim as magic damage each tick (bypasses all mitigation — true DoT).

### Elementalist Explosion

```
// Physical variant (BP6):
explodeDmg = floor(killed.MaxHP × 0.10 × (100 / (100 + hit.MR)))

// True damage (BP8):
explodeDmg = floor(killed.MaxHP × 0.20)
```

---

## Tuning Knobs

All values are defined as constants in `ChampionRoster.cs` and the `CombatantTraitModifiers` defaults set by `TraitEffectApplier`. Tunable without recompile by extracting to a `TraitConfig` ScriptableObject in a future pass.

| Parameter | Default | Notes |
|---|---|---|
| Vanguard DEF/MR per breakpoint | 25 / 60 / 120 / 250 | Higher = tankier frontlines |
| Striker % per stack | 6% / 12% / 20% / 35% | Higher = more burst scaling |
| Elementalist MG bonus | 25 / 55 / 95 / 150 | Higher = stronger magic output |
| Ranger attack speed | ×1.176 AttackSpeedMult (+17.6% AS) | Higher mult = faster fire rate |
| Kinetic mana per interval | 5 (BP2) / 15 (BP4) | Higher = more frequent bonus attacks |
| Dreadknight omnivamp | 15% (BP2) / 30% (BP4) | Higher = more sustain |
| Warden shield | 250 / 450 / 750 | Higher = more frontline durability |
| Trickster bleed | 25% MaxHP / 7 ticks | Duration or dmg adjustable independently |
| Mana max | 100 | Lowering = more frequent bonus attacks |
| Mana tick interval | 5 ticks | Lowering = faster mana ramp |

---

## UI Requirements (Prototype)

| Information | Display Location | Update Trigger |
|---|---|---|
| Per-trait count (e.g. "Vanguard 2/4") | `TraitHUDController` panel (Battle scene, placement phase) | `TraitTracker.OnTraitCountsChanged` |
| Active breakpoint highlight | Gold label color | When `activeBreakpoint[trait] > 0` |
| Partial progress | White label color | When count > 0 but no breakpoint met |
| Inactive trait | Gray label color | When count = 0 |

---

## Acceptance Criteria

- [ ] Placing Ironclad + Phalanx shows "Vanguard 2/4" in gold in the HUD
- [ ] Removing one Vanguard drops label to white "Vanguard 1/2"
- [ ] Starting battle with 2 Vanguards logs `[Trait] Ironclad: DEF=43 MR=37` (base + 25 bonus)
- [ ] Striker stacks increase effective damage each attack; damage log shows growth
- [ ] Trickster dash fires when unit HP crosses below 50%; unit is untargetable for 2 ticks
- [ ] Warden front-row units absorb shield damage before HP drops
- [ ] Kinetic mana logs bonus action after 20 ticks (2× mana intervals at 5 mana/5ticks to reach 100)
- [ ] Elementalist BP2 correctly adds +25 MG to Elementalist units
- [ ] `TraitEffectApplier.Apply()` called after battle start logs error and no-ops
- [ ] All 8 trait breakpoint effects can be observed in play mode without runtime exceptions

---

## Open Questions

| Question | Owner | Resolution |
|---|---|---|
| Should mana bonus action be a copy of the unit's standard attack, or a separate "ability"? | Designer | Prototype: standard attack copy. Full ability system deferred to Phase 2. |
| Should Elementalist BP8 chain limit be 1 or unlimited? | Designer | Prototype: 1 chain (no secondary chain) to avoid runaway cascades. |
| Should Trickster dash re-trigger between battles or reset only on new combat? | Designer | Resets per combat; does not re-trigger mid-combat after triggering once. |
