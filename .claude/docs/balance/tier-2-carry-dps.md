# TFT Set 9 Tier 2 Carry DPS Analysis

This document provides a dedicated mathematical breakdown of base auto-attack (Normal) DPS and Spell DPS for all 2-Gold carry champions in Teamfight Tactics (Set 9). 

The goal is to understand how Tier-2 carries scale their damage output across star upgrades (1, 2, and 3-Star), focusing on their unique utility "catches" (AoE distribution, knockup combo triggers, clone duplicates, and critical steroids).

---

## 🧮 Mathematical Formulas

We divide a champion's damage output into two categories:
1.  **Normal DPS (Auto-Attacks)**:
    $$\text{Normal DPS} = \text{AD} \times \text{AS}$$
    *Recall: AD multiplies by compounding 1.8x per star level, while AS remains flat.*
2.  **Spell DPS (Ability Casts)**:
    *   **For Flat Casters**:
        $$\text{Spell DPS} = \frac{\text{Spell Damage} \times \text{AS}}{\text{Attacks to Cast}}$$
        *Where \(\text{Attacks to Cast} = \text{Max Mana} / 10\) (since each auto-attack generates 10 mana).*
    *   **For Special Casters (Clones, Triggers, Splitting)**: Calculated using custom cycle duration and projectile distributions (see breakdowns below).

---

## 📊 Master DPS Summary Table

Here is the consolidated DPS comparison across all 4 Tier-2 carry champions:

| Champion | Role / Caster Type | Star | Base AD | AS | Normal DPS | Spell DPS | Total DPS | Spell % of Total |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Jinx** | AD Split Rocket Caster | 1★<br>2★<br>3★ | 50<br>90<br>162 | 0.75 | 37.5<br>67.5<br>121.5 | 48.2<br>85.4<br>157.6 | **85.7**<br>**152.9**<br>**279.1** | 56.2%<br>55.9%<br>56.5% |
| **Teemo** | AP circular AoE (Single Target) | 1★<br>2★<br>3★ | 40<br>72<br>130 | 0.70 | 28.0<br>50.4<br>91.0 | 36.4<br>54.6<br>81.9 | **64.4**<br>**105.0**<br>**172.9** | 56.5%<br>52.0%<br>47.4% |
| **Taliyah** | AP Combo (Self-Trigger only) | 1★<br>2★<br>3★ | 40<br>72<br>130 | 0.70 | 28.0<br>50.4<br>91.0 | 39.7<br>59.5<br>89.3 | **67.7**<br>**109.9**<br>**180.3** | 58.6%<br>54.1%<br>49.5% |
| **Zed** | AD Melee (Zed + Shadow Clone) | 1★<br>2★<br>3★ | 55<br>99<br>178 | 0.70 | 38.5<br>69.3<br>124.6 | 20.4<br>35.7<br>63.4 | **58.9**<br>**105.0**<br>**188.0** | 34.6%<br>34.0%<br>33.7% |

---

## 🔍 Detailed Champion Breakdowns

> [!NOTE]
> Like Tier-1 carries, Tier-2 carries are balanced around unique mechanical properties. Taliyah is a combo-multiplier unit, Jinx splits her damage to clean up low-health targets, Teemo acts as an anti-heal AoE zoner, and Zed relies on positioning and critical strike multipliers.

### 1. Jinx (Fishbones!)
*   **Mechanic**: Fires 5 rockets at random enemies within a 2-hex radius of her current target. Each rocket deals physical damage equal to a percentage of AD plus a flat base value.
*   **Formula**:
    $$\text{Rocket Damage} = (\text{AD} \times \text{ADPercent}) + \text{FlatDamage}$$
    $$\text{Spell DPS} = \frac{(\text{Rocket Damage} \times 5) \times \text{AS}}{\text{Attacks to Cast}}$$
    *Note: Max Mana is 70, starting mana is 10. Jinx requires 6 attacks for her first cast and 7 attacks for subsequent casts. We use the 7-attack cycle for sustain math.*
*   **1★ Calculations**:
    *   Normal DPS: \(50 \times 0.75 = 37.5\)
    *   Single Rocket Damage: \((50 \times 1.50) + 15 = 90\)
    *   Total Spell Damage (5 rockets): \(90 \times 5 = 450\)
    *   Spell DPS: \(\frac{450 \times 0.75}{7} = 48.2\)
    *   Total: \(37.5 + 48.2 = 85.7\)
*   **Combat Role & Nuance**: Jinx's spell acts as a **semi-AoE cleanup tool**. Rather than concentrated burst on a single target, her 5 rockets disperse among nearby enemies, making her exceptionally good at finishing off multiple low-health targets.

### 2. Teemo (Noxious Trap)
*   **Mechanic**: Throws an explosive mushroom. On detonation, enemies in a 1-hex radius are Wounded (-33% healing) and dealt magic damage over a 3-second duration.
*   **1★ Calculations (with Multi-Target Circular AoE)**:
    *   Normal DPS: \(40 \times 0.70 = 28.0\)
    *   Spell DPS (Single Target): \(\frac{260 \times 0.70}{5} = 36.4\) \(\rightarrow\) **Total DPS: 64.4**
    *   **2 Targets Hit**: Total DPS = \(28.0 + 36.4 \times 2 = 100.8\)
    *   **3 Targets Hit**: Total DPS = \(28.0 + 36.4 \times 3 = 137.2\)
*   **Combat Role & Nuance**: Teemo is an **anti-healing AoE burner**. Since his poison damage is fully applied to all units caught in the 1-hex circle, his teamfight contribution scales linearly with target density.

### 3. Taliyah (Seismic Shove)
*   **Mechanic**: 
    *   **Active**: Deals magic damage to the current target and knocks them up (stun for 2 seconds).
    *   **Passive**: Whenever *any* enemy is knocked up or back by *anything* (Taliyah's active or an ally's ability), she throws a boulder dealing magic damage.
*   **Spell Cycle Math (Self-Trigger Only)**:
    *   Max Mana = 60, Starting Mana = 20 (4 attacks to first cast, 6 for subsequent casts).
    *   Taliyah's active automatically knocks up the target, triggering exactly **1 passive boulder**.
    *   Cycle Damage = Active Damage + 1 Boulder Damage.
*   **1★ Calculations (Self-Trigger)**:
    *   Normal DPS: \(40 \times 0.70 = 28.0\)
    *   Total Spell Damage: \(220 \text{ (Active)} + 120 \text{ (Passive)} = 340\)
    *   Spell DPS: \(\frac{340 \times 0.70}{6} = 39.7\)
    *   Total: \(28.0 + 39.7 = 67.7\)
*   **Combat Role & Nuance (Synergy Multipliers)**:
    *   Taliyah is a **synergistic combo carry**. In a team with knockup allies (e.g. Sett, Vi, Poppy), every external knockup triggers a passive boulder, adding substantial DPS without requiring Taliyah to attack or spend mana:
    *   **Formula**:
        $$\text{Bonus Passive DPS} = \frac{\text{Boulder Damage} \times \text{AS} \times P}{6} = \frac{120 \times 0.70 \times P}{6} = 14.0 \times P$$
        *Where \(P\) is the number of external knockups triggered during Taliyah's 6-attack cycle.*
    *   **With 2 External Knockups**: 1★ Total DPS increases to \(67.7 + 28.0 = \mathbf{95.7}\)
    *   **With 4 External Knockups**: 1★ Total DPS increases to \(67.7 + 56.0 = \mathbf{123.7}\)

### 4. Zed (Living Shadow)
*   **Mechanic**: Creates a shadow clone at the furthest enemy within 2 hexes. Zed and his shadow slash adjacent targets, dealing physical damage equal to a percentage of AD plus flat base damage.
*   **Spell Cycle Math**:
    *   Max Mana = 70. Requires 7 attacks to cast.
    *   If Zed and his clone slash the same target (typical in close combat), the damage is doubled.
*   **1★ Calculations (Double Slash)**:
    *   Normal DPS: \(55 \times 0.70 = 38.5\)
    *   Single Slash Damage: \((55 \times 1.40) + 25 = 102\)
    *   Double Slash Damage: \(102 \times 2 = 204\)
    *   Spell DPS: \(\frac{204 \times 0.70}{7} = 20.4\)
    *   Total: \(38.5 + 20.4 = 58.9\)
*   **Combat Role & Nuance (Crit/Ionia Steroid)**:
    *   Zed is a **critical strike assassin**. As an Ionian unit, his Ionia Bonus grants **+15% Critical Strike Chance** and **+15% Critical Damage**. Because critical hits apply to both his auto-attacks and physical slashes, Zed's actual combat DPS scales exponentially when equipped with Crit-chance items or active traits.

---

## 💡 Key Design & Balance Takeaways

1.  **Synergy vs. Standalone Power**:
    *   **Jhin and Cassiopeia** (Tier 1) are highly stable standalone DPS units.
    *   **Taliyah and Zed** (Tier 2) are highly conditional carries. Zed requires specific positioning and critical stats to excel, while Taliyah's DPS can literally double when paired with knockup champions. Our custom auto-battler should design Tier-2 units around these synergistic multiplier relationships.
2.  **Mana-to-Spell-Value Ratios**:
    *   Higher cost carries should have more impactful spell effects relative to their mana pools. Tier-2 carries represent a middle-ground where abilities are either low mana (Teemo 50, Taliyah 60) for fast cycling, or high mana with duplicate multipliers (Zed 70 with duplicate slashes).
