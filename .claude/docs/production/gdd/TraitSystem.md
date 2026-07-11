# TraitSystem

> **Status**: Draft
> **Last Updated**: 2026-07-01
> **Implements Pillar**: Strategic & Empowering — dual-axis trait synergies are the primary build-diversity engine; hitting a breakpoint makes the team visibly and dramatically stronger.

## Summary

TraitSystem counts how many champions on the active battle board share each trait, determines which breakpoints are active, and applies their effects in two passes: (1) `TraitEffectApplier.Apply()` modifies combatant stats directly on `AutoBattleResolver` before the simulation begins, and (2) new per-unit variables on the internal `Combatant` class govern runtime combat behaviors during the tick loop.

The roster consists of **30 champions** distributed across **15 traits** (7 Vertical, 8 Horizontal).

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `AutoBattleResolver`, `BattleBoardManager`

---

## Champion Model

Each champion has **two traits** — one Vertical and one Horizontal — plus a Role. Every placed champion contributes to both its Vertical and Horizontal trait count simultaneously.

```csharp
public enum VerticalTrait   { None, Vanguard, Striker, Elementalist, Ranger, Astral, Wild, Shadow }
public enum HorizontalTrait { None, Kinetic, Dreadknight, Warden, Trickster, Oracle, Guardian, Tech, Void }
public enum ChampionRole    { Tank, Carry, Support }
```

### Full Champion Roster

| Champion | Cost | Vertical | Horizontal | Role | HP | ATK | DEF | MG | MR | AS | CRIT | Range |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| **Ironclad** | 1 | Vanguard | Warden | Tank | 100 | 13 | 18 | 4 | 12 | 0.21 | 2 | 1 |
| **Bloodhound** | 1 | Striker | Dreadknight | Carry | 70 | 18 | 5 | 0 | 5 | 0.33 | 12 | 1 |
| **Pyromancer** | 1 | Elementalist | Kinetic | Carry | 60 | 8 | 3 | 20 | 8 | 0.28 | 8 | 2 |
| **Aegis** | 1 | Vanguard | Guardian | Tank | 100 | 11 | 15 | 0 | 10 | 0.21 | 2 | 1 |
| **Wildcat** | 1 | Wild | Trickster | Carry | 70 | 15 | 5 | 0 | 5 | 0.33 | 12 | 1 |
| **Cosmic Sprite** | 1 | Astral | Kinetic | Support | 60 | 7 | 5 | 18 | 8 | 0.28 | 8 | 2 |
| **Novice Cleric** | 1 | Astral | Oracle | Support | 65 | 6 | 5 | 18 | 8 | 0.25 | 5 | 2 |
| **Tech Scrapper** | 1 | Striker | Tech | Carry | 75 | 16 | 6 | 0 | 5 | 0.30 | 10 | 1 |
| **Windrunner** | 2 | Ranger | Kinetic | Carry | 75 | 20 | 5 | 0 | 6 | 0.42 | 14 | 3 |
| **Grove Keeper** | 2 | Elementalist | Warden | Support | 85 | 7 | 8 | 26 | 14 | 0.24 | 4 | 2 |
| **Shadowblade** | 2 | Striker | Trickster | Carry | 65 | 24 | 4 | 0 | 6 | 0.56 | 20 | 1 |
| **Sun Warden** | 2 | Astral | Warden | Tank | 110 | 12 | 15 | 0 | 12 | 0.21 | 2 | 1 |
| **Venom Stalker** | 2 | Shadow | Void | Carry | 85 | 18 | 6 | 0 | 6 | 0.33 | 14 | 1 |
| **Forest Sentinel**| 2 | Wild | Guardian | Tank | 115 | 11 | 18 | 0 | 12 | 0.20 | 2 | 1 |
| **Void Mage** | 2 | Elementalist | Void | Carry | 80 | 7 | 5 | 24 | 8 | 0.26 | 8 | 2 |
| **Phalanx** | 3 | Vanguard | Dreadknight | Tank | 145 | 16 | 26 | 5 | 18 | 0.21 | 3 | 1 |
| **Stormbringer** | 3 | Ranger | Warden | Support | 95 | 18 | 8 | 30 | 12 | 0.33 | 7 | 3 |
| **Starweaver** | 3 | Astral | Oracle | Support | 120 | 10 | 8 | 28 | 10 | 0.28 | 5 | 3 |
| **Rust Colossus** | 3 | Wild | Tech | Tank | 150 | 15 | 22 | 0 | 15 | 0.22 | 3 | 1 |
| **Night Stalker** | 3 | Shadow | Trickster | Carry | 100 | 22 | 8 | 0 | 8 | 0.35 | 18 | 1 |
| **Storm Ranger** | 3 | Ranger | Tech | Carry | 95 | 20 | 8 | 0 | 6 | 0.33 | 10 | 3 |
| **Grave Knight** | 4 | Shadow | Dreadknight | Tank | 160 | 18 | 20 | 0 | 16 | 0.25 | 4 | 1 |
| **Arcane Sage** | 4 | Elementalist | Tech | Carry | 110 | 12 | 8 | 38 | 10 | 0.33 | 18 | 2 |
| **Void Ranger** | 4 | Ranger | Void | Carry | 105 | 25 | 6 | 0 | 6 | 0.38 | 16 | 3 |
| **Divine Paladin** | 4 | Vanguard | Oracle | Support | 170 | 15 | 24 | 20 | 18 | 0.22 | 4 | 1 |
| **Phantom Assassin**| 4 | Elementalist | Trickster | Carry | 85 | 15 | 6 | 42 | 10 | 0.42 | 22 | 2 |
| **Dread Overlord** | 5 | Vanguard | Trickster | Tank | 195 | 26 | 36 | 8 | 26 | 0.24 | 4 | 1 |
| **Cosmic Leviathan**| 5 | Astral | Guardian | Tank | 250 | 25 | 28 | 30 | 22 | 0.25 | 4 | 1 |
| **Reaper** | 5 | Shadow | Void | Carry | 180 | 32 | 10 | 0 | 10 | 0.45 | 22 | 1 |
| **Beastmaster** | 5 | Wild | Warden | Tank | 220 | 22 | 24 | 0 | 20 | 0.24 | 4 | 1 |

---

## Trait Breakpoints & Effects

### Vertical Traits

#### 1. Vanguard (Breakpoints: 2 / 4 / 6 / 8)
*   **(2)** $+25$ Armor & MR.
*   **(4)** $+60$ Armor & MR.
*   **(6)** $+120$ Armor & MR. Tank Vanguards gain $+30$ extra Armor & MR.
*   **(8)** $+250$ Armor & MR. All placed allies gain $+60$ Armor & MR.

#### 2. Striker (Breakpoints: 2 / 4 / 6 / 8)
*   **(2)** $+6\%$ AD per attack (up to 8 stacks).
*   **(4)** $+12\%$ AD per attack (up to 8 stacks).
*   **(6)** $+20\%$ AD per attack. Carry Strikers gain $+30\%$ Attack Speed at max stacks.
*   **(8)** $+35\%$ AD per attack. Attacks bypass $40\%$ target Armor.

#### 3. Elementalist (Breakpoints: 2 / 4 / 6 / 8)
*   **(2)** $+25$ AP.
*   **(4)** $+55$ AP.
*   **(6)** $+95$ AP. On kill, deal $10\%$ enemy max HP as magic damage to adjacent hexes.
*   **(8)** $+150$ AP. Explosion deals $20\%$ max HP as true damage and chains.

#### 4. Ranger (Breakpoints: 2 / 4 / 6)
*   **(2)** $+1$ Hex Range, $+15\%$ Attack Speed.
*   **(4)** $+2$ Hex Range. Attacks deal $+5\%$ damage per hex distance.
*   **(6)** $+3$ Hex Range. Distance multiplier increases to $+12\%$ per hex.

#### 5. Astral (Breakpoints: 2 / 4 / 6) - *NEW*
*   **(2)** Astral champions gain $+25$ AP.
*   **(4)** Astral champions gain $+60$ AP. On casting their first skill, they gain a Shield equal to $20\%$ of their Max HP.
*   **(6)** Astral champions gain $+100$ AP. Active skill casts stun hit enemies for 2 ticks (0.2 seconds).

#### 6. Wild (Breakpoints: 2 / 4 / 6) - *NEW*
*   **(2)** Wild champions gain $+15\%$ Attack Speed.
*   **(4)** Wild champions gain $+30\%$ Attack Speed. Basic attacks deal $+12$ flat bonus physical damage.
*   **(6)** Wild champions gain $+50\%$ Attack Speed. Dropping below $50\%$ HP grants $+30\%$ Omnivamp.

#### 7. Shadow (Breakpoints: 2 / 4 / 6) - *NEW*
*   **(2)** Shadow champions gain $15\%$ Evasion (chance to dodge physical auto-attacks).
*   **(4)** Shadow champions gain $25\%$ Evasion. Dropping below $40\%$ HP silences adjacent enemies for 3 ticks (0.3 seconds).
*   **(6)** Shadow champions gain $40\%$ Evasion. Basic attacks slow target's attack speed by $30\%$ for 3 ticks (0.3 seconds).

---

### Horizontal Traits

#### 1. Kinetic (Breakpoints: 2 / 4)
*   **(2)** Every 3 seconds, allies restore 5 Mana. Supports restore $+10$ Mana.
*   **(4)** Every 3 seconds, allies restore 15 Mana. All allies start combat with $+30$ Mana.

#### 2. Dreadknight (Breakpoints: 2 / 4)
*   **(2)** Units gain $15\%$ Omnivamp.
*   **(4)** Units gain $30\%$ Omnivamp. Below $40\%$ HP, gain a shield equal to $25\%$ max HP for 5 seconds.

#### 3. Warden (Breakpoints: 2 / 3 / 4)
*   **(2)** Front 2 rows gain $250$ Shield for 12s. Back 2 rows gain $+20$ AP.
*   **(3)** Front 2 rows gain $450$ Shield. Back 2 rows gain $+35$ AP, $+15\%$ Attack Speed.
*   **(4)** Front 2 rows gain $750$ Shield. Back 2 rows gain $+60$ AP, $+30\%$ Attack Speed.

#### 4. Trickster (Breakpoints: 2 / 4)
*   **(2)** Below $50\%$ HP, become untargetable for 2 ticks (0.2 seconds) and dash behind furthest backliner (once per combat).
*   **(4)** First attack after dashing inflicts a bleed dealing $25\%$ target max HP as magic damage over 7 ticks.

#### 5. Oracle (Breakpoints: 2 / 3) - *NEW*
*   **(2)** Every 4 seconds, heals the lowest-HP % ally within 2 hexes for $20\%$ of their Max HP.
*   **(3)** Heal becomes global, cleanses stuns/silences, and grants the target $+25\%$ Attack Speed for 3 ticks (0.3 seconds).

#### 6. Guardian (Breakpoints: 2 / 4) - *NEW*
*   **(2)** Once per combat, when an adjacent ally drops below $50\%$ HP, grant them a $250$ HP Shield for 4 seconds.
*   **(4)** Grant a $500$ HP Shield instead; adjacent allies gain $+35$ DEF.

#### 7. Tech (Breakpoints: 2 / 4) - *NEW*
*   **(2)** Start combat with $+150$ Max HP and $+15$ ATK.
*   **(4)** Start combat with $+300$ Max HP, $+30$ ATK, and $+20$ AP.

#### 8. Void (Breakpoints: 2 / 4) - *NEW*
*   **(2)** Attacks and skills execute enemies below $15\%$ HP.
*   **(4)** Execute threshold increased to $25\%$ HP. Triggering an execute deals $20\%$ of the target's Max HP as true damage to all adjacent enemies.

---

## Architecture

Four components collaborate to resolve traits dynamically:

| Component | Responsibility |
|---|---|
| `ChampionRoster` | Defines all 30 champions, their stats, vertical and horizontal trait tags. |
| `TraitTracker` | Counts placed champions and calculates active breakpoints. Exposes `OnTraitCountsChanged`. |
| `TraitEffectApplier` | Takes active breakpoints and calculates runtime modifiers to inject into the resolver. |
| `TraitHUDController` | Listens to `TraitTracker` and prints active synergies onto the UI canvas. |

### Data Structures

```csharp
public struct CombatantTraitModifiers
{
    public ChampionRole Role;
    public bool         IsFrontRow;
    public int          BonusDEF, BonusMR, BonusMG, BonusATK, BonusRange;
    public float        AttackSpeedMult;
    public int          InitialShield, InitialMana;
    public float        OmnivampPct;
    
    // --- Trait Flags & Settings ---
    public bool         DreadknightShieldOnLowHP;
    public float        StrikerPctPerStack;
    public bool         StrikerBypassArmor;
    public bool         StrikerMaxStackSpeedBonus;
    public float        ElementalistExplosionPct;
    public bool         ElementalistTrueDamage;
    public float        RangerBonusDmgPerHex;
    
    // --- New Trait Modifiers ---
    public int          AstralAPBonus;
    public bool         AstralShieldOnCast;
    public bool         AstralStunOnCast;
    public float        WildASMult;
    public int          WildFlatOnHitDmg;
    public bool         WildOmnivampBelowHalf;
    public float        ShadowEvasionPct;
    public bool         ShadowSilencePulseOnLowHP;
    public bool         ShadowSlowOnHit;
    public bool         OracleHealEnabled;
    public bool         OracleCleanses;
    public bool         GuardianShieldEnabled;
    public int          GuardianShieldAmt;
    public int          TechHPBonus, TechATKBonus, TechAPBonus;
    public float        VoidExecuteHPThreshold;
    public bool         VoidExplodeOnExecute;
}
```

---

## Core Rules

1. Each champion contributes to **exactly two** trait counts: its Vertical trait and its Horizontal trait.
2. Trait counts are rebuilt from scratch on every `RegisterPlacement` / `UnregisterPlacement` — no incremental updates. `RegisterPlacement`'s caller (`BattleBoardManager`) is responsible for resolving the placed unit's `ChampionData` before calling it — in production this identity is a `StudentId` GUID bridged via `StudentData.ChampionId` (see `StudentRoster.md`), not the champion's own slug; in the standalone/prototype context (`BattleTest.unity`) the two coincide. `TraitTracker` itself is agnostic to which ID space it's given, as long as the same key is used consistently for register/unregister — see `BattleBoardManager.md` Core Rule 5 for the `_championDataLookup` keying this depends on.
3. `TraitEffectApplier.Apply()` must be called **after** `SetUnitPositions()` and **before** `BeginBattle()`. Calling it after battle starts logs an error and no-ops.
4. Action speed bonuses multiply `AttackSpeed` directly. 
5. Warden front/back split is determined by the player's chosen row at the moment `OnStartBattle` fires.
6. New traits like **Shadow Evasion** allow units to completely dodge physical basic attacks. Dodge results in $0$ damage.
7. New traits like **Void Execute** check the victim's HP post-damage. If below the threshold, the unit dies immediately.

---

## Formulas

### Oracle Healing
```
Heal = Target.MaxHP * 0.20
```

### Void Execute True Damage Splash (BP4)
```
SplashDamage = Target.MaxHP * 0.20  (Bypasses Armor & MR)
```

---

## Tuning Knobs

| Parameter | Default | Notes |
|---|---|---|
| Astral AP bonus | +25 / +60 / +100 | AP boost for Astral characters |
| Wild Attack Speed | +15% / +30% / +50% | AS modifier multiplier |
| Shadow Evasion | 15% / 25% / 40% | Dodge rate against physical attacks |
| Oracle healing | 20% Max HP | Heals lowest HP% ally every 4 seconds |
| Guardian shield | 250 / 500 HP | Lifeline shield to low-HP adjacent allies |
| Void Execute threshold | 15% / 25% HP | Execute target if HP falls below |

---

## UI Requirements

| Information | Display Location | Update Trigger |
|---|---|---|
| Per-trait counts (e.g. "Oracle 1/2") | `TraitHUDController` panel (placement phase) | `TraitTracker.OnTraitCountsChanged` |
| Active breakpoint highlight | Gold label color | When `activeBreakpoint[trait] > 0` |
| Inactive trait | Gray label color | When count = 0 |

---

## Acceptance Criteria

- [x] Dragging any of the 30 champions onto the board increments their corresponding Horizontal and Vertical trait count — `TraitTracker.VerticalToTraitType`/`HorizontalToTraitType` and the `Breakpoints` dictionary now cover all 15 traits (fixed a crash where placing any Astral/Wild/Shadow/Oracle/Guardian/Tech/Void champion threw `ArgumentOutOfRangeException`, previously masked because `RegisterPlacement` never ran in production — see `BattleBoardManager.md` Core Rule 5).
- [ ] In production (`Battle.unity` with `RunManager`), dragging a recruited/trained student (`StudentId` GUID) onto the board increments the same trait counts identically to the standalone champion-ID case.

**Note — implementation status**: counting/breakpoint-detection (`TraitTracker`) is now wired for all 15 traits. `TraitEffectApplier` (the actual combat-effect application — shields, evasion%, execute threshold, etc. for Astral/Wild/Shadow/Oracle/Guardian/Tech/Void) remains unimplemented; those 7 traits will show correct counts and gold breakpoint highlighting in the HUD, but produce no in-battle mechanical effect yet. This is a known follow-up, not a regression from this fix.
- [ ] Placing Aegis and Sun Warden increments Guardian and Astral traits in the HUD.
- [ ] Stun durations and Silence pulses resolve in the C# log using the exact tick values (e.g., 2 ticks = 0.2 seconds, 3 ticks = 0.3 seconds).
- [ ] Dodged physical attacks from Shadow evasion print `DODGE` and deal $0$ damage.
- [ ] Void executes check HP thresholds and immediately trigger target death.
