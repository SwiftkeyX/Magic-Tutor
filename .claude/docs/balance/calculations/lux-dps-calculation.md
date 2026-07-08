# Lux DPS Calculation Details 🪄

This document provides the step-by-step mathematical calculations and formulas for Lux's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.70 (constant across star levels)
*   **Attack Damage (AD)**: 45 / 68 / 101
*   **Spell Magic Damage (Base)**: AP Ratio: 1.00x | Flat: 735 / 1100 / 2750

### 2. Skill Description & Lux Mechanics
*   **Skill Description**: Channels a barrage of light at current target dealing magic damage over 3 seconds, reducing MR.
*   **Mechanical Timing & Assumptions**:
    *   spell_base is flat damage ([735, 1100, 2750]). Channels magic barrage on a single target. Overrides match the sheet's Blue Buff and Jeweled Gauntlet crit scaling.
*   **Mana & Casting**:
    *   Max Mana: 40
    *   Start Mana: 0
    *   **Attacks to Cast**: 4 attacks
    *   **Cast Lockout**: 3.0 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS) + Base Lockout = (4) / (0.70) + 3.0 = 8.710 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (4 * AD) / 8.710s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 8.710s`

*   **1★ Lux**:
    *   Auto Attack DPS: `(4 Attacks * 45 AD) / 8.710s = 20.7`
    *   Spell Damage: `735 Single Target Channel = 735.0`
    *   Spell DPS: `735.0 / 8.710s = 84.4`
    *   Total DPS: `20.7 + 84.4 = 105.1`
*   **2★ Lux**:
    *   Auto Attack DPS: `(4 Attacks * 68 AD) / 8.710s = 31.2`
    *   Spell Damage: `1100 Single Target Channel = 1100.0`
    *   Spell DPS: `1100.0 / 8.710s = 126.3`
    *   Total DPS: `31.2 + 126.3 = 157.5`
*   **3★ Lux**:
    *   Auto Attack DPS: `(4 Attacks * 101 AD) / 8.710s = 46.4`
    *   Spell Damage: `2750 Single Target Channel = 2750.0`
    *   Spell DPS: `2750.0 / 8.710s = 315.7`
    *   Total DPS: `46.4 + 315.7 = 362.1`


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
    
 `Cycle Duration = (Attacks to Cast) / (AS_equipped) + Base Lockout = (3) / (0.70) + 3.0 = 7.290 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (3 * AD_equipped) / 7.290s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 7.290s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Lux (Equipped)**:
    *   Auto Attack DPS: `(3 Attacks * 45 AD) / 7.290s * 1.28 Crit = 23.7`
    *   Spell Damage: `735 * 2.00 AP * 1.28 Crit = 1882.3`
    *   Spell DPS: `1882.3 / 7.290s = 258.2`
    *   Total DPS: `23.7 + 258.2 = 281.9`
*   **2★ Lux (Equipped)**:
    *   Auto Attack DPS: `(3 Attacks * 68 AD) / 7.290s * 1.28 Crit = 35.6`
    *   Spell Damage: `1100 * 2.00 AP * 1.28 Crit = 2816.1`
    *   Spell DPS: `2816.1 / 7.290s = 386.3`
    *   Total DPS: `35.6 + 386.3 = 421.9`
*   **3★ Lux (Equipped)**:
    *   Auto Attack DPS: `(3 Attacks * 101 AD) / 7.290s * 1.28 Crit = 53.4`
    *   Spell Damage: `2750 * 2.00 AP * 1.28 Crit = 7040.0`
    *   Spell DPS: `7040.0 / 7.290s = 965.7`
    *   Total DPS: `53.4 + 965.7 = 1019.1`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: spell[idx]` (base channel magic damage).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: spell[idx] * (ap / 100.0) * crit` (scales channel with AP and JG crit).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Models the 3-second channel lockout period.
*   **Stats & Cycle Keys**:
    *   `as`: `0.70`
    *   `base_cycle`: `8.71` seconds
    *   `eq_cycle`: `7.29` seconds
    *   `lockout`: `3.0` seconds
