# TFT Set 9 Tier 4 Carry DPS Analysis (Unified)

This document provides a unified mathematical breakdown of baseline (unequipped) and well-equipped (3-item) DPS scaling for all 4-Gold carry champions in Teamfight Tactics (Set 9).

Putting baseline and itemized calculations in a single document guarantees that both calculations utilize identical, up-to-date mechanical assumptions (AS-scaling lockouts, targeting splits, and cast animation frames).

---

## 📊 Master DPS Summary Tables

### 📘 Table 1: Baseline (Unequipped) DPS Summary
*Consolidated comparison across 1★, 2★, and 3★ star levels with NO items equipped. AoE/multi-target spells are shown using typical combat averages (3 targets for Aphelios, 2 targets for Azir/Gwen/Yasuo, and 4 targets for Zeri).*

| Champion | Star | Base AD | AS | Normal DPS | Spell DPS | Total DPS |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Aphelios** (3 Targets, 6 Chakrams) | 1★<br>2★<br>3★ | 65<br>98<br>146 | 0.75 | 48.8<br>73.2<br>109.8 | 41.5<br>62.3<br>93.4 | **90.3**<br>**135.5**<br>**203.2** |
| **Azir** (2 Soldiers Avg) | 1★<br>2★<br>3★ | 30<br>45<br>68 | 0.75 | 21.3<br>32.0<br>48.0 | 52.2<br>78.2<br>239.6 | **73.5**<br>**110.2**<br>**287.6** |
| **Gwen** (2 Targets Avg) | 1★<br>2★<br>3★ | 55<br>83<br>124 | 0.80 | 38.7<br>58.1<br>87.1 | 105.6<br>158.4<br>422.4 | **144.3**<br>**216.5**<br>**509.5** |
| **Kai'Sa** (15 Missiles Split) | 1★<br>2★<br>3★ | 45<br>68<br>101 | 0.80 | 35.2<br>52.8<br>79.2 | 73.3<br>110.0<br>305.5 | **108.5**<br>**162.8**<br>**384.7** |
| **Lux** (Single Target Channel) | 1★<br>2★<br>3★ | 45<br>68<br>101 | 0.70 | 22.7<br>34.1<br>51.1 | 92.8<br>138.9<br>347.3 | **115.5**<br>**173.0**<br>**398.4** |
| **Yasuo** (2 Targets Avg) | 1★<br>2★<br>3★ | 75<br>113<br>169 | 0.80 | 55.0<br>82.6<br>123.9 | 61.9<br>92.9<br>139.3 | **116.9**<br>**175.5**<br>**263.2** |
| **Zeri** (4 Targets Avg Chain) | 1★<br>2★<br>3★ | 65<br>98<br>146 | 0.80 | 49.3<br>74.0<br>111.0 | 73.8<br>110.7<br>166.0 | **123.1**<br>**184.7**<br>**277.0** |

### ⚔️ Table 2: Well-Equipped (3-Item) DPS Summary
*Consolidated comparison across 1★, 2★, and 3★ star levels with 3 core optimal items equipped.*

| Champion | Star | AD (Equipped) | AS (Equipped) | Normal DPS | Spell DPS | Total DPS |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Aphelios** (3 Targets, 6 Chakrams) | 1★<br>2★<br>3★ | 137<br>206<br>308 | 0.825 | 131.6<br>197.4<br>296.1 | 115.4<br>173.1<br>259.7 | **247.0**<br>**370.5**<br>**555.8** |
| **Azir** (2 Soldiers Avg) | 1★<br>2★<br>3★ | 30<br>45<br>68 | 1.30 | 43.0<br>64.6<br>97.7 | 157.8<br>229.6<br>717.5 | **200.8**<br>**294.2**<br>**815.2** |
| **Gwen** (2 Targets Avg) | 1★<br>2★<br>3★ | 55<br>83<br>124 | 0.80 | 52.8<br>79.7<br>118.9 | 288.0<br>432.0<br>1152.0 | **340.8**<br>**511.7**<br>**1270.9** |
| **Kai'Sa** (15 Missiles Split) | 1★<br>2★<br>3★ | 45<br>68<br>101 | 0.92 | 44.6<br>67.0<br>100.4 | 299.8<br>449.7<br>1249.0 | **344.4**<br>**516.7**<br>**1349.4** |
| **Lux** (Single Target Channel) | 1★<br>2★<br>3★ | 45<br>68<br>101 | 0.70 | 23.7<br>35.6<br>53.4 | 258.2<br>386.3<br>965.7 | **281.9**<br>**421.9**<br>**1019.1** |
| **Yasuo** (2 Targets Avg) | 1★<br>2★<br>3★ | 166<br>249<br>374 | 0.80 | 137.2<br>205.9<br>308.8 | 154.4<br>231.6<br>347.3 | **291.6**<br>**437.5**<br>**656.1** |
| **Zeri** (4 Targets Avg Chain) | 1★<br>2★<br>3★ | 94<br>141<br>212 | 1.00 | 129.7<br>194.5<br>291.8 | 194.5<br>291.7<br>437.6 | **324.2**<br>**486.2**<br>**729.4** |

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
    *   *Modeling note*: as a simplification, these docs apply the average crit multiplier to spell damage in baseline rows too (strictly, TFT abilities only crit with JG/IE equipped).
*   **AP vs. AD-ratio spells**: AP multiplies a spell's flat/base damage component only. Pure AD-ratio components do not scale with AP — matching TFT's per-star ability data.

---

## 🔍 Detailed Champion Breakdowns

### 1. Aphelios 🌙

#### ⚙️ Mechanics & Combat Nuance
*   **Ability**: Fires a blast dealing AD physical damage in an area. For 7 seconds, equips Chakrams (+3 base, +1 per enemy hit). Attacks deal bonus physical damage per Chakram, and he heals.
*   **Cast Lockout**: 1.0s base.
*   **Attacks to Cast**: 10 attacks (mana is 50 / 100).
*   **Chakram Stacking**: Assuming blast hits 3 enemies avg, equipping 6 Chakrams (+48% AD scaling) for 7s.

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 65 | AS = 0.75 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS**:
    *   Cycle Duration = \((10 + 1.0) / 0.75 = \mathbf{14.67\text{ seconds}}\)
    *   Normal DPS = \((10 \times 65) / 14.67 \times 1.10 = \mathbf{48.8}\)
    *   Spell DPS = \((5.25 \text{ attacks} \times 0.48 \text{ Chakram AD} + 6.00 \text{ blast AD}) \times 65 \text{ AD} \times 1.10 / 14.67 = \mathbf{41.5}\)
    *   Total DPS = \(48.8 + 41.5 = \mathbf{90.3}\)

#### 🧮 Equipped Calculations (Deathblade + Infinity Edge + Last Whisper)
*   **Stats**: AD = 137 (+111% AD) | AS = 0.825 | Crit Chance = 70% (1.28x avg multiplier)
*   **Cycle & DPS**:
    *   Cycle Duration = \((10 + 1.0) / 0.825 = \mathbf{13.33\text{ seconds}}\)
    *   Normal DPS = \((10 \times 137) / 13.33 \times 1.28 = \mathbf{131.6}\)
    *   Spell DPS = \((5.77 \text{ attacks} \times 0.48 \text{ Chakram AD} + 6.00 \text{ blast AD}) \times 137 \text{ AD} \times 1.28 / 13.33 = \mathbf{115.4}\)
    *   Total DPS = \(131.6 + 115.4 = \mathbf{247.0}\)

---

### 2. Azir ☀️

#### ⚙️ Mechanics & Combat Nuance
*   **Ability**: Passive: Every 3rd attack, Sand Soldiers deal magic damage. Active: Summons a Sand Soldier.
*   **Cast Lockout**: 0.8s base.
*   **Attacks to Cast**: 5 attacks (mana is 40 / 50).
*   **Soldier Counts**: Modeled around an average of 2 active Sand Soldiers over a standard fight.

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 30 | AS = 0.75 | AP = 100 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS**:
    *   Cycle Duration = \((5 + 0.8) / 0.75 = \mathbf{7.73\text{ seconds}}\)
    *   Normal DPS = \((5 \times 30) / 7.73 \times 1.10 = \mathbf{21.3}\)
    *   Spell DPS = \((110 \text{ base} \times 2 \text{ soldiers} \times 5 / 3) / 7.73 \times 1.10 = \mathbf{52.2}\)
    *   Total DPS = \(21.3 + 52.2 = \mathbf{73.5}\)

#### 🧮 Equipped Calculations (Guinsoo's Rageblade + Nashor's Tooth + Jeweled Gauntlet)
*   **Stats (Average fight ramping)**: AD = 30 | AS = 1.30 | AP = 150 | Crit Chance = 40% (1.28x avg multiplier)
*   **Cycle & DPS**:
    *   Cycle Duration = \((5 + 0.8) / 1.30 = \mathbf{4.46\text{ seconds}}\)
    *   Normal DPS = \((5 \times 30) / 4.46 \times 1.28 = \mathbf{43.0}\)
    *   Spell DPS = \((110 \text{ base} \times 1.50 \text{ AP} \times 2 \text{ soldiers} \times 1.28 \text{ Crit} \times 5 / 3) / 4.46 = \mathbf{157.8}\)
    *   Total DPS = \(43.0 + 157.8 = \mathbf{200.8}\)

---

### 3. Gwen ✂️

#### ⚙️ Mechanics & Combat Nuance
*   **Ability**: Dashes and snips 3 times in a cone dealing magic damage. Every 3rd cast grants armor and MR.
*   **Cast Lockout**: 1.0s base.
*   **Attacks to Cast**: 4 attacks (mana is 0 / 35).

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 55 | AS = 0.80 | AP = 100 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Assuming 2 Targets Hit Avg)**:
    *   Cycle Duration = \((4 + 1.0) / 0.80 = \mathbf{6.25\text{ seconds}}\)
    *   Normal DPS = \((4 \times 55) / 6.25 \times 1.10 = \mathbf{38.7}\)
    *   Spell DPS = \((100 \text{ base} \times 3 \text{ snips} \times 2 \text{ targets}) / 6.25 \times 1.10 = \mathbf{105.6}\)
    *   Total DPS = \(38.7 + 105.6 = \mathbf{144.3}\)

#### 🧮 Equipped Calculations (Blue Buff + Jeweled Gauntlet + Giant Slayer)
*   **Stats**: AD = 55 | AS = 0.80 | AP = 150 | Crit Chance = 40% (1.28x avg multiplier, +25% Giant Slayer amp)
*   **Cycle & DPS (Blue Buff reduces mana to 25, requiring 3 attacks to cast)**:
    *   Cycle Duration = \((3 + 1.0) / 0.80 = \mathbf{5.00\text{ seconds}}\)
    *   Normal DPS = \((3 \times 55) / 5.00 \times 1.28 \text{ Crit} \times 1.25 \text{ Amp} = \mathbf{52.8}\)
    *   Spell DPS = \((300 \text{ base} \times 1.50 \text{ AP} \times 2 \text{ targets}) \times 1.28 \text{ Crit} \times 1.25 \text{ Amp} / 5.00 = \mathbf{288.0}\)
    *   Total DPS = \(52.8 + 288.0 = \mathbf{340.8}\)

---

### 4. Kai'Sa 👾

#### ⚙️ Mechanics & Combat Nuance
*   **Ability**: Dashes and fires 15 missiles split across nearest 4 targets dealing magic damage.
*   **Cast Lockout**: 1.5s base.
*   **Attacks to Cast**: 12 attacks (mana is 40 / 120) / 8 attacks (equipped with Spear of Shojin).

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 45 | AS = 0.80 | AP = 100 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS**:
    *   Cycle Duration = \((12 + 1.5) / 0.80 = \mathbf{16.88\text{ seconds}}\)
    *   Normal DPS = \((12 \times 45) / 16.88 \times 1.10 = \mathbf{35.2}\)
    *   Spell DPS = \((75 \text{ base} \times 15 \text{ missiles}) / 16.88 \times 1.10 = \mathbf{73.3}\)
    *   Total DPS = \(35.2 + 73.3 = \mathbf{108.5}\)

#### 🧮 Equipped Calculations (Spear of Shojin + Jeweled Gauntlet + Archangel's Staff)
*   **Stats**: AD = 45 | AS = 0.92 | AP = 215 (Average: 100 + Shojin 25 + JG 20 + Archangel's +70 time-avg) | Crit Chance = 40% (1.28x avg multiplier, spell crits enabled)
*   **Cycle & DPS (Spear of Shojin requires 8 attacks to cast)**:
    *   Cycle Duration = \((8 + 1.5) / 0.92 = \mathbf{10.33\text{ seconds}}\)
    *   Normal DPS = \((8 \times 45) / 10.33 \times 1.28 = \mathbf{44.6}\)
    *   Spell DPS = \((75 \text{ base} \times 2.15 \text{ AP} \times 15 \text{ missiles}) \times 1.28 \text{ Crit} / 10.33 = \mathbf{299.8}\)
    *   Total DPS = \(44.6 + 299.8 = \mathbf{344.4}\)

---

### 5. Lux ☀️

#### ⚙️ Mechanics & Combat Nuance
*   **Ability**: Channels a barrage of light at current target dealing magic damage over 3 seconds, reducing MR.
*   **Cast Lockout**: 3.0s base (channel duration).
*   **Attacks to Cast**: 4 attacks (mana is 0 / 40) / 3 attacks (equipped with Blue Buff).

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 45 | AS = 0.70 | AP = 100 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS**:
    *   Cycle Duration = \((4 + 3.0) / 0.70 = \mathbf{8.71\text{ seconds}}\)
    *   Normal DPS = \((4 \times 45) / 8.71 \times 1.10 = \mathbf{22.7}\)
    *   Spell DPS = \(735 \text{ base} / 8.71 \times 1.10 = \mathbf{92.8}\)
    *   Total DPS = \(22.7 + 92.8 = \mathbf{115.5}\)

#### 🧮 Equipped Calculations (Blue Buff + Jeweled Gauntlet + Rabadon's Deathcap)
*   **Stats**: AD = 45 | AS = 0.70 | AP = 200 (100 + BB 10 + JG 20 + Rabadon 70) | Crit Chance = 40% (1.28x avg multiplier, spell crits enabled)
*   **Cycle & DPS (Blue Buff requires 3 attacks to cast)**:
    *   Cycle Duration = \((3 + 3.0) / 0.70 = \mathbf{7.29\text{ seconds}}\)
    *   Normal DPS = \((3 \times 45) / 7.29 \times 1.28 = \mathbf{23.7}\)
    *   Spell DPS = \((735 \text{ base} \times 2.00 \text{ AP} \times 1.28 \text{ Crit}) / 7.29 = \mathbf{258.2}\)
    *   Total DPS = \(23.7 + 258.2 = \mathbf{281.9}\)

---

### 6. Yasuo 🌪️

#### ⚙️ Mechanics & Combat Nuance
*   **Ability**: Whirlwind knocks up and deals physical damage to target and slash splash damage to adjacent enemies.
*   **Cast Lockout**: 1.2s base.
*   **Attacks to Cast**: 6 attacks (mana is 50 / 110).
*   **Splash Assumption**: Assuming slashes hit 2 targets total (1 main + 1 adjacent = 6.75x AD total).

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 75 | AS = 0.80 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS**:
    *   Cycle Duration = \((6 + 1.2) / 0.80 = \mathbf{9.00\text{ seconds}}\)
    *   Normal DPS = \((6 \times 75) / 9.00 \times 1.10 = \mathbf{55.0}\)
    *   Spell DPS = \(6.75 \text{ AD} \times 75 \text{ AD} \times 1.10 / 9.00 = \mathbf{61.9}\)
    *   Total DPS = \(55.0 + 61.9 = \mathbf{116.9}\)

#### 🧮 Equipped Calculations (Infinity Edge + Deathblade + Bloodthirster)
*   **Stats**: AD = 166 (+121% AD) | AS = 0.80 | Crit Chance = 60% (1.24x avg multiplier)
*   **Cycle & DPS**:
    *   Cycle Duration = \((6 + 1.2) / 0.80 = \mathbf{9.00\text{ seconds}}\)
    *   Normal DPS = \((6 \times 166) / 9.00 \times 1.24 = \mathbf{137.2}\)
    *   Spell DPS = \(6.75 \text{ AD} \times 166 \text{ AD} \times 1.24 / 9.00 = \mathbf{154.4}\)
    *   Total DPS = \(137.2 + 154.4 = \mathbf{291.6}\)

---

### 7. Zeri ⚡

#### ⚙️ Mechanics & Combat Nuance
*   **Ability**: Passive: Execute enemies below 10% health. Active: Attacks chain physical lightning to 3 additional enemies for 8 seconds.
*   **Cast Lockout**: 0.8s base.
*   **Attacks to Cast**: 5 attacks (mana is 0 / 50).
*   **Buff Uptime**: Active duration (8s) is longer than mana generation cycle (7.25s), resulting in 100% buff uptime.

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 65 | AS = 0.80 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Assuming 4 Targets Hit Total = 2.5x AD total output)**:
    *   Cycle Duration = \((5 + 0.8) / 0.80 = \mathbf{7.25\text{ seconds}}\)
    *   Normal DPS = \((5 \times 65) / 7.25 \times 1.10 = \mathbf{49.3}\)
    *   Spell DPS = \((5 \times 65 \times 1.5 \text{ splash ratio}) / 7.25 \times 1.10 = \mathbf{73.8}\)
    *   Total DPS = \(49.3 + 73.8 = \mathbf{123.1}\)

#### 🧮 Equipped Calculations (Infinity Edge + Last Whisper + Giant Slayer)
*   **Stats**: AD = 94 (+45% AD) | AS = 1.00 | Crit Chance = 70% (1.28x avg multiplier, +25% Giant Slayer amp)
*   **Cycle & DPS**:
    *   Cycle Duration = \((5 + 0.8) / 1.00 = \mathbf{5.80\text{ seconds}}\)
    *   Normal DPS = \((5 \times 94) / 5.80 \times 1.28 \text{ Crit} \times 1.25 \text{ Amp} = \mathbf{129.7}\)
    *   Spell DPS = \((5 \times 94 \times 1.5 \text{ splash}) / 5.80 \times 1.28 \text{ Crit} \times 1.25 \text{ Amp} = \mathbf{194.5}\)
    *   Total DPS = \(129.7 + 194.5 = \mathbf{324.2}\)
