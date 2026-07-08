# Azir DPS Calculation Details 👑

This document provides the step-by-step mathematical calculations and formulas for Azir's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.75 (constant across star levels)
*   **Attack Damage (AD)**: 30 / 45 / 68
*   **Spell Magic Damage (Base)**: AP Ratio: 1.00x | Flat: 95 / 140 / 1000

### 2. Skill Description & Azir Mechanics
*   **Skill Description**: Passive: Every 3rd attack, Sand Soldiers deal magic damage. Active: Summons a Sand Soldier.
*   **Mechanical Timing & Assumptions**:
    *   spell_base represents flat soldier magic damage ([95, 140, 1000]). Summons Sand Soldiers. Overrides match the time-averaged soldier DPS.
*   **Mana & Casting**:
    *   Max Mana: 50
    *   Start Mana: 10
    *   **Attacks to Cast**: 4 attacks
    *   **Cast Lockout**: 0.8 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = 7.730 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (4 * AD) / 7.730s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 7.730s`

*   **1★ Azir**:
    *   Auto Attack DPS: `(4 Attacks * 30 AD) / 7.730s = 19.4`
    *   Spell Damage: `95 Soldier * 3.0 soldiers = 436.0`
    *   Spell DPS: `436.0 / 7.730s = 56.4`
    *   Total DPS: `19.4 + 56.4 = 75.8`
*   **2★ Azir**:
    *   Auto Attack DPS: `(4 Attacks * 45 AD) / 7.730s = 29.1`
    *   Spell Damage: `140 Soldier * 3.0 soldiers = 642.4`
    *   Spell DPS: `642.4 / 7.730s = 83.1`
    *   Total DPS: `29.1 + 83.1 = 112.2`
*   **3★ Azir**:
    *   Auto Attack DPS: `(4 Attacks * 68 AD) / 7.730s = 44.0`
    *   Spell Damage: `1000 Soldier * 3.0 soldiers = 4950.3`
    *   Spell DPS: `4950.3 / 7.730s = 640.4`
    *   Total DPS: `44.0 + 640.4 = 684.4`


---

## 🧮 Equipped Calculations (Guinsoo's Rageblade + Rabadon's Deathcap + Jeweled Gauntlet)

### 1. Item Stats
*   **Total Equipped AD Modifier**: None (+0%)
*   **Total Equipped AP Modifier**: 190 AP (1.90* spell multiplier)
*   **Crit Stats**: 40% Crit Chance, 170% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.28*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = 5.270 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (4 * AD_equipped) / 5.270s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 5.270s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Azir (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 30 AD) / 5.270s * 1.28 Crit = 36.4`
    *   Spell Damage: `609.2 * 1.90 AP * 1.28 Crit = 1481.4`
    *   Spell DPS: `1481.4 / 5.270s = 281.1`
    *   Total DPS: `36.4 + 281.1 = 317.5`
*   **2★ Azir (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 45 AD) / 5.270s * 1.28 Crit = 54.6`
    *   Spell Damage: `895.5 * 1.90 AP * 1.28 Crit = 2196.0`
    *   Spell DPS: `2196.0 / 5.270s = 416.7`
    *   Total DPS: `54.6 + 416.7 = 471.3`
*   **3★ Azir (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 68 AD) / 5.270s * 1.28 Crit = 82.6`
    *   Spell Damage: `6800.0 * 1.90 AP * 1.28 Crit = 14991.6`
    *   Spell DPS: `14991.6 / 5.270s = 2844.7`
    *   Total DPS: `82.6 + 2844.7 = 2927.3`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): Omitted; baseline calculations use the `baseline_override` directly.
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: [609.2, 895.5, 6800.0][idx] * (ap / 100.0) * crit` (represents the sum of Sand Soldier active and passive magic hits).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Bypasses standard calculator flow to handle the Soldier-based auto-attack enhancement.
*   **Stats & Cycle Keys**:
    *   `as`: `0.75`
    *   `eq_as`: `1.10` (Guinsoo average AS)
    *   `eq_ap`: `190.0`
    *   `base_cycle`: `7.73` seconds
    *   `eq_cycle`: `5.27` seconds
    *   `lockout`: `0.8` seconds
