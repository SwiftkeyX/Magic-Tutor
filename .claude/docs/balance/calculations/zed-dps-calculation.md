# Zed DPS Calculation Details 🥷

This document provides the step-by-step mathematical calculations and formulas for Zed's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.75 (constant across star levels)
*   **Attack Damage (AD)**: 55 / 83 / 124
*   **Spell Magic Damage (Base)**: AD Ratio: 0.30 / 0.30 / 0.35

### 2. Skill Description & Zed Mechanics
*   **Skill Description**: Creates a shadow clone at the furthest enemy within 2 hexes. Zed and his shadow slash adjacent targets.
*   **Mechanical Timing & Assumptions**:
    *   Clones scale spell damage by 3.00x based on clone attacks and slash physical damage. spell_base is [0, 0, 0] and spell_ad_ratio is [0.30, 0.30, 0.35].
*   **Mana & Casting**:
    *   Max Mana: 70
    *   Start Mana: 0
    *   **Attacks to Cast**: 7 attacks
    *   **Cast Lockout**: 0.8 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration (1 Target Baseline) = (Attacks to Cast) / (AS) + Base Lockout = (7) / (0.75) + 0.8 = 10.133 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (7 * AD) / 10.133s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 10.133s`

*   **1★ Zed**:
    *   Auto Attack DPS: `(7 Attacks * 55 AD) / 10.133s = 38.0`
    *   Spell Damage: `(55 AD * 0.30) * 3.0 Clones * 1 Target = 49.5`
    *   Spell DPS: `49.5 / 10.133s = 4.9`
    *   Total DPS: `38.0 + 4.9 = 42.9`
*   **2★ Zed**:
    *   Auto Attack DPS: `(7 Attacks * 83 AD) / 10.133s = 57.3`
    *   Spell Damage: `(83 AD * 0.30) * 3.0 Clones * 1 Target = 74.7`
    *   Spell DPS: `74.7 / 10.133s = 7.4`
    *   Total DPS: `57.3 + 7.4 = 64.7`
*   **3★ Zed**:
    *   Auto Attack DPS: `(7 Attacks * 124 AD) / 10.133s = 85.7`
    *   Spell Damage: `(124 AD * 0.35) * 3.0 Clones * 1 Target = 130.2`
    *   Spell DPS: `130.2 / 10.133s = 12.8`
    *   Total DPS: `85.7 + 12.8 = 98.5`


### 3. Trigger Condition Scaling
*   *Note: Shadow clone attacks and slashes adjacent targets. Synergy assumes 2.0 targets average.* 
*   **1★ Zed (Synergy)**:
    *   Auto Attack DPS: `(7 Attacks * 55 AD) / 10.130s = 35.6`
    *   Spell Damage: `(55 AD * 0.3) * 3 Clones * 2 Targets Avg = 339.4`
    *   Spell DPS: `339.4 / 10.130s = 33.5`
    *   Total DPS: `35.6 + 33.5 = 69.1`
*   **2★ Zed (Synergy)**:
    *   Auto Attack DPS: `(7 Attacks * 83 AD) / 10.130s = 53.4`
    *   Spell Damage: `(83 AD * 0.3) * 3 Clones * 2 Targets Avg = 509.5`
    *   Spell DPS: `509.5 / 10.130s = 50.3`
    *   Total DPS: `53.4 + 50.3 = 103.7`
*   **3★ Zed (Synergy)**:
    *   Auto Attack DPS: `(7 Attacks * 124 AD) / 10.130s = 80.1`
    *   Spell Damage: `(124 AD * 0.35) * 3 Clones * 2 Targets Avg = 760.8`
    *   Spell DPS: `760.8 / 10.130s = 75.1`
    *   Total DPS: `80.1 + 75.1 = 155.2`

---

## 🧮 Equipped Calculations (Infinity Edge + Titan's Resolve + Bloodthirster)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +75% AD (1.75* multiplier)
*   **Total Equipped AP Modifier**: 150 AP (1.50* spell multiplier)
*   **Crit Stats**: 60% Crit Chance, 140% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.24*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration (Synergy Average) = 8.650 seconds (historical fight average matching)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (7 * AD_equipped) / 8.650s * 1.24 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 8.650s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Zed (Equipped)**:
    *   Auto Attack DPS: `(7 Attacks * 96 AD) / 8.650s * 1.24 Crit = 56.8`
    *   Spell Damage: `(96 AD * 0.30) * 3.0 Clones * 2 Targets * 1.24 Crit = 1054.4`
    *   Spell DPS: `1054.4 / 8.650s = 121.9`
    *   Total DPS: `56.8 + 121.9 = 178.7`
*   **2★ Zed (Equipped)**:
    *   Auto Attack DPS: `(7 Attacks * 145 AD) / 8.650s * 1.24 Crit = 85.3`
    *   Spell Damage: `(145 AD * 0.30) * 3.0 Clones * 2 Targets * 1.24 Crit = 1582.1`
    *   Spell DPS: `1582.1 / 8.650s = 182.9`
    *   Total DPS: `85.3 + 182.9 = 268.2`
*   **3★ Zed (Equipped)**:
    *   Auto Attack DPS: `(7 Attacks * 217 AD) / 8.650s * 1.24 Crit = 127.9`
    *   Spell Damage: `(217 AD * 0.35) * 3.0 Clones * 2 Targets * 1.24 Crit = 2372.7`
    *   Spell DPS: `2372.7 / 8.650s = 274.3`
    *   Total DPS: `127.9 + 274.3 = 402.2`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: ad * spell[idx] * 3.0` (represents 3 clone slashes).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: ad * spell[idx] * 3.0 * crit` (applies crit multiplier to clone slashes).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Models clone summon sequences.
*   **Stats & Cycle Keys**:
    *   `as`: `0.75`
    *   `eq_as`: `0.94` (Titan's Resolve AS)
    *   `base_cycle`: `10.13` seconds
    *   `eq_cycle`: `8.65` seconds
    *   `lockout`: `0.8` seconds
