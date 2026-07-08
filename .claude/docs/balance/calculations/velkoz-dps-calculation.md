# Vel'Koz DPS Calculation Details 👁️

This document provides the step-by-step mathematical calculations and formulas for Vel'Koz's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.70 (constant across star levels)
*   **Attack Damage (AD)**: 40 / 60 / 90
*   **Spell Magic Damage (Base)**: AP Ratio: 1.00x | Flat: 220 / 330 / 550

### 2. Skill Description & Vel'Koz Mechanics
*   **Skill Description**: Plasma Fission fires a bolt that splits in two at right angles, dealing 50% damage to all targets passed through.
*   **Mechanical Timing & Assumptions**:
    *   spell_base represents the flat base damage ([220, 330, 550]). Splitting magic bolt. Applies target density factor of 2.00 (two split beams). Overrides match the sheet's AP values.
*   **Mana & Casting**:
    *   Max Mana: 60
    *   Start Mana: 0
    *   **Attacks to Cast**: 6 attacks
    *   **Cast Lockout**: 0.8 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration (1 Target Baseline) = (Attacks to Cast) / (AS) + Base Lockout = (6) / (0.70) + 0.8 = 9.371 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (6 * AD) / 9.371s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 9.371s`

*   **1★ Vel'Koz**:
    *   Auto Attack DPS: `(6 Attacks * 40 AD) / 9.371s = 25.6`
    *   Spell Damage: `220 * 1.0 Bolt (1 Target) = 220.0`
    *   Spell DPS: `220.0 / 9.371s = 23.5`
    *   Total DPS: `25.6 + 23.5 = 49.1`
*   **2★ Vel'Koz**:
    *   Auto Attack DPS: `(6 Attacks * 60 AD) / 9.371s = 38.4`
    *   Spell Damage: `330 * 1.0 Bolt (1 Target) = 330.0`
    *   Spell DPS: `330.0 / 9.371s = 35.2`
    *   Total DPS: `38.4 + 35.2 = 73.6`
*   **3★ Vel'Koz**:
    *   Auto Attack DPS: `(6 Attacks * 90 AD) / 9.371s = 57.6`
    *   Spell Damage: `550 * 1.0 Bolt (1 Target) = 550.0`
    *   Spell DPS: `550.0 / 9.371s = 58.7`
    *   Total DPS: `57.6 + 58.7 = 116.3`


### 3. Trigger Condition Scaling
*   *Note: Plasma Fission splits in two. Synergy assumes 2 targets hit on average.* 
*   **1★ Vel'Koz (Synergy)**:
    *   Auto Attack DPS: `(6 Attacks * 40 AD) / 9.710s = 24.7`
    *   Spell Damage: `220 * 2.0 splitting (2 Targets Avg) = 440.0`
    *   Spell DPS: `440.0 / 9.710s = 45.3`
    *   Total DPS: `24.7 + 45.3 = 70.0`
*   **2★ Vel'Koz (Synergy)**:
    *   Auto Attack DPS: `(6 Attacks * 60 AD) / 9.710s = 37.1`
    *   Spell Damage: `330 * 2.0 splitting (2 Targets Avg) = 660.0`
    *   Spell DPS: `660.0 / 9.710s = 68.0`
    *   Total DPS: `37.1 + 68.0 = 105.0`
*   **3★ Vel'Koz (Synergy)**:
    *   Auto Attack DPS: `(6 Attacks * 90 AD) / 9.710s = 55.6`
    *   Spell Damage: `550 * 2.0 splitting (2 Targets Avg) = 1100.0`
    *   Spell DPS: `1100.0 / 9.710s = 113.3`
    *   Total DPS: `55.6 + 113.3 = 168.9`

---

## 🧮 Equipped Calculations (Blue Buff + Jeweled Gauntlet + Rabadon's Deathcap)

### 1. Item Stats
*   **Total Equipped AD Modifier**: None (+0%)
*   **Total Equipped AP Modifier**: 200 AP (2.00* spell multiplier)
*   **Crit Stats**: 40% Crit Chance, 170% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.28*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration (Synergy Average) = 8.290 seconds (historical fight average matching)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (5 * AD_equipped) / 8.290s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 8.290s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Vel'Koz (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 40 AD) / 8.290s * 1.28 Crit = 30.9`
    *   Spell Damage: `(220 * 2.0 splitting) * 2.00 AP * 1.28 Crit = 1126.6`
    *   Spell DPS: `1126.6 / 8.290s = 135.9`
    *   Total DPS: `30.9 + 135.9 = 166.8`
*   **2★ Vel'Koz (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 60 AD) / 8.290s * 1.28 Crit = 46.4`
    *   Spell Damage: `(330 * 2.0 splitting) * 2.00 AP * 1.28 Crit = 1689.5`
    *   Spell DPS: `1689.5 / 8.290s = 203.8`
    *   Total DPS: `46.4 + 203.8 = 250.2`
*   **3★ Vel'Koz (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 90 AD) / 8.290s * 1.28 Crit = 69.5`
    *   Spell Damage: `(550 * 2.0 splitting) * 2.00 AP * 1.28 Crit = 2816.1`
    *   Spell DPS: `2816.1 / 8.290s = 339.7`
    *   Total DPS: `69.5 + 339.7 = 409.2`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: spell[idx] * 2.0` (target density factor of 2.0).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: (spell[idx] * 2.0) * (ap / 100.0) * crit` (scales split magic bolts).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Models multi-beam split pathing.
*   **Stats & Cycle Keys**:
    *   `as`: `0.70`
    *   `base_cycle`: `9.71` seconds
    *   `eq_cycle`: `8.29` seconds
    *   `lockout`: `0.8` seconds
