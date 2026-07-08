# Rek'Sai DPS Calculation Details 🦈

This document provides the step-by-step mathematical calculations and formulas for Rek'Sai's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.75 (constant across star levels)
*   **Attack Damage (AD)**: 60 / 90 / 135
*   **Spell Magic Damage (Base)**: AD Ratio: 2.50 / 2.50 / 2.50

### 2. Skill Description & Rek'Sai Mechanics
*   **Skill Description**: Furious Bite deals physical damage. If target is below 66% health, deals true damage instead. True damage bite ratio: 2.50x AD. If target is marked by a previous bite, deals bonus true damage: 2.40x AD (Total bite = 4.90x AD).
*   **Mechanical Timing & Assumptions**:
    *   spell_base is [0, 0, 0] since there is no flat damage, and spell_ad_ratio is [2.50, 2.50, 2.50]. Overrides assume mark applied for 4.90x AD true damage.
*   **Mana & Casting**:
    *   Max Mana: 80
    *   Start Mana: 0
    *   **Attacks to Cast**: 8 attacks
    *   **Cast Lockout**: 0.8 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS) + Base Lockout = (8) / (0.75) + 0.8 = 11.467 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (8 * AD) / 11.467s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 11.467s`

*   **1★ Rek'Sai**:
    *   Auto Attack DPS: `(8 Attacks * 60 AD) / 11.467s = 41.9`
    *   Spell Damage: `60 AD * 4.90 Execute (Target > 66% HP) = 294.0`
    *   Spell DPS: `294.0 / 11.467s = 25.6`
    *   Total DPS: `41.9 + 25.6 = 67.5`
*   **2★ Rek'Sai**:
    *   Auto Attack DPS: `(8 Attacks * 90 AD) / 11.467s = 62.8`
    *   Spell Damage: `90 AD * 4.90 Execute (Target > 66% HP) = 441.0`
    *   Spell DPS: `441.0 / 11.467s = 38.5`
    *   Total DPS: `62.8 + 38.5 = 101.2`
*   **3★ Rek'Sai**:
    *   Auto Attack DPS: `(8 Attacks * 135 AD) / 11.467s = 94.2`
    *   Spell Damage: `135 AD * 4.90 Execute (Target > 66% HP) = 661.5`
    *   Spell DPS: `661.5 / 11.467s = 57.7`
    *   Total DPS: `94.2 + 57.7 = 151.9`


---

## 🧮 Equipped Calculations (Bloodthirster + Titan's Resolve + Infinity Edge)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +105% AD (2.05* multiplier)
*   **Total Equipped AP Modifier**: 150 AP (1.50* spell multiplier)
*   **Crit Stats**: 60% Crit Chance, 140% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.24*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS_equipped) + Base Lockout = (8) / (0.94) + 0.8 = 9.360 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (8 * AD_equipped) / 9.360s * 1.24 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 9.360s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Rek'Sai (Equipped)**:
    *   Auto Attack DPS: `(8 Attacks * 123 AD) / 9.360s * 1.24 Crit = 130.3`
    *   Spell Damage: `123 AD * 4.90 Execute (Target < 66% HP, Marked) = 602.8`
    *   Spell DPS: `602.8 / 9.360s = 64.4`
    *   Total DPS: `130.3 + 64.4 = 194.7`
*   **2★ Rek'Sai (Equipped)**:
    *   Auto Attack DPS: `(8 Attacks * 184 AD) / 9.360s * 1.24 Crit = 195.5`
    *   Spell Damage: `184 AD * 4.90 Execute (Target < 66% HP, Marked) = 904.2`
    *   Spell DPS: `904.2 / 9.360s = 96.6`
    *   Total DPS: `195.5 + 96.6 = 292.1`
*   **3★ Rek'Sai (Equipped)**:
    *   Auto Attack DPS: `(8 Attacks * 277 AD) / 9.360s * 1.24 Crit = 293.2`
    *   Spell Damage: `277 AD * 4.90 Execute (Target < 66% HP, Marked) = 1356.3`
    *   Spell DPS: `1356.3 / 9.360s = 144.9`
    *   Total DPS: `293.2 + 144.9 = 438.1`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: ad * 4.90` (represents maximum bite physical/true execution damage).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: ad * 4.90` (bite true damage portion does not scale with crit/items).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Models execution behavior below 66% health.
*   **Stats & Cycle Keys**:
    *   `as`: `0.75`
    *   `eq_as`: `0.94` (Titan's/BT AS)
    *   `eq_ad_mult`: `2.05` (incorporates BT + Titan's Resolve AD stacking)
    *   `base_cycle`: `11.73` seconds
    *   `eq_cycle`: `9.36` seconds
    *   `lockout`: `0.8` seconds
