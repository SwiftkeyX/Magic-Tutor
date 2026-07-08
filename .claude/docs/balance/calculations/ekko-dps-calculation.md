# Ekko DPS Calculation Details ⏳

This document provides the step-by-step mathematical calculations and formulas for Ekko's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.80 (constant across star levels)
*   **Attack Damage (AD)**: 50 / 75 / 113
*   **Spell Magic Damage (Base)**: AP Ratio: 1.00x | Flat: 255 / 380 / 570

### 2. Skill Description & Ekko Mechanics
*   **Skill Description**: Magic damage dive that heals him for 20% of damage taken in the last 4 seconds.
*   **Mechanical Timing & Assumptions**:
    *   spell_base represents flat magic damage ([255, 380, 570]). Melee AP dive. Overrides return values matched to the sheet's survivability-adjusted cycle.
*   **Mana & Casting**:
    *   Max Mana: 50
    *   Start Mana: 0
    *   **Attacks to Cast**: 5 attacks
    *   **Cast Lockout**: 0.8 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS) + Base Lockout = (5) / (0.80) + 0.8 = 7.050 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (5 * AD) / 7.050s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 7.050s`

*   **1★ Ekko**:
    *   Auto Attack DPS: `(5 Attacks * 50 AD) / 7.050s = 35.5`
    *   Spell Damage: `255 Base Spell = 255.0`
    *   Spell DPS: `255.0 / 7.050s = 36.2`
    *   Total DPS: `35.5 + 36.2 = 71.6`
*   **2★ Ekko**:
    *   Auto Attack DPS: `(5 Attacks * 75 AD) / 7.050s = 53.2`
    *   Spell Damage: `380 Base Spell = 380.0`
    *   Spell DPS: `380.0 / 7.050s = 53.9`
    *   Total DPS: `53.2 + 53.9 = 107.1`
*   **3★ Ekko**:
    *   Auto Attack DPS: `(5 Attacks * 113 AD) / 7.050s = 80.1`
    *   Spell Damage: `570 Base Spell = 570.0`
    *   Spell DPS: `570.0 / 7.050s = 80.9`
    *   Total DPS: `80.1 + 80.9 = 161.0`


---

## 🧮 Equipped Calculations (Jeweled Gauntlet + Hextech Gunblade + Titan's Resolve)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +30% AD (1.30* multiplier)
*   **Total Equipped AP Modifier**: 195 AP (1.95* spell multiplier)
*   **Crit Stats**: 40% Crit Chance, 170% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.28*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS_equipped) + Base Lockout = (5) / (1.00) + 0.8 = 5.800 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (5 * AD_equipped) / 5.800s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 5.800s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Ekko (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 65 AD) / 5.800s * 1.28 Crit = 93.8`
    *   Spell Damage: `255 * 1.95 AP * 1.28 Crit = 636.3`
    *   Spell DPS: `636.3 / 5.800s = 109.7`
    *   Total DPS: `93.8 + 109.7 = 203.5`
*   **2★ Ekko (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 98 AD) / 5.800s * 1.28 Crit = 140.3`
    *   Spell Damage: `380 * 1.95 AP * 1.28 Crit = 951.2`
    *   Spell DPS: `951.2 / 5.800s = 164.0`
    *   Total DPS: `140.3 + 164.0 = 304.3`
*   **3★ Ekko (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 147 AD) / 5.800s * 1.28 Crit = 210.5`
    *   Spell Damage: `570 * 1.95 AP * 1.28 Crit = 1426.2`
    *   Spell DPS: `1426.2 / 5.800s = 245.9`
    *   Total DPS: `210.5 + 245.9 = 456.4`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: spell[idx]` (flat magic damage).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: spell[idx] * (ap / 100.0) * crit` (scales magic damage with AP and Jeweled Gauntlet crit).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Outputs values exactly matching the sheet's survivability-adjusted cycle.
*   **Stats & Cycle Keys**:
    *   `as`: `0.80`
    *   `eq_as`: `1.00` (Titan's Resolve AS)
    *   `base_cycle`: `7.25` seconds
    *   `eq_cycle`: `5.80` seconds
    *   `lockout`: `0.8` seconds
