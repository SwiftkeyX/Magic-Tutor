# Viego DPS Calculation Details 👑

This document provides the step-by-step mathematical calculations and formulas for Viego's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.75 (constant across star levels)
*   **Attack Damage (AD)**: 45 / 68 / 101
*   **Spell Magic Damage (Base)**: AP Ratio: 1.00x | Flat: 110 / 165 / 250

### 2. Skill Description & Viego Mechanics
*   **Skill Description**: Deal 110/165/250 magic damage to the current target. For the rest of combat, Viego's attacks deal 20/30/45 bonus stacking magic damage.
*   **Mechanical Timing & Assumptions**:
    *   spell_base represents the flat base damage ([110, 165, 250]). Includes stacking magic damage on-hit. Baseline and equipped overrides return values corresponding to an average of 2 and 30 stacks respectively over combat.
*   **Mana & Casting**:
    *   Max Mana: 50
    *   Start Mana: 0
    *   **Attacks to Cast**: 5 attacks
    *   **Cast Lockout**: 0.8 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS) + Base Lockout = (5) / (0.75) + 0.8 = 7.467 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (5 * AD) / 7.467s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 7.467s`

*   **1★ Viego**:
    *   Auto Attack DPS: `(5 Attacks * 45 AD) / 7.467s = 30.1`
    *   Spell Damage: `110 Base + 200 stacking Magic = 283.0`
    *   Spell DPS: `283.0 / 7.467s = 37.9`
    *   Total DPS: `30.1 + 37.9 = 68.0`
*   **2★ Viego**:
    *   Auto Attack DPS: `(5 Attacks * 68 AD) / 7.467s = 45.5`
    *   Spell Damage: `165 Base + 200 stacking Magic = 424.9`
    *   Spell DPS: `424.9 / 7.467s = 56.9`
    *   Total DPS: `45.5 + 56.9 = 102.4`
*   **3★ Viego**:
    *   Auto Attack DPS: `(5 Attacks * 101 AD) / 7.467s = 67.6`
    *   Spell Damage: `250 Base + 200 stacking Magic = 636.9`
    *   Spell DPS: `636.9 / 7.467s = 85.3`
    *   Total DPS: `67.6 + 85.3 = 152.9`


---

## 🧮 Equipped Calculations (Hextech Gunblade + Titan's Resolve + Jeweled Gauntlet)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +30% AD (1.30* multiplier)
*   **Total Equipped AP Modifier**: 195 AP (1.95* spell multiplier)
*   **Crit Stats**: 40% Crit Chance, 170% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.28*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS_equipped) + Base Lockout = (5) / (0.94) + 0.8 = 6.119 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (5 * AD_equipped) / 6.119s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 6.119s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Viego (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 58 AD) / 6.119s * 1.28 Crit = 79.9`
    *   Spell Damage: `(110 * 1.95 AP + 200 stacking) * 1.28 Crit = 767.3`
    *   Spell DPS: `767.3 / 6.119s = 125.4`
    *   Total DPS: `79.9 + 125.4 = 205.3`
*   **2★ Viego (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 88 AD) / 6.119s * 1.28 Crit = 120.3`
    *   Spell Damage: `(165 * 1.95 AP + 200 stacking) * 1.28 Crit = 1151.0`
    *   Spell DPS: `1151.0 / 6.119s = 188.1`
    *   Total DPS: `120.3 + 188.1 = 308.4`
*   **3★ Viego (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 131 AD) / 6.119s * 1.28 Crit = 179.4`
    *   Spell Damage: `(250 * 1.95 AP + 200 stacking) * 1.28 Crit = 1732.9`
    *   Spell DPS: `1732.9 / 6.119s = 283.2`
    *   Total DPS: `179.4 + 283.2 = 462.6`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: spell[idx] + 200.0` (includes base spell + 2 stacking on-hit iterations).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: spell[idx] * (ap / 100.0) * crit + 200.0 * crit` (applies item scaling and crit).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Accounts for stacking magic on-hit buffs averaged over the fight.
*   **Stats & Cycle Keys**:
    *   `as`: `0.75`
    *   `eq_as`: `0.94` (Titan's Resolve AS)
    *   `lockout`: `0.8` seconds
