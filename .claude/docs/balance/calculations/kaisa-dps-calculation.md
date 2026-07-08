# Kai'Sa DPS Calculation Details 👾

This document provides the step-by-step mathematical calculations and formulas for Kai'Sa's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.80 (constant across star levels)
*   **Attack Damage (AD)**: 45 / 68 / 101
*   **Spell Magic Damage (Base)**: AP Ratio: 1.00x | Flat: 75 / 111 / 300

### 2. Skill Description & Kai'Sa Mechanics
*   **Skill Description**: Dashes and fires 15 missiles split across nearest 4 targets dealing magic damage.
*   **Mechanical Timing & Assumptions**:
    *   spell_base represents the flat base damage ([75, 111, 300]). Fires 15 magic missiles. Applies target density factor of 15.00. Overrides match Shojin cycle times.
*   **Mana & Casting**:
    *   Max Mana: 120
    *   Start Mana: 40
    *   **Attacks to Cast**: 8 attacks
    *   **Cast Lockout**: 1.5 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = 16.880 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (8 * AD) / 16.880s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 16.880s`

*   **1★ Kai'Sa**:
    *   Auto Attack DPS: `(12 Attacks * 45 AD) / 16.880s = 32.0`
    *   Spell Damage: `75 * 15 missiles = 1125.0`
    *   Spell DPS: `1125.0 / 16.880s = 66.6`
    *   Total DPS: `32.0 + 66.6 = 98.6`
*   **2★ Kai'Sa**:
    *   Auto Attack DPS: `(12 Attacks * 68 AD) / 16.880s = 48.3`
    *   Spell Damage: `111 * 15 missiles = 1665.0`
    *   Spell DPS: `1665.0 / 16.880s = 98.6`
    *   Total DPS: `48.3 + 98.6 = 147.0`
*   **3★ Kai'Sa**:
    *   Auto Attack DPS: `(12 Attacks * 101 AD) / 16.880s = 71.8`
    *   Spell Damage: `300 * 15 missiles = 4500.0`
    *   Spell DPS: `4500.0 / 16.880s = 266.6`
    *   Total DPS: `71.8 + 266.6 = 338.4`


---

## 🧮 Equipped Calculations (Spear of Shojin + Jeweled Gauntlet + Archangel's Staff)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +15% AD (1.15* multiplier)
*   **Total Equipped AP Modifier**: 215 AP (2.15* spell multiplier)
*   **Crit Stats**: 40% Crit Chance, 170% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.28*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = 10.330 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (6 * AD_equipped) / 10.330s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 10.330s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Kai'Sa (Equipped)**:
    *   Auto Attack DPS: `(6 Attacks * 52 AD) / 10.330s * 1.28 Crit = 44.6`
    *   Spell Damage: `(75 * 15 missiles) * 2.15 AP * 1.28 Crit = 3096.9`
    *   Spell DPS: `3096.9 / 10.330s = 299.8`
    *   Total DPS: `44.6 + 299.8 = 344.4`
*   **2★ Kai'Sa (Equipped)**:
    *   Auto Attack DPS: `(6 Attacks * 78 AD) / 10.330s * 1.28 Crit = 67.0`
    *   Spell Damage: `(111 * 15 missiles) * 2.15 AP * 1.28 Crit = 4645.4`
    *   Spell DPS: `4645.4 / 10.330s = 449.7`
    *   Total DPS: `67.0 + 449.7 = 516.7`
*   **3★ Kai'Sa (Equipped)**:
    *   Auto Attack DPS: `(6 Attacks * 116 AD) / 10.330s * 1.28 Crit = 100.4`
    *   Spell Damage: `(300 * 15 missiles) * 2.15 AP * 1.28 Crit = 12902.2`
    *   Spell DPS: `12902.2 / 10.330s = 1249.0`
    *   Total DPS: `100.4 + 1249.0 = 1349.4`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: spell[idx] * 15.0` (15 missiles total).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: (spell[idx] * 15.0) * (ap / 100.0) * crit` (applies AP scaling and JG crit to all 15 missiles).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Returns pre-calculated values corresponding to Shojin cycles and Archangel ramping.
*   **Stats & Cycle Keys**:
    *   `as`: `0.80`
    *   `eq_as`: `0.92` (Shojin AS bonus)
    *   `base_cycle`: `16.88` seconds
    *   `eq_cycle`: `10.33` seconds
    *   `lockout`: `1.5` seconds
