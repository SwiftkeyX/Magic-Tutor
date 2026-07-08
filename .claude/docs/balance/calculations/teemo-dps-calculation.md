# Teemo DPS Calculation Details 🍄

This document provides the step-by-step mathematical calculations and formulas for Teemo's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.70 (constant across star levels)
*   **Attack Damage (AD)**: 40 / 60 / 90
*   **Spell Magic Damage (Base)**: AP Ratio: 1.00x | Flat: 260 / 390 / 585

### 2. Skill Description & Teemo Mechanics
*   **Skill Description**: Throws an explosive mushroom. On detonation, enemies in a 1-hex radius are Wounded (-33% healing) and dealt magic damage over 3 seconds.
*   **Mechanical Timing & Assumptions**:
    *   spell_base represents the flat base damage ([260, 390, 585]). Applies a target density multiplier of 1.33 to represent detonation on 1.33 targets average. Uses overrides to match sheet.
*   **Mana & Casting**:
    *   Max Mana: 50
    *   Start Mana: 0
    *   **Attacks to Cast**: 5 attacks
    *   **Cast Lockout**: 0.8 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration (1 Target Baseline) = (Attacks to Cast) / (AS) + Base Lockout = (5) / (0.70) + 0.8 = 7.943 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (5 * AD) / 7.943s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 7.943s`

*   **1★ Teemo**:
    *   Auto Attack DPS: `(5 Attacks * 40 AD) / 7.943s = 25.2`
    *   Spell Damage: `260 Base * 1 Target = 260.0`
    *   Spell DPS: `260.0 / 7.943s = 32.7`
    *   Total DPS: `25.2 + 32.7 = 57.9`
*   **2★ Teemo**:
    *   Auto Attack DPS: `(5 Attacks * 60 AD) / 7.943s = 37.8`
    *   Spell Damage: `390 Base * 1 Target = 390.0`
    *   Spell DPS: `390.0 / 7.943s = 49.1`
    *   Total DPS: `37.8 + 49.1 = 86.9`
*   **3★ Teemo**:
    *   Auto Attack DPS: `(5 Attacks * 90 AD) / 7.943s = 56.7`
    *   Spell Damage: `585 Base * 1 Target = 585.0`
    *   Spell DPS: `585.0 / 7.943s = 73.7`
    *   Total DPS: `56.7 + 73.7 = 130.3`


### 3. Trigger Condition Scaling
*   *Note: Poison ticks scale linearly with enemy density. Multi-target assumes average 1.33 targets.* 
*   **1★ Teemo (Synergy)**:
    *   Auto Attack DPS: `(5 Attacks * 40 AD) / 7.940s = 25.2`
    *   Spell Damage: `260 Base * 1.33 Targets = 345.4`
    *   Spell DPS: `345.4 / 7.940s = 43.5`
    *   Total DPS: `25.2 + 43.5 = 68.7`
*   **2★ Teemo (Synergy)**:
    *   Auto Attack DPS: `(5 Attacks * 60 AD) / 7.940s = 37.8`
    *   Spell Damage: `390 Base * 1.33 Targets = 518.5`
    *   Spell DPS: `518.5 / 7.940s = 65.3`
    *   Total DPS: `37.8 + 65.3 = 103.1`
*   **3★ Teemo (Synergy)**:
    *   Auto Attack DPS: `(5 Attacks * 90 AD) / 7.940s = 56.7`
    *   Spell Damage: `585 Base * 1.33 Targets = 778.1`
    *   Spell DPS: `778.1 / 7.940s = 98.0`
    *   Total DPS: `56.7 + 98.0 = 154.7`

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
    
 `Cycle Duration (Synergy Average) = 6.860 seconds (historical fight average matching)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (4 * AD_equipped) / 6.860s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 6.860s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Teemo (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 40 AD) / 6.860s * 1.28 Crit = 29.9`
    *   Spell Damage: `(260 * 2.0 Targets) * 2.00 AP * 1.28 Crit = 884.9`
    *   Spell DPS: `884.9 / 6.860s = 129.0`
    *   Total DPS: `29.9 + 129.0 = 158.9`
*   **2★ Teemo (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 60 AD) / 6.860s * 1.28 Crit = 44.8`
    *   Spell Damage: `(390 * 2.0 Targets) * 2.00 AP * 1.28 Crit = 1328.1`
    *   Spell DPS: `1328.1 / 6.860s = 193.6`
    *   Total DPS: `44.8 + 193.6 = 238.4`
*   **3★ Teemo (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 90 AD) / 6.860s * 1.28 Crit = 67.2`
    *   Spell Damage: `(585 * 2.0 Targets) * 2.00 AP * 1.28 Crit = 1991.5`
    *   Spell DPS: `1991.5 / 6.860s = 290.3`
    *   Total DPS: `67.2 + 290.3 = 357.5`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: spell[idx] * 2.0` (target density factor of 2.0).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: (spell[idx] * 2.0) * (ap / 100.0) * crit` (applies AP and JG crit).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Returns pre-calculated values corresponding to time-averaged toxic trap tick damage.
*   **Stats & Cycle Keys**:
    *   `as`: `0.70`
    *   `base_cycle`: `7.94` seconds
    *   `eq_cycle`: `6.86` seconds
    *   `lockout`: `0.8` seconds
