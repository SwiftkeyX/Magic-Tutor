# TFT Set 9 Tier 3 Carry DPS Analysis (Unified)

This document provides a unified mathematical breakdown of baseline (unequipped) and well-equipped (3-item) DPS scaling for all 3-Gold carry champions in Teamfight Tactics (Set 9).

Putting baseline and itemized calculations in a single document guarantees that both calculations utilize identical, up-to-date mechanical assumptions (AS-scaling lockouts, targeting splits, and cast animation frames).

---

### 📊 Master DPS Summary Tables

### 📘 Table 1: Baseline (Unequipped) DPS Summary
*Consolidated comparison across 1★, 2★, and 3★ star levels with NO items equipped.*

| Champion | Star | Base AD | AS | Normal DPS | Spell DPS | Total DPS |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Akshan** (Single Target) | 1★<br>2★<br>3★ | 60<br>90<br>135 | 0.75 | 38.9<br>58.3<br>87.5 | 33.6<br>52.2<br>80.9 | **72.5**<br>**110.5**<br>**168.4** |
| **Darius** (1 Reset / 2 Casts Avg) | 1★<br>2★<br>3★ | 65<br>98<br>146 | 0.70 | 40.9<br>61.4<br>92.1 | 37.6<br>56.0<br>82.7 | **78.5**<br>**117.4**<br>**174.8** |
| **Ekko** (Single Target) | 1★<br>2★<br>3★ | 50<br>75<br>113 | 0.80 | 37.9<br>56.9<br>85.3 | 38.7<br>57.7<br>86.5 | **76.6**<br>**114.6**<br>**171.8** |
| **Garen** (2 Targets Avg) | 1★<br>2★<br>3★ | 70<br>105<br>158 | 0.75 | 42.0<br>63.0<br>94.5 | 67.2<br>103.3<br>160.7 | **109.2**<br>**166.3**<br>**255.2** |
| **Kalista** (Single Target, 12 stack Rend) | 1★<br>2★<br>3★ | 45<br>68<br>101 | 0.85 | 37.1<br>55.7<br>83.5 | 27.0<br>40.5<br>67.5 | **64.1**<br>**96.2**<br>**151.0** |
| **Karma** (2 Targets Avg) | 1★<br>2★<br>3★ | 45<br>68<br>101 | 0.70 | 29.9<br>44.8<br>67.3 | 75.2<br>112.8<br>169.3 | **105.1**<br>**157.6**<br>**236.6** |
| **Katarina** (1.5 Targets / 4.5 hits Avg) | 1★<br>2★<br>3★ | 50<br>75<br>113 | 0.75 | 35.9<br>53.8<br>80.7 | 58.5<br>88.8<br>141.2 | **94.4**<br>**142.6**<br>**221.9** |
| **Rek'Sai** (Target < 66% HP, Marked) | 1★<br>2★<br>3★ | 60<br>90<br>135 | 0.75 | 45.0<br>67.5<br>101.2 | 25.1<br>37.6<br>56.4 | **70.1**<br>**105.1**<br>**157.6** |
| **Vel'Koz** (1.5 Targets Avg splits) | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 27.2<br>40.8<br>61.2 | 37.4<br>56.1<br>93.4 | **64.6**<br>**96.9**<br>**154.6** |

### ⚔️ Table 2: Well-Equipped (3-Item) DPS Summary
*Consolidated comparison across 1★, 2★, and 3★ star levels with 3 core optimal items equipped. Ramping items (AA, Guinsoo) represent **[Start / End / Average]**.*

| Champion | Star | AD (Equipped) | AS (Equipped) | Normal DPS | Spell DPS | Total DPS |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Akshan** (Single Target) | 1★<br>2★<br>3★ | 133<br>200<br>300 | 0.83 | 161.3<br>242.0<br>362.9 | 82.2<br>125.4<br>191.5 | **243.5**<br>**367.4**<br>**554.4** |
| **Darius** (1 Reset / 2 Casts Avg) | 1★<br>2★<br>3★ | 133<br>200<br>300 | 0.88 | 118.7<br>178.1<br>267.1 | 103.3<br>154.2<br>228.5 | **222.0**<br>**332.3**<br>**495.6** |
| **Ekko** (Single Target) | 1★<br>2★<br>3★ | 85<br>128<br>191 | 1.00 | 93.8<br>140.8<br>211.1 | 109.7<br>163.5<br>245.3 | **203.5**<br>**304.3**<br>**456.4** |
| **Garen** (2 Targets Avg) | 1★<br>2★<br>3★ | 119<br>179<br>268 | 0.94 | 97.4<br>146.1<br>219.2 | 194.8<br>299.5<br>465.7 | **292.2**<br>**445.6**<br>**684.9** |
| **Kalista** (Single Target, 12 stack Rend) | 1★<br>2★<br>3★ | 45<br>68<br>101 | 1.13 / 2.92 / 2.02 | 57.4 / 148.4 / 102.7<br>86.1 / 222.6 / 154.1<br>129.2 / 333.9 / 231.1 | 75.8 / 195.9 / 135.5<br>113.8 / 293.8 / 203.3<br>189.6 / 489.7 / 338.8 | **133.2 / 344.3 / 238.2**<br>**199.9 / 516.4 / 357.4**<br>**318.8 / 823.6 / 569.9** |
| **Karma** (2 Targets Avg) | 1★<br>2★<br>3★ | 45<br>68<br>101 | 0.70 | 33.6<br>50.4<br>75.6 | 158.7 / 285.6 / 211.6<br>238.1 / 428.5 / 317.4<br>357.1 / 642.8 / 476.1 | **192.3 / 319.2 / 245.2**<br>**288.5 / 478.9 / 367.8**<br>**432.7 / 718.4 / 551.7** |
| **Katarina** (1.5 Targets / 4.5 hits Avg) | 1★<br>2★<br>3★ | 50<br>75<br>113 | 0.75 | 41.7<br>62.6<br>93.9 | 113.5<br>172.3<br>274.1 | **155.2**<br>**234.9**<br>**368.0** |
| **Rek'Sai** (Target < 66% HP, Marked) | 1★<br>2★<br>3★ | 123<br>185<br>277 | 0.94 | 130.3<br>195.5<br>293.2 | 64.4<br>96.6<br>144.9 | **194.7**<br>**292.1**<br>**438.1** |
| **Vel'Koz** (1.5 Targets Avg splits) | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 30.9<br>46.4<br>69.5 | 102.0<br>152.9<br>254.9 | **132.9**<br>**199.3**<br>**324.4** |

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
*   **AP vs. AD-ratio spells**: AP multiplies a spell's flat/base damage component only (e.g. Darius's 55/80/110 flat). Pure AD-ratio components (Darius's 3.5 × AD, Garen's spins, Rek'Sai's bite) do not scale with AP — matching TFT's per-star ability data.

---

## 🔍 Detailed Champion Breakdowns

### 1. Akshan 🦅

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Locks on to the farthest enemy and fires a rapid channel of 6 sniper shots. Each shot deals physical damage scaling with AD.
*   **Cast Lockout**: 3.0 seconds base (channel duration).
*   **Attacks to Cast**: 11 attacks (max mana is 110).

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 60 | AS = 0.75 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Spell damage: \(((60 \text{ AD} \times 1.25) + 20) \times 6 = 570\))**:
    *   Cycle Duration = \((11 + 3.0) / 0.75 = \mathbf{18.67\text{ seconds}}\)
    *   Normal DPS = \((11 \times 60) / 18.67 \times 1.10 = \mathbf{38.9}\)
    *   Spell DPS = \(570 / 18.67 \times 1.10 = \mathbf{33.6}\)
    *   Total DPS = \(38.9 + 33.6 = \mathbf{72.5}\)
*   **Star Scaling (Unequipped; flat per shot scales 20/35/60 per star)**:
    *   **1★**: AD = 60 | Spell = 570 | **Total DPS: 72.5**
    *   **2★**: AD = 90 | Spell = 885 | **Total DPS: 110.5**
    *   **3★**: AD = 135 | Spell = 1372.5 | **Total DPS: 168.4**

#### 🧮 Equipped Calculations (Deathblade + Infinity Edge + Runaan's Hurricane)
*   **Stats**: AD = 133 (+121% AD) | AS = 0.83 | Crit Chance = 60% (1.24x avg multiplier)
*   **Cycle & DPS**:
    *   Cycle Duration = \((11 + 3.0) / 0.83 = \mathbf{16.87\text{ seconds}}\)
    *   Normal DPS (with Runaan's 1.5x Amp) = \((11 \times 133 \times 1.5) / 16.87 \times 1.24 = \mathbf{161.3}\)
    *   Spell Damage = \((133 \text{ AD} \times 1.25 + 20) \times 6 \text{ shots} \times 1.24 = 1385.7\)
    *   Spell DPS = \(1385.7 / 16.87 = \mathbf{82.2}\)
*   **Output Breakdown**:
    | Component | Raw Damage | DPS Contribution | Total DPS |
    | :--- | :---: | :---: | :---: |
    | **Normal Attacks + Bolts**| 2721.2 (11 hits) | 161.3 | \- |
    | **Bullet Spray Spell** | 1385.7 (6 hits) | 82.2 | \- |
    | **Combined** | 4106.9 | \- | **243.5** |

---

### 2. Darius 🪓

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Slashes his target, dealing AD-scaling physical damage. If this kills the target, he immediately casts again dealing reduced damage (10% falloff at 1★).
*   **Cast Lockout**: 1.0 second base (per cast).
*   **Attacks to Cast**: 9 attacks (max mana is 90).

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 65 | AS = 0.70 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Assuming 1 Reset / 2 Casts average, total lockout = 2.0s)**:
    *   Cycle Duration = \((9 + 2.0) / 0.70 = \mathbf{15.71\text{ seconds}}\)
    *   Normal DPS = \((9 \times 65) / 15.71 \times 1.10 = \mathbf{40.9}\)
    *   Spell Damage = \(((65 \text{ AD} \times 3.5 + 55) + (65 \text{ AD} \times 3.5 + 55) \times 0.90) = 536.8\)
    *   Spell DPS = \(536.8 / 15.71 \times 1.10 = \mathbf{37.6}\)
    *   Total DPS = \(40.9 + 37.6 = \mathbf{78.5}\)
*   **Star Scaling (Unequipped, 1 Reset Avg; flat scales 55/80/110 per star)**:
    *   **1★**: AD = 65 | Spell = 536.8 | **Total DPS: 78.5**
    *   **2★**: AD = 98 | Spell = 800.4 | **Total DPS: 117.4**
    *   **3★**: AD = 146 | Spell = 1181.6 | **Total DPS: 174.8**

#### 🧮 Equipped Calculations (Infinity Edge + Bloodthirster + Titan's Resolve)
*   **Stats**: AD = 133 (+105% AD) | AS = 0.88 | AP = 150 | Crit Chance = 60% (1.24x avg multiplier)
*   **Cycle & DPS (Assuming 1 Reset / 2 Casts average, total lockout = 2.0s; AP amplifies only the 55 flat, not the 3.5 × AD part)**:
    *   Cycle Duration = \((9 + 2.0) / 0.88 = \mathbf{12.50\text{ seconds}}\)
    *   Normal DPS = \((9 \times 133) / 12.50 \times 1.24 = \mathbf{118.7}\)
    *   Spell Damage = \(((133 \text{ AD} \times 3.5) + (55 \times 1.50 \text{ AP})) \times 1.90 \text{ (reset chain)} \times 1.24 = 1291.1\)
    *   Spell DPS = \(1291.1 / 12.50 = \mathbf{103.3}\)
*   **Output Breakdown**:
    | Component | Raw Damage | DPS Contribution | Total DPS |
    | :--- | :---: | :---: | :---: |
    | **Normal Attacks** | 1484.3 (9 hits) | 118.7 | \- |
    | **Guillotine Casts** | 1291.1 (2 hits) | 103.3 | \- |
    | **Combined** | 2775.4 | \- | **222.0** |

---

### 3. Ekko ⏳

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Magic damage dive that heals him for 20% of damage taken in the last 4 seconds.
*   **Cast Lockout**: 0.8 seconds base.
*   **Attacks to Cast**: 5 attacks (max mana is 50).

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 50 | AS = 0.80 | AP = 100 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Spell damage: 255)**:
    *   Cycle Duration = \((5 + 0.8) / 0.80 = \mathbf{7.25\text{ seconds}}\)
    *   Normal DPS = \((5 \times 50) / 7.25 \times 1.10 = \mathbf{37.9}\)
    *   Spell DPS = \(255 / 7.25 \times 1.10 = \mathbf{38.7}\)
    *   Total DPS = \(37.9 + 38.7 = \mathbf{76.6}\)
*   **Star Scaling (Unequipped)**:
    *   **1★**: AD = 50 | Spell = 255 | **Total DPS: 76.6**
    *   **2★**: AD = 75 | Spell = 380 | **Total DPS: 114.6**
    *   **3★**: AD = 113 | Spell = 570 | **Total DPS: 171.8**

#### 🧮 Equipped Calculations (Jeweled Gauntlet + Hextech Gunblade + Titan's Resolve)
*   **Stats**: AD = 85 (+70% AD) | AS = 1.00 | AP = 195 (100 + JG 20 + Gunblade 25 + Titan's 50) | Crit Chance = 40% (1.28x avg multiplier, spell crits enabled)
*   **Cycle & DPS**:
    *   Cycle Duration = \((5 + 0.8) / 1.00 = \mathbf{5.80\text{ seconds}}\)
    *   Normal DPS = \((5 \times 85) / 5.80 \times 1.28 = \mathbf{93.8}\)
    *   Spell Damage = \(255 \text{ base} \times 1.95 \text{ AP} \times 1.28 \text{ (Crit)} = 636.5\)
    *   Spell DPS = \(636.5 / 5.80 = \mathbf{109.7}\)
*   **Output Breakdown**:
    | Component | Raw Damage | DPS Contribution | Total DPS |
    | :--- | :---: | :---: | :---: |
    | **Normal Attacks** | 544.0 (5 hits) | 93.8 | \- |
    | **Chronobreak Cast** | 636.5 (1 hit) | 109.7 | \- |
    | **Combined** | 1180.5 | \- | **203.5** |

---

### 4. Garen 🌀

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Spins for 4.0 seconds, dealing physical damage to adjacent enemies. The spin active duration is a steroid buff phase, meaning the **4.0s duration does not scale with AS**, but Garen spins more times based on Attack Speed.
*   **Cast Lockout**: 4.0 seconds (spin duration active).
*   **Attacks to Cast**: 8 attacks.

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 70 | AS = 0.75 | AP = 100 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Assuming 2 Targets Hit Avg, spins = 8)**:
    *   Cycle Duration = \((8 / 0.75) + 4.0 = \mathbf{14.67\text{ seconds}}\)
    *   Normal DPS = \((8 \times 70) / 14.67 \times 1.10 = \mathbf{42.0}\)
    *   Spell Damage = \((70 \text{ AD} \times 0.80) \times 8 \text{ spins} \times 2 \text{ targets} = 896.0\)
    *   Spell DPS = \(896.0 / 14.67 \times 1.10 = \mathbf{67.2}\)
    *   Total DPS = \(42.0 + 67.2 = \mathbf{109.2}\)
*   **Star Scaling (Unequipped, 2 Targets; spin AD-ratio scales 0.80/0.82/0.85 per star)**:
    *   **1★**: AD = 70 | Spell = 896.0 | **Total DPS: 109.2**
    *   **2★**: AD = 105 | Spell = 1377.6 | **Total DPS: 166.3**
    *   **3★**: AD = 158 | Spell = 2142.0 | **Total DPS: 255.2**

#### 🧮 Equipped Calculations (Bloodthirster + Titan's Resolve + Jeweled Gauntlet)
*   **Stats**: AD = 119 (+70% AD) | AS = 0.94 | AP = 170 (no effect — Judgement is a pure AD-ratio spin) | Crit Chance = 40% (1.28x avg multiplier, JG spell crits enabled)
*   **Cycle & DPS (Assuming 2 Targets Hit Avg; +25% bonus AS → 2.5 spins/s × 4s = 10 spins)**:
    *   Cycle Duration = \((8 / 0.94) + 4.0 = \mathbf{12.53\text{ seconds}}\)
    *   Normal DPS = \((8 \times 119) / 12.53 \times 1.28 = \mathbf{97.4}\)
    *   Single Spin Damage = \((119 \text{ AD} \times 0.80) \times 1.28 \text{ (Crit)} = 121.9\)
    *   Spell DPS = \(121.9 \times 10 \text{ spins} \times 2 \text{ targets} / 12.53 = \mathbf{194.8}\)
*   **AoE Multi-Target Outputs**:
    | Target Count | Spell Damage (10 spins) | Spell DPS | Total DPS |
    | :--- | :---: | :---: | :---: |
    | **1 Target** | 1218.6 | 97.4 | **194.8** |
    | **2 Targets (Avg)** | 2437.1 | 194.8 | **292.2** |
    | **3 Targets** | 3655.7 | 292.2 | **389.6** |

---

### 5. Kalista 🎯

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Basic attacks stack spears. Active casts and inserts 6 spears. Speared targets take true damage when ripped out (automatic execute).
*   **Cast Lockout**: 0.8 seconds base.
*   **Attacks to Cast**: 6 attacks.

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 45 | AS = 0.85 | AP = 100 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (12 total spears stacked: 6 autos + 6 cast)**:
    *   Cycle Duration = \((6 + 0.8) / 0.85 = \mathbf{8.00\text{ seconds}}\)
    *   Normal DPS = \((6 \times 45) / 8.00 \times 1.10 = \mathbf{37.1}\)
    *   Spell Damage (12 spears, True damage) = \(18 \text{ base} \times 12 = 216.0\)
    *   Spell DPS = \(216.0 / 8.00 = \mathbf{27.0}\) (true damage does not crit at baseline)
    *   Total DPS = \(37.1 + 27.0 = \mathbf{64.1}\)
*   **Star Scaling (Unequipped; spear true damage scales 18/27/45 per star)**:
    *   **1★**: AD = 45 | Spell = 216.0 | **Total DPS: 64.1**
    *   **2★**: AD = 68 | Spell = 324.0 | **Total DPS: 96.2**
    *   **3★**: AD = 101 | Spell = 540.0 | **Total DPS: 151.0**

#### 🧮 Equipped Calculations (Guinsoo's Rageblade + Spear of Shojin + Jeweled Gauntlet)
*   **Stats**: AD = 45 | AP = 165 | Crit Chance = 40% (1.28x avg multiplier for normal attacks, spell crits enabled)
    *   Start AS (0s) = 1.13 AS
    *   End AS (30s) = 2.92 AS
    *   Average AS = 2.02 AS
*   **Ramping Cycle & Rend True Damage (12 spears total: 6 autos + 6 cast)**:
    *   Spell Damage = \(18 \text{ base} \times 1.65 \text{ AP} \times 12 \text{ spears} \times 1.28 \text{ (Crit)} = 456.2\) true damage (JG lets the Rend crit).
    *   *Start (0s)*: Cycle = \(6.8 / 1.13 = \mathbf{6.02\text{s}}\) (Normal DPS = 57.4)
    *   *End (30s)*: Cycle = \(6.8 / 2.92 = \mathbf{2.33\text{s}}\) (Normal DPS = 148.4)
    *   *Average*: Cycle = \(6.8 / 2.02 = \mathbf{3.37\text{s}}\) (Normal DPS = 102.7)
*   **Ramping Outputs**:
    | Phase | Base AS | Cycle Duration | Normal DPS | Spell DPS | Total DPS |
    | :--- | :---: | :---: | :---: | :---: | :---: |
    | **Start (0s)** | 1.13 | 6.02s | 57.4 | 75.8 | **133.2** |
    | **End (30s)** | 2.92 | 2.33s | 148.4 | 195.9 | **344.3** |
    | **Average** | 2.02 | 3.37s | 102.7 | 135.5 | **238.2** |

---

### 6. Karma 🪷

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Launches energy bursts in a 1-hex radius. Every 3rd cast fires 3 bursts instead of 1.
*   **Cast Lockout**: 0.8 seconds base.
*   **Attacks to Cast**: 5 attacks (baseline unequipped) / 4 attacks (equipped with Blue Buff).
*   **Projectile Nuance**: 1.67 average bursts per cast.

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 45 | AS = 0.70 | AP = 100 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Assuming 2 Targets Hit Avg, 1.67 bursts)**:
    *   Cycle Duration = \((5 + 0.8) / 0.70 = \mathbf{8.29\text{ seconds}}\)
    *   Normal DPS = \((5 \times 45) / 8.29 \times 1.10 = \mathbf{29.9}\)
    *   Spell Damage = \((170 \text{ base} \times 1.67 \text{ bursts}) \times 2 \text{ targets} = 567.8\)
    *   Spell DPS = \(567.8 / 8.29 \times 1.10 = \mathbf{75.2}\)
    *   Total DPS = \(29.9 + 75.2 = \mathbf{105.1}\)
*   **Star Scaling (Unequipped, 2 Targets; burst base 170/255/382.5 per star)**:
    *   **1★**: Normal = 29.9 | Spell (2 Tgt) = 75.2 | **Total DPS: 105.1**
    *   **2★**: Normal = 44.8 | Spell = 112.8 | **Total DPS: 157.6**
    *   **3★**: Normal = 67.3 | Spell = 169.3 | **Total DPS: 236.6**

#### 🧮 Equipped Calculations (Blue Buff + Jeweled Gauntlet + Archangel's Staff)
*   **Stats**: AD = 45 | AS = 0.70 | Crit Chance = 40% (1.28x avg multiplier, spell crits enabled)
    *   Start AP (0s) = 150 AP (1.50x spell damage)
    *   End AP (30s) = 270 AP (2.70x spell damage)
    *   Average AP = 200 AP (2.00x spell damage) — Archangel's time-averages +70 AP over a 30s fight
*   **Cycle & DPS (Assuming 2 Targets Hit Avg, Blue Buff requires 4 attacks to cast)**:
    *   Cycle Duration = \((4 + 0.8) / 0.70 = \mathbf{6.86\text{ seconds}}\)
    *   Normal DPS = \((4 \times 45) / 6.86 \times 1.28 = \mathbf{33.6}\)
*   **Ramping Outputs**:
    | Phase | AP Mod | Burst Damage | Spell DPS (1 Target) | Total DPS (1 Target) | Spell DPS (2 Targets) | Total DPS (2 Targets) |
    | :--- | :---: | :---: | :---: | :---: | :---: | :---: |
    | **Start (0s)** | 1.50x | 326.4 | 79.3 | **112.9** | 158.7 | **192.3** |
    | **End (30s)** | 2.70x | 587.5 | 142.8 | **176.4** | 285.6 | **319.2** |
    | **Average** | 2.00x | 435.2 | 105.8 | **139.4** | 211.6 | **245.2** |

---

### 7. Katarina 🔪

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Voracity throws 3 daggers next to enemies, slashes them for magic damage, and teleports.
*   **Cast Lockout**: 1.2 seconds base.
*   **Attacks to Cast**: 8 attacks.

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 50 | AS = 0.75 | AP = 100 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Assuming 1.5 Targets Avg = 4.5 slashes total)**:
    *   Cycle Duration = \((8 + 1.2) / 0.75 = \mathbf{12.27\text{ seconds}}\)
    *   Normal DPS = \((8 \times 50) / 12.27 \times 1.10 = \mathbf{35.9}\)
    *   Spell Damage = \(145 \text{ base} \times 4.5 \text{ hits} = 652.5\)
    *   Spell DPS = \(652.5 / 12.27 \times 1.10 = \mathbf{58.5}\)
    *   Total DPS = \(35.9 + 58.5 = \mathbf{94.4}\)
*   **Star Scaling (Unequipped, 1.5 Targets; dagger base 145/220/350 per star)**:
    *   **1★**: Normal = 35.9 | Spell = 58.5 | **Total DPS: 94.4**
    *   **2★**: Normal = 53.8 | Spell = 88.8 | **Total DPS: 142.6**
    *   **3★**: Normal = 80.7 | Spell = 141.2 | **Total DPS: 221.9**

#### 🧮 Equipped Calculations (Jeweled Gauntlet + Hextech Gunblade + Ionic Spark)
*   **Stats**: AD = 50 | AS = 0.75 | AP = 145 (100 + JG 20 + Gunblade 25) | Crit Chance = 40% (1.28x avg multiplier, spell crits enabled)
*   **Cycle & DPS (Assuming 1.5 Targets Avg = 4.5 slashes total)**:
    *   Cycle Duration = \((8 + 1.2) / 0.75 = \mathbf{12.27\text{ seconds}}\)
    *   Normal DPS = \((8 \times 50) / 12.27 \times 1.28 = \mathbf{41.7}\)
    *   Dagger Hit Damage = \(145 \text{ base} \times 1.45 \text{ AP} \times 1.28 \text{ (Crit)} \times 1.15 \text{ (Spark)} = 309.5\)
*   **AoE Multi-Target Outputs**:
    | Target Count | Dagger Hits | Spell Damage | Spell DPS | Total DPS |
    | :--- | :---: | :---: | :---: | :---: |
    | **1 Target** | 3 hits | 928.4 | 75.7 | **117.4** |
    | **1.5 Targets (Avg)**| 4.5 hits | 1392.6 | 113.5 | **155.2** |
    | **2 Targets** | 6 hits | 1856.9 | 151.4 | **193.1** |

---

### 8. Rek'Sai 🦎

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Furious Bite deals physical damage. If the target is below 66% health, deals true damage instead.
*   **Cast Lockout**: 0.8 seconds base.
*   **Attacks to Cast**: 8 attacks.

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 60 | AS = 0.75 | AP = 100 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Assuming target below 66% health AND marked: ratio = 2.5 bite + 2.4 mark = 4.90x AD, per Riot Set 9 data)**:
    *   Cycle Duration = \((8 + 0.8) / 0.75 = \mathbf{11.73\text{ seconds}}\)
    *   Normal DPS = \((8 \times 60) / 11.73 \times 1.10 = \mathbf{45.0}\)
    *   Spell Damage = \(60 \text{ AD} \times 4.90 = 294.0\) true damage.
    *   Spell DPS = \(294.0 / 11.73 = \mathbf{25.1}\) (True damage does not crit at baseline).
    *   Total DPS = \(45.0 + 25.1 = \mathbf{70.1}\)
    *   *Note: Against an unmarked target the ratio is only 2.5x AD (\(60 \times 2.5 = 150\)) → Spell DPS 12.8, Total DPS 57.8.*

#### 🧮 Equipped Calculations (Bloodthirster + Titan's Resolve + Infinity Edge)
*   **Stats (Max Titan's Stacks)**: AD = 123 (+105% AD) | AS = 0.94 | Crit Chance = 60% with IE (1.24x avg multiplier for auto-attacks) | Titan's +50 AP has no effect (bite is a pure AD-ratio spell)
*   **Cycle & DPS (Assuming target below 66% health AND marked = 4.90x AD)**:
    *   Cycle Duration = \((8 + 0.8) / 0.94 = \mathbf{9.36\text{ seconds}}\)
    *   Normal DPS = \((8 \times 123) / 9.36 \times 1.24 = \mathbf{130.3}\)
    *   Spell Damage = \(123 \text{ AD} \times 4.90 = 602.7\) true damage (Bite is true damage, does not crit).
    *   Spell DPS = \(602.7 / 9.36 = \mathbf{64.4}\)
    *   Total Output = \(130.3 + 64.4 = \mathbf{194.7}\)

---

### 9. Vel'Koz 👁️

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Plasma Fission fires a bolt that splits in two at right angles, dealing 50% damage to all targets passed through.
*   **Cast Lockout**: 0.8 seconds base.
*   **Attacks to Cast**: 6 attacks (baseline unequipped) / 5 attacks (equipped with Blue Buff).

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 40 | AS = 0.70 | AP = 100 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Assuming 1.5 Targets Avg = 75% extra damage split)**:
    *   Cycle Duration = \((6 + 0.8) / 0.70 = \mathbf{9.71\text{ seconds}}\)
    *   Normal DPS = \((6 \times 40) / 9.71 \times 1.10 = \mathbf{27.2}\)
    *   Spell Damage = \(220 \text{ base} \times 1.5 \text{ targets} = 330.0\)
    *   Spell DPS = \(330.0 / 9.71 \times 1.10 = \mathbf{37.4}\)
    *   Total DPS = \(27.2 + 37.4 = \mathbf{64.6}\)
*   **Star Scaling (Unequipped, 1.5 Targets; bolt base 220/330/550 per star)**:
    *   **1★**: Normal = 27.2 | Spell = 37.4 | **Total DPS: 64.6**
    *   **2★**: Normal = 40.8 | Spell = 56.1 | **Total DPS: 96.9**
    *   **3★**: Normal = 61.2 | Spell = 93.4 | **Total DPS: 154.6**

#### 🧮 Equipped Calculations (Blue Buff + Jeweled Gauntlet + Rabadon's Deathcap)
*   **Stats**: AD = 40 | AS = 0.70 | AP = 200 (100 + BB 10 + JG 20 + Rabadon 70) | Crit Chance = 40% (1.28x avg multiplier, spell crits enabled)
*   **Cycle & DPS (Assuming 1.5 Targets Avg, Blue Buff requires 5 attacks to cast)**:
    *   Cycle Duration = \((5 + 0.8) / 0.70 = \mathbf{8.29\text{ seconds}}\)
    *   Normal DPS = \((5 \times 40) / 8.29 \times 1.28 = \mathbf{30.9}\)
    *   Base Spell Damage = \(220 \text{ base} \times 2.00 \text{ AP} \times 1.28 = 563.2\)
*   **AoE Multi-Target / Split Outputs**:
    | Target Count | Spell Damage | Spell DPS | Total DPS |
    | :--- | :---: | :---: | :---: |
    | **1 Target (No splits)** | 563.2 | 68.0 | **98.9** |
    | **1.5 Targets (Avg splits)**| 844.8 | 102.0 | **132.9** |
    | **2 Targets (Full splits)** | 1126.4 | 135.9 | **166.8** |
