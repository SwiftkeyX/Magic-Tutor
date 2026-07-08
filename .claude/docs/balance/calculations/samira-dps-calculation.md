# Samira DPS Calculation Details 🌹

This document provides the step-by-step mathematical calculations and formulas for Samira's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.70 (constant across star levels)
*   **Attack Damage (AD)**: 45 / 68 / 101
*   **Spell Magic Damage (Base)**: AD Ratio: 1.90 / 1.90 / 2.00

### 2. Skill Description & Samira Mechanics
*   **Skill Description**: Shoot at the current target and deal 190/190/200% Attack Damage to the first enemy hit. Reduce their Armor by 10/15/20 for the rest of combat.
*   **Mechanical Timing & Assumptions**:
    *   spell_base is [0, 0, 0] since she has no flat spell damage, and spell_ad_ratio is the AD ratio ([1.90, 1.90, 2.00]). Overrides match the armor-shred-assisted sheet values.
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

*   **1★ Samira**:
    *   Auto Attack DPS: `(3 Attacks * 45 AD) / 4.786s = 26.5`
    *   Spell Damage: `45 AD * 1.90 AD Ratio = 159.8`
    *   Spell DPS: `159.8 / 4.786s = 33.4`
    *   Total DPS: `26.5 + 33.4 = 59.9`
*   **2★ Samira**:
    *   Auto Attack DPS: `(3 Attacks * 68 AD) / 4.786s = 40.1`
    *   Spell Damage: `68 AD * 1.90 AD Ratio = 239.8`
    *   Spell DPS: `239.8 / 4.786s = 50.1`
    *   Total DPS: `40.1 + 50.1 = 90.2`
*   **3★ Samira**:
    *   Auto Attack DPS: `(3 Attacks * 101 AD) / 4.786s = 59.6`
    *   Spell Damage: `101 AD * 2.00 AD Ratio = 359.9`
    *   Spell DPS: `359.9 / 4.786s = 75.2`
    *   Total DPS: `59.6 + 75.2 = 134.8`


---

## 🧮 Equipped Calculations (Deathblade + Infinity Edge + Last Whisper)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +111% AD (2.11* multiplier)
*   **Total Equipped AP Modifier**: 100 AP (1.00* spell multiplier)
*   **Crit Stats**: 70% Crit Chance, 140% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.28*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS_equipped) + Base Lockout = (3) / (0.77) + 0.5 = 4.396 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (3 * AD_equipped) / 4.396s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 4.396s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Samira (Equipped)**:
    *   Auto Attack DPS: `(3 Attacks * 95 AD) / 4.396s * 1.28 Crit = 45.8`
    *   Spell Damage: `95 AD * 1.90 * 1.28 Crit = 199.1`
    *   Spell DPS: `199.1 / 4.396s = 45.3`
    *   Total DPS: `45.8 + 45.3 = 91.1`
*   **2★ Samira (Equipped)**:
    *   Auto Attack DPS: `(3 Attacks * 143 AD) / 4.396s * 1.28 Crit = 68.6`
    *   Spell Damage: `143 AD * 1.90 * 1.28 Crit = 298.9`
    *   Spell DPS: `298.9 / 4.396s = 68.0`
    *   Total DPS: `68.6 + 68.0 = 136.6`
*   **3★ Samira (Equipped)**:
    *   Auto Attack DPS: `(3 Attacks * 213 AD) / 4.396s * 1.28 Crit = 102.9`
    *   Spell Damage: `213 AD * 2.00 * 1.28 Crit = 448.4`
    *   Spell DPS: `448.4 / 4.396s = 102.0`
    *   Total DPS: `102.9 + 102.0 = 204.9`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: ad * spell[idx]` (deals AD physical ratio).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: ad * spell[idx] * crit` (applies equipped crit to spell).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Accounts for the armor-shred-assisted damage increases in actual combat.
*   **Stats & Cycle Keys**:
    *   `as`: `0.70`
    *   `lockout`: `0.5` seconds
