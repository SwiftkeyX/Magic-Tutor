# Active Skill System

> **Status**: Draft
> **Last Updated**: 2026-07-01
> **Implements Pillar**: Empowering — the payoff moment; the player watches the team they built and trained cast powerful abilities to dismantle the enemy.

## Summary

The Active Skill System introduces mana, mana generation rules, and active spellcasting to all 30 heroes in the auto-battler. Each hero is assigned an ability archetype (such as Projectile, Laser, Area AoE, Bouncing Chain, Teleport, or Self-Buff) and a unique active ability that triggers automatically when their mana bar reaches maximum capacity ($100\%$ mana).

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

Every skill declares a **Target Team** — `Enemy` (default) or `Ally` — which the Base Filter is
scoped to. `Enemy` is the original behavior (all 10 launch heroes use it). `Ally` scopes the same
4 filters and 5 sorts to the caster's own team instead, so e.g. "Global → Lowest HP % ally" reuses
the exact `Global` + `Lowest HP %` building blocks, just aimed at allies. This keeps the filter/sort
vocabulary unified rather than forking a parallel ally-only set.

| Base Filter | Target List Scoped To (Enemy Team) | Target List Scoped To (Ally Team) |
| :--- | :--- | :--- |
| **Adjacent** | The 6 surrounding hex tiles, if enemy-occupied. | The 6 surrounding hex tiles, if ally-occupied. |
| **Within Range** | Enemy units within $R$ hex distance from the caster. | Ally units within $R$ hex distance from the caster. |
| **Global** | All active enemy units on the board. | All active ally units on the board (including self). |
| **Linear Path** | Hexes intersected by a line from the caster through the target hex to the board edge, aimed at the best-sorted enemy in range. | Same, aimed at the best-sorted ally in range. |

| Priority Sort | Sorting Rule |
| :--- | :--- |
| **Current Target** | The target currently being basic-attacked (Enemy team only — has no ally equivalent). |
| **Furthest** | Sorts filtered list by hex distance (descending). |
| **Closest** | Sorts filtered list by hex distance (ascending). |
| **Lowest HP %** | Sorts filtered list by (Current HP / Max HP) (ascending). |
| **Largest Cluster** | Sorts hexes by the highest count of same-team units within a radius of $C$ hexes. |

#### Ally-Support Resolution

When Target Team = `Ally`, an archetype's damage-resolution step is replaced by a support step,
using the same flat-multiplier pattern as damage (mirrors `OffenseMultiplier`, just for support):

| Field | Effect |
| :--- | :--- |
| **Heal Multiplier** | Restores magic healing equal to `caster.MG * HealMultiplier` to the target ally. |
| **Mana Restore** | Restores a flat amount of Mana to the target ally. |
| **Cleanses Status** | If true, removes any active Stun or Silence from the target ally. |
| **Shield (existing fields)** | `FlatShieldAmount` / `ShieldPctOfMaxHP` apply to the target ally instead of the caster when Target Team = `Ally`. |

A single cast can combine these (e.g. a heal-and-cleanse, or a shield-and-mana-restore) — they are
independent flags/values, not a single enum choice.

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
*   **Resolution**: Deals magic/physical damage and applies status effects on impact (Target Team = `Enemy`), or applies Ally-Support Resolution to the target (Target Team = `Ally`, e.g. Novice Cleric's heal).
*   **Lockout**: Caster pauses basic attacks for 1 tick during cast launch.

### Template B: Exploding Projectile
*   **Targeting**: `Base Filter` $\rightarrow$ `Priority Sort`.
*   **Delivery**: Projectile travels to target hex; collides with the first unit intersected (matching Target Team).
*   **Resolution**: Triggers an AoE explosion in a radius of $R$ hexes. Deals $100\%$ damage to the collided unit and $Splash\%$ damage to adjacent units (Target Team = `Enemy`); or applies Ally-Support Resolution to the collided ally and the splash radius (Target Team = `Ally`, e.g. Cosmic Sprite's mana restore splashed to adjacent allies).
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
*   **Targeting**: Self, optionally extended to an Ally-Team `Base Filter` for a secondary target (e.g. Aegis's "Self & Lowest HP % adjacent ally", Beastmaster's "Self (Board-wide)" hitting all allies).
*   **Delivery**: Caster plays an activation animation (particle burst/aura).
*   **Resolution**: Grants temporary stats or alters basic attacks (AS buff, splash, bleed) to self for $T$ ticks; if a secondary Ally-Team target/filter is set, also applies Ally-Support Resolution to that target/those targets (shield, heal, or stat buff).
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

### 11. Aegis (1 Gold, Vanguard / Guardian, Tank)
*   **Template**: `Template G: Self-Buff` (Utility Shield)
*   **TFT Counterpart**: *Poppy / Leona (1-Cost)*
*   **Mana**: 0/80
*   **Targeting**: Self & Lowest HP % adjacent ally
*   **Skill: Shield Cover**: Gains a Shield of $1.5 \times \text{DEF}$. Shields the lowest-HP % adjacent ally for $1.5 \times \text{DEF}$ as well. Both shields last 3 ticks (0.3 seconds).

### 12. Wildcat (1 Gold, Wild / Trickster, Carry)
*   **Template**: `Template F: Blink & Strike`
*   **TFT Counterpart**: *Olaf / Warwick (1-Cost)*
*   **Mana**: 0/60
*   **Targeting**: Current Target
*   **Skill: Claw Dash**: Blinks to the hex behind the current target, slashes for physical damage ($1.4 \times \text{ATK}$), and gains $+20\%$ Attack Speed for 4 ticks (0.4 seconds).

### 13. Cosmic Sprite (1 Gold, Astral / Kinetic, Support)
*   **Template**: `Template B: Exploding Projectile`
*   **TFT Counterpart**: *Brand / Annie (1-Cost)*
*   **Mana**: 0/80
*   **Targeting**: Current Target (1-hex splash radius)
*   **Skill: Starlight Spark**: Launches a magic spark that explodes on first collision. Deals magic damage ($1.8 \times \text{MG}$) to hit enemies. Restores $+15$ Mana to adjacent allies.

### 14. Novice Cleric (1 Gold, Astral / Oracle, Support)
*   **Template**: `Template A: Standard Projectile`
*   **TFT Counterpart**: *Soraka (1-Cost)*
*   **Mana**: 0/65
*   **Targeting**: Global $\rightarrow$ Lowest HP % ally
*   **Skill: Holy Blessing**: Fires a healing spark at the lowest HP % ally. Heals them for magic healing ($2.0 \times \text{MG}$) and removes any active stuns or silences.

### 15. Tech Scrapper (1 Gold, Striker / Tech, Carry)
*   **Template**: `Template G: Self-Buff` (Conic Slash)
*   **TFT Counterpart**: *Kha'Zix (1-Cost)*
*   **Mana**: 0/70
*   **Targeting**: Current Target (Frontal Cone)
*   **Skill: Scrap Cleave**: Swings a makeshift blade. Deals physical damage ($1.3 \times \text{ATK}$) in a 3-hex frontal cone and shreds target DEF by $20\%$ for 4 ticks (0.4 seconds).

### 16. Sun Warden (2 Gold, Astral / Warden, Tank)
*   **Template**: `Template G: Self-Buff` (Aura shield)
*   **TFT Counterpart**: *Taric (2-Cost)*
*   **Mana**: 0/90
*   **Targeting**: Self (Adjacent Allies)
*   **Skill: Solar Flare**: Shines solar energy. Deals magic damage ($1.0 \times \text{MG}$) to adjacent enemies and shields adjacent allies for $1.5 \times \text{DEF}$ for 3 ticks (0.3 seconds).

### 17. Venom Stalker (2 Gold, Shadow / Void, Carry)
*   **Template**: `Template G: Self-Buff` (Poison claws)
*   **TFT Counterpart**: *Cassiopeia (2-Cost)*
*   **Mana**: 0/75
*   **Targeting**: Current Target
*   **Skill: Toxic Bite**: Bites the target, dealing physical damage ($1.5 \times \text{ATK}$) and infecting them with venom. The venom deals magic DoT ($0.3 \times \text{MG}$) per tick for 5 ticks. If target HP drops below $15\%$ during this time, they are executed.

### 18. Forest Sentinel (2 Gold, Wild / Guardian, Tank)
*   **Template**: `Template G: Self-Buff` (Wood armor)
*   **TFT Counterpart**: *Rammus (2-Cost)*
*   **Mana**: 0/100
*   **Targeting**: Self (2-hex Taunt)
*   **Skill: Barkshield**: Gains $+40$ DEF/MR and Taunts all enemies within 2 hexes for 3 ticks (0.3 seconds). While active, basic attacks deal $+15$ bonus physical damage on-hit.

### 19. Void Mage (2 Gold, Elementalist / Void, Carry)
*   **Template**: `Template B: Exploding Projectile`
*   **TFT Counterpart**: *Malzahar (2-Cost)*
*   **Mana**: 0/80
*   **Targeting**: Current Target (1-hex splash radius)
*   **Skill: Void Sphere**: Fires a dark orb that explodes on target, dealing magic damage ($1.6 \times \text{MG}$) and reducing MR by $30\%$ for 4 ticks (0.4 seconds) to all units hit.

### 20. Starweaver (3 Gold, Astral / Oracle, Support)
*   **Template**: `Template G: Channel` (Healing Beam)
*   **TFT Counterpart**: *Soraka / Janna (3-Cost)*
*   **Mana**: 0/100
*   **Targeting**: Global $\rightarrow$ Lowest HP % ally
*   **Skill: Astral Cascade**: Channels for 3 ticks (0.3 seconds). Every tick, heals the lowest HP % ally for $1.2 \times \text{MG}$ and grants them a $100$ HP Shield. Interrupted if Starweaver is stunned.

### 21. Rust Colossus (3 Gold, Wild / Tech, Tank)
*   **Template**: `Template A: Standard Projectile` (Slam)
*   **TFT Counterpart**: *Blitzcrank / Nautilus (3-Cost)*
*   **Mana**: 0/90
*   **Targeting**: Current Target
*   **Skill: Iron Fist**: Slams the ground, dealing physical damage ($1.4 \times \text{ATK} + 0.5 \times \text{DEF}$) to the current target and stunning them for 2 ticks (0.2 seconds).

### 22. Night Stalker (3 Gold, Shadow / Trickster, Carry)
*   **Template**: `Template F: Blink & Strike`
*   **TFT Counterpart**: *Shaco (3-Cost)*
*   **Mana**: 0/60
*   **Targeting**: Global $\rightarrow$ Lowest HP % enemy
*   **Skill: Shadow Shroud**: Teleports behind the lowest-health enemy, shedding threat. The next attack is guaranteed to Crit and deals $+50\%$ bonus physical damage.

### 23. Storm Ranger (3 Gold, Ranger / Tech, Carry)
*   **Template**: `Template C: Laser / Piercing Beam`
*   **TFT Counterpart**: *Lucian / Ezreal (3-Cost)*
*   **Mana**: 0/80
*   **Targeting**: Linear Path (Through Current Target)
*   **Skill: Overcharge Beam**: Fires a linear lightning beam. Deals physical damage ($1.5 \times \text{ATK}$) to all enemies on the line and shocks them (reduces Attack Speed by $25\%$ for 3 ticks).

### 24. Grave Knight (4 Gold, Shadow / Dreadknight, Tank)
*   **Template**: `Template G: Self-Buff` (Siphon Aura)
*   **TFT Counterpart**: *Aatrox / Hecarim (4-Cost)*
*   **Mana**: 0/110
*   **Targeting**: Self (Adjacent Hexes)
*   **Skill: Soul Drain**: Emits a shadow aura for 4 ticks. Every tick, deals magic damage ($0.5 \times \text{MG}$) to adjacent enemies, heals self for $100\%$ of damage dealt, and steals $5$ Armor/MR from each hit target (stacking).

### 25. Arcane Sage (4 Gold, Elementalist / Tech, Carry)
*   **Template**: `Template D: Ground-Targeted AoE`
*   **TFT Counterpart**: *Viktor / Karma (4-Cost)*
*   **Mana**: 0/95
*   **Targeting**: Global $\rightarrow$ Largest Cluster (2-hex radius)
*   **Skill: Chrono Shift**: Drops a time distortion field. Deals magic damage ($2.5 \times \text{MG}$) to all enemies hit. The field lasts 4 ticks: enemies inside have action progress slowed by $30\%$, while allies inside have action progress accelerated by $+30\%$.

### 26. Void Ranger (4 Gold, Ranger / Void, Carry)
*   **Template**: `Template C: Laser / Piercing Beam`
*   **TFT Counterpart**: *Caitlyn (4-Cost)*
*   **Mana**: 0/90
*   **Targeting**: Linear Path (Through Current Target)
*   **Skill: Nether Arrow**: Fires a heavy dark arrow that pierces all targets. Deals physical damage ($2.2 \times \text{ATK}$) to all enemies on the line, executing any hit target whose health drops below $20\%$.

### 27. Divine Paladin (4 Gold, Vanguard / Oracle, Support/Tank)
*   **Template**: `Template G: Self-Buff` (Guardian Stance)
*   **TFT Counterpart**: *Galio (4-Cost)*
*   **Mana**: 0/100
*   **Targeting**: Self (2-hex radius Taunt)
*   **Skill: Guardian Shield**: Taunts all enemies within 2 hexes for 3 ticks. Gains a Shield of $3.5 \times \text{DEF}$. While active, heals adjacent allies for $1.0 \times \text{MG}$ every tick.

### 28. Cosmic Leviathan (5 Gold, Astral / Guardian, Tank)
*   **Template**: `Template D: Ground-Targeted AoE (Leap & Pull)`
*   **TFT Counterpart**: *Ornn / Malphite (5-Cost)*
*   **Mana**: 0/150
*   **Targeting**: Global $\rightarrow$ Largest Cluster (2-hex radius)
*   **Skill: Supernova**: Leaps and crashes on the largest enemy cluster. Deals magic damage ($2.5 \times \text{MG}$), stuns all targets hit for 2 ticks, and pulls them toward the center hex of the impact.

### 29. Reaper (5 Gold, Shadow / Void, Carry)
*   **Template**: `Template F: Blink & Strike`
*   **TFT Counterpart**: *Karthus / Urgot / Yasuo (5-Cost)*
*   **Mana**: 0/80
*   **Targeting**: Global $\rightarrow$ Lowest HP % enemy
*   **Skill: Death's Scythe**: Teleports to the lowest HP % enemy and slashes, dealing true damage ($3.5 \times \text{ATK}$). If the target dies, executes them and resets Reaper's Mana to $50$, casting again on the next tick.

### 30. Beastmaster (5 Gold, Wild / Warden, Tank/Support)
*   **Template**: `Template G: Self-Buff` (Primal Cry)
*   **TFT Counterpart**: *Sejuani / Gnar (5-Cost)*
*   **Mana**: 50/150
*   **Targeting**: Self (Board-wide)
*   **Skill: Primal Roar**: Stuns all enemies on the board for 1.5 ticks (0.15s). Increases all allies' AD and AP by $+50\%$ and Attack Speed by $+30\%$ for 6 ticks (0.6 seconds).

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
- [ ] Ally-Team skills (heal, shield, mana restore, cleanse) resolve against the caster's own team and produce an observable effect (HP/mana/shield change, trace-logged) rather than a silent no-op.
