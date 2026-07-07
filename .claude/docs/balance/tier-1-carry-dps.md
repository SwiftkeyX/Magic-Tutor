# TFT Set 9 Tier 1 Carry DPS Analysis (Unified)

This document provides a unified mathematical breakdown of baseline (unequipped) and well-equipped (3-item) DPS scaling for all 1-Gold carry champions in Teamfight Tactics (Set 9). 

Putting baseline and itemized calculations in a single document guarantees that both calculations utilize identical, up-to-date mechanical assumptions (AS-scaling lockouts, targeting splits, and cast animation frames).

---

## 📊 Master DPS Summary Tables

### 📘 Table 1: Baseline (Unequipped) DPS Summary
*Consolidated comparison across 1★, 2★, and 3★ star levels with NO items equipped.*

| Champion | Star | Base AD | AS | Normal DPS | Spell DPS | Total DPS |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Cassiopeia** (Single Target) | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 26.4<br>39.6<br>59.4 | 45.8<br>68.6<br>103.0 | **72.2**<br>**108.2**<br>**162.4** |
| **Jhin** (2 Targets Avg) | 1★<br>2★<br>3★ | 54<br>81<br>122 | 0.70 | 36.7<br>55.0<br>82.6 | 37.6<br>56.5<br>84.7 | **74.3**<br>**111.5**<br>**167.3** |
| **Malzahar** (2 Targets Avg) | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 27.2<br>40.8<br>61.2 | 49.8<br>74.7<br>113.2 | **77.0**<br>**115.5**<br>**174.4** |
| **Samira** (Single Target) | 1★<br>2★<br>3★ | 45<br>68<br>101 | 0.70 | 27.4<br>41.0<br>61.5 | 19.0<br>28.5<br>42.7 | **46.4**<br>**69.5**<br>**104.2** |
| **Tristana** (1–2 Splash Targets) | 1★<br>2★<br>3★ | 45<br>68<br>101 | 0.70 | 20.4<br>30.6<br>45.9 | 38.8 / 53.3<br>58.2 / 80.6<br>87.3 / 119.7 | **59.2 / 73.7**<br>**88.8 / 111.2**<br>**133.2 / 165.6** |
| **Viego** (30s Avg Stacks) | 1★<br>2★<br>3★ | 45<br>68<br>101 | 0.75 | 32.0<br>48.0<br>72.0 | 44.1<br>66.2<br>99.6 | **76.1**<br>**114.2**<br>**171.6** |

### ⚔️ Table 2: Well-Equipped (3-Item) DPS Summary
*Consolidated comparison across 1★, 2★, and 3★ star levels with 3 core optimal items equipped. Ramping items (AA, Guinsoo) represent **[Start / End / Average]**.*

| Champion | Star | AD (Equipped) | AS (Equipped) | Normal DPS | Spell DPS | Total DPS |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Cassiopeia** (Single Target) | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 28.7<br>43.1<br>64.6 | 111.8 / 201.3 / 149.1<br>167.7 / 301.9 / 223.7<br>251.6 / 452.9 / 335.5 | **140.5 / 230.0 / 177.8**<br>**210.8 / 345.0 / 266.8**<br>**316.2 / 517.5 / 400.1** |
| **Jhin** (2 Targets Avg) | 1★<br>2★<br>3★ | 114<br>171<br>257 | 0.77 | 99.1<br>148.7<br>223.1 | 94.8<br>142.1<br>213.2 | **193.9**<br>**290.8**<br>**436.3** |
| **Malzahar** (2 Targets Avg) | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.81 | 34.6<br>51.8<br>77.8 | 204.3<br>306.5<br>464.4 | **238.9**<br>**358.3**<br>**542.2** |
| **Samira** (Single Target) | 1★<br>2★<br>3★ | 65<br>98<br>146 | 0.77 | 45.8<br>68.6<br>102.9 | 45.3<br>68.0<br>102.0 | **91.1**<br>**136.6**<br>**204.9** |
| **Tristana** (1–2 Splash Targets) | 1★<br>2★<br>3★ | 65<br>98<br>146 | 0.90 / 2.37 / 1.63 | 93.8 / 225.5 / 161.0<br>140.7 / 338.3 / 241.5<br>211.1 / 507.4 / 362.3 | 32.7 / 100.2 / 65.6 | **126.5 / 325.7 / 226.6** (1 Spl)<br>**159.2 / 425.9 / 292.2** (2 Spl) |
| **Viego** (30s Avg Stacks) | 1★<br>2★<br>3★ | 77<br>116<br>173 | 0.94 | 79.9<br>120.3<br>179.4 | 125.4<br>188.1<br>283.2 | **205.3**<br>**308.4**<br>**462.6** |

---

## 📖 Glossary & Formula Index

*   **AD (Attack Damage)**: The physical damage dealt by a basic auto-attack. AD scales by 1.5x per star level (2★ = 1.5x, 3★ = 2.25x).
*   **AS (Attack Speed)**: The rate of basic attacks per second. Attack Speed remains flat across star levels.
*   **AP (Ability Power)**: The scaling multiplier applied to base spell damage (base is 100 AP / 1.0x).
*   **Attacks to Cast**: The number of basic attacks required to generate the mana pool needed for a cast (each attack generates 10 mana by default, except with Shojin).
*   **Cast Lockout (s)**: The duration spent executing the spell cast animation, during which basic attacks are paused. **Lockout time scales with Attack Speed**, modeled as:
    $$\text{Lockout}(\text{AS}) = \frac{\text{Base Lockout}}{\text{AS}}$$
*   **Cycle Duration (s)**: The total time of one full combat loop:
    $$\text{Cycle Duration} = \frac{\text{Attacks to Cast} + \text{Base Lockout}}{\text{AS}}$$
*   **Normal DPS**: The average damage per second contributed strictly by basic auto-attacks over a cycle:
    $$\text{Normal DPS} = \frac{\text{Attacks to Cast} \times \text{AD}}{\text{Cycle Duration}} \times \text{Crit} \times \text{Amp}$$
*   **Spell DPS**: The average damage per second contributed strictly by the spell over a cycle:
    $$\text{Spell DPS} = \frac{\text{Spell Damage}}{\text{Cycle Duration}} \times \text{Crit} \times \text{Amp}$$
*   **Total DPS**: The combined damage output over a cycle:
    $$\text{Total DPS} = \text{Normal DPS} + \text{Spell DPS}$$
*   **Crit Multiplier (Crit)**: The average multiplier applied by Critical strike chance and damage:
    $$\text{Average Crit Multiplier} = 1 + \text{Crit Chance} \times (\text{Crit Damage} - 1)$$
    *   *Baseline (No Crit Items)*: 25% Crit Chance, 140% Crit Damage (\(1.10\text{x}\) multiplier) — TFT base crit damage is 140%.
    *   *Jeweled Gauntlet (JG)*: 40% Crit Chance, 170% Crit Damage (\(1.28\text{x}\) multiplier). Spells can crit.
    *   *Infinity Edge (IE)*: 60% Crit Chance, 140% Crit Damage (\(1.24\text{x}\) multiplier). Spells can crit.
    *   *IE + Last Whisper (LW)*: 70% Crit Chance, 140% Crit Damage (\(1.28\text{x}\) multiplier).
*   **AP vs. AD-ratio spells**: AP multiplies a spell's flat/base damage component only. Pure AD-ratio components (e.g. Jhin's 7.44 × AD) do not scale with AP — matching TFT's per-star ability data.

---

## 🔍 Detailed Champion Breakdowns

### 1. Cassiopeia 🐍

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Fires a blast of energy dealing magic damage. If the target is already Wounded (reduced healing), deals 30% bonus damage. Since her Wound lasts 5 seconds and she casts roughly every 3-4 seconds, every cast after the first one gains the 30% damage boost.
*   **Cast Lockout**: 0.5 seconds base.
*   **Attacks to Cast**: 3 (baseline unequipped) / 2 (equipped with Blue Buff start).

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 40 | AS = 0.70 | AP = 100 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Wounded target damage: \(160 \text{ base} \times 1.30 = 208\))**:
    *   Cycle Duration = \((3 + 0.5) / 0.70 = 5.00\text{ seconds}\)
    *   Normal DPS = \((3 \times 40) / 5.00 \times 1.10 = \mathbf{26.4}\)
    *   Spell DPS = \(208 / 5.00 \times 1.10 = \mathbf{45.8}\)
    *   Total DPS = \(26.4 + 45.8 = \mathbf{72.2}\)
*   **Star Scaling (Unequipped)**:
    *   **1★**: AD = 40 | Spell = 208 | **Total DPS: 72.2**
    *   **2★**: AD = 60 | Spell = 312 | **Total DPS: 108.2**
    *   **3★**: AD = 90 | Spell = 468 | **Total DPS: 162.4**

#### 🧮 Equipped Calculations (Blue Buff + Jeweled Gauntlet + Archangel's Staff)
*   **Stats**: AD = 40 | AS = 0.70 | Crit Chance = 40% (1.28x avg multiplier, spell crits enabled)
    *   Start AP (0s) = 150 AP (1.50x spell damage)
    *   End AP (30s) = 270 AP (2.70x spell damage)
    *   Average AP = 200 AP (2.00x spell damage) — Archangel's time-averages +70 AP over a 30s fight
*   **Cycle & DPS (Equipped with Blue Buff start requires 2 attacks to cast)**:
    *   Cycle Duration = \((2 + 0.5) / 0.70 = \mathbf{3.57\text{ seconds}}\)
    *   Normal DPS = \((2 \times 40) / 3.57 \times 1.28 = \mathbf{28.7}\)
*   **Ramping Outputs**:
    | Phase | AP Mod | Spell Damage (Wounded) | Spell DPS | Total DPS |
    | :--- | :---: | :---: | :---: | :---: |
    | **Start (0s)** | 1.50x | 399.4 | 111.8 | **140.5** |
    | **End (30s)** | 2.70x | 718.8 | 201.3 | **230.0** |
    | **Average** | 2.00x | 532.5 | 149.1 | **177.8** |

#### 🔍 Analyze DPS
*   **Tank Melt Specialization**: Cassiopeia is a backliner who only focuses on 1 hero at a time. This concentrated, single-target magic damage output results in exceptionally high sustain DPS, making her highly effective at burning down frontline tanks.

---

### 2. Jhin 🏹

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Channels and fires a piercing physical line spell scaling with AD.
*   **Cast Lockout**: 1.6 seconds base.
*   **Attacks to Cast**: 12 attacks.
*   **Line Pierce Falloff**: Spell pierces through multiple enemies, dealing reduced damage per target: Target 1 (100%), Target 2 (44%), Target 3 (19.4%), Target 4 (8.5%).

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 54 | AS = 0.70 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (1 Target Spell damage: \(54 \text{ AD} \times 7.44 + 60 = 461.8\))**:
    *   Cycle Duration = \((12 + 1.6) / 0.70 = 19.43\text{ seconds}\)
    *   Normal DPS = \((12 \times 54) / 19.43 \times 1.10 = \mathbf{36.7}\)
    *   Spell DPS (1 Target) = \(461.8 / 19.43 \times 1.10 = \mathbf{26.1}\)
    *   Total DPS (1 Target) = \(36.7 + 26.1 = \mathbf{62.8}\)
*   **Multi-Target Scaling (2-Target Average = 1.44x spell damage; flat spell damage scales 60/90/135 per star)**:
    *   **1★**: AD = 54 | Normal = 36.7 | Spell = 37.6 | **Total DPS: 74.3**
    *   **2★**: AD = 81 | Normal = 55.0 | Spell = 56.5 | **Total DPS: 111.5**
    *   **3★**: AD = 122 | Normal = 82.6 | Spell = 84.7 | **Total DPS: 167.3**

#### 🧮 Equipped Calculations (Deathblade + Infinity Edge + Last Whisper)
*   **Stats**: AD = 114 (+111% AD) | AS = 0.77 | Crit Chance = 70% (1.28x avg multiplier)
*   **Cycle & DPS**:
    *   Cycle Duration = \((12 + 1.6) / 0.77 = \mathbf{17.66\text{ seconds}}\)
    *   Normal DPS = \((12 \times 114) / 17.66 \times 1.28 = \mathbf{99.1}\)
    *   Spell Damage (1 Target) = \((114 \times 7.44 + 60) \times 1.28 = 1162.4\)
*   **AoE Multi-Target Outputs**:
    | Target Count | Damage Multiplier | Spell DPS | Total DPS |
    | :--- | :---: | :---: | :---: |
    | **1 Target** | 1.00x | 65.8 | **164.9** |
    | **2 Targets (Avg)**| 1.44x | 94.8 | **193.9** |
    | **3 Targets** | 1.63x | 107.5 | **206.6** |
    | **4 Targets** | 1.72x | 113.1 | **212.2** |

#### 🔍 Analyze DPS
*   **Burst AoE Distribution**: Jhin is a backliner who does big burst AoE damage. While his raw single-target DPS is limited by his long 1.6s cast lockout, Jhin's DPS can match other carries when his skill hits at least 2 people. This is highly consistent in real combat due to his large skill hitbox.

---

### 3. Malzahar 🌌

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Spawns two portals dealing magic damage in a rectangular area. Portal hits instantly destroy 50% of active shields on all hit targets.
*   **Cast Lockout**: 0.8 seconds base.
*   **Attacks to Cast**: 6 attacks (baseline unequipped) / 4 attacks (equipped with Spear of Shojin).

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 40 | AS = 0.70 | AP = 100 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (1 Target Spell damage: \(220 \text{ base}\))**:
    *   Cycle Duration = \((6 + 0.8) / 0.70 = 9.71\text{ seconds}\)
    *   Normal DPS = \((6 \times 40) / 9.71 \times 1.10 = \mathbf{27.2}\)
*   **Star & Multi-Target Scaling (Typical 2-Target Average; spell base 220/330/500 per star)**:
    *   **1★**: Normal = 27.2 | Spell (2 Tgt = 440) = \(440 / 9.71 \times 1.10 = 49.8\) | **Total DPS: 77.0**
    *   **2★**: Normal = 40.8 | Spell (2 Tgt = 660) = \(660 / 9.71 \times 1.10 = 74.7\) | **Total DPS: 115.5**
    *   **3★**: Normal = 61.2 | Spell (2 Tgt = 1000) = \(1000 / 9.71 \times 1.10 = 113.2\) | **Total DPS: 174.4**

#### 🧮 Equipped Calculations (Spear of Shojin + Jeweled Gauntlet + Rabadon's Deathcap)
*   **Stats**: AD = 40 | AS = 0.81 | AP = 215 | Crit Chance = 40% (1.28x avg multiplier, spell crits enabled)
*   **Cycle & DPS (Spear of Shojin requires 4 attacks to cast)**:
    *   Cycle Duration = \((4 + 0.8) / 0.81 = \mathbf{5.93\text{ seconds}}\)
    *   Normal DPS = \((4 \times 40) / 5.93 \times 1.28 = \mathbf{34.6}\)
    *   Base Spell Damage (1 Target) = \(220 \text{ base} \times 2.15 \text{ AP} \times 1.28 = 605.4\)
*   **AoE Multi-Target Outputs**:
    | Target Count | Spell Damage | Spell DPS | Total DPS |
    | :--- | :---: | :---: | :---: |
    | **1 Target** | 605.4 | 102.2 | **136.8** |
    | **2 Targets (Avg)**| 1210.9 | 204.3 | **238.9** |
    | **3 Targets** | 1816.3 | 306.5 | **341.1** |

#### 🔍 Analyze DPS
*   **High-Impact Small-AoE Portals**: Malzahar is a backliner who does AoE damage. Similar to Jhin, his DPS is not high when his skill only hits 1 person. However, when it hits at least 2 targets, his DPS skyrockets even higher than Jhin's. Note that his portal hitbox is relatively small for an AoE spell, meaning his skill typically hits only 1–2 enemies at a time.

---

### 4. Samira 🌹

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Fires a quick physical spell that deals AD-scaling damage and permanently shreds target Armor by flat values.
*   **Cast Lockout**: 0.8 seconds base.
*   **Attacks to Cast**: 3 attacks (baseline unequipped) / 2 attacks (equipped with Blue Buff).

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 45 | AS = 0.70 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Spell damage: \(45 \text{ AD} \times 1.75 + 15 = 93.75\))**:
    *   Cycle Duration = \((3 + 0.8) / 0.70 = 5.43\text{ seconds}\)
    *   Normal DPS = \((3 \times 45) / 5.43 \times 1.10 = \mathbf{27.4}\)
    *   Spell DPS = \(93.75 / 5.43 \times 1.10 = \mathbf{19.0}\)
    *   Total DPS = \(27.4 + 19.0 = \mathbf{46.4}\)
*   **Star Scaling (Unequipped; flat spell damage scales 15/22.5/33.75 per star)**:
    *   **1★**: AD = 45 | Spell = 93.75 | **Total DPS: 46.4**
    *   **2★**: AD = 68 | Spell = 140.6 | **Total DPS: 69.5**
    *   **3★**: AD = 101 | Spell = 210.9 | **Total DPS: 104.2**

#### 🧮 Equipped Calculations (Blue Buff + Infinity Edge + Last Whisper)
*   **Stats**: AD = 65 (+45% AD) | AS = 0.77 | Crit Chance = 70% (1.28x avg multiplier)
*   **Cycle & DPS (Blue Buff requires 2 attacks to cast)**:
    *   Cycle Duration = \((2 + 0.8) / 0.77 = \mathbf{3.64\text{ seconds}}\)
    *   Normal DPS = \((2 \times 65) / 3.64 \times 1.28 = \mathbf{45.8}\)
    *   Spell Damage = \((65 \text{ AD} \times 1.75 + 15) \times 1.28 = 164.8\)
*   **Output Breakdown**:
    | Component | Raw Damage | DPS Contribution | Total DPS |
    | :--- | :---: | :---: | :---: |
    | **Normal Attacks** | 166.4 (2 hits) | 45.8 | \- |
    | **Spell Cast** | 164.8 (1 hit) | 45.3 | \- |
    | **Combined** | 331.2 | \- | **91.1** |

#### 🔍 Analyze DPS
*   **Utility & Armor Shredding**: Samira is a backliner who focuses on 1 target and shreds their armor quickly. Although focusing on a single target (similar to Cassiopeia) might suggest high DPS, Samira's personal DPS is super low when you only look at the numbers. However, this raw DPS does not account for the permanent armor reduction from her skill, which significantly increases the damage output of her entire team against her target.

---

### 5. Tristana 🔫

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Active grants +70% Attack Speed for 4 seconds. During the buff, her attacks explode dealing 60% AD bonus physical splash damage to adjacent hexes.
*   **Mana Rules**: She cannot gain mana while the buff is active.
*   **Cast Lockout**: 0 seconds (Steroid caster, buff starts instantly).
*   **Attacks to Cast**: 4 attacks (charges 40 max mana).

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 45 | AS = 0.70 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle Duration**:
    *   Charging Phase: \(4 / 0.70 = 5.71\text{s}\)
    *   Active Steroid Phase: 4.0 seconds at \(0.70 \times 1.70 = 1.19\text{ AS}\) (4.76 attacks).
    *   Total Cycle = \(5.71 + 4.0 = \mathbf{9.71\text{ seconds}}\)
*   **Star & Splash Scaling (Assuming 1 Splash Target Average)**:
    *   **1★**: Normal = \((4 \times 45) / 9.71 \times 1.10 = 20.4\) | Spell (4.76 splash attacks) = \((4.76 \times 45 \times 1.60) / 9.71 \times 1.10 = 38.8\) | **Total DPS: 59.2**
    *   **2★**: Normal = 30.6 | Spell = 58.2 | **Total DPS: 88.8**
    *   **3★**: Normal = 45.9 | Spell = 87.3 | **Total DPS: 133.2**
*   **Star & Splash Scaling (Assuming 2 Splash Targets - 3 Total Hit)**:
    *   **1★**: Normal = 20.4 | Spell (4.76 attacks @ 2.20x AD total) = \((4.76 \times 45 \times 2.20) / 9.71 \times 1.10 = 53.3\) | **Total DPS: 73.7**
    *   **2★**: Normal = 30.6 | Spell = 80.6 | **Total DPS: 111.2**
    *   **3★**: Normal = 45.9 | Spell = 119.7 | **Total DPS: 165.6**

#### 🧮 Equipped Calculations (Guinsoo's Rageblade + Last Whisper + Infinity Edge)
*   **Stats**: AD = 65 (+45% AD) | Crit Chance = 70% (1.28x avg multiplier)
    *   Start AS (0s) = 0.90 AS (1.39 AS active steroid)
    *   End AS (30s) = 2.37 AS (2.86 AS active steroid)
    *   Average AS = 1.63 AS (2.12 AS active steroid)
*   **Ramping Cycle & Splash DPS (Assuming 1 Splash Target)**:
    *   *Start (0s)*: Cycle = \((4 / 0.90) + 4.0 = \mathbf{8.46\text{s}}\) (Total Attacks = 9.54)
    *   *End (30s)*: Cycle = \((4 / 2.37) + 4.0 = \mathbf{5.69\text{s}}\) (Total Attacks = 15.42)
    *   *Average*: Cycle = \((4 / 1.63) + 4.0 = \mathbf{6.45\text{s}}\) (Total Attacks = 12.48)
*   **Ramping Outputs (1 Splash Target)**:
    | Phase | Base AS | Cycle Duration | Total Attacks | Normal DPS | Spell (Splash) DPS | Total DPS |
    | :--- | :---: | :---: | :---: | :---: | :---: | :---: |
    | **Start (0s)** | 0.90 | 8.46s | 9.54 | 93.8 | 32.7 | **126.5** |
    | **End (30s)** | 2.37 | 5.69s | 15.42 | 225.5 | 100.2 | **325.7** |
    | **Average** | 1.63 | 6.45s | 12.48 | 161.0 | 65.6 | **226.6** |

*   **Ramping Outputs (2 Splash Targets)**:
    | Phase | Base AS | Cycle Duration | Total Attacks | Normal DPS | Spell (Splash) DPS | Total DPS |
    | :--- | :---: | :---: | :---: | :---: | :---: | :---: |
    | **Start (0s)** | 0.90 | 8.46s | 9.54 | 94.0 | 65.5 | **159.5** |
    | **End (30s)** | 2.37 | 5.69s | 15.42 | 226.0 | 200.5 | **426.5** |
    | **Average** | 1.63 | 6.45s | 12.48 | 161.0 | 131.2 | **292.2** |

#### 🔍 Analyze DPS
*   **Super-Linear Attack Speed Scaling**: Tristana is a backline carry whose baseline DPS appears low because she lacks flat spell damage.
*   **Why normal carries scale linearly (1:1)**: For a standard carry (e.g., Cassiopeia), their cycle duration is modeled as $\text{Cycle Duration} = \frac{N + L}{\text{AS}}$ (where $N$ is attacks to cast, and $L$ is cast lockout). Since their damage per cycle $D$ (autos + spell) is constant with respect to AS, their DPS formula is:
    $$\text{DPS} = \frac{D}{\text{Cycle Duration}} = \text{AS} \times \frac{D}{N + L} = \text{Constant} \times \text{AS}$$
    This represents a mathematically perfect linear (1:1) relationship: doubling AS doubles DPS.
*   **Why Tristana scales super-linearly (better than 1:1)**: Because Tristana's active steroid phase is a *fixed* 4.0 seconds (which does not scale down with AS), her cycle duration is $\frac{4}{\text{AS}} + 4.0$. However, her damage per cycle *increases* with AS because she squeezes more splash attacks into that 4s window: $D(\text{AS}) = [4 + (6.8 + 4.08 \times S) \times \text{AS}] \times \text{AD}$ (where $S$ is splash targets). Her DPS formula is:
    $$\text{DPS}(\text{AS}) = \text{AS} \times \text{AD} \times \frac{4 + (6.8 + 4.08 \times S) \times \text{AS}}{4 + 4 \times \text{AS}}$$
    Because both the numerator and denominator scale with AS, her DPS scaling curve is concave up (super-linear). For instance, tripling her AS from 0.70 to 2.10 yields a **4.03x** DPS increase (climbing from 73.7 to 292.2 with 2 splash targets), showing that she scales far better with AS than any linear champion.

---

### 6. Viego 👑

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Strikes his target, dealing magic damage. Auto-attacks permanently gain bonus stacking magic damage on-hit for the rest of combat.
*   **Cast Lockout**: 0.8 seconds base.
*   **Attacks to Cast**: 5 attacks.

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 45 | AS = 0.75 | AP = 100 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Scaled over 30s fight: Avg 2 Stacks = 40 Magic Damage per auto-attack on-hit)**:
    *   Cycle Duration = \((5 + 0.8) / 0.75 = 7.73\text{ seconds}\)
    *   Normal DPS = \((5 \times 45) / 7.73 \times 1.10 = \mathbf{32.0}\)
    *   Spell DPS = \((110 \text{ active} + 200 \text{ on-hit}) / 7.73 \times 1.10 = \mathbf{44.1}\)  *(Note: 200 on-hit is calculated as 5 basic attacks × average stack damage of 40)*
    *   Total DPS = \(32.0 + 44.1 = \mathbf{76.1}\)
*   **Star Scaling (Unequipped, 30s avg stacks; active 110/165/250, stack 20/30/45 per star)**:
    *   **1★**: Normal = 32.0 | Spell = 44.1 | **Total DPS: 76.1**
    *   **2★**: Normal = 48.0 | Spell = 66.2 | **Total DPS: 114.2**
    *   **3★**: Normal = 72.0 | Spell = 99.6 | **Total DPS: 171.6**

#### 🧮 Equipped Calculations (Jeweled Gauntlet + Hextech Gunblade + Titan's Resolve)
*   **Stats (Max Titan's Stacks)**: AD = 77 (+70% AD) | AS = 0.94 | AP = 195 | Crit Chance = 40% (1.28x avg multiplier, spell crits enabled)
*   **Cycle & DPS (Scaled over 30s fight: Avg 2 Stacks = 40 Magic Damage per auto-attack on-hit → 200 base on-hit damage over 5-attack cycle)**:
    *   Cycle Duration = \((5 + 0.8) / 0.94 = \mathbf{6.17\text{ seconds}}\)
    *   Normal DPS = \((5 \times 77) / 6.17 \times 1.28 = \mathbf{79.9}\)
    *   Spell Damage per cycle = \((110 \text{ active} + 200 \text{ on-hit}) \times 1.95 \text{ AP} \times 1.28 = 773.8\)  *(Note: 200 on-hit is calculated as 5 basic attacks × average stack damage of 40)*
*   **Output Breakdown**:
    | Component | Raw Damage | DPS Contribution | Total DPS |
    | :--- | :---: | :---: | :---: |
    | **Normal Attacks** | 492.8 (5 hits) | 79.9 | \- |
    | **Spell Cast & Stacks** | 773.8 (1 cast + 5 on-hits) | 125.4 | \- |
    | **Combined** | 1266.6 | \- | **205.3** |

#### 🔍 Analyze DPS
*   **Close-Range Melee Scaling**: Viego is a close-ranged carry who also focuses mainly on 1 target.
*   **Engagement Scaling**: Stacking magic damage on-hit over a 30s fight duration (averaging 2 stacks active) raises his baseline DPS to **76.1** unequipped and **205.3** equipped, rewarding long melee engagements.
