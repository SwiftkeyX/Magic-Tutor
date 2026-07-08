# Tristana DPS Calculation Details 💣

This document provides the step-by-step mathematical calculations and formulas for Tristana's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.70 (constant across star levels)
*   **Attack Damage (AD)**: 45 / 68 / 101
*   **Spell Magic Damage (Base)**: AP Ratio: 1.00x | Flat: 0 / 0 / 0

### 2. Skill Description & Tristana Mechanics
*   **Skill Description**: Gain 1 Attack Speed for 4 seconds. For the duration, attacks explode on impact and deal 60% Attack Damage physical damage to enemies within 1 hex.
*   **Mechanical Timing & Assumptions**:
    *   spell_base is [0, 0, 0] since she has no base spell damage. Spell damage is physical and calculated as a percentage of her Attack Damage (60% AD). Average equipped AS is set to 1.63 to model Guinsoo stacks + active steroid AS.
*   **Mana & Casting**:
    *   Max Mana: 40
    *   Start Mana: 0
    *   **Attacks to Cast**: 4 attacks
    *   **Cast Lockout**: 0.0 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS) + Base Lockout = (4) / (0.70) + 0.0 = 5.714 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (4 * AD) / 5.714s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 5.714s`

*   **1★ Tristana**:
    *   Auto Attack DPS: `(4 Attacks * 45 AD) / 5.714s = 31.5`
    *   Spell Damage: `4.72 Attacks * (0.60 * 45 AD) * 1.0 Splash Target = 127.4`
    *   Spell DPS: `127.4 / 5.714s = 22.3`
    *   Total DPS: `31.5 + 22.3 = 53.8`
*   **2★ Tristana**:
    *   Auto Attack DPS: `(4 Attacks * 68 AD) / 5.714s = 47.6`
    *   Spell Damage: `4.72 Attacks * (0.60 * 68 AD) * 1.0 Splash Target = 190.9`
    *   Spell DPS: `190.9 / 5.714s = 33.4`
    *   Total DPS: `47.6 + 33.4 = 81.0`
*   **3★ Tristana**:
    *   Auto Attack DPS: `(4 Attacks * 101 AD) / 5.714s = 70.7`
    *   Spell Damage: `4.72 Attacks * (0.60 * 101 AD) * 1.0 Splash Target = 286.9`
    *   Spell DPS: `286.9 / 5.714s = 50.2`
    *   Total DPS: `70.7 + 50.2 = 120.9`


---

## 🧮 Equipped Calculations (Guinsoo's Rageblade + Last Whisper + Infinity Edge)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +45% AD (1.45* multiplier)
*   **Total Equipped AP Modifier**: 100 AP (1.00* spell multiplier)
*   **Crit Stats**: 70% Crit Chance, 140% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.28*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS_equipped) + Steroid Duration = (4) / (1.63) + 4.0 = 6.454 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (4 * AD_equipped) / 6.454s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 6.454s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Tristana (Equipped)**:
    *   *Note: Tristana's rotation consists of 4 basic attacks to gain mana (4 / 1.63 AS = 2.454s) plus her 4.0-second active AS steroid (total cycle = 6.454s). During the 4.0s steroid, she gains +1.0 AS, resulting in 2.63 AS. The number of attacks during the steroid is 4.0s * 2.63 AS = 10.52 attacks.*
    *   Auto Attack DPS: `((4 + 10.52) * 65 AD) / 6.454s * 1.28 Crit = 187.3`
    *   Spell Damage (1 Splash): `10.52 attacks * (0.60 * 65 AD) * 1.28 Crit = 525.4`
    *   Spell DPS (1 Splash): `525.4 / 6.454s = 81.4`
    *   Total DPS (1 Splash): `187.3 + 81.4 = 268.7`
*   **2★ Tristana (Equipped)**:
    *   *Note: Tristana's rotation consists of 4 basic attacks to gain mana (4 / 1.63 AS = 2.454s) plus her 4.0-second active AS steroid (total cycle = 6.454s). During the 4.0s steroid, she gains +1.0 AS, resulting in 2.63 AS. The number of attacks during the steroid is 4.0s * 2.63 AS = 10.52 attacks.*
    *   Auto Attack DPS: `((4 + 10.52) * 99 AD) / 6.454s * 1.28 Crit = 282.1`
    *   Spell Damage (1 Splash): `10.52 attacks * (0.60 * 99 AD) * 1.28 Crit = 525.4`
    *   Spell DPS (1 Splash): `525.4 / 6.454s = 122.7`
    *   Total DPS (1 Splash): `282.1 + 122.7 = 404.8`
*   **3★ Tristana (Equipped)**:
    *   *Note: Tristana's rotation consists of 4 basic attacks to gain mana (4 / 1.63 AS = 2.454s) plus her 4.0-second active AS steroid (total cycle = 6.454s). During the 4.0s steroid, she gains +1.0 AS, resulting in 2.63 AS. The number of attacks during the steroid is 4.0s * 2.63 AS = 10.52 attacks.*
    *   Auto Attack DPS: `((4 + 10.52) * 146 AD) / 6.454s * 1.28 Crit = 420.3`
    *   Spell Damage (1 Splash): `10.52 attacks * (0.60 * 146 AD) * 1.28 Crit = 525.4`
    *   Spell DPS (1 Splash): `525.4 / 6.454s = 182.8`
    *   Total DPS (1 Splash): `420.3 + 182.8 = 603.1`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: 4.72 * (0.60 * ad) * 1.0` (unequipped physical splash damage during steroid).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: 10.52 * (0.60 * ad) * crit * 1.0` (scales explosion physical damage with crit).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Accounts for cycle timing and active steroid attack scaling.
*   **Stats & Cycle Keys**:
    *   `as`: `0.70`
    *   `eq_as`: `1.63` (models Guinsoo stacking + active AS steroid)
    *   `lockout`: `0.0` seconds
