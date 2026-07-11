# TFT Set 9 Tier 5 Carry DPS Analysis (Unified)

This document provides a unified mathematical breakdown of baseline (unequipped) and well-equipped (3-item) DPS scaling for all 5-Gold carry champions in Teamfight Tactics (Set 9).

Putting baseline and itemized calculations in a single document guarantees that both calculations utilize identical, up-to-date mechanical assumptions (AS-scaling lockouts, targeting splits, and cast animation frames).

---

## 📊 Master DPS Summary Tables

### 📘 Table 1: Baseline (Unequipped) DPS Summary
*Consolidated comparison across 1★, 2★, and 3★ star levels with NO items equipped. AoE/multi-target spells are shown using typical combat averages (1 adjacent target for Aatrox, 5 wave targets + 2 essence targets for Ahri, and 2000 HP target for Bel'Veth).*

| Champion | Star | Base AD | AS | Normal DPS | Spell DPS | Total DPS |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Aatrox** (1 adjacent target) | 1★<br>2★<br>3★ | 80<br>120<br>180 | 0.80 | 58.7<br>88.0<br>132.0 | 46.9<br>70.4<br>105.6 | **105.6**<br>**158.4**<br>**237.6** |
| **Ahri** (5 Wave targets avg) | 1★<br>2★<br>3★ | 50<br>75<br>113 | 0.85 | 39.0<br>58.4<br>87.7 | 100.2<br>148.1<br>830.7 | **139.2**<br>**206.5**<br>**918.4** |
| **Bel'Veth** (2000 HP target avg) | 1★<br>2★<br>3★ | 80<br>120<br>180 | 0.85 | 60.3<br>90.5<br>135.7 | 59.9<br>89.8<br>749.6 | **120.2**<br>**180.3**<br>**885.3** |

### ⚔️ Table 2: Well-Equipped (3-Item) DPS Summary
*Consolidated comparison across 1★, 2★, and 3★ star levels with 3 core optimal items equipped.*

| Champion | Star | AD (Equipped) | AS (Equipped) | Normal DPS | Spell DPS | Total DPS |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Aatrox** (1 adjacent target) | 1★<br>2★<br>3★ | 177<br>266<br>398 | 0.80 | 146.3<br>219.5<br>329.2 | 117.1<br>175.7<br>263.5 | **263.4**<br>**395.2**<br>**592.7** |
| **Ahri** (5 Wave targets avg) | 1★<br>2★<br>3★ | 50<br>75<br>113 | 0.85 | 43.5<br>65.3<br>97.9 | 280.0<br>413.5<br>2320.6 | **323.5**<br>**478.8**<br>**2418.5** |
| **Bel'Veth** (2000 HP target avg) | 1★<br>2★<br>3★ | 177<br>266<br>398 | 0.85 | 150.5<br>225.8<br>338.6 | 124.8<br>187.2<br>1358.3 | **275.3**<br>**413.0**<br>**1696.9** |

---

## 📖 Glossary & Formula Index

*   **AD (Attack Damage)**: The physical damage dealt by a basic auto-attack. AD scales by 1.5x per star level (2★ = 1.5x, 3★ = 2.25x).
*   **AS (Attack Speed)**: The rate of basic attacks per second. Attack Speed remains flat across star levels.
*   **AP (Ability Power)**: The scaling multiplier applied to base magic damage (base is 100 AP / 1.0x).
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
    *   *Modeling note*: as a simplification, these docs apply the average crit multiplier to spell damage in baseline rows too (strictly, TFT abilities only crit with JG/IE equipped). True damage never crits at baseline.
*   **AP vs. AD-ratio spells**: AP multiplies a spell's flat/base damage component only. Pure AD-ratio components (e.g. Aatrox/Bel'Veth AD lashes) do not scale with AP — matching TFT's per-star ability data.

---

## 🔍 Detailed Champion Breakdowns

### 1. Aatrox 😈

#### ⚙️ Mechanics & Combat Nuance
*   **Ability**: Transforms for a duration, gaining Omnivamp and converting 100% of bonus AS to AD. While transformed, attacks deal physical damage in an area.
*   **Cast Lockout**: 1.0s base.
*   **Attacks to Cast**: 5 attacks (mana is 0 / 50).
*   **Uptime**: Active transformation duration (12s) is longer than mana generation cycle (7.5s), resulting in 100% active transform uptime.

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 80 | AS = 0.80 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Assuming 1 adjacent target = 1.8x AD total physical output)**:
    *   Cycle Duration = \((5 + 1.0) / 0.80 = \mathbf{7.50\text{ seconds}}\)
    *   Normal DPS = \((5 \times 80) / 7.50 \times 1.10 = \mathbf{58.7}\)
    *   Spell DPS = \((5 \times 80 \times 0.80) / 7.50 \times 1.10 = \mathbf{46.9}\)
    *   Total DPS = \(58.7 + 46.9 = \mathbf{105.6}\)

#### 🧮 Equipped Calculations (Deathblade + Infinity Edge + Bloodthirster)
*   **Stats**: AD = 177 (+121% AD) | AS = 0.80 | Crit Chance = 60% (1.24x avg multiplier)
*   **Cycle & DPS**:
    *   Cycle Duration = \((5 + 1.0) / 0.80 = \mathbf{7.50\text{ seconds}}\)
    *   Normal DPS = \((5 \times 177) / 7.50 \times 1.24 = \mathbf{146.3}\)
    *   Spell DPS = \((5 \times 177 \times 0.80) / 7.50 \times 1.24 = \mathbf{117.1}\)
    *   Total DPS = \(146.3 + 117.1 = \mathbf{263.4}\)

---

### 2. Ahri 🦊

#### ⚙️ Mechanics & Combat Nuance
*   **Ability**: Steals essence from targets dealing magic damage. Every 3rd cast unleashes a massive wave dealing magic damage to all enemies hit.
*   **Cast Lockout**: 1.0s base.
*   **Attacks to Cast**: 5 attacks (baseline unequipped) / 4 attacks (equipped with Blue Buff).
*   **Wave Assumptions**: Assuming wave hits 5 targets and essence steal hits 2 targets average.

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 50 | AS = 0.85 | AP = 100 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Average spell damage per cast = 643.3)**:
    *   Cycle Duration = \((5 + 1.0) / 0.85 = \mathbf{7.06\text{ seconds}}\)
    *   Normal DPS = \((5 \times 50) / 7.06 \times 1.10 = \mathbf{39.0}\)
    *   Spell DPS = \(643.3 / 7.06 \times 1.10 = \mathbf{100.2}\)
    *   Total DPS = \(39.0 + 100.2 = \mathbf{139.2}\)

#### 🧮 Equipped Calculations (Blue Buff + Jeweled Gauntlet + Rabadon's Deathcap)
*   **Stats**: AD = 50 | AS = 0.85 | AP = 200 (100 + BB 10 + JG 20 + Rabadon 70) | Crit Chance = 40% (1.28x avg multiplier, spell crits enabled)
*   **Cycle & DPS (Blue Buff requires 4 attacks to cast)**:
    *   Cycle Duration = \((4 + 1.0) / 0.85 = \mathbf{5.88\text{ seconds}}\)
    *   Normal DPS = \((4 \times 50) / 5.88 \times 1.28 = \mathbf{43.5}\)
    *   Spell DPS = \((643.3 \text{ base} \times 2.00 \text{ AP} \times 1.28 \text{ Crit}) / 5.88 = \mathbf{280.0}\)
    *   Total DPS = \(43.5 + 280.0 = \mathbf{323.5}\)

---

### 3. Bel'Veth 👑

#### ⚙️ Mechanics & Combat Nuance
*   **Ability**: Lashes out 6 times (1★, 2★) / 25 times (3★). Each lash deals physical damage equal to 60% AD + 1% / 1.5% / 5% target max health as true damage.
*   **Cast Lockout**: 1.2s base.
*   **Attacks to Cast**: 5 attacks (mana is 20 / 70).
*   **Target HP**: Standardized around a 2000 Health target (1% = 20 true damage per lash).

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 80 | AS = 0.85 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Spell damage = \(((80 \times 0.60 \times 1.10) + 20) \times 6 = 436.8\); the AD part crits, the true-damage part does not)**:
    *   Cycle Duration = \((5 + 1.2) / 0.85 = \mathbf{7.29\text{ seconds}}\)
    *   Normal DPS = \((5 \times 80) / 7.29 \times 1.10 = \mathbf{60.3}\)
    *   Spell DPS = \(436.8 / 7.29 = \mathbf{59.9}\)
    *   Total DPS = \(60.3 + 59.9 = \mathbf{120.2}\)

#### 🧮 Equipped Calculations (Deathblade + Infinity Edge + Bloodthirster)
*   **Stats**: AD = 177 (+121% AD) | AS = 0.85 | Crit Chance = 60% (1.24x avg multiplier)
*   **Cycle & DPS**:
    *   Cycle Duration = \((5 + 1.2) / 0.85 = \mathbf{7.29\text{ seconds}}\)
    *   Normal DPS = \((5 \times 177) / 7.29 \times 1.24 = \mathbf{150.5}\)
    *   Spell DPS = \(((177 \times 0.60 \times 1.24 + 20) \times 6) / 7.29 = \mathbf{124.8}\)
    *   Total DPS = \(150.5 + 124.8 = \mathbf{275.3}\)
