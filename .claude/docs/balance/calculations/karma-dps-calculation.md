# Karma DPS Calculation Details ☄️

This document provides the step-by-step mathematical calculations and formulas for Karma's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.70 (constant across star levels)
*   **Attack Damage (AD)**: 45 / 68 / 101
*   **Spell Magic Damage (Base)**: AP Ratio: 1.00x | Flat: 170 / 255 / 382.5

### 2. Skill Description & Karma Mechanics
*   **Skill Description**: Launches energy bursts in a 1-hex radius. Every 3rd cast fires 3 bursts instead of 1.
*   **Mechanical Timing & Assumptions**:
    *   spell_base is the flat base damage ([170, 255, 382.5]). Splash energy bursts are scaled by target density. Overrides model the 3rd-cast burst amplification.
*   **Mana & Casting**:
    *   Max Mana: 50
    *   Start Mana: 0
    *   **Attacks to Cast**: 5 attacks
    *   **Cast Lockout**: 0.8 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration (1 Target Baseline) = (Attacks to Cast) / (AS) + Base Lockout = (5) / (0.70) + 0.8 = 7.943 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (5 * AD) / 7.943s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 7.943s`

*   **1★ Karma**:
    *   Auto Attack DPS: `(5 Attacks * 45 AD) / 7.943s = 28.3`
    *   Spell Damage: `170 * 1.67 cast multiplier * 1 Target = 283.9`
    *   Spell DPS: `283.9 / 7.943s = 35.7`
    *   Total DPS: `28.3 + 35.7 = 64.1`
*   **2★ Karma**:
    *   Auto Attack DPS: `(5 Attacks * 68 AD) / 7.943s = 42.8`
    *   Spell Damage: `255 * 1.67 cast multiplier * 1 Target = 425.8`
    *   Spell DPS: `425.8 / 7.943s = 53.6`
    *   Total DPS: `42.8 + 53.6 = 96.4`
*   **3★ Karma**:
    *   Auto Attack DPS: `(5 Attacks * 101 AD) / 7.943s = 63.6`
    *   Spell Damage: `382.5 * 1.67 cast multiplier * 1 Target = 638.8`
    *   Spell DPS: `638.8 / 7.943s = 80.4`
    *   Total DPS: `63.6 + 80.4 = 144.0`


### 3. Trigger Condition Scaling
*   *Note: Launches energy bursts in a 1-hex radius. Every 3rd cast fires 3 bursts. Synergy assumes 1.33 targets average per burst (2.22 targets total).* 
*   **1★ Karma (Synergy)**:
    *   Auto Attack DPS: `(5 Attacks * 45 AD) / 8.290s = 27.1`
    *   Spell Damage: `(170 * 1.67 * 1.33 Targets) = 377.6`
    *   Spell DPS: `377.6 / 8.290s = 45.5`
    *   Total DPS: `27.1 + 45.5 = 72.7`
*   **2★ Karma (Synergy)**:
    *   Auto Attack DPS: `(5 Attacks * 68 AD) / 8.290s = 41.0`
    *   Spell Damage: `(255 * 1.67 * 1.33 Targets) = 566.4`
    *   Spell DPS: `566.4 / 8.290s = 68.3`
    *   Total DPS: `41.0 + 68.3 = 109.3`
*   **3★ Karma (Synergy)**:
    *   Auto Attack DPS: `(5 Attacks * 101 AD) / 8.290s = 60.9`
    *   Spell Damage: `(382.5 * 1.67 * 1.33 Targets) = 849.6`
    *   Spell DPS: `849.6 / 8.290s = 102.5`
    *   Total DPS: `60.9 + 102.5 = 163.4`

---

## 🧮 Equipped Calculations (Blue Buff + Jeweled Gauntlet + Archangel's Staff)

### 1. Item Stats
*   **Total Equipped AD Modifier**: None (+0%)
*   **Total Equipped AP Modifier**: 200 AP (2.00* spell multiplier)
*   **Crit Stats**: 40% Crit Chance, 170% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.28*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration (Synergy Average) = 6.860 seconds (historical fight average matching)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (4 * AD_equipped) / 6.860s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 6.860s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Karma (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 45 AD) / 6.860s * 1.28 Crit = 33.6`
    *   Spell Damage: `(170 * 1.67 * 2.0 Targets) * 2.00 AP * 1.28 Crit = 965.2`
    *   Spell DPS: `965.2 / 6.860s = 140.7`
    *   Total DPS: `33.6 + 140.7 = 174.3`
*   **2★ Karma (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 68 AD) / 6.860s * 1.28 Crit = 50.4`
    *   Spell Damage: `(255 * 1.67 * 2.0 Targets) * 2.00 AP * 1.28 Crit = 1448.1`
    *   Spell DPS: `1448.1 / 6.860s = 211.1`
    *   Total DPS: `50.4 + 211.1 = 261.5`
*   **3★ Karma (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 101 AD) / 6.860s * 1.28 Crit = 75.6`
    *   Spell Damage: `(382.5 * 1.67 * 2.0 Targets) * 2.00 AP * 1.28 Crit = 2171.9`
    *   Spell DPS: `2171.9 / 6.860s = 316.6`
    *   Total DPS: `75.6 + 316.6 = 392.2`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: spell[idx] * 1.67 * 2.0` (1.67x time-averaged 3rd-cast amplifier multiplied by 2.0 target density).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: (spell[idx] * 1.67 * 2.0) * (ap / 100.0) * crit` (adds AP and JG crit).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Models the average 3rd cast surge cadence.
*   **Stats & Cycle Keys**:
    *   `as`: `0.70`
    *   `base_cycle`: `8.29` seconds
    *   `eq_cycle`: `6.86` seconds
    *   `lockout`: `0.8` seconds
