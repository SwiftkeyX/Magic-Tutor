# Jinx DPS Calculation Details 🚀

This document provides the step-by-step mathematical calculations and formulas for Jinx's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.75 (constant across star levels)
*   **Attack Damage (AD)**: 50 / 75 / 113
*   **Spell Magic Damage (Base)**: AD Ratio: 1.50 / 1.55 / 1.60 + Flat: 15 / 20 / 35

### 2. Skill Description & Jinx Mechanics
*   **Skill Description**: Fires 5 rockets, splitting damage across nearby targets. Spell deals physical damage: AD * 1.50/1.55/1.60 + 15/20/35 per rocket.
*   **Mechanical Timing & Assumptions**:
    *   spell_base represents the flat base damage ([15, 20, 35]) and spell_ad_ratio is the AD ratio ([1.50, 1.55, 1.60]). target_density is 2.0. Overrides match the rocket launch sequence.
*   **Mana & Casting**:
    *   Max Mana: 75
    *   Start Mana: 0
    *   **Attacks to Cast**: 8 attacks
    *   **Cast Lockout**: 0.8 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = 10.800 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (8 * AD) / 10.800s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 10.800s`

*   **1★ Jinx**:
    *   Auto Attack DPS: `(8 Attacks * 50 AD) / 10.800s = 33.9`
    *   Spell Damage: `(50 AD * 1.50 + 15) * 2.0 Targets = 469.8`
    *   Spell DPS: `469.8 / 10.800s = 43.5`
    *   Total DPS: `33.9 + 43.5 = 77.4`
*   **2★ Jinx**:
    *   Auto Attack DPS: `(8 Attacks * 75 AD) / 10.800s = 50.9`
    *   Spell Damage: `(75 AD * 1.55 + 20) * 2.0 Targets = 704.2`
    *   Spell DPS: `704.2 / 10.800s = 65.2`
    *   Total DPS: `50.9 + 65.2 = 116.1`
*   **3★ Jinx**:
    *   Auto Attack DPS: `(8 Attacks * 113 AD) / 10.800s = 76.3`
    *   Spell Damage: `(113 AD * 1.60 + 35) * 2.0 Targets = 1057.3`
    *   Spell DPS: `1057.3 / 10.800s = 97.9`
    *   Total DPS: `76.3 + 97.9 = 174.2`


---

## 🧮 Equipped Calculations (Guinsoo's Rageblade + Deathblade + Runaan's Hurricane)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +86% AD (1.86* multiplier)
*   **Total Equipped AP Modifier**: 100 AP (1.00* spell multiplier)
*   **Crit Stats**: 25% Crit Chance, 140% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.10*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = 8.170 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (8 * AD_equipped) / 8.170s * 1.10 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 8.170s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Jinx (Equipped)**:
    *   Auto Attack DPS: `(8 Attacks * 93 AD) / 8.170s * 1.10 Crit = 95.4`
    *   Spell Damage: `(93 AD * 1.50 + 15) * 2.0 Targets * 1.10 Crit = 1147.9`
    *   Spell DPS: `1147.9 / 8.170s = 140.5`
    *   Total DPS: `95.4 + 140.5 = 235.9`
*   **2★ Jinx (Equipped)**:
    *   Auto Attack DPS: `(8 Attacks * 140 AD) / 8.170s * 1.10 Crit = 143.1`
    *   Spell Damage: `(140 AD * 1.55 + 20) * 2.0 Targets * 1.10 Crit = 1722.2`
    *   Spell DPS: `1722.2 / 8.170s = 210.8`
    *   Total DPS: `143.1 + 210.8 = 353.9`
*   **3★ Jinx (Equipped)**:
    *   Auto Attack DPS: `(8 Attacks * 210 AD) / 8.170s * 1.10 Crit = 214.6`
    *   Spell Damage: `(210 AD * 1.60 + 35) * 2.0 Targets * 1.10 Crit = 2582.5`
    *   Spell DPS: `2582.5 / 8.170s = 316.1`
    *   Total DPS: `214.6 + 316.1 = 530.7`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: spell[idx] * 2.0` (target density factor of 2.0).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: spell[idx] * 2.0` (represents physical damage split across targets).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Models the rocket launch sequence.
*   **Stats & Cycle Keys**:
    *   `as`: `0.75`
    *   `eq_as`: `1.05` (Guinsoo average AS)
    *   `base_cycle`: `10.80` seconds
    *   `eq_cycle`: `8.17` seconds
    *   `lockout`: `0.8` seconds
