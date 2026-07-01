# Active Skill System

> **Status**: Draft
> **Last Updated**: 2026-07-01
> **Implements Pillar**: Empowering — the payoff moment; the player watches the team they built and trained cast powerful abilities to dismantle the enemy.

## Summary

The Active Skill System introduces mana, mana generation rules, and active spellcasting to all 10 heroes in the auto-battler. Each hero is assigned an ability archetype (such as Projectile, Laser, Area AoE, Bouncing Chain, Teleport, or Self-Buff) and a unique active ability that triggers automatically when their mana bar reaches maximum capacity ($100\%$ mana).

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `AutoBattleResolver`, `HexGrid`, `BattleBoardManager`

---

## Overview

Currently, champions in the `AutoBattleResolver` only basic attack or move, and mana is used solely as a tick counter for the Kinetic horizontal trait (triggering bonus auto-attacks). The Active Skill System replaces this placeholder by giving every unit a functional mana pool, custom mana generation (from basic attacking and taking pre-mitigated damage), a cast lockout state machine, stackable targeting logic, and custom skill execution based on 7 defined archetype templates.

## Player Fantasy

The player watches their champions trade blows, build up their blue mana bars, and trigger visual spell casts. The tank slamming their shield to protect the front line, the assassin vanishing to strike the back line, and the mage channeling a linear laser beam create distinct, high-impact payoff moments that validate the player's team building.

---

## Detailed Design

### Core Rules

1.  **Mana Bar Initialization**: Every combatant has a current `Mana` (default $0$), `MaxMana` (default $100$), and `StartingMana` (default $0$, unless overridden by traits or champion definitions).
2.  **Mana Generation**:
    *   **Attacking**: Each basic attack performed grants $+10$ mana to the attacker upon hit resolution.
    *   **Defending**: When a unit takes damage, it generates mana based on the **pre-mitigated damage** (damage calculated before Armor, MR, or Shields reduce it).
3.  **Casting Lockout**: When a unit's mana reaches its `MaxMana` threshold, it stops basic attacking, consumes all mana (resetting to $0$, plus any overflow), and triggers its active skill.
    *   During the casting window (typically 1–2 ticks depending on the skill archetype), the unit's combat state is set to `Casting` or `Channeling`.
    *   While casting, the unit is locked out of basic attacks and cannot generate mana.
    *   If the unit is stunned or silenced, any active channel is cancelled immediately.
4.  **Casting & Silences**: A unit with the `Silenced` status effect cannot cast skills and cannot generate mana until the silence expires.
5.  **Stackable Targeting Selectors**: Abilities select targets by combining a spatial **Base Filter** and one or more **Priority Sorts**.

---

### Stackable Targeting System

| Base Filter | Target List Scoped To |
| :--- | :--- |
| **Adjacent** | The 6 surrounding hex tiles (1-hex distance). |
| **Within Range** | Enemy units within $R$ hex distance from the caster. |
| **Global** | All active enemy units on the board. |
| **Linear Path** | Hexes intersected by a line from the caster through the target hex to the board edge. |

| Priority Sort | Sorting Rule |
| :--- | :--- |
| **Current Target** | The target currently being basic-attacked. |
| **Furthest** | Sorts filtered list by hex distance (descending). |
| **Closest** | Sorts filtered list by hex distance (ascending). |
| **Lowest HP %** | Sorts filtered list by (Current HP / Max HP) (ascending). |
| **Largest Cluster** | Sorts hexes by the highest count of enemies within a radius of $C$ hexes. |

---

### States and Transitions

Units in `AutoBattleResolver` transition through the following states:

| State | Entry Condition | Exit Condition | Behavior |
|---|---|---|---|
| **Idle** | Default/Spawn | Timer ready $\rightarrow$ `Attacking` / `Moving` | Waiting for action tick |
| **Moving** | No enemy in range on tick | Step completed | Moves 1 hex toward target |
| **Attacking** | Ready tick + enemy in range | Action resolves | Plays attack animation, deals damage |
| **Casting** | Mana $\ge$ MaxMana | Cast duration ticks expire | Plays cast animation, locks attacks, fires spell |
| **Channeling** | Mana $\ge$ MaxMana (Channel type) | Channel ticks expire OR Stunned | Plays continuous animation, deals tick damage |
| **Stunned** | CC effect applied | CC timer expires | Frozen, cannot act, action timers paused |

---

## Formulas

### Mana Gain from Damage Taken
This formula calculates how much mana a unit generates when hit, scaling with the severity of the hit relative to their maximum health, using pre-mitigated damage to ensure armor/shields do not nerf mana gain.

```
ManaGain = max(1, (PreMitigatedDamage * ManaDamageMultiplier) / MaxHP) * 100
```

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `PreMitigatedDamage` | int | $1$ to $\infty$ | Combat event | Raw damage calculated before DEF/MR/Shield mitigation |
| `ManaDamageMultiplier` | float | $0.05$ to $0.15$ | Tuning knob | Constant multiplier (Default: `0.08`) |
| `MaxHP` | int | $50$ to $500$ | Champion Data | Caster's maximum health pool |

**Expected output range**: $1$ to $40$ mana per hit.  
**Edge cases**: Clamp to maximum of $40$ mana per single hit to prevent instant $0$-to-$100$ casts on massive damage spikes.

---

## Skill Archetype Templates

All active abilities are implemented by extending or parameterizing these 7 templates:

### Template A: Standard Projectile
*   **Targeting**: `Base Filter` $\rightarrow$ `Priority Sort`.
*   **Delivery**: Projectile travels at speed $V$ (hexes/sec) to the target's hex. Single-target collision.
*   **Resolution**: Deals magic/physical damage and applies status effects on impact.
*   **Lockout**: Caster pauses basic attacks for 1 tick during cast launch.

### Template B: Exploding Projectile
*   **Targeting**: `Base Filter` $\rightarrow$ `Priority Sort`.
*   **Delivery**: Projectile travels to target hex; collides with the first enemy unit intersected.
*   **Resolution**: Triggers an AoE explosion in a radius of $R$ hexes. Deals $100\%$ damage to the collided unit, and $Splash\%$ damage to adjacent enemies.
*   **Lockout**: Caster locked out of attacks for 2 ticks (1 tick launch, 1 tick recovery).

### Template C: Laser / Piercing Beam
*   **Targeting**: `Linear Path` from Caster through `Target Hex` to range $R$.
*   **Delivery**: Beam instantly manifests along the path.
*   **Collision**: Piercing (hits all units on path).
*   **Resolution**: Deals damage and shreds defensive stats of all units hit.
*   **Lockout**: Caster locked out of attacks for 2 ticks.

### Template D: Ground-Targeted AoE (Hex Area)
*   **Targeting**: `Base Filter` $\rightarrow$ `Largest Cluster (Radius C)`.
*   **Delivery**: Spawns an effect directly on that hex after a brief warning delay.
*   **Resolution**: Deals damage (instant or over time) and crowd control (stun/root) to all enemies inside the $C$-radius zone.
*   **Lockout**: Caster locked out of attacks for 1 tick.

### Template E: Bouncing Chain
*   **Targeting**: 
    1. First hit: `Within Range` $\rightarrow$ `Current Target`.
    2. Next hit: `Within Bounce Range` $\rightarrow$ `Closest Untargeted`.
*   **Delivery**: Projectile arcs from target to target up to $N$ times.
*   **Resolution**: Deals damage/effects per bounce.
*   **Lockout**: Caster locked out of attacks for 1 tick.

### Template F: Blink & Strike (Teleport)
*   **Targeting**: `Base Filter` $\rightarrow$ `Priority Sort`.
*   **Delivery**: Caster vanishes and instantly teleports to an open adjacent hex of the target.
*   **Resolution**: Plays a strike animation dealing physical/magic damage and drops current enemy threat (aggro reset).
*   **Lockout**: Caster locked out of attacks for 1 tick.

### Template G: Self-Buff / Steroid
*   **Targeting**: Self.
*   **Delivery**: Caster plays an activation animation (particle burst/aura).
*   **Resolution**: Grants temporary stats or alters basic attacks (AS buff, splash, bleed) for $T$ ticks.
*   **Lockout**: Caster locked out of attacks for 1 tick (activation cast).

---

## Hero Skill Definitions

### 1. Ironclad (1 Gold, Vanguard / Warden, Tank)
*   **Template**: `Template G: Self-Buff` (with frontal cone slam)
*   **TFT Counterpart**: *Poppy / Leona (1-Cost)*
*   **Mana**: 0/80
*   **Targeting**: Current Target (Frontal Cone)
*   **Skill: Iron Shield Slam**: Deals physical damage ($1.2 \times \text{ATK} + 0.4 \times \text{DEF}$) in a 3-hex cone. Gains a **200 HP Shield** for 3 ticks.

### 2. Bloodhound (1 Gold, Striker / Dreadknight, Carry)
*   **Template**: `Template G: Self-Buff / Steroid`
*   **TFT Counterpart**: *Olaf / Warwick (1-Cost)*
*   **Mana**: 0/60
*   **Targeting**: Self
*   **Skill: Blood Frenzy**: Enters a bloodlust for 5 ticks. Gains **+40% Attack Speed** and **doubles Dreadknight Omnivamp** healing. Attacks during Frenzy grant 2 Striker stacks.

### 3. Pyromancer (1 Gold, Elementalist / Kinetic, Carry)
*   **Template**: `Template B: Exploding Projectile`
*   **TFT Counterpart**: *Brand / Annie (1-Cost)*
*   **Mana**: 0/90
*   **Targeting**: Current Target (1-hex splash radius)
*   **Skill: Fireball Blast**: Launches a fireball that explodes on first collision. Deals magic damage ($2.2 \times \text{MG}$) to the target and $50\%$ splash damage to adjacent enemies.

### 4. Windrunner (2 Gold, Ranger / Kinetic, Carry)
*   **Template**: `Template C: Laser / Piercing Beam`
*   **TFT Counterpart**: *Caitlyn / Ezreal (2-Cost)*
*   **Mana**: 30/100
*   **Targeting**: Linear Path (Through Current Target)
*   **Skill: Gale Piercer**: Channels for 2 ticks, then fires a piercing arrow dealing physical damage ($1.8 \times \text{ATK}$) to all enemies standing on the line, amplified by **+8% per hex distance**.

### 5. Grove Keeper (2 Gold, Elementalist / Warden, Support)
*   **Template**: `Template D: Ground-Targeted AoE`
*   **TFT Counterpart**: *Zyra / Lulu (2-Cost)*
*   **Mana**: 0/120
*   **Targeting**: Global $\rightarrow$ Largest Cluster (1-hex radius)
*   **Skill: Verdant Roots**: Spawns briars under the largest concentration of enemies. Deals magic damage ($1.2 \times \text{MG}$) over 3 ticks and **roots** them for the duration.

### 6. Shadowblade (2 Gold, Striker / Trickster, Carry)
*   **Template**: `Template F: Blink & Strike`
*   **TFT Counterpart**: *Katarina / Talon (2-Cost)*
*   **Mana**: 0/50
*   **Targeting**: Global $\rightarrow$ Furthest Enemy
*   **Skill: Shadow Strike**: Teleports behind furthest enemy. Deals critical physical damage ($1.5 \times \text{ATK} \times \text{CRIT Multiplier}$) and inflicts bleed ($15\%$ of target's Max HP as magic damage over 4 ticks).

### 7. Phalanx (3 Gold, Vanguard / Dreadknight, Tank)
*   **Template**: `Template G: Self-Buff` (with Taunt)
*   **TFT Counterpart**: *Shen / Taric (3-Cost)*
*   **Mana**: 0/100
*   **Targeting**: Self (2-hex radius Taunt)
*   **Skill: Unbreakable Bastion**: Gains a shield equal to **25% of Max HP** and Taunts enemies within 2 hexes for 3 ticks. Intercepts $30\%$ of damage taken by adjacent allies.

### 8. Stormbringer (3 Gold, Ranger / Warden, Support)
*   **Template**: `Template E: Bouncing Chain`
*   **TFT Counterpart**: *Sona / Janna (3-Cost)*
*   **Mana**: 0/80
*   **Targeting**: 
    - Initial: Range 3 $\rightarrow$ Current Target.
    - Bounces: Range 3 from last bounce $\rightarrow$ Closest Untargeted.
*   **Skill: Chain Lightning**: Bounces up to 4 times. Deals magic damage ($1.4 \times \text{MG}$) and **Silences** them for 2 ticks.
*   **Resolved ambiguity**: an earlier draft also said "Shields hit allies for 150 HP," which didn't fit the chain's enemies-only targeting (Current Target → Closest Untargeted). Dropped per design decision — Chain Lightning is damage + Silence only, no ally shield.

### 9. Phantom Assassin (4 Gold, Elementalist / Trickster, Carry)
*   **Template**: `Template F: Blink & Strike (Multi-Target Teleportation)`
*   **TFT Counterpart**: *Akali / Fiora (4-Cost)*
*   **Mana**: 0/110
*   **Targeting**: Global/Local $\rightarrow$ Successive lowest HP % units
*   **Skill: Phantom Flurry**: Becomes untargetable and teleports to strike 3 low-HP enemies in rapid succession, dealing magic damage ($3.2 \times \text{MG}$) to each, before returning.

### 10. Dread Overlord (5 Gold, Vanguard / Trickster, Tank)
*   **Template**: `Template D: Ground-Targeted AoE (Leap & Crash)`
*   **TFT Counterpart**: *Sion / Aatrox / Ornn (5-Cost)*
*   **Mana**: 50/150
*   **Targeting**: Global $\rightarrow$ Largest Cluster (2-hex radius)
*   **Skill: Dread Cataclysm**: Leaps and crashes on the largest cluster of enemies. Deals magic damage ($2.0 \times \text{MG}$), knocks up and **stuns** all enemies within 2 hexes for 2 ticks. Impacted cells become a "Dread Zone" reducing enemy defenses by $40\%$ for 5 ticks.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|---|---|---|
| Mana reaches 100 during basic attack animation | Let the active attack animation finish, then trigger the cast lockout on the next tick. | Prevents frame cancellation of auto-attacks. |
| Multiple units cast on the same tick | Order casting resolution by unit speed (SPD) descending. | High-speed casters should get their spells off first. |
| Target dies while projectile is in flight | The projectile still travels to the destination hex and despawns or explodes. | Keeps visual tracking logical for the player. |
| Cast is interrupted by a stun | The casting state is exited, mana is lost, and the unit transitions to `Stunned`. | Standard CC counter-play. |

---

## Dependencies

| System | Direction | Nature |
|---|---|---|
| `AutoBattleResolver` | This modifies it | Integrates casting states, mana loops, and skill resolution formulas. |
| `HexGrid` | This reads it | Uses hex grid coordinates and neighbor checks to evaluate targeting paths and clusters. |
| `BattleHUD` | It depends on this | Renders mana bars and skill casting names/animations. |

---

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|---|---|---|---|---|
| `ManaDamageMultiplier` | 0.08 | 0.02 - 0.20 | Units generate more mana when damaged | Units generate less mana when damaged |
| `CastLockoutDuration` | 1 tick | 1 - 3 ticks | Spells take longer to cast (locks unit longer) | Spells cast faster (more responsive) |
| `MaxManaGainPerHit` | 40 | 10 - 100 | Limits spike mana gains from single hits | Makes tanks cast slower under heavy fire |

---

## Acceptance Criteria

- [ ] Every champion gains $+10$ mana per basic attack hit.
- [ ] Every champion gains mana when taking damage according to the pre-mitigated formula.
- [ ] A unit reaching $100\%$ mana pauses basic attacking, plays a casting state loop, consumes its mana, and triggers its skill behavior.
- [ ] Linear, cluster, bouncing, blink, and area targeting functions choose hexes correctly according to filters and sorts.
- [ ] Silenced units cannot cast skills or generate mana.
- [ ] Stunned units interrupt active channels and lose mana.
- [ ] All skill damage and shielding scales precisely with champion stats ($\text{ATK}, \text{DEF}, \text{MG}$) as detailed in their GDD definitions.
- [ ] Visual log output or floating text notifies the board when a skill is cast.
