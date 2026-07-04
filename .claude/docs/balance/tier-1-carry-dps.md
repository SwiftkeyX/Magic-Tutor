# TFT Set 9 Tier 1 Carry DPS Analysis

This document provides a dedicated mathematical breakdown of base auto-attack (Normal) DPS and Spell DPS for all 1-Gold carry champions in Teamfight Tactics (Set 9). 

The goal is to understand how physical (AD-based) and magical (AP-based) carries scale their damage across star upgrades (1, 2, and 3-Star), providing a template for balancing our custom auto-battler.

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
    *   **For Special Carries (Steroids, Channels, Stacks)**: Calculated using custom cycle duration math (see breakdowns below).

---

## 📊 Master DPS Summary Table

Here is the consolidated DPS comparison across all 7 Tier-1 carry champions:

| Champion | Role / Caster Type | Star | Base AD | AS | Normal DPS | Spell DPS | Total DPS | Spell % of Total |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Cassiopeia** | AP Caster (Wounded) | 1★<br>2★<br>3★ | 40<br>72<br>130 | 0.70 | 28.0<br>50.4<br>90.7 | 48.5<br>72.8<br>109.2 | **76.5**<br>**123.2**<br>**199.9** | 63.4%<br>59.1%<br>54.6% |
| **Jhin** | Physical Line Caster | 1★<br>2★<br>3★ | 54<br>97<br>175 | 0.70 | 37.8<br>68.0<br>122.5 | 24.6<br>43.4<br>76.7 | **59.2**<br>**105.6**<br>**188.7** | 41.5%<br>41.1%<br>40.6% |
| **Kayle** | Passive Ascent (Lvl 1-5)<br>Passive Ascent (Lvl 6-8)<br>Passive Ascent (Lvl 9) | 1★<br>2★<br>3★ | 30<br>54<br>97 | 0.75 | 22.5<br>40.5<br>72.9 | 22.5<br>33.8<br>52.5<br>30.0<br>43.8<br>67.5<br>45.0<br>63.8<br>97.5 | **45.0**<br>**74.3**<br>**125.4**<br>**52.5**<br>**84.3**<br>**140.4**<br>**67.5**<br>**104.3**<br>**170.4** | 50.0%<br>45.5%<br>41.9%<br>57.1%<br>52.0%<br>48.1%<br>66.7%<br>61.2%<br>57.2% |
| **Malzahar** | AP AoE Portal Caster | 1★<br>2★<br>3★ | 40<br>72<br>130 | 0.70 | 28.0<br>50.4<br>90.7 | 30.8<br>46.2<br>70.0 | **58.8**<br>**96.6**<br>**160.7** | 52.4%<br>47.8%<br>43.6% |
| **Samira** | AD Shred Caster | 1★<br>2★<br>3★ | 45<br>81<br>146 | 0.70 | 31.5<br>56.7<br>102.1 | 20.0<br>35.9<br>68.0 | **51.5**<br>**92.6**<br>**170.1** | 38.8%<br>38.8%<br>40.0% |
| **Tristana** | AD AS-Steroid | 1★<br>2★<br>3★ | 45<br>81<br>146 | 0.70 | 31.5<br>56.7<br>102.1 | 9.1<br>16.4<br>29.4 | **40.6**<br>**73.1**<br>**131.5** | 22.4%<br>22.4%<br>22.4% |
| **Viego** | AP Stacking Melee Carry | 1★<br>2★<br>3★ | 45<br>81<br>146 | 0.75 | 33.8<br>60.8<br>109.4 | 31.5<br>47.3<br>71.3 | **65.3**<br>**108.1**<br>**180.7** | 48.2%<br>43.8%<br>39.5% |

---

## 🔍 Detailed Champion Breakdowns

> [!NOTE]
> Raw single-target DPS numbers only tell part of the story. Many of these carries possess critical "catches" to their abilities — such as line piercing, shield destruction, AoE splash, or stack building — which dictate their true combat value.

### 1. Cassiopeia (Twin Fang)
*   **Mechanic**: Deals magic damage. If the target is already Wounded (reduced healing), deals 30% bonus damage. Since her Wound lasts 5 seconds and she casts roughly every 4.3 seconds, **every cast after the first one gains the 30% damage boost**.
*   **Formula (Wounded)**: 
    $$\text{Spell DPS} = \frac{(Base \times 1.3) \times \text{AS}}{\text{Max Mana}/10} = \frac{(Base \times 1.3) \times 0.70}{3}$$
*   **1★ Calculations**:
    *   Normal DPS: \(40 \times 0.70 = 28.0\)
    *   Spell DPS: \(\frac{208 \times 0.70}{3} = 48.5\)
    *   Total: \(28.0 + 48.5 = 76.5\)
*   **Combat Role & Nuance**: Cassiopeia is a **hyper-focused single-target specialist**. She has zero AoE or splash utility, but she possesses the highest single-target sustain DPS in Tier 1. Her job is to quickly melt down individual frontliners one by one.

### 2. Jhin (Curtain Call)
*   **Mechanic**: Physical damage line spell scaling at **744% of AD** plus flat damage. However, Jhin has a **1.6-second channel duration** during which he does not auto-attack.
*   **Spell Cycle Math**:
    *   Cycle requires 12 attacks + 1.6s channel. 
    *   Cycle duration = \((12 / 0.70) + 1.6 = 18.74\) seconds.
    *   Cycle Damage = \((12 \times \text{AD}) + (7.44 \times \text{AD} + \text{Flat})\).
*   **Combat Role & Nuance**: While Jhin's single-target DPS is lower than Cassiopeia's, his ability **pierces through targets in a line with a wide hitbox**, allowing him to hit 2 to 4 enemies in a row. Hits are subject to a **56% damage falloff per target** (Target 1: 100%, Target 2: 44%, Target 3: 19.4%, Target 4: 8.5%).
*   **1★ Calculations (Multi-Target Line Pierce)**:
    *   **1 Target**: Cycle Damage = \(648 + 461.8 = 1109.8\) \(\rightarrow\) **Total DPS: 59.2** (Normal: 34.6, Spell: 24.6)
    *   **2 Targets** (adds 44% spell damage): Cycle Damage = \(648 + 665.0 = 1313.0\) \(\rightarrow\) **Total DPS: 70.1** (Normal: 34.6, Spell: 35.5)
    *   **3 Targets** (adds 63.4% spell damage): Cycle Damage = \(648 + 754.6 = 1402.6\) \(\rightarrow\) **Total DPS: 74.8** (Normal: 34.6, Spell: 40.2)
    *   **4 Targets** (adds 71.9% spell damage): Cycle Damage = \(648 + 793.8 = 1441.8\) \(\rightarrow\) **Total DPS: 76.9** (Normal: 34.6, Spell: 42.3)

### 3. Kayle (Divine Ascent)
*   **Mechanic**: Pure passive carry. Has 0 mana and does not cast active spells. Instead, her attacks scale as player level increases:
    *   **Level 1-5**: Base attacks + flat bonus magic damage (\(30 / 45 / 70\)).
    *   **Level 6-8**: Every 3rd attack fires a wave dealing magic damage (\(30 / 40 / 60\)) and shreds target MR.
    *   **Level 9**: Every attack launches a wave.
*   **1★ Calculations (Level 6-8)**:
    *   Normal DPS: \(30 \times 0.75 = 22.5\)
    *   Spell DPS (Bonus + Wave/3): \((30 \times 0.75) + \frac{30 \times 0.75}{3} = 22.5 + 7.5 = 30.0\)
    *   Total: \(22.5 + 30.0 = 52.5\)

### 4. Malzahar (Call of the Void)
*   **Mechanic**: Standard AP carry. Spawns two portals dealing magic damage.
*   **1★ Calculations (with Multi-Target AoE)**:
    *   Normal DPS: \(40 \times 0.70 = 28.0\)
    *   Spell DPS (Single Target): \(\frac{220 \times 0.70}{5} = 30.8\) \(\rightarrow\) **Total DPS: 58.8**
    *   Spell DPS (2 Targets): \(\frac{440 \times 0.70}{5} = 61.6\) \(\rightarrow\) **Total DPS: 89.6**
    *   Spell DPS (3 Targets): \(\frac{660 \times 0.70}{5} = 92.4\) \(\rightarrow\) **Total DPS: 120.4**
*   **Combat Role & Nuance**: In addition to dealing magic damage in a wide rectangular AoE (hitting multiple frontliners), Malzahar's portals instantly **destroy 50% of active shields** on all hit targets. This makes him a vital utility counter to defensive comps utilizing Bastion/Warden shields.

### 5. Samira (Flair)
*   **Mechanic**: Physical caster. Attacks deal physical damage and shred target Armor by a flat value for the rest of combat.
*   **1★ Calculations (with Armor Shred Mitigation Math)**:
    *   Normal DPS: \(45 \times 0.70 = 31.5\)
    *   Spell DPS (Raw/Unmitigated): \(\frac{(45 \times 1.9) \times 0.70}{3} = 20.0\)
    *   Total DPS (Raw/Unmitigated): \(31.5 + 20.0 = 51.5\)
    *   **Against Squishy Target (20 Base Armor)**:
        *   *Pre-shred (Multiplier = \(\frac{100}{100 + 20} = 0.833\))*: Mitigated DPS = \(51.5 \times 0.833 = 42.9\)
        *   *After 1st cast (-10 Armor \(\rightarrow\) 10 Armor, Multiplier = 0.909)*: Mitigated DPS = \(51.5 \times 0.909 = 46.8\) (a +9.1% damage increase)
        *   *After 2nd cast (-20 Armor \(\rightarrow\) 0 Armor, Multiplier = 1.000)*: Mitigated DPS = \(51.5 \times 1.000 = 51.5\) (a +20.0% damage increase over baseline)
    *   **Against Tank Target (50 Base Armor)**:
        *   *Pre-shred (Multiplier = \(\frac{100}{100 + 50} = 0.667\))*: Mitigated DPS = \(51.5 \times 0.667 = 34.4\)
        *   *After 1st cast (-10 Armor \(\rightarrow\) 40 Armor, Multiplier = 0.714)*: Mitigated DPS = \(51.5 \times 0.714 = 36.8\) (a +7.0% damage increase)
        *   *After 2nd cast (-20 Armor \(\rightarrow\) 30 Armor, Multiplier = 0.769)*: Mitigated DPS = \(51.5 \times 0.769 = 39.6\) (a +15.1% damage increase over baseline)

### 6. Tristana (Rapid Fire)
*   **Mechanic**: AD steroid carry. Activates a 4-second buff granting +70% AS. During the buff, her attacks deal 60% of AD as bonus splash physical damage to adjacent hexes. She cannot gain mana while the buff is active.
*   **Spell Cycle Math**:
    *   Charging phase: 4 attacks to gain 40 mana (takes \(4 / 0.70 = 5.71\) seconds).
    *   Active phase: 4 seconds of attacks at \(0.70 \times 1.70 = 1.19\) AS (4.76 attacks).
    *   Total Cycle duration: \(5.71 + 4 = 9.71\) seconds.
    *   Total Single-Target Damage in cycle: \((4 \times \text{AD}) + (4.76 \times \text{AD}) = 8.76 \times \text{AD}\).
*   **1★ Calculations (with Multi-Target Splash)**:
    *   If her active attacks splash onto \(K\) additional adjacent enemies, each active attack deals \(\text{AD} \times (1 + 0.60 \times K)\) total damage:
        *   **0 Splash Targets** (Single Target): **40.6 DPS** (Normal: 31.5, Spell: 9.1)
        *   **1 Splash Target** (\(K=1\)): **53.8 DPS** (adds +2.856 AD per cycle \(\rightarrow\) 522 total damage)
        *   **2 Splash Targets** (\(K=2\)): **67.1 DPS** (adds +5.712 AD per cycle \(\rightarrow\) 651 total damage)
        *   **3 Splash Targets** (\(K=3\)): **80.3 DPS** (adds +8.568 AD per cycle \(\rightarrow\) 780 total damage)
    *   *Note: In clustered fights, Tristana's splash damage effectively doubles her total combat DPS output.*

### 7. Viego (Blade of the Ruined King)
*   **Mechanic**: AP melee carry. Active deals magic damage, and permanently grants Viego's attacks bonus stacking magic damage on-hit for the rest of combat.
*   **1★ Calculations (at 1 Stack)**:
    *   Normal DPS: \(45 \times 0.75 = 33.8\)
    *   Spell DPS (Active + 1 Stack): \(\frac{110 \times 0.75}{5} + (20 \times 0.75) = 16.5 + 15.0 = 31.5\)
    *   Total: \(33.8 + 31.5 = 65.3\)
    *   *Note: At 2 stacks, spell DPS increases to \(16.5 + 30.0 = 46.5\), bringing total DPS to 80.3.*

---

## 💡 Key Design & Balance Takeaways

1.  **Magical vs. Physical Splits**:
    *   **AP Carries** (Cassiopeia, Malzahar) derive **50% to 65%** of their total DPS from their abilities. Their auto-attacks are purely a vehicle to generate mana.
    *   **AD Carries** (Samira, Jhin, Tristana) derive **60% to 80%** of their DPS from auto-attacks. Their spells act as utility multipliers (AS buffs, armor shreds, or burst executions).
2.  **Star Upgrade Scaling**:
    *   Because AD scales at compounding 1.8x, physical carries scale their auto-attacks linearly.
    *   AP carries scale their spell damage at different patch-dependent rates (typically 1.5x to 1.6x multiplier per star level). In our custom auto-battler, we should use a consistent multiplier to keep star-upgrades feeling equally impactful across roles.
