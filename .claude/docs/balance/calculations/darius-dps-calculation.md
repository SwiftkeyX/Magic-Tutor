# Darius DPS Calculation Details 🪓

This document provides the step-by-step mathematical calculations and formulas for Darius's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.70 (constant across star levels)
*   **Attack Damage (AD)**: 65 / 98 / 146
*   **Spell Magic Damage (Base)**: AD Ratio: 3.50 / 3.50 / 3.50 + Flat: 55 / 80 / 110

### 2. Skill Description & Darius Mechanics
*   **Skill Description**: Slashes target dealing physical damage. If this kills them, he immediately casts again dealing reduced damage (10% falloff at 1★).
*   **Mechanical Timing & Assumptions**:
    *   spell_base is flat damage ([55, 80, 110]) and spell_ad_ratio is AD ratio ([3.50, 3.50, 3.50]). Overrides reflect resetting execution damage.
*   **Mana & Casting**:
    *   Max Mana: 90
    *   Start Mana: 0
    *   **Attacks to Cast**: 9 attacks
    *   **Cast Lockout**: 1.0 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration (1 Target Baseline) = (Attacks to Cast) / (AS) + Base Lockout = (9) / (0.70) + 1.0 = 13.857 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (9 * AD) / 13.857s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 13.857s`

*   **1★ Darius**:
    *   Auto Attack DPS: `(9 Attacks * 65 AD) / 13.857s = 42.2`
    *   Spell Damage: `(65 AD * 3.50 + 55) * 1.0 Cast (No resets) = 282.5`
    *   Spell DPS: `282.5 / 13.857s = 20.4`
    *   Total DPS: `42.2 + 20.4 = 62.6`
*   **2★ Darius**:
    *   Auto Attack DPS: `(9 Attacks * 98 AD) / 13.857s = 63.6`
    *   Spell Damage: `(98 AD * 3.50 + 80) * 1.0 Cast (No resets) = 423.0`
    *   Spell DPS: `423.0 / 13.857s = 30.5`
    *   Total DPS: `63.6 + 30.5 = 94.2`
*   **3★ Darius**:
    *   Auto Attack DPS: `(9 Attacks * 146 AD) / 13.857s = 94.8`
    *   Spell Damage: `(146 AD * 3.50 + 110) * 1.0 Cast (No resets) = 621.0`
    *   Spell DPS: `621.0 / 13.857s = 44.8`
    *   Total DPS: `94.8 + 44.8 = 139.6`


### 3. Trigger Condition Scaling
*   *Note: The synergy cycle of 15.710s represents a 1-reset average sequence (1.90 average casts per cycle). It is derived as:* 
    *   `Synergy Cycle = (9 Attacks / 0.70 AS) + 1.90 Casts * 1.0s Cast Lockout + 0.953s animation/target transition overhead = 12.857s + 1.90s + 0.953s = 15.710 seconds` 
*   **1★ Darius (Synergy)**:
    *   Auto Attack DPS: `(9 Attacks * 65 AD) / 15.710s = 37.2`
    *   Spell Damage: `(65 AD * 3.5 + 55) * 1.90 resetting = 536.8`
    *   Spell DPS: `536.8 / 15.710s = 34.2`
    *   Total DPS: `37.2 + 34.2 = 71.4`
*   **2★ Darius (Synergy)**:
    *   Auto Attack DPS: `(9 Attacks * 98 AD) / 15.710s = 56.1`
    *   Spell Damage: `(98 AD * 3.5 + 80) * 1.90 resetting = 803.7`
    *   Spell DPS: `803.7 / 15.710s = 51.2`
    *   Total DPS: `56.1 + 51.2 = 107.3`
*   **3★ Darius (Synergy)**:
    *   Auto Attack DPS: `(9 Attacks * 146 AD) / 15.710s = 83.6`
    *   Spell Damage: `(146 AD * 3.5 + 110) * 1.90 resetting = 1179.9`
    *   Spell DPS: `1179.9 / 15.710s = 75.1`
    *   Total DPS: `83.6 + 75.1 = 158.7`

---

## 🧮 Equipped Calculations (Infinity Edge + Bloodthirster + Titan's Resolve)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +75% AD (1.75* multiplier)
*   **Total Equipped AP Modifier**: 150 AP (1.50* spell multiplier)
*   **Crit Stats**: 60% Crit Chance, 140% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.24*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration (Synergy Average) = 12.500 seconds (historical fight average matching)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (9 * AD_equipped) / 12.500s * 1.24 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 12.500s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Darius (Equipped)**:
    *   Auto Attack DPS: `(9 Attacks * 114 AD) / 12.500s * 1.24 Crit = 118.7`
    *   Spell Damage: `((114 AD * 3.50) + (55 * 1.50 AP)) * 1.90 resetting * 1.24 Crit = 1291.2`
    *   Spell DPS: `1291.2 / 12.500s = 103.3`
    *   Total DPS: `118.7 + 103.3 = 222.0`
*   **2★ Darius (Equipped)**:
    *   Auto Attack DPS: `(9 Attacks * 172 AD) / 12.500s * 1.24 Crit = 177.3`
    *   Spell Damage: `((172 AD * 3.50) + (80 * 1.50 AP)) * 1.90 resetting * 1.24 Crit = 1937.5`
    *   Spell DPS: `1937.5 / 12.500s = 155.0`
    *   Total DPS: `177.3 + 155.0 = 332.3`
*   **3★ Darius (Equipped)**:
    *   Auto Attack DPS: `(9 Attacks * 256 AD) / 12.500s * 1.24 Crit = 264.4`
    *   Spell Damage: `((256 AD * 3.50) + (110 * 1.50 AP)) * 1.90 resetting * 1.24 Crit = 2890.0`
    *   Spell DPS: `2890.0 / 12.500s = 231.2`
    *   Total DPS: `264.4 + 231.2 = 495.6`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: (ad * 3.50 + [55, 80, 110][idx]) * 1.90` (represents execution slash with a 1.90x reset density factor).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: ((ad * 3.50) + ([55, 80, 110][idx] * 1.50)) * 1.90 * crit` (accounts for item-stat scaling and resets).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Overrides standard DPS equations with reset-averaged combat DPS.
*   **Stats & Cycle Keys**:
    *   `as`: `0.70`
    *   `eq_as`: `0.88` (Bloodthirster/Titan's AS)
    *   `base_cycle`: `15.71` seconds
    *   `eq_cycle`: `12.50` seconds
    *   `lockout`: `1.0` seconds
