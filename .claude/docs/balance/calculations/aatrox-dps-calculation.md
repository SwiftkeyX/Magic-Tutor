# Aatrox DPS Calculation Details 😈

This document provides the step-by-step mathematical calculations and formulas for Aatrox's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.80 (constant across star levels)
*   **Attack Damage (AD)**: 80 / 120 / 180
*   **Spell Magic Damage (Base)**: AD Ratio: 0.80 / 0.80 / 0.80

### 2. Skill Description & Aatrox Mechanics
*   **Skill Description**: Transforms for a duration, convert AS to AD. Attacks deal physical damage in an area. Splash Assumptions: Assuming attacks hit an average of 2 targets total (1 main + 1 adjacent target), scaling auto-attack damage multiplier to 1.80x (normal + 80% splash).
*   **Mechanical Timing & Assumptions**:
    *   spell_base is [0, 0, 0] since there is no flat spell damage, and spell_ad_ratio is [0.80, 0.80, 0.80] (splash AD ratio).
*   **Mana & Casting**:
    *   Max Mana: 50
    *   Start Mana: 0
    *   **Attacks to Cast**: 5 attacks
    *   **Cast Lockout**: 1.0 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = 7.500 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (5 * AD) / 7.500s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 7.500s`

*   **1★ Aatrox**:
    *   Auto Attack DPS: `(5 Attacks * 80 AD) / 7.500s = 53.3`
    *   Spell Damage: `(5.0 attacks * 80 AD) * 0.80 Splash = 320.0`
    *   Spell DPS: `320.0 / 7.500s = 42.7`
    *   Total DPS: `53.3 + 42.7 = 96.0`
*   **2★ Aatrox**:
    *   Auto Attack DPS: `(5 Attacks * 120 AD) / 7.500s = 80.0`
    *   Spell Damage: `(5.0 attacks * 120 AD) * 0.80 Splash = 480.0`
    *   Spell DPS: `480.0 / 7.500s = 64.0`
    *   Total DPS: `80.0 + 64.0 = 144.0`
*   **3★ Aatrox**:
    *   Auto Attack DPS: `(5 Attacks * 180 AD) / 7.500s = 120.0`
    *   Spell Damage: `(5.0 attacks * 180 AD) * 0.80 Splash = 720.0`
    *   Spell DPS: `720.0 / 7.500s = 96.0`
    *   Total DPS: `120.0 + 96.0 = 216.0`


---

## 🧮 Equipped Calculations (Deathblade + Infinity Edge + Bloodthirster)

### 1. Item Stats
*   **Total Equipped AD Modifier**: +121% AD (2.21* multiplier)
*   **Total Equipped AP Modifier**: 100 AP (1.00* spell multiplier)
*   **Crit Stats**: 60% Crit Chance, 140% Crit Damage.
    *   **Average Crit Multiplier**:
        
 `Crit_equipped = 1 + Crit Chance * (Crit Damage - 1) = 1.24*`

### 2. Cycle & Auto Attack DPS
*   **Cycle Duration**:
    
 `Cycle Duration = 7.500 seconds (adjusted for combat timing)`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (5 * AD_equipped) / 7.500s * 1.24 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 7.500s`

#### Well-Equipped DPS Summary (Average 30s)
*   **1★ Aatrox (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 177 AD) / 7.500s * 1.24 Crit = 146.3`
    *   Spell Damage: `(5.0 attacks * 177 AD) * 0.80 Splash * 1.24 Crit = 878.2`
    *   Spell DPS: `878.2 / 7.500s = 117.1`
    *   Total DPS: `146.3 + 117.1 = 263.4`
*   **2★ Aatrox (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 265 AD) / 7.500s * 1.24 Crit = 219.5`
    *   Spell Damage: `(5.0 attacks * 265 AD) * 0.80 Splash * 1.24 Crit = 1317.8`
    *   Spell DPS: `1317.8 / 7.500s = 175.7`
    *   Total DPS: `219.5 + 175.7 = 395.2`
*   **3★ Aatrox (Equipped)**:
    *   Auto Attack DPS: `(5 Attacks * 398 AD) / 7.500s * 1.24 Crit = 329.2`
    *   Spell Damage: `(5.0 attacks * 398 AD) * 0.80 Splash * 1.24 Crit = 1976.2`
    *   Spell DPS: `1976.2 / 7.500s = 263.5`
    *   Total DPS: `329.2 + 263.5 = 592.7`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: (5.0 * ad) * 0.80` (representing 5 transform-boosted attacks dealing 80% splash).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: (5.0 * ad) * 0.80 * crit` (adds equipped crit scaling to the transform slashes).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Used to bypass the generic math and return the exact unequipped/equipped calculations representing 100% active transform uptime.
*   **Stats & Cycle Keys**:
    *   `as`: `0.80`
    *   `eq_as`: `0.80`
    *   `base_attacks`: `5`
    *   `base_cycle` / `eq_cycle`: `7.50` seconds
    *   `lockout`: `1.0` seconds
