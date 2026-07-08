# Zeri DPS Calculation Details ⚡

This document provides the step-by-step mathematical calculations and formulas for Zeri's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.80 (constant across star levels)
*   **Attack Damage (AD)**: 65 / 98 / 146
*   **Spell Magic Damage (Base)**: AD Ratio: 1.50 / 1.50 / 1.50

### 2. Skill Description & Zeri Mechanics
*   **Skill Description**: Lightning active deals physical magic damage. Attacks chain lightning to 3 additional enemies. Chain Lightning Assumptions: Assuming a total of 4 targets hit (1 main target + 3 chain targets). Chain deals 50% damage, increasing auto-attack output multiplier to 2.50x (normal + 1.5x splash).
*   **Mechanical Timing & Assumptions**:
    *   spell_base is [0, 0, 0] and spell_ad_ratio is [1.50, 1.50, 1.50] representing active on-hit lightning scaling. target_density is 4.0 (total hit). Calculated dynamically without overrides.
*   **Mana & Casting**:
    *   Max Mana: 50
    *   Start Mana: 0
    *   **Attacks to Cast**: 5 attacks
    *   **Cast Lockout**: 0.8 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = 7.250 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (5 * AD) / 7.250s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 7.250s`

*   **1★ Zeri**:
    *   Auto Attack DPS: `(5 Attacks * 65 AD) / 7.250s = 44.8`
    *   Spell Damage: `(5.0 attacks * 65 AD) * 1.50 Chain = 487.5`
    *   Spell DPS: `487.5 / 7.250s = 67.2`
    *   Total DPS: `44.8 + 67.2 = 112.1`
*   **2★ Zeri**:
    *   Auto Attack DPS: `(5 Attacks * 98 AD) / 7.250s = 67.6`
    *   Spell Damage: `(5.0 attacks * 98 AD) * 1.50 Chain = 735.0`
    *   Spell DPS: `735.0 / 7.250s = 101.4`
    *   Total DPS: `67.6 + 101.4 = 169.0`
*   **3★ Zeri**:
    *   Auto Attack DPS: `(5 Attacks * 146 AD) / 7.250s = 100.7`
    *   Spell Damage: `(5.0 attacks * 146 AD) * 1.50 Chain = 1095.0`
    *   Spell DPS: `1095.0 / 7.250s = 151.0`
    *   Total DPS: `100.7 + 151.0 = 251.7`


---

## 🧮 Equipped Calculations (Infinity Edge + Last Whisper + Deathblade)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +111% AD (2.11* multiplier)
*   **Total Equipped AP Modifier**: 100 AP (1.00* spell multiplier)
*   **Crit Stats**: 70% Crit Chance, 140% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.28*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS_equipped) + Base Lockout = (5) / (1.00) + 0.8 = 5.800 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (5 * AD_equipped) / 5.800s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 5.800s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Zeri (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 137 AD) / 5.800s * 1.28 Crit = 151.2`
    *   Spell Damage: `(5.0 attacks * 137 AD) * 1.50 Chain * 1.28 Crit = 1315.2`
    *   Spell DPS: `1315.2 / 5.800s = 226.8`
    *   Total DPS: `151.2 + 226.8 = 377.9`
*   **2★ Zeri (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 207 AD) / 5.800s * 1.28 Crit = 228.4`
    *   Spell Damage: `(5.0 attacks * 207 AD) * 1.50 Chain * 1.28 Crit = 1987.2`
    *   Spell DPS: `1987.2 / 5.800s = 342.6`
    *   Total DPS: `228.4 + 342.6 = 571.0`
*   **3★ Zeri (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 308 AD) / 5.800s * 1.28 Crit = 339.9`
    *   Spell Damage: `(5.0 attacks * 308 AD) * 1.50 Chain * 1.28 Crit = 2956.8`
    *   Spell DPS: `2956.8 / 5.800s = 509.8`
    *   Total DPS: `339.9 + 509.8 = 849.7`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: (5.0 * ad) * 1.50` (representing chain lightning scaling).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp, ratio=[1.50, 1.50, 1.50]: (5.0 * ad) * ratio[idx] * crit` (scales lightning with crit).
*   **Baseline / Equipped Overrides**: Removed; calculated dynamically.
*   **Stats & Cycle Keys**:
    *   `as`: `0.80`
    *   `eq_as`: `1.00` (Last Whisper AS bonus + active lightning AS)
    *   `base_attacks`: `5`
    *   `base_cycle`: `7.25` seconds
    *   `eq_cycle`: `5.80` seconds
    *   `lockout`: `0.8` seconds
