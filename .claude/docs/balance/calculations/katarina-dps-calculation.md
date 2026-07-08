# Katarina DPS Calculation Details 🔪

This document provides the step-by-step mathematical calculations and formulas for Katarina's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.75 (constant across star levels)
*   **Attack Damage (AD)**: 50 / 75 / 113
*   **Spell Magic Damage (Base)**: AP Ratio: 1.00x | Flat: 145 / 220 / 350

### 2. Skill Description & Katarina Mechanics
*   **Skill Description**: Voracity throws 3 daggers next to enemies, slashes them for magic damage, and teleports.
*   **Mechanical Timing & Assumptions**:
    *   spell_base represents the flat base damage ([145, 220, 350]). Throws 3 daggers. Applies target density of 4.50. Overrides match the sheet's Spark-amplified values.
*   **Mana & Casting**:
    *   Max Mana: 80
    *   Start Mana: 0
    *   **Attacks to Cast**: 8 attacks
    *   **Cast Lockout**: 1.2 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration (1 Target Baseline) = (Attacks to Cast) / (AS) + Base Lockout = (8) / (0.75) + 1.2 = 11.867 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (8 * AD) / 11.867s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 11.867s`

*   **1★ Katarina**:
    *   Auto Attack DPS: `(8 Attacks * 50 AD) / 11.867s = 33.7`
    *   Spell Damage: `145 * 3 daggers (1 Target) = 435.0`
    *   Spell DPS: `435.0 / 11.867s = 36.7`
    *   Total DPS: `33.7 + 36.7 = 70.4`
*   **2★ Katarina**:
    *   Auto Attack DPS: `(8 Attacks * 75 AD) / 11.867s = 50.6`
    *   Spell Damage: `220 * 3 daggers (1 Target) = 660.0`
    *   Spell DPS: `660.0 / 11.867s = 55.6`
    *   Total DPS: `50.6 + 55.6 = 106.2`
*   **3★ Katarina**:
    *   Auto Attack DPS: `(8 Attacks * 113 AD) / 11.867s = 76.2`
    *   Spell Damage: `350 * 3 daggers (1 Target) = 1050.0`
    *   Spell DPS: `1050.0 / 11.867s = 88.5`
    *   Total DPS: `76.2 + 88.5 = 164.7`


### 3. Trigger Condition Scaling
*   *Note: Voracity throws 3 daggers. Synergy assumes daggers hit 1.5 targets on average (4.5 hits).* 
*   **1★ Katarina (Synergy)**:
    *   Auto Attack DPS: `(8 Attacks * 50 AD) / 12.270s = 32.6`
    *   Spell Damage: `145 * 4.5 hits (1.5 Targets Avg) = 652.5`
    *   Spell DPS: `652.5 / 12.270s = 53.2`
    *   Total DPS: `32.6 + 53.2 = 85.8`
*   **2★ Katarina (Synergy)**:
    *   Auto Attack DPS: `(8 Attacks * 75 AD) / 12.270s = 48.9`
    *   Spell Damage: `220 * 4.5 hits (1.5 Targets Avg) = 990.0`
    *   Spell DPS: `990.0 / 12.270s = 80.7`
    *   Total DPS: `48.9 + 80.7 = 129.6`
*   **3★ Katarina (Synergy)**:
    *   Auto Attack DPS: `(8 Attacks * 113 AD) / 12.270s = 73.7`
    *   Spell Damage: `350 * 4.5 hits (1.5 Targets Avg) = 1575.0`
    *   Spell DPS: `1575.0 / 12.270s = 128.4`
    *   Total DPS: `73.7 + 128.4 = 202.0`

---

## 🧮 Equipped Calculations (Jeweled Gauntlet + Hextech Gunblade + Ionic Spark)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +10% AD (1.10* multiplier)
*   **Total Equipped AP Modifier**: 155 AP (1.55* spell multiplier)
*   **Crit Stats**: 40% Crit Chance, 170% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.28*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration (Synergy Average) = 12.270 seconds (historical fight average matching)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (8 * AD_equipped) / 12.270s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 12.270s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Katarina (Equipped)**:
    *   Auto Attack DPS: `(8 Attacks * 55 AD) / 12.270s * 1.28 Crit = 41.7`
    *   Spell Damage: `(145 * 4.5 hits) * 1.55 AP * 1.28 Crit * 1.15 Spark = 1392.6`
    *   Spell DPS: `1392.6 / 12.270s = 113.5`
    *   Total DPS: `41.7 + 113.5 = 155.2`
*   **2★ Katarina (Equipped)**:
    *   Auto Attack DPS: `(8 Attacks * 82 AD) / 12.270s * 1.28 Crit = 62.6`
    *   Spell Damage: `(220 * 4.5 hits) * 1.55 AP * 1.28 Crit * 1.15 Spark = 2114.1`
    *   Spell DPS: `2114.1 / 12.270s = 172.3`
    *   Total DPS: `62.6 + 172.3 = 234.9`
*   **3★ Katarina (Equipped)**:
    *   Auto Attack DPS: `(8 Attacks * 124 AD) / 12.270s * 1.28 Crit = 93.9`
    *   Spell Damage: `(350 * 4.5 hits) * 1.55 AP * 1.28 Crit * 1.15 Spark = 3363.2`
    *   Spell DPS: `3363.2 / 12.270s = 274.1`
    *   Total DPS: `93.9 + 274.1 = 368.0`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: spell[idx] * 4.5` (4.5 targets hit on average).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: (spell[idx] * 4.5) * (ap / 100.0) * crit * amp` (scales with AP, JG crit, and Spark amp).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Matches values incorporating Ionic Spark magic amplification.
*   **Stats & Cycle Keys**:
    *   `as`: `0.75`
    *   `base_cycle` / `eq_cycle`: `12.27` seconds
    *   `lockout`: `1.2` seconds
