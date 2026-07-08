# Malzahar DPS Calculation Details 🔮

This document provides the step-by-step mathematical calculations and formulas for Malzahar's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.70 (constant across star levels)
*   **Attack Damage (AD)**: 40 / 60 / 90
*   **Spell Magic Damage (Base)**: AP Ratio: 1.00x | Flat: 220 / 330 / 500

### 2. Skill Description & Malzahar Mechanics
*   **Skill Description**: Open two portals near the current target. Destroy 50% of Shields and deal 220/330/500 magic damage to all enemies caught between the portals.
*   **Mechanical Timing & Assumptions**:
    *   spell_base represents the flat base damage ([220, 330, 500]). Applies target density multiplier of 1.50 directly to the spell base. Calculated dynamically without overrides.
*   **Mana & Casting**:
    *   Max Mana: 50
    *   Start Mana: 0
    *   **Attacks to Cast**: 5 attacks
    *   **Cast Lockout**: 0.8 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS) + Base Lockout = (5) / (0.70) + 0.8 = 7.943 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (5 * AD) / 7.943s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 7.943s`

*   **1★ Malzahar**:
    *   Auto Attack DPS: `(5 Attacks * 40 AD) / 7.943s = 25.2`
    *   Spell Damage: `220 * 1.50 Target Density = 330.0`
    *   Spell DPS: `330.0 / 7.943s = 41.5`
    *   Total DPS: `25.2 + 41.5 = 66.7`
*   **2★ Malzahar**:
    *   Auto Attack DPS: `(5 Attacks * 60 AD) / 7.943s = 37.8`
    *   Spell Damage: `330 * 1.50 Target Density = 495.0`
    *   Spell DPS: `495.0 / 7.943s = 62.3`
    *   Total DPS: `37.8 + 62.3 = 100.1`
*   **3★ Malzahar**:
    *   Auto Attack DPS: `(5 Attacks * 90 AD) / 7.943s = 56.7`
    *   Spell Damage: `500 * 1.50 Target Density = 750.0`
    *   Spell DPS: `750.0 / 7.943s = 94.4`
    *   Total DPS: `56.7 + 94.4 = 151.1`


---

## 🧮 Equipped Calculations (Spear of Shojin + Jeweled Gauntlet + Rabadon's Deathcap)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +15% AD (1.15* multiplier)
*   **Total Equipped AP Modifier**: 215 AP (2.15* spell multiplier)
*   **Crit Stats**: 40% Crit Chance, 170% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.28*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS_equipped) + Base Lockout = (4) / (0.70) + 0.8 = 6.514 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (4 * AD_equipped) / 6.514s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 6.514s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Malzahar (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 46 AD) / 6.514s * 1.28 Crit = 36.2`
    *   Spell Damage: `(220 * 1.50) * 2.15 AP * 1.28 Crit = 908.2`
    *   Spell DPS: `908.2 / 6.514s = 139.4`
    *   Total DPS: `36.2 + 139.4 = 175.6`
*   **2★ Malzahar (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 69 AD) / 6.514s * 1.28 Crit = 54.2`
    *   Spell Damage: `(330 * 1.50) * 2.15 AP * 1.28 Crit = 1362.2`
    *   Spell DPS: `1362.2 / 6.514s = 209.1`
    *   Total DPS: `54.2 + 209.1 = 263.3`
*   **3★ Malzahar (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 103 AD) / 6.514s * 1.28 Crit = 81.0`
    *   Spell Damage: `(500 * 1.50) * 2.15 AP * 1.28 Crit = 2064.0`
    *   Spell DPS: `2064.0 / 6.514s = 316.8`
    *   Total DPS: `81.0 + 316.8 = 397.8`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: spell[idx] * 1.50` (1.5 targets hit baseline).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: (spell[idx] * 1.50) * (ap / 100.0) * crit` (scales with AP and JG crit).
*   **Baseline / Equipped Overrides**: Removed; calculated dynamically.
*   **Stats & Cycle Keys**:
    *   `as`: `0.70`
    *   `base_attacks`: `5`
    *   `lockout`: `0.8` seconds
