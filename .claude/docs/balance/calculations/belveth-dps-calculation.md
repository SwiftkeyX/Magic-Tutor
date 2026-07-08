# Bel'Veth DPS Calculation Details 🦈

This document provides the step-by-step mathematical calculations and formulas for Bel'Veth's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.85 (constant across star levels)
*   **Attack Damage (AD)**: 80 / 120 / 180
*   **Spell Magic Damage (Base)**: AD Ratio: 0.60 / 0.60 / 0.60

### 2. Skill Description & Bel'Veth Mechanics
*   **Skill Description**: Lashes out 6 times (1★, 2★) / 25 times (3★). Each lash deals physical damage equal to 60% AD, plus a true damage execute component based on target max health: 1% / 1.5% / 5% target max health.
*   **Mechanical Timing & Assumptions**:
    *   spell_base is [0, 0, 0] since there is no flat damage component, and spell_ad_ratio is [0.60, 0.60, 0.60] (lash AD scaling).
*   **Mana & Casting**:
    *   Max Mana: 70
    *   Start Mana: 20
    *   **Attacks to Cast**: 5 attacks
    *   **Cast Lockout**: 1.2 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = 7.290 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (5 * AD) / 7.290s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 7.290s`

*   **1★ Bel'Veth**:
    *   Auto Attack DPS: `(5 Attacks * 80 AD) / 7.290s = 54.9`
    *   Spell Damage: `(80 AD * 0.60 + 2000 HP * 0.010) * 6 lashes = 408.0`
    *   Spell DPS: `408.0 / 7.290s = 56.0`
    *   Total DPS: `54.9 + 56.0 = 110.8`
*   **2★ Bel'Veth**:
    *   Auto Attack DPS: `(5 Attacks * 120 AD) / 7.290s = 82.3`
    *   Spell Damage: `(120 AD * 0.60 + 2000 HP * 0.015) * 6 lashes = 612.0`
    *   Spell DPS: `612.0 / 7.290s = 84.0`
    *   Total DPS: `82.3 + 84.0 = 166.3`
*   **3★ Bel'Veth**:
    *   Auto Attack DPS: `(5 Attacks * 180 AD) / 7.290s = 123.5`
    *   Spell Damage: `(180 AD * 0.60 + 2000 HP * 0.050) * 25 lashes = 5200.0`
    *   Spell DPS: `5200.0 / 7.290s = 713.3`
    *   Total DPS: `123.5 + 713.3 = 836.8`


---

## 🧮 Equipped Calculations (Deathblade + Infinity Edge + Bloodthirster)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +121% AD (2.21* multiplier)
*   **Total Equipped AP Modifier**: 100 AP (1.00* spell multiplier)
*   **Crit Stats**: 60% Crit Chance, 140% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.24*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = 7.290 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (5 * AD_equipped) / 7.290s * 1.24 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 7.290s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Bel'Veth (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 177 AD) / 7.290s * 1.24 Crit = 150.5`
    *   Spell Damage: `(177 AD * 0.60 * 1.24 Crit + 2000 HP * 0.010) * 6 lashes = 909.8`
    *   Spell DPS: `909.8 / 7.290s = 124.8`
    *   Total DPS: `150.5 + 124.8 = 275.3`
*   **2★ Bel'Veth (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 265 AD) / 7.290s * 1.24 Crit = 225.8`
    *   Spell Damage: `(265 AD * 0.60 * 1.24 Crit + 2000 HP * 0.015) * 6 lashes = 1364.7`
    *   Spell DPS: `1364.7 / 7.290s = 187.2`
    *   Total DPS: `225.8 + 187.2 = 413.0`
*   **3★ Bel'Veth (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 398 AD) / 7.290s * 1.24 Crit = 338.6`
    *   Spell Damage: `(398 AD * 0.60 * 1.24 Crit + 2000 HP * 0.050) * 25 lashes = 9902.0`
    *   Spell DPS: `9902.0 / 7.290s = 1358.3`
    *   Total DPS: `338.6 + 1358.3 = 1696.9`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: (ad * 0.60 + 2000.0 * ((5.0 if idx==2 else (1.5 if idx==1 else 1.0)) / 100.0)) * (25 if idx==2 else 6)` (calculates physical lash scaling plus true damage execute against a standardized 2000 HP target).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: (ad * 0.60 * crit + 2000.0 * ((5.0 if idx==2 else (1.5 if idx==1 else 1.0)) / 100.0)) * (25 if idx==2 else 6)` (applies crit multiplier to the physical damage portion).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Maps exact execute calculations to the final output.
*   **Stats & Cycle Keys**:
    *   `as`: `0.85`
    *   `eq_as`: `0.85`
    *   `base_cycle` / `eq_cycle`: `7.29` seconds
    *   `lockout`: `1.2` seconds
