# Garen DPS Calculation Details ⚔️

This document provides the step-by-step mathematical calculations and formulas for Garen's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.75 (constant across star levels)
*   **Attack Damage (AD)**: 70 / 105 / 158
*   **Spell Magic Damage (Base)**: AD Ratio: 0.80 / 0.82 / 0.85

### 2. Skill Description & Garen Mechanics
*   **Skill Description**: Judgement is a pure AD-ratio spin (does not scale with AP). Spin AD-ratio scales: 80% / 82% / 85% per star level. Number of spins = 8 (unequipped) | 10 (equipped with +25% bonus AS).
*   **Mechanical Timing & Assumptions**:
    *   spell_base is [0, 0, 0] since he has no flat damage, and spell_ad_ratio is the spin scaling ([0.80, 0.82, 0.85]). Overrides output 8 spins baseline / 10 spins equipped.
*   **Mana & Casting**:
    *   Max Mana: 80
    *   Start Mana: 0
    *   **Attacks to Cast**: 8 attacks
    *   **Cast Lockout**: 4.0 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS) + Base Lockout = (8) / (0.75) + 4.0 = 14.670 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (8 * AD) / 14.670s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 14.670s`

*   **1★ Garen**:
    *   Auto Attack DPS: `(8 Attacks * 70 AD) / 14.670s = 38.2`
    *   Spell Damage: `(70 AD * 0.80) * 8 spins = 448.0`
    *   Spell DPS: `448.0 / 14.670s = 30.5`
    *   Total DPS: `38.2 + 30.5 = 68.7`
*   **2★ Garen**:
    *   Auto Attack DPS: `(8 Attacks * 105 AD) / 14.670s = 57.3`
    *   Spell Damage: `(105 AD * 0.82) * 8 spins = 688.8`
    *   Spell DPS: `688.8 / 14.670s = 47.0`
    *   Total DPS: `57.3 + 47.0 = 104.2`
*   **3★ Garen**:
    *   Auto Attack DPS: `(8 Attacks * 158 AD) / 14.670s = 86.2`
    *   Spell Damage: `(158 AD * 0.85) * 8 spins = 1074.4`
    *   Spell DPS: `1074.4 / 14.670s = 73.2`
    *   Total DPS: `86.2 + 73.2 = 159.4`


### 3. Trigger Condition Scaling
*   *Note: Judgement spins around adjacent enemies. Synergy assumes 2.0 targets hit on average.* 
*   **1★ Garen (Synergy)**:
    *   Auto Attack DPS: `(8 Attacks * 70 AD) / 14.670s = 38.2`
    *   Spell Damage: `(70 AD * 0.80) * 8 spins * 2.0 Targets = 896.0`
    *   Spell DPS: `896.0 / 14.670s = 61.1`
    *   Total DPS: `38.2 + 61.1 = 99.3`
*   **2★ Garen (Synergy)**:
    *   Auto Attack DPS: `(8 Attacks * 105 AD) / 14.670s = 57.3`
    *   Spell Damage: `(105 AD * 0.82) * 8 spins * 2.0 Targets = 1377.6`
    *   Spell DPS: `1377.6 / 14.670s = 93.9`
    *   Total DPS: `57.3 + 93.9 = 151.2`
*   **3★ Garen (Synergy)**:
    *   Auto Attack DPS: `(8 Attacks * 158 AD) / 14.670s = 86.2`
    *   Spell Damage: `(158 AD * 0.85) * 8 spins * 2.0 Targets = 2148.8`
    *   Spell DPS: `2148.8 / 14.670s = 146.5`
    *   Total DPS: `86.2 + 146.5 = 232.6`

---

## 🧮 Equipped Calculations (Bloodthirster + Titan's Resolve + Jeweled Gauntlet)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +40% AD (1.40* multiplier)
*   **Total Equipped AP Modifier**: 170 AP (1.70* spell multiplier)
*   **Crit Stats**: 40% Crit Chance, 170% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.28*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS_equipped) + Base Lockout = (8) / (0.94) + 4.0 = 12.530 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (8 * AD_equipped) / 12.530s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 12.530s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Garen (Equipped)**:
    *   Auto Attack DPS: `(8 Attacks * 98 AD) / 12.530s * 1.28 Crit = 97.4`
    *   Spell Damage: `(98 AD * 0.80) * 10 spins * 1.28 Crit = 1220.4`
    *   Spell DPS: `1220.4 / 12.530s = 97.4`
    *   Total DPS: `97.4 + 97.4 = 194.8`
*   **2★ Garen (Equipped)**:
    *   Auto Attack DPS: `(8 Attacks * 147 AD) / 12.530s * 1.28 Crit = 146.3`
    *   Spell Damage: `(147 AD * 0.82) * 10 spins * 1.28 Crit = 1833.1`
    *   Spell DPS: `1833.1 / 12.530s = 146.3`
    *   Total DPS: `146.3 + 146.3 = 292.6`
*   **3★ Garen (Equipped)**:
    *   Auto Attack DPS: `(8 Attacks * 221 AD) / 12.530s * 1.28 Crit = 219.0`
    *   Spell Damage: `(221 AD * 0.85) * 10 spins * 1.28 Crit = 2744.1`
    *   Spell DPS: `2744.1 / 12.530s = 219.0`
    *   Total DPS: `219.0 + 219.0 = 438.0`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: (ad * spell[idx]) * 8.0` (8 spins baseline).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: (ad * spell[idx]) * 10.0 * crit` (10 spins equipped).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Models the channel duration as cast lockout during which normal auto-attacks are paused.
*   **Stats & Cycle Keys**:
    *   `as`: `0.75`
    *   `eq_as`: `0.94` (Titan's Resolve/Bloodthirster AS)
    *   `base_cycle`: `14.67` seconds
    *   `eq_cycle`: `12.53` seconds
    *   `lockout`: `4.0` seconds
