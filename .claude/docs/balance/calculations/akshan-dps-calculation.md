# Akshan DPS Calculation Details 🦅

This document provides the step-by-step mathematical calculations and formulas for Akshan's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.75 (constant across star levels)
*   **Attack Damage (AD)**: 60 / 90 / 135
*   **Spell Magic Damage (Base)**: AD Ratio: 1.25 / 1.25 / 1.25 + Flat: 20 / 35 / 60

### 2. Skill Description & Akshan Mechanics
*   **Skill Description**: Locks on to farthest enemy and fires a rapid channel of 6 sniper shots. Each shot deals physical damage: AD * 1.25 + 20/35/60.
*   **Mechanical Timing & Assumptions**:
    *   spell_base represents the base flat damage ([20, 35, 60]) and spell_ad_ratio represents the AD ratio ([1.25, 1.25, 1.25]). Locked to target density 6.0.
*   **Mana & Casting**:
    *   Max Mana: 110
    *   Start Mana: 0
    *   **Attacks to Cast**: 11 attacks
    *   **Cast Lockout**: 3.0 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS) + Base Lockout = (11) / (0.75) + 3.0 = 17.667 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (11 * AD) / 17.667s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 17.667s`

*   **1★ Akshan**:
    *   Auto Attack DPS: `(11 Attacks * 60 AD) / 17.667s = 37.4`
    *   Spell Damage: `(60 AD * 1.25 + 20) * 6 shots = 570.0`
    *   Spell DPS: `570.0 / 17.667s = 32.3`
    *   Total DPS: `37.4 + 32.3 = 69.6`
*   **2★ Akshan**:
    *   Auto Attack DPS: `(11 Attacks * 90 AD) / 17.667s = 56.0`
    *   Spell Damage: `(90 AD * 1.25 + 35) * 6 shots = 885.0`
    *   Spell DPS: `885.0 / 17.667s = 50.1`
    *   Total DPS: `56.0 + 50.1 = 106.1`
*   **3★ Akshan**:
    *   Auto Attack DPS: `(11 Attacks * 135 AD) / 17.667s = 84.1`
    *   Spell Damage: `(135 AD * 1.25 + 60) * 6 shots = 1372.5`
    *   Spell DPS: `1372.5 / 17.667s = 77.7`
    *   Total DPS: `84.1 + 77.7 = 161.7`


---

## 🧮 Equipped Calculations (Deathblade + Infinity Edge + Runaan's Hurricane)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +121% AD (2.21* multiplier)
*   **Total Equipped AP Modifier**: 100 AP (1.00* spell multiplier)
*   **Crit Stats**: 60% Crit Chance, 140% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.24*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = 16.870 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (11 * AD_equipped) / 16.870s * 1.24 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 16.870s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Akshan (Equipped)**:
    *   Auto Attack DPS: `(11 Attacks * 133 AD) / 16.870s * 1.24 Crit = 161.3`
    *   Spell Damage: `(133 AD * 1.25 + 20) * 6 shots * 1.24 Crit = 1386.7`
    *   Spell DPS: `1386.7 / 16.870s = 82.2`
    *   Total DPS: `161.3 + 82.2 = 243.5`
*   **2★ Akshan (Equipped)**:
    *   Auto Attack DPS: `(11 Attacks * 199 AD) / 16.870s * 1.24 Crit = 246.1`
    *   Spell Damage: `(199 AD * 1.25 + 35) * 6 shots * 1.24 Crit = 2046.3`
    *   Spell DPS: `2046.3 / 16.870s = 121.3`
    *   Total DPS: `246.1 + 121.3 = 367.4`
*   **3★ Akshan (Equipped)**:
    *   Auto Attack DPS: `(11 Attacks * 298 AD) / 16.870s * 1.24 Crit = 367.4`
    *   Spell Damage: `(298 AD * 1.25 + 60) * 6 shots * 1.24 Crit = 3154.7`
    *   Spell DPS: `3154.7 / 16.870s = 187.0`
    *   Total DPS: `367.4 + 187.0 = 554.4`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: 6.0 * (ad * spell[idx] + [20, 35, 60][idx])` (calculates physical damage of 6 sniper shots).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: 6.0 * (ad * spell[idx] + [20, 35, 60][idx]) * crit` (multiplies total channel damage by crit multiplier).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Bypasses standard dps calculations to accurately apply the 3.0s channel lockout time to the cycle duration.
*   **Stats & Cycle Keys**:
    *   `as`: `0.75`
    *   `eq_as`: `0.83` (Runaan's AS bonus)
    *   `base_cycle`: `18.67` seconds
    *   `eq_cycle`: `16.87` seconds
    *   `lockout`: `3.0` seconds
