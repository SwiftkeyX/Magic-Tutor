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

### Balancing Implication
When choosing between tuning a champion's HP or Armor/MR:
* **Increasing HP** increases durability against *both* physical and magic damage, and amplifies flat shields or %-HP healing.
* **Increasing Armor/MR** increases durability against only one damage type, and makes healing or shielding *more valuable per point healed* (since each point of health is worth more effective health).

---

## 2. Attack Damage (AD) and Attack Speed (AS)

In TFT, auto-attack DPS (before mitigation) is represented by:

$$\text{Base Attack DPS} = \text{AD} \times \text{AS}$$

* **AD** is a flat number representing damage per attack. It multiplies by exactly **1.8×** per star-level upgrade (1-star to 2-star, and 2-star to 3-star).
* **AS** is a float representing attacks per second. When an item or trait grants "+30% Attack Speed," it is always an additive bonus calculated on the *base* AS:
  $$\text{Current AS} = \text{Base AS} \times (1 + \text{Bonus AS}\%)$$

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

## 5. Cost Scaling (Stat Budget by Tier)

To keep champions balanced as they scale in cost (1c to 5c), TFT follows a **Cost Scaling Law**:
* A higher-cost champion offers approximately **15% to 25% more stat budget** per gold level.
* However, their scaling is also driven by **ability impact** and **trait synergies** rather than just raw numbers — a 5-cost tank earns its price with a battle-warping ability (a backline blink, a revive, a knock-away), not just higher health.
