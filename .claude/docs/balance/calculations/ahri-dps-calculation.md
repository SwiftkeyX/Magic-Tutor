# Ahri DPS Calculation Details 🦊

This document provides the step-by-step mathematical calculations and formulas for Ahri's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.85 (constant across star levels)
*   **Attack Damage (AD)**: 50 / 75 / 113
*   **Spell Magic Damage (Base)**: AP Ratio: 1.00x | Flat: 250 / 375 / 3000

### 2. Skill Description & Ahri Mechanics
*   **Skill Description**: Steals essence from targets dealing magic damage. Every 3rd cast unleashes a massive wave dealing magic damage to all enemies hit.
*   **Mechanical Timing & Assumptions**:
    *   spell_base represents flat magic damage ([250, 375, 3000]). Steals essence and waves. Overrides account for the massive 3rd cast wave damage averaged across the fight.
*   **Mana & Casting**:
    *   Max Mana: 50
    *   Start Mana: 0
    *   **Attacks to Cast**: 5 attacks
    *   **Cast Lockout**: 1.0 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = 7.060 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (5 * AD) / 7.060s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 7.060s`

*   **1★ Ahri**:
    *   Auto Attack DPS: `(5 Attacks * 50 AD) / 7.060s = 35.4`
    *   Spell Damage: `250 Essence Steal (Wave Average) = 643.2`
    *   Spell DPS: `643.2 / 7.060s = 91.1`
    *   Total DPS: `35.4 + 91.1 = 126.5`
*   **2★ Ahri**:
    *   Auto Attack DPS: `(5 Attacks * 75 AD) / 7.060s = 53.1`
    *   Spell Damage: `375 Essence Steal (Wave Average) = 951.0`
    *   Spell DPS: `951.0 / 7.060s = 134.7`
    *   Total DPS: `53.1 + 134.7 = 187.8`
*   **3★ Ahri**:
    *   Auto Attack DPS: `(5 Attacks * 113 AD) / 7.060s = 80.0`
    *   Spell Damage: `3000 Essence Steal (Wave Average) = 5333.1`
    *   Spell DPS: `5333.1 / 7.060s = 755.4`
    *   Total DPS: `80.0 + 755.4 = 835.4`


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
    
 `Cycle Duration = 5.880 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (4 * AD_equipped) / 5.880s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 5.880s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Ahri (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 50 AD) / 5.880s * 1.28 Crit = 43.5`
    *   Spell Damage: `643.3 * 2.00 AP * 1.28 Crit = 1646.4`
    *   Spell DPS: `1646.4 / 5.880s = 280.0`
    *   Total DPS: `43.5 + 280.0 = 323.5`
*   **2★ Ahri (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 75 AD) / 5.880s * 1.28 Crit = 65.3`
    *   Spell Damage: `950.8 * 2.00 AP * 1.28 Crit = 2431.4`
    *   Spell DPS: `2431.4 / 5.880s = 413.5`
    *   Total DPS: `65.3 + 413.5 = 478.8`
*   **3★ Ahri (Equipped)**:
    *   Auto Attack DPS: `(4 Attacks * 113 AD) / 5.880s * 1.28 Crit = 97.9`
    *   Spell Damage: `5333.3 * 2.00 AP * 1.28 Crit = 13645.1`
    *   Spell DPS: `13645.1 / 5.880s = 2320.6`
    *   Total DPS: `97.9 + 2320.6 = 2418.5`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): Omitted; baseline calculations use the `baseline_override` directly.
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: [643.3, 950.8, 5333.3][idx] * (ap / 100.0) * crit` (represents the average spell damage over a 30s fight factoring in the massive 3rd wave).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Returns pre-calculated, time-averaged DPS tuples to model the ramping and wave cadence of her ability over combat.
*   **Stats & Cycle Keys**:
    *   `as`: `0.85`
    *   `eq_as`: `0.85`
    *   `eq_ap`: `200.0` (Archangel average AP + JG + BB AP)
    *   `base_cycle`: `7.06` seconds
    *   `eq_cycle`: `5.88` seconds
    *   `lockout`: `1.0` seconds
