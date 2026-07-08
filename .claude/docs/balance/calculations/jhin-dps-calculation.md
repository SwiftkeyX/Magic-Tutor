# Jhin DPS Calculation Details 🎻

This document provides the step-by-step mathematical calculations and formulas for Jhin's baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

---

## ⚙️ Core Variables & Mechanics

### 1. Stats & Scaling
*   **Attack Speed (AS)**: 0.70 (constant across star levels)
*   **Attack Damage (AD)**: 54 / 81 / 122
*   **Spell Magic Damage (Base)**: AD Ratio: 3.00 / 3.00 / 3.44 + Flat: 60 / 90 / 135

### 2. Skill Description & Jhin Mechanics
*   **Skill Description**: Take aim at the current target and deal 300%/300%/344% AD + 60/90/135 physical damage to enemies in a line; each hit reduces this damage by 56% (or deals 44% of the previous hit's damage). Ionia Bonus: +25% Attack Damage.
*   **Mechanical Timing & Assumptions**:
    *   spell_base represents the flat base damage ([60, 90, 135]) and spell_ad_ratio represents the AD scaling (3.00x/3.00x/3.44x). target_density is set to 2.0 (number of targets hit), and pierce_falloff is set to 0.56. Overrides represent the exact hybrid math.
*   **Mana & Casting**:
    *   Max Mana: 120
    *   Start Mana: 0
    *   **Attacks to Cast**: 12 attacks
    *   **Cast Lockout**: 1.6 seconds

---

## 🧮 Baseline (Unequipped) Calculations

### 1. Formula Definitions
*   **Cycle Duration**:
    
 `Cycle Duration = (Attacks to Cast) / (AS) + Base Lockout = (12) / (0.70) + 1.6 = 18.743 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (12 * AD) / 18.743s`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 18.743s`

*   **1★ Jhin**:
    *   Auto Attack DPS: `(12 Attacks * 54 AD) / 18.743s = 34.6`
    *   Spell Damage: `(54 AD * 3.00 + 60) * 1.44 Pierce = 665.4`
    *   Spell DPS: `665.4 / 18.743s = 35.5`
    *   Total DPS: `34.6 + 35.5 = 70.1`
*   **2★ Jhin**:
    *   Auto Attack DPS: `(12 Attacks * 81 AD) / 18.743s = 51.9`
    *   Spell Damage: `(81 AD * 3.00 + 90) * 1.44 Pierce = 997.1`
    *   Spell DPS: `997.1 / 18.743s = 53.2`
    *   Total DPS: `51.9 + 53.2 = 105.1`
*   **3★ Jhin**:
    *   Auto Attack DPS: `(12 Attacks * 122 AD) / 18.743s = 78.1`
    *   Spell Damage: `(122 AD * 3.44 + 135) * 1.44 Pierce = 1501.3`
    *   Spell DPS: `1501.3 / 18.743s = 80.1`
    *   Total DPS: `78.1 + 80.1 = 158.2`


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
    
 `Cycle Duration = (Attacks to Cast) / (AS_equipped) + Base Lockout = (12) / (0.77) + 1.6 = 17.184 seconds`

*   **Auto Attack DPS**:
    
 `Auto Attack DPS = (12 * AD_equipped) / 17.184s * 1.28 Crit`

### 3. Spell Damage & DPS
*   **Spell Damage**:
    
 `Spell Damage = Equipped Spell Damage Formula`

*   **Spell DPS**:
    
 `Spell DPS = Spell Damage / 17.184s`

#### 2 Targets Avg (1.44x - Typical)
*   **1★ Jhin (Equipped)**:
    *   Auto Attack DPS: `(12 Attacks * 114 AD) / 17.184s * 1.28 Crit = 99.1`
    *   Spell Damage: `(114 AD * 3.00 + 60) * 1.44 Pierce * 1.28 Crit = 1629.1`
    *   Spell DPS: `1629.1 / 17.184s = 94.8`
    *   Total DPS: `99.1 + 94.8 = 193.9`
*   **2★ Jhin (Equipped)**:
    *   Auto Attack DPS: `(12 Attacks * 171 AD) / 17.184s * 1.28 Crit = 148.7`
    *   Spell Damage: `(171 AD * 3.00 + 90) * 1.44 Pierce * 1.28 Crit = 2441.9`
    *   Spell DPS: `2441.9 / 17.184s = 142.1`
    *   Total DPS: `148.7 + 142.1 = 290.8`
*   **3★ Jhin (Equipped)**:
    *   Auto Attack DPS: `(12 Attacks * 257 AD) / 17.184s * 1.28 Crit = 223.1`
    *   Spell Damage: `(257 AD * 3.44 + 135) * 1.44 Pierce * 1.28 Crit = 3663.7`
    *   Spell DPS: `3663.7 / 17.184s = 213.2`
    *   Total DPS: `223.1 + 213.2 = 436.3`


## 💻 Script Correlation

In the Python DPS script ([champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py)):
*   **Base Spell Damage Formula** (`base_spell`): `lambda ad, spell, idx: ad * spell[idx] * 1.44` (multiplies AD-ratio by 1.44 target density).
*   **Equipped Spell Damage Formula** (`eq_spell`): `lambda ad, spell, idx, ap, crit, amp: ad * spell[idx] * 1.44 * crit` (scales spell damage by crit multiplier).
*   **Baseline / Equipped Overrides** (`baseline_override` / `equipped_override`): Forces output to match calculations with a 1.6s cast lockout.
*   **Stats & Cycle Keys**:
    *   `as`: `0.70`
    *   `base_cycle`: `18.74` seconds (computed as 12 / 0.70 + 1.6)
    *   `eq_cycle`: `17.66` seconds (historical sync override)
    *   `lockout`: `1.6` seconds

---

## 🔍 Alternative Equipped Comparison (Guinsoo's Rageblade)

This alternative configuration evaluates Jhin with **Guinsoo's Rageblade + Deathblade + Infinity Edge**. Guinsoo's Rageblade stacks Attack Speed by 6% per attack. Over a 30-second fight, this ramps Jhin's average Attack Speed to **1.10 AS**.

### 1. Stats & Cycle Timing
*   **Total Equipped AD Modifier**: +101% AD (2.01* multiplier)
*   **Total Equipped AP Modifier**: 100 AP (1.00* spell multiplier)
*   **Average AS**: 1.10 AS
*   **Crit Stats**: 60% Crit Chance, 140% Crit Damage (Crit Multiplier: `1.24*`)
*   **Cycle Duration**:
    
    `Cycle Duration = (Attacks to Cast) / (AS_avg) + Base Lockout = (12) / (1.10) + 1.6 = 12.509 seconds`

### 2. Auto Attack DPS
*   `Auto Attack DPS = (12 * AD_equipped) / 12.509s * 1.24 Crit`
    *   **1★ Jhin (151 AD)**: `(12 * 151) / 12.509 * 1.24 = 179.6`
    *   **2★ Jhin (227 AD)**: `(12 * 227) / 12.509 * 1.24 = 270.0`
    *   **3★ Jhin (340 AD)**: `(12 * 340) / 12.509 * 1.24 = 404.4`

### 3. Spell Damage & Spell DPS (2 Targets Avg)
*   `Spell Damage = (AD_equipped * AD_ratio + flat) * 1.44 Pierce * 1.24 Crit`
    *   **1★ Jhin**: `(151 * 3.00 + 60) * 1.44 * 1.24 = 916.0` | `Spell DPS = 916.0 / 12.509s = 73.2`
    *   **2★ Jhin**: `(227 * 3.00 + 90) * 1.44 * 1.24 = 1376.7` | `Spell DPS = 1376.7 / 12.509s = 110.1`
    *   **3★ Jhin**: `(340 * 3.44 + 135) * 1.44 * 1.24 = 2329.5` | `Spell DPS = 2329.5 / 12.509s = 186.2`

### 4. Alternative DPS Summary (Guinsoo's build)
*   **1★ Jhin (Equipped)**: Auto Attack DPS: `179.6` | Spell DPS: `73.2` | **Total DPS: 252.8**
*   **2★ Jhin (Equipped)**: Auto Attack DPS: `270.0` | Spell DPS: `110.1` | **Total DPS: 380.1**
*   **3★ Jhin (Equipped)**: Auto Attack DPS: `404.4` | Spell DPS: `186.2` | **Total DPS: 590.6**

> [!NOTE]
> Compared to the optimal utility build (**Deathblade + Infinity Edge + Last Whisper**), the Guinsoo's build increases Jhin's personal DPS by shortening his cycle from `17.66` seconds to `12.51` seconds. However, this comes at the cost of losing Last Whisper's team-wide Armor Shred utility.
