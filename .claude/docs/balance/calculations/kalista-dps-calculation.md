# Kalista DPS Calculation Details 🎯

This document provides the step-by-step mathematical calculations and formulas for Kalista's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.85 (constant across star levels)
*   **Attack Damage (AD)**: 45 / 68 / 101
*   **Spell Magic Damage (Base)**: AP Ratio: 1.00x | Flat: 18 / 27 / 45

### 2. Skill Description & Kalista Mechanics
*   **Skill Description**: spears deal true damage: 18 / 27 / 45 per spear. Active cast stacks 6 spears. Basic attacks stack 1 spear. Total spears stacked per cycle (6 attacks + 6 cast) = 12 spears.
*   **Mechanical Timing & Assumptions**:
    *   spell_base is the flat spear true damage ([18, 27, 45]). Stacks true damage spears via attacks and active cast. Overrides match the sheet's Guinsoo average AS (2.02 AS).
*   **Mana & Casting**:
    *   Max Mana: 60
    *   Start Mana: 0
    *   **Attacks to Cast**: 6 attacks
    *   **Cast Lockout**: 0.8 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = 8.000 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (6 * AD) / 8.000s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 8.000s`

*   **1★ Kalista**:
    *   Auto Attack DPS: `(6 Attacks * 45 AD) / 8.000s = 33.8`
    *   Spell Damage: `18 on-hit * 12 spears = 216.0`
    *   Spell DPS: `216.0 / 8.000s = 27.0`
    *   Total DPS: `33.8 + 27.0 = 60.8`
*   **2★ Kalista**:
    *   Auto Attack DPS: `(6 Attacks * 68 AD) / 8.000s = 51.0`
    *   Spell Damage: `27 on-hit * 12 spears = 324.0`
    *   Spell DPS: `324.0 / 8.000s = 40.5`
    *   Total DPS: `51.0 + 40.5 = 91.5`
*   **3★ Kalista**:
    *   Auto Attack DPS: `(6 Attacks * 101 AD) / 8.000s = 75.8`
    *   Spell Damage: `45 on-hit * 12 spears = 540.0`
    *   Spell DPS: `540.0 / 8.000s = 67.5`
    *   Total DPS: `75.8 + 67.5 = 143.2`


---

## 🧮 Equipped Calculations (Guinsoo's Rageblade + Spear of Shojin + Jeweled Gauntlet)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +15% AD (1.15* multiplier)
*   **Total Equipped AP Modifier**: 145 AP (1.45* spell multiplier)
*   **Crit Stats**: 40% Crit Chance, 170% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.28*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = 3.370 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (4 * AD_equipped) / 3.370s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 3.370s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Kalista (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 52 AD) / 3.370s * 1.28 Crit = 102.7`
    *   Spell Damage: `(18 * 1.45 AP * 12 spears) * 1.28 Crit = 456.6`
    *   Spell DPS: `456.6 / 3.370s = 135.5`
    *   Total DPS: `102.7 + 135.5 = 238.2`
*   **2★ Kalista (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 78 AD) / 3.370s * 1.28 Crit = 154.1`
    *   Spell Damage: `(27 * 1.45 AP * 12 spears) * 1.28 Crit = 685.1`
    *   Spell DPS: `685.1 / 3.370s = 203.3`
    *   Total DPS: `154.1 + 203.3 = 357.4`
*   **3★ Kalista (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 116 AD) / 3.370s * 1.28 Crit = 245.7`
    *   Spell Damage: `(45 * 1.45 AP * 12 spears) * 1.28 Crit = 1092.6`
    *   Spell DPS: `1092.6 / 3.370s = 324.2`
    *   Total DPS: `245.7 + 324.2 = 569.9`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: spell[idx] * 12.0` (true damage of 12 stacked spears per cycle).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: (spell[idx] * (ap / 100.0) * 12.0) * crit` (scales true damage spears with AP and JG crit).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Incorporates active true damage execute calculation.
*   **Stats & Cycle Keys**:
    *   `as`: `0.85`
    *   `eq_as`: `2.02` (Guinsoo high stack rate AS)
    *   `base_cycle`: `8.00` seconds
    *   `eq_cycle`: `3.37` seconds
    *   `lockout`: `0.8` seconds
