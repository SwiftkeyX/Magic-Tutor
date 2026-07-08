# Aphelios DPS Calculation Details 🌙

This document provides the step-by-step mathematical calculations and formulas for Aphelios's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.75 (constant across star levels)
*   **Attack Damage (AD)**: 65 / 98 / 146
*   **Spell Magic Damage (Base)**: AD Ratio: 2.00 / 2.00 / 2.50

### 2. Skill Description & Aphelios Mechanics
*   **Skill Description**: Fires a blast dealing physical damage: AD * 2.00 / 2.00 / 2.50 in a 2-hex area. Equips Chakrams (+3 base, +1 per enemy hit). Each Chakram adds +8% AD bonus physical damage on-hit, totaling +48% AD scaling bonus per attack.
*   **Mechanical Timing & Assumptions**:
    *   spell_base is [0, 0, 0] and spell_ad_ratio is [2.00, 2.00, 2.50]. Overrides incorporate complex stacking Chakram damage.
*   **Mana & Casting**:
    *   Max Mana: 100
    *   Start Mana: 50
    *   **Attacks to Cast**: 5 attacks
    *   **Cast Lockout**: 1.0 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = 14.670 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (5 * AD) / 14.670s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 14.670s`

*   **1★ Aphelios**:
    *   Auto Attack DPS: `(10 Attacks * 65 AD) / 14.670s = 44.3`
    *   Spell Damage: `65 AD * (2.00 blast * 3 Targets + 5.25 Chakrams * 0.48) = 553.8`
    *   Spell DPS: `553.8 / 14.670s = 37.8`
    *   Total DPS: `44.3 + 37.8 = 82.1`
*   **2★ Aphelios**:
    *   Auto Attack DPS: `(10 Attacks * 98 AD) / 14.670s = 66.8`
    *   Spell Damage: `98 AD * (2.00 blast * 3 Targets + 5.25 Chakrams * 0.48) = 835.0`
    *   Spell DPS: `835.0 / 14.670s = 56.9`
    *   Total DPS: `66.8 + 56.9 = 123.7`
*   **3★ Aphelios**:
    *   Auto Attack DPS: `(10 Attacks * 146 AD) / 14.670s = 99.5`
    *   Spell Damage: `146 AD * (2.50 blast * 3 Targets + 5.25 Chakrams * 0.48) = 1462.9`
    *   Spell DPS: `1462.9 / 14.670s = 99.7`
    *   Total DPS: `99.5 + 99.7 = 199.2`


---

## 🧮 Equipped Calculations (Guinsoo's Rageblade + Deathblade + Infinity Edge)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +100% AD (2.00* multiplier)
*   **Total Equipped AP Modifier**: 100 AP (1.00* spell multiplier)
*   **Crit Stats**: 60% Crit Chance, 140% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.24*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = 7.860 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (5 * AD_equipped) / 7.860s * 1.24 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 7.860s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Aphelios (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 130 AD) / 7.860s * 1.24 Crit = 198.5`
    *   Spell Damage: `130 AD * (2.00 blast * 3 Targets + 9.80 Chakrams * 0.48) * 1.24 Crit = 1669.5`
    *   Spell DPS: `1669.5 / 7.860s = 212.4`
    *   Total DPS: `198.5 + 212.4 = 410.9`
*   **2★ Aphelios (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 196 AD) / 7.860s * 1.24 Crit = 300.8`
    *   Spell Damage: `196 AD * (2.00 blast * 3 Targets + 9.80 Chakrams * 0.48) * 1.24 Crit = 2529.3`
    *   Spell DPS: `2529.3 / 7.860s = 321.8`
    *   Total DPS: `300.8 + 321.8 = 622.6`
*   **3★ Aphelios (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 292 AD) / 7.860s * 1.24 Crit = 447.3`
    *   Spell Damage: `292 AD * (2.50 blast * 3 Targets + 9.80 Chakrams * 0.48) * 1.24 Crit = 4289.2`
    *   Spell DPS: `4289.2 / 7.860s = 545.7`
    *   Total DPS: `447.3 + 545.7 = 993.0`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: ad * ((2.50 if idx==2 else 2.00) * 3.0 + 5.25 * 0.48)` (models area blast plus baseline Chakram stack physical damage).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: ad * ((2.50 if idx==2 else 2.00) * 3.0 + 9.8 * 0.48) * crit` (models AP area blast plus Guinsoo/crit-scaled Chakrams).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Utilized to account for complex dynamic ramping of Chakram auto-attack damage.
*   **Stats & Cycle Keys**:
    *   `as`: `0.75`
    *   `eq_as`: `1.40` (represents average AS over 30s fight with Guinsoo stacking)
    *   `eq_ad_mult`: `2.00` (Deathblade + Infinity Edge AD bonus multiplier)
    *   `base_cycle`: `14.67` seconds
    *   `eq_cycle`: `7.86` seconds
    *   `lockout`: `1.0` seconds
