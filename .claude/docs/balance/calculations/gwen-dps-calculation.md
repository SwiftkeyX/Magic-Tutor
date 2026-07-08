# Gwen DPS Calculation Details ✂️

This document provides the step-by-step mathematical calculations and formulas for Gwen's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.80 (constant across star levels)
*   **Attack Damage (AD)**: 55 / 83 / 124
*   **Spell Magic Damage (Base)**: AP Ratio: 1.00x | Flat: 100 / 150 / 400

### 2. Skill Description & Gwen Mechanics
*   **Skill Description**: Dashes and snips 3 times in a cone dealing magic damage. Every 3rd cast grants armor and MR.
*   **Mechanical Timing & Assumptions**:
    *   spell_base represents flat magic damage ([100, 150, 400]). Snips in a cone. Target density multiplier is 6.00 (average snips hit). Calculated dynamically without overrides.
*   **Mana & Casting**:
    *   Max Mana: 35
    *   Start Mana: 0
    *   **Attacks to Cast**: 4 attacks
    *   **Cast Lockout**: 1.0 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = 6.250 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (4 * AD) / 6.250s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 6.250s`

*   **1★ Gwen**:
    *   Auto Attack DPS: `(4 Attacks * 55 AD) / 6.250s = 35.2`
    *   Spell Damage: `100 * 6.0 snips = 600.0`
    *   Spell DPS: `600.0 / 6.250s = 96.0`
    *   Total DPS: `35.2 + 96.0 = 131.2`
*   **2★ Gwen**:
    *   Auto Attack DPS: `(4 Attacks * 83 AD) / 6.250s = 53.1`
    *   Spell Damage: `150 * 6.0 snips = 900.0`
    *   Spell DPS: `900.0 / 6.250s = 144.0`
    *   Total DPS: `53.1 + 144.0 = 197.1`
*   **3★ Gwen**:
    *   Auto Attack DPS: `(4 Attacks * 124 AD) / 6.250s = 79.4`
    *   Spell Damage: `400 * 6.0 snips = 2400.0`
    *   Spell DPS: `2400.0 / 6.250s = 384.0`
    *   Total DPS: `79.4 + 384.0 = 463.4`


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
    
 `Cycle Duration = 5.000 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (3 * AD_equipped) / 5.000s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 5.000s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Gwen (Equipped)**:
    *   Auto Attack DPS: `(3 Attacks * 55 AD) / 5.000s * 1.28 Crit = 42.2`
    *   Spell Damage: `(100 * 6.0 snips) * 2.00 AP * 1.28 Crit = 1536.0`
    *   Spell DPS: `1536.0 / 5.000s = 307.2`
    *   Total DPS: `42.2 + 307.2 = 349.4`
*   **2★ Gwen (Equipped)**:
    *   Auto Attack DPS: `(3 Attacks * 83 AD) / 5.000s * 1.28 Crit = 63.7`
    *   Spell Damage: `(150 * 6.0 snips) * 2.00 AP * 1.28 Crit = 2304.0`
    *   Spell DPS: `2304.0 / 5.000s = 460.8`
    *   Total DPS: `63.7 + 460.8 = 524.5`
*   **3★ Gwen (Equipped)**:
    *   Auto Attack DPS: `(3 Attacks * 124 AD) / 5.000s * 1.28 Crit = 95.2`
    *   Spell Damage: `(400 * 6.0 snips) * 2.00 AP * 1.28 Crit = 6144.0`
    *   Spell DPS: `6144.0 / 5.000s = 1228.8`
    *   Total DPS: `95.2 + 1228.8 = 1324.0`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: spell[idx] * 6.0` (models 3 cone snips with a target density factor of 2.0).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: (spell[idx] * 6.0) * (ap / 100.0) * crit` (scales spell damage with AP and JG crit).
*   **Baseline / Equipped Overrides**: Removed; calculated dynamically.
*   **Stats & Cycle Keys**:
    *   `as`: `0.80`
    *   `base_cycle`: `6.25` seconds
    *   `eq_cycle`: `5.00` seconds
    *   `lockout`: `1.0` seconds
