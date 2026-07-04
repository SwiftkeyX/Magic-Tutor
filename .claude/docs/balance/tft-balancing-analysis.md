# TFT Stats and Balancing Analysis

This document is a deep dive into the mathematical mechanics of stats in Teamfight Tactics (TFT) by Riot Games — the "why" behind the numbers.

> **Scope**: TFT data and math only. For compact lookup tables, see `tft-balancing-rules.md`. For how these techniques are implemented in this project, see `balance-framework.md`. For pending research and application work, see `tft-balancing-research-plan.md`.

---

## 1. Health, Armor, and Magic Resist (Effective Health Pool - EHP)

In auto-battlers, player input is limited to pre-battle team formulation, positioning, and economy management. Once the battle starts, the game executes a deterministic or semi-random simulation. Because of this, balancing is fundamentally a mathematical optimization problem.

### What is EHP?
**Effective Health Pool (EHP)** represents the *actual* amount of raw, pre-mitigation damage a unit must take from enemies before it is defeated.

While a unit's visual health bar shows its base **HP**, damage mitigation (like Armor and Magic Resist) reduces incoming damage, making each point of actual HP survive longer. EHP mathematically combines base HP and defense stats into a single value representing the unit's true durability.

For example, if a unit has **1,000 HP** and **50% damage reduction**:
* It only takes **50 damage** from a **100-damage raw attack**.
* To defeat this unit, enemies must inflict a total of **2,000 raw damage** (since only half goes through).
* Thus, its **Effective Health Pool (EHP)** is **2,000**.

### The Math
TFT calculates damage mitigation using the League of Legends style defense formula.

$$\text{Damage Multiplier} = \frac{100}{100 + \text{Defense}}$$

$$\text{Damage Taken} = \text{Raw Damage} \times \text{Damage Multiplier}$$

This formula is designed to prevent defensive stats from making a unit immune to damage. Instead, it scales **Effective Health Pool (EHP)** linearly, but physical and magic durability are calculated completely separately:

For Physical attacks (reduced by Armor):
$$\text{EHP}_{\text{physical}} = \text{HP} \times \left(1 + \frac{\text{Armor}}{100}\right)$$

For Magic attacks/spells (reduced by MR):
$$\text{EHP}_{\text{magic}} = \text{HP} \times \left(1 + \frac{\text{MR}}{100}\right)$$

Note that Physical and Magic EHP are entirely separate pools. A unit with 200 Armor but 0 MR will have 3,000 physical EHP but only 1,000 magic EHP.

> [!NOTE]
> Every **1 point of Armor/MR** increases a unit's durability against that damage type by **exactly 1% of its base Health**.
> * A unit with 1,000 HP and 0 Armor has **1,000 physical EHP**.
> * A unit with 1,000 HP and 100 Armor has **2,000 physical EHP** (takes 50% damage).
> * A unit with 1,000 HP and 200 Armor has **3,000 physical EHP** (takes 33.3% damage).

### Real TFT Champion Durability (EHP) Examples

To see how these formulas scale in practice, here is a table of actual Teamfight Tactics (Set 9) champions showing EHP across roles, gold costs, and star levels. 

*(Recall that Armor/MR do not scale with star level, while base HP scales compounding by 1.8x at 2-star and 3.24x at 3-star).*

| Champion | Cost | Role | HP (1-Star) | Armor / MR | 1-Star EHP | 2-Star EHP | 3-Star EHP |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| **Cho'Gath** | 1-Gold | Tank | 700 | 30 | 910 | 1,638 | 2,948 |
| **Cassiopeia** | 1-Gold | Carry | 500 | 20 | 600 | 1,080 | 1,944 |
| **Sett** | 2-Gold | Tank | 800 | 50 | 1,200 | 2,160 | 3,888 |
| **Jinx** | 2-Gold | Carry | 600 | 20 | 720 | 1,296 | 2,333 |
| **Taric** | 3-Gold | Tank | 800 | 50 | 1,200 | 2,160 | 3,888 |
| **Sona** | 3-Gold | Support/Carry | 700 | 25 | 875 | 1,575 | 2,835 |
| **Sejuani** | 4-Gold | Tank | 1,100 | 60 | 1,760 | 3,168 | 5,702 |
| **Zeri** | 4-Gold | Carry | 750 | 25 | 937.5 | 1,688 | 3,038 |
| **K'Sante** | 5-Gold | Tank | 1,000 | 50 | 1,500 | 2,700 | 4,860 |
| **Ahri** | 5-Gold | Carry | 850 | 40 | 1,190 | 2,142 | 3,856 |

### Balancing Implication
When choosing between tuning a champion's HP or Armor/MR:
* **Increasing HP** increases durability against *both* physical and magic damage, and amplifies flat shields or %-HP healing.
* **Increasing Armor/MR** increases durability against only one damage type, and makes healing or shielding *more valuable per point healed* (since each point of health is worth more effective health).

---

## 2. DPS (Damage Per Second)

In TFT, auto-attack DPS (before mitigation) is represented by:

$$\text{Base Attack DPS} = \text{AD} \times \text{AS}$$

*   **AD** is a flat number representing damage per attack. It multiplies by exactly **1.8×** per star-level upgrade (1-star to 2-star, and 2-star to 3-star).
*   **AS** is a float representing attacks per second. Attack Speed does not scale with star level. When an item or trait grants a percentage bonus, it is always an additive bonus calculated on the *base* AS:
    $$\text{Current AS} = \text{Base AS} \times (1 + \text{Bonus AS}\%)$$

### Real TFT Champion Auto-Attack DPS Examples (Physical Units & Tanks)

For physical damage carries and tanks, their base damage scales primarily through auto-attacks:

| Champion | Cost | Role | AD (1-Star) | AS | 1-Star DPS | 2-Star DPS | 3-Star DPS |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| **Cho'Gath** | 1-Gold | Tank | 65 | 0.50 | 32.5 | 58.5 | 105.3 |
| **Sett** | 2-Gold | Tank | 60 | 0.60 | 36.0 | 64.8 | 116.6 |
| **Jinx** | 2-Gold | Carry | 50 | 0.75 | 37.5 | 67.5 | 121.5 |
| **Taric** | 3-Gold | Tank | 60 | 0.60 | 36.0 | 64.8 | 116.6 |
| **Sejuani** | 4-Gold | Tank | 60 | 0.60 | 36.0 | 64.8 | 116.6 |
| **Zeri** | 4-Gold | Carry | 65 | 0.80 | 52.0 | 93.6 | 168.5 |
| **K'Sante** | 5-Gold | Tank | 60 | 0.70 | 42.0 | 75.6 | 136.1 |

### Real TFT Champion Spell-Cycle DPS Examples (AP & Magic Units)

For AP carries and caster supports, base auto-attack DPS does not reflect their true threat level because their primary damage scales via spell casts. Instead, we use the **Spell-Cycle DPS** formula, which combines base auto-attacks and spell damage over a complete mana-charging cycle:

$$\text{Spell-Cycle DPS} = (\text{AD} \times \text{AS}) + \frac{\text{Spell Damage} \times \text{AS}}{\text{Attacks to Cast}}$$

*   Where \(\text{Attacks to Cast} = \text{Max Mana} / 10\) (since each auto-attack generates 10 mana).
*   *Note: This cycle ignores spell cast execution/lockout times and incoming hit mana, representing a pure sustain DPS scenario.*

| Champion | Cost | Role | AD (1c) | AS | Max Mana | Base Spell Dmg (1 / 2 / 3-Star) | 1-Star DPS | 2-Star DPS | 3-Star DPS |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Cassiopeia** | 1-Gold | Carry | 40 | 0.70 | 30 | 160 / 240 / 360 *(Base)*<br>208 / 312 / 468 *(Wounded)* | 65.3 *(Base)*<br>76.5 *(Wounded)* | 106.4 *(Base)*<br>123.2 *(Wounded)* | 174.7 *(Base)*<br>199.9 *(Wounded)* |
| **Sona** | 3-Gold | Support/Carry | 45 | 0.65 | 80 | 170 / 255 / 420 | 43.1 | 73.4 | 128.9 |
| **Ahri** | 5-Gold | Carry | 50 | 0.85 | 50 | 105 / 150 / 1,000 *(Essence)*<br>156.7 / 230 / 1,333 *(Cycle Avg)* | 69.1 | 115.6 | 364.3 |

### TFT Set 9 Tier 1 Carry DPS Analysis Summary

Below is a summary table comparing the Normal auto-attack DPS and Spell-Cycle DPS for all 1-Gold carry champions in TFT Set 9:

| Champion | Spell Type | 1★ Normal / Spell DPS | 2★ Normal / Spell DPS | 3★ Normal / Spell DPS | 1★ / 2★ / 3★ Total DPS |
| :--- | :--- | :---: | :---: | :---: | :---: |
| **Cassiopeia** | AP Caster (Wounded) | 28.0 / 48.5 | 50.4 / 72.8 | 90.7 / 109.2 | **76.5 / 123.2 / 199.9** |
| **Jhin** | Physical Line Caster | 34.6 / 24.6 | 62.2 / 43.4 | 112.0 / 76.7 | **59.2 / 105.6 / 188.7** |
| **Kayle (Lvl 6-8)** | Passive Hybrid Ascent | 22.5 / 30.0 | 40.5 / 43.8 | 72.9 / 67.5 | **52.5 / 84.3 / 140.4** |
| **Malzahar** | AP AoE Portal Caster | 28.0 / 30.8 | 50.4 / 46.2 | 90.7 / 70.0 | **58.8 / 96.6 / 160.7** |
| **Samira** | AD Shred Caster | 31.5 / 20.0 | 56.7 / 35.9 | 102.1 / 68.0 | **51.5 / 92.6 / 170.1** |
| **Tristana** | AD AS-Steroid | 31.5 / 9.1 | 56.7 / 16.4 | 102.1 / 29.4 | **40.6 / 73.1 / 131.5** |
| **Viego (1 Stack)** | AP Stacking Melee | 33.8 / 31.5 | 60.8 / 47.3 | 109.4 / 71.3 | **65.3 / 108.1 / 180.7** |

### TFT Set 9 Reference Spell Descriptions & Combat Nuances

To understand why simple cycle DPS calculations can be misleading or approximate, we must inspect the actual skill behaviors from the TFT Set 9 database:

#### 1. Cassiopeia — Twin Fang (1-Gold Caster)
*   **Description**: Deals magic damage to the current target and **Wounds** them for 5 seconds (reduces healing by 33%). If they are already **Wounded**, deals 30% bonus magic damage.
*   **Nuance**: Cassiopeia has a tiny 30 mana pool, meaning she casts every 3 auto-attacks. Because the Wound lasts 5 seconds and she casts roughly every 4.3 seconds (at 0.70 AS), **every cast after her very first cast deals the 30% bonus damage**. This makes her true combat DPS much closer to the "Wounded" calculation.

#### 2. Sona — Crescendo (3-Gold Support/Caster)
*   **Description**: Sends a wave of sound at the largest clump of enemies, dealing magic damage to enemies hit; each successive target hit reduces the wave's damage by 33%. Allies hit by the wave gain 20% / 25% / 35% Attack Speed for the rest of combat.
*   **Nuance**: Sona's spell has a high base damage (170/255) but is subject to dropoff on multiple hits. Because it is an AOE spell, its actual damage output scales exponentially depending on enemy positioning, and her utility is heavily amplified by the permanent AS buff she provides to allies.

#### 3. Ahri — Essence Thief (5-Gold Caster)
*   **Description**: Steals essence from enemies around the current target, dealing magic damage and 20% Mana Reaving them. Every 3rd cast, she unleashes a wave that deals massive magic damage to all enemies hit, dealing 33% more damage to enemies whose essence has been stolen.
*   **Nuance**: Ahri's casting cycle has a 3-part sequence. Casts 1 and 2 deal minor single-target damage (105 / 150 / 1000). Cast 3 deals a huge AOE burst (260 / 390 / 1999). Over a full 3-cast sequence, her average spell damage per cast is significantly higher than her base essence steal (calculated as `(Essence * 2 + Wave) / 3` in the table above). Her real-world DPS is even higher because the wave hits multiple units and gains a 33% multiplier on previously hit targets.

### Tick Rate and Quantization
In a digital game engine, time is split into discrete frames or server ticks, so a unit's real attack interval is always rounded to a whole number of ticks.

* TFT runs its simulation at **10 ticks per second** (0.1s tick rate).
* At this resolution, rounding error stays small: an attack interval dropping from 11 ticks to 10 ticks is only a ~9% change, so attack-speed buffs translate near-linearly into real DPS.
* The general principle: **the coarser the tick rate, the larger the rounding cliffs.** With big ticks, small AS buffs can either do nothing (the rounded interval doesn't change) or jump an entire tick (a massive DPS spike). A fine tick rate — or a float accumulator that carries fractional progress across ticks — is what keeps percentage AS bonuses honest.

---

## 3. Mana and the Ability Cycle

TFT champions gain mana through active participation in combat:
1. **Attacking**: A flat **10 mana** per auto-attack.
2. **Defending (Mana-on-Hit)**: **7% of pre-mitigation damage taken** is converted into mana.
   * **What is pre-mitigation damage?** This is the raw damage of the attack *before* it is reduced by Armor, MR, or active shields.
     - *Why use pre-mitigation?* If mana gain scaled with *post-mitigation* (actual HP lost) damage, a heavily-armored tank with 300 Armor (75% damage reduction) would gain 4× less mana than a squishy carry when hit by the same attack. This would prevent tanks from ever casting their abilities. By using pre-mitigation damage, both tanks and carries gain the exact same amount of mana when struck by a specific enemy attack.
   * **The Cap per Instance of Damage**: To prevent a champion from immediately reaching maximum mana from a single massive hit (e.g. taking 1,500 damage from a high-tier spell, which would translate to $1,500 \times 0.07 = 105$ mana), Riot caps the maximum mana gained from a single instance of damage to exactly **42.5 mana** (which is hit when taking 607 or more raw damage). This prevents runaway cast loops and prevents hyper-tanks with shields from spamming abilities infinitely.
3. **Traits/Items**: E.g., Scholar, Invoker, Blue Buff, Shojin.

When mana reaches the Max Mana threshold, the champion halts auto-attacking to cast their ability, and their mana resets to 0.

### Balancing Implication
Riot uses **Max Mana** and **Starting Mana** as primary dials:
* **Starting Mana** controls how quickly a unit gets their first cast. High starting mana (e.g., 90/100) is placed on crowd-control tanks (like Sejuani or Cho'Gath) so they disrupt the board early.
* **Max Mana** determines the cycle time. A spammer carry (like Ryze or Ezreal) might have a 0/40 mana pool, casting every 4 attacks. A high-impact hyper-carry (like Karthus or Gangplank) might have 0/120, requiring long windups.

---

## 4. Critical Strike Chance & Damage

* Base Stats in TFT: **25% Crit Chance** and **140% Crit Damage** (deals +40% bonus damage).
* **Crit Overflow**: If a unit's Crit Chance exceeds 100% (via items like Infinity Edge or Jeweled Gauntlet), every 1% of excess Crit Chance is converted into **+1% Crit Damage**.
  $$\text{Final Crit Damage} = 140\% + (\text{Crit Chance} - 100\%)$$

---

## 5. Star-Level Scaling Multipliers

When a unit is upgraded to 2-star and 3-star, only a few stats scale:

| Stat Type | 1-Star | 2-Star | 3-Star | Scaling Rule |
| :--- | :---: | :---: | :---: | :--- |
| **Health (HP)** | 1.0x | 1.8x | 3.24x | Compounding 1.8x multiplier |
| **Attack Damage (AD)** | 1.0x | 1.8x | 3.24x | Compounding 1.8x multiplier |
| **Armor & MR** | Flat | Flat | Flat | Does NOT scale with star level |
| **Attack Speed (AS)** | Flat | Flat | Flat | Does NOT scale with star level |
| **Range** | Flat | Flat | Flat | Does NOT scale with star level |

---

## 6. Cost Scaling (Stat Budget by Tier)

To keep champions balanced as they scale in cost (1c to 5c), TFT follows a **Cost Scaling Law**:
* A higher-cost champion offers approximately **15% to 25% more stat budget** per gold level.
* However, their scaling is also driven by **ability impact** and **trait synergies** rather than just raw numbers — a 5-cost tank earns its price with a battle-warping ability (a backline blink, a revive, a knock-away), not just higher health.
