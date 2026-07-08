# Taliyah DPS Calculation Details 🪨

This document provides the step-by-step mathematical calculations and formulas for Taliyah's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.70 (constant across star levels)
*   **Attack Damage (AD)**: 40 / 60 / 90
*   **Spell Magic Damage (Base)**: AP Ratio: 1.00x | Flat: 150 / 225 / 340

### 2. Skill Description & Taliyah Mechanics
*   **Skill Description**: Active: deals magic damage to the current target and knocks them up (2s stun). Passive: whenever any enemy is knocked up or back by anything, she throws a boulder dealing magic damage.
*   **Mechanical Timing & Assumptions**:
    *   spell_base represents the flat base damage ([150, 225, 340]). Calculates active seismic shove plus average passive boulder triggers. Overrides represent target density 2.0.
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

*   **1★ Taliyah**:
    *   Auto Attack DPS: `(6 Attacks * 40 AD) / 9.371s = 25.6`
    *   Spell Damage: `150 Active + 120 Passive = 270.0`
    *   Spell DPS: `270.0 / 9.371s = 28.8`
    *   Total DPS: `25.6 + 28.8 = 54.4`
*   **2★ Taliyah**:
    *   Auto Attack DPS: `(6 Attacks * 60 AD) / 9.371s = 38.4`
    *   Spell Damage: `225 Active + 180 Passive = 405.0`
    *   Spell DPS: `405.0 / 9.371s = 43.2`
    *   Total DPS: `38.4 + 43.2 = 81.6`
*   **3★ Taliyah**:
    *   Auto Attack DPS: `(6 Attacks * 90 AD) / 9.371s = 57.6`
    *   Spell Damage: `340 Active + 270 Passive = 610.0`
    *   Spell DPS: `610.0 / 9.371s = 65.1`
    *   Total DPS: `57.6 + 65.1 = 122.7`


### 3. Trigger Condition Scaling
*   *Note: Active stuns/knocks up target; passive launches boulders whenever any enemy is knocked up by any source.* 
*   **1★ Taliyah (Synergy)**:
    *   Auto Attack DPS: `(6 Attacks * 40 AD) / 9.370s = 28.0`
    *   Spell Damage: `150 Active + 120 Passive + (2 * 120 Free Boulders) = 634.3`
    *   Spell DPS: `634.3 / 9.370s = 67.7`
    *   Total DPS: `28.0 + 67.7 = 95.7`
*   **2★ Taliyah (Synergy)**:
    *   Auto Attack DPS: `(6 Attacks * 60 AD) / 9.370s = 42.0`
    *   Spell Damage: `225 Active + 180 Passive + (2 * 180 Free Boulders) = 952.0`
    *   Spell DPS: `952.0 / 9.370s = 101.6`
    *   Total DPS: `42.0 + 101.6 = 143.6`
*   **3★ Taliyah (Synergy)**:
    *   Auto Attack DPS: `(6 Attacks * 90 AD) / 9.370s = 63.0`
    *   Spell Damage: `340 Active + 270 Passive + (2 * 270 Free Boulders) = 1427.1`
    *   Spell DPS: `1427.1 / 9.370s = 152.3`
    *   Total DPS: `63.0 + 152.3 = 215.3`

---

## 🧮 Equipped Calculations (Jeweled Gauntlet + Archangel's Staff + Hextech Gunblade)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +10% AD (1.10* multiplier)
*   **Total Equipped AP Modifier**: 215 AP (2.15* spell multiplier)
*   **Crit Stats**: 40% Crit Chance, 170% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.28*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration (Synergy Average) = 9.710 seconds (historical fight average matching)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (6 * AD_equipped) / 9.710s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 9.710s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Taliyah (Equipped)**:
    *   Auto Attack DPS: `(6 Attacks * 44 AD) / 9.710s * 1.28 Crit = 31.6`
    *   Spell Damage: `(150 Active + 120 Passive * 2) * 2.15 AP * 1.28 Crit = 1265.2`
    *   Spell DPS: `1265.2 / 9.710s = 130.3`
    *   Total DPS: `31.6 + 130.3 = 161.9`
*   **2★ Taliyah (Equipped)**:
    *   Auto Attack DPS: `(6 Attacks * 66 AD) / 9.710s * 1.28 Crit = 47.4`
    *   Spell Damage: `(225 Active + 180 Passive * 2) * 2.15 AP * 1.28 Crit = 1898.3`
    *   Spell DPS: `1898.3 / 9.710s = 195.5`
    *   Total DPS: `47.4 + 195.5 = 242.9`
*   **3★ Taliyah (Equipped)**:
    *   Auto Attack DPS: `(6 Attacks * 99 AD) / 9.710s * 1.28 Crit = 71.1`
    *   Spell Damage: `(340 Active + 270 Passive * 2) * 2.15 AP * 1.28 Crit = 2847.0`
    *   Spell DPS: `2847.0 / 9.710s = 293.2`
    *   Total DPS: `71.1 + 293.2 = 364.3`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: spell[idx] + (spell[idx] * 0.75 * 2.0)` (represents active knockup damage + passive boulder triggers).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: (spell[idx] + spell[idx] * 0.75 * 2.0) * (ap / 100.0) * crit` (applies AP and JG crit).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Models secondary boulder reactions.
*   **Stats & Cycle Keys**:
    *   `as`: `0.70`
    *   `base_cycle`: `9.37` seconds
    *   `eq_cycle`: `9.71` seconds
    *   `lockout`: `0.8` seconds
