# Cassiopeia DPS Calculation Details 🐍

This document provides the step-by-step mathematical calculations and formulas for Cassiopeia's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.70 (constant across star levels)
*   **Attack Damage (AD)**: 40 / 60 / 90
*   **Spell Magic Damage (Base)**: AP Ratio: 1.00x | Flat: 160 / 240 / 360

### 2. Skill Description & Cassiopeia Mechanics
*   **Skill Description**: Deal 160/240/360 magic damage to the current target and Wound them for 5 seconds. If they are already Wounded, deal 30% bonus magic damage. (Wound: reduces healing received by 33%).
*   **Mechanical Timing & Assumptions**:
    *   Spell base represents the flat base damage ([160, 240, 360]). Spell damage is multiplied by 1.30 to account for wound bonus, which is maintained continuously because her 4.79s unequipped cast cycle is shorter than the 5s wound duration.
*   **Mana & Casting**:
    *   Max Mana: 30
    *   Start Mana: 0
    *   **Attacks to Cast**: 3 attacks
    *   **Cast Lockout**: 0.5 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS) + Base Lockout = (3) / (0.70) + 0.5 = 4.786 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (3 * AD) / 4.786s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 4.786s`

*   **1★ Cassiopeia**:
    *   Auto Attack DPS: `(3 Attacks * 40 AD) / 4.786s = 25.1`
    *   Spell Damage: `160 * 1.30 Wound = 208.2`
    *   Spell DPS: `208.2 / 4.786s = 43.5`
    *   Total DPS: `25.1 + 43.5 = 68.6`
*   **2★ Cassiopeia**:
    *   Auto Attack DPS: `(3 Attacks * 60 AD) / 4.786s = 37.6`
    *   Spell Damage: `240 * 1.30 Wound = 312.0`
    *   Spell DPS: `312.0 / 4.786s = 65.2`
    *   Total DPS: `37.6 + 65.2 = 102.8`
*   **3★ Cassiopeia**:
    *   Auto Attack DPS: `(3 Attacks * 90 AD) / 4.786s = 56.4`
    *   Spell Damage: `360 * 1.30 Wound = 468.0`
    *   Spell DPS: `468.0 / 4.786s = 97.8`
    *   Total DPS: `56.4 + 97.8 = 154.2`


---

## 🧮 Equipped Calculations (Blue Buff + Jeweled Gauntlet + Archangel's Staff)

### 1. Item Stats
*   **Total Equipped AD Modifier**: None (+0%)
*   **Total Equipped AP Modifier**: 200 AP (2.00* spell multiplier)
*   **Crit Stats**: 40% Crit Chance, 170% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.28*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS_equipped) + Base Lockout = (2) / (0.70) + 0.5 = 3.357 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (2 * AD_equipped) / 3.357s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 3.357s`

#### Average Phase (30s Fight Average, 2.00x AP)
*   **1★ Cassiopeia (Equipped)**:
    *   Auto Attack DPS: `(2 Attacks * 40 AD) / 3.357s * 1.28 Crit = 30.5`
    *   Spell Damage: `(160 * 1.30) * 2.00 AP * 1.28 Crit = 532.4`
    *   Spell DPS: `532.4 / 3.357s = 158.6`
    *   Total DPS: `30.5 + 158.6 = 189.1`
*   **2★ Cassiopeia (Equipped)**:
    *   Auto Attack DPS: `(2 Attacks * 60 AD) / 3.357s * 1.28 Crit = 45.8`
    *   Spell Damage: `(240 * 1.30) * 2.00 AP * 1.28 Crit = 798.7`
    *   Spell DPS: `798.7 / 3.357s = 237.9`
    *   Total DPS: `45.8 + 237.9 = 283.7`
*   **3★ Cassiopeia (Equipped)**:
    *   Auto Attack DPS: `(2 Attacks * 90 AD) / 3.357s * 1.28 Crit = 68.6`
    *   Spell Damage: `(360 * 1.30) * 2.00 AP * 1.28 Crit = 1198.1`
    *   Spell DPS: `1198.1 / 3.357s = 356.9`
    *   Total DPS: `68.6 + 356.9 = 425.5`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: spell[idx] * 1.30` (includes the 30% wound bonus multiplier).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: (spell[idx] * 1.30) * (ap / 100.0) * crit` (applies AP and JG crit scaling).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Returns pre-calculated values corresponding to continuous wound maintenance over the fight duration.
*   **Stats & Cycle Keys**:
    *   `as`: `0.70`
    *   `base_attacks`: `3`
    *   `eq_cycle`: `3.3571` seconds
    *   `lockout`: `0.5` seconds
