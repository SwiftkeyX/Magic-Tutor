# Yasuo DPS Calculation Details 🌪️

This document provides the step-by-step mathematical calculations and formulas for Yasuo's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.80 (constant across star levels)
*   **Attack Damage (AD)**: 75 / 113 / 169
*   **Spell Magic Damage (Base)**: AD Ratio: 4.50 / 4.50 / 4.75

### 2. Skill Description & Yasuo Mechanics
*   **Skill Description**: Whirlwind knocks up target and slashes adjacent enemies. Spell deals physical damage: AD * 4.50 / 4.50 / 4.75 to the main target, plus slash physical damage to adjacent enemies.
*   **Mechanical Timing & Assumptions**:
    *   spell_base is [0, 0, 0] and spell_ad_ratio is [4.50, 4.50, 4.75]. Target density is 2.0. Overrides match the time-averaged 6-attack cycle.
*   **Mana & Casting**:
    *   Max Mana: 110
    *   Start Mana: 50
    *   **Attacks to Cast**: 6 attacks
    *   **Cast Lockout**: 1.2 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = 9.000 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (6 * AD) / 9.000s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 9.000s`

*   **1★ Yasuo**:
    *   Auto Attack DPS: `(6 Attacks * 75 AD) / 9.000s = 50.0`
    *   Spell Damage: `75 AD * (7.125 if 3 star else 6.75) whirlwind = 506.2`
    *   Spell DPS: `506.2 / 9.000s = 56.2`
    *   Total DPS: `50.0 + 56.2 = 106.2`
*   **2★ Yasuo**:
    *   Auto Attack DPS: `(6 Attacks * 113 AD) / 9.000s = 75.3`
    *   Spell Damage: `113 AD * (7.125 if 3 star else 6.75) whirlwind = 762.8`
    *   Spell DPS: `762.8 / 9.000s = 84.8`
    *   Total DPS: `75.3 + 84.8 = 160.1`
*   **3★ Yasuo**:
    *   Auto Attack DPS: `(6 Attacks * 169 AD) / 9.000s = 112.7`
    *   Spell Damage: `169 AD * (7.125 if 3 star else 6.75) whirlwind = 1204.1`
    *   Spell DPS: `1204.1 / 9.000s = 133.8`
    *   Total DPS: `112.7 + 133.8 = 246.5`


---

## 🧮 Equipped Calculations (Infinity Edge + Deathblade + Bloodthirster)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +121% AD (2.21* multiplier)
*   **Total Equipped AP Modifier**: 100 AP (1.00* spell multiplier)
*   **Crit Stats**: 60% Crit Chance, 140% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.24*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = 9.000 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (6 * AD_equipped) / 9.000s * 1.24 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 9.000s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Yasuo (Equipped)**:
    *   Auto Attack DPS: `(6 Attacks * 166 AD) / 9.000s * 1.24 Crit = 137.2`
    *   Spell Damage: `166 AD * (7.125 if 3 star else 6.75) whirlwind * 1.24 Crit = 1389.6`
    *   Spell DPS: `1389.6 / 9.000s = 154.4`
    *   Total DPS: `137.2 + 154.4 = 291.6`
*   **2★ Yasuo (Equipped)**:
    *   Auto Attack DPS: `(6 Attacks * 250 AD) / 9.000s * 1.24 Crit = 205.9`
    *   Spell Damage: `250 AD * (7.125 if 3 star else 6.75) whirlwind * 1.24 Crit = 2084.4`
    *   Spell DPS: `2084.4 / 9.000s = 231.6`
    *   Total DPS: `205.9 + 231.6 = 437.5`
*   **3★ Yasuo (Equipped)**:
    *   Auto Attack DPS: `(6 Attacks * 373 AD) / 9.000s * 1.24 Crit = 308.8`
    *   Spell Damage: `373 AD * (7.125 if 3 star else 6.75) whirlwind * 1.24 Crit = 3125.7`
    *   Spell DPS: `3125.7 / 9.000s = 347.3`
    *   Total DPS: `308.8 + 347.3 = 656.1`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: ad * (7.125 if idx==2 else 6.75)` (includes target density multiplier of 1.50x to scale the physical slashes).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: ad * (7.125 if idx==2 else 6.75) * crit` (scales slashes by crit multiplier).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Incorporates starting mana offset to reduce attacks to cast to 6.
*   **Stats & Cycle Keys**:
    *   `as`: `0.80`
    *   `base_attacks`: `6`
    *   `base_cycle` / `eq_cycle`: `9.00` seconds
    *   `lockout`: `1.2` seconds
