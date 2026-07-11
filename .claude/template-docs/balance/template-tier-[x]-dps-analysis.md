# TFT Set 9 Tier [X] Carry DPS Analysis (Unified)

This document provides a unified mathematical breakdown of baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels for all Tier [X] carries.

All calculations are derived from first principles using base stats sourced directly from the **TFT Set 9 Basic data** spreadsheet.

---

## 🗺️ Index & Navigation

*   [Table 1: Baseline (Unequipped) DPS Summary](#table-1-baseline-unequipped-dps-summary)
*   [Table 2: Well-Equipped (3-Item) DPS Summary](#table-2-well-equipped-3-item-dps-summary)
*   **Carries Breakdown**:
    *   [[Champion 1 Name]](#1-champion-1-name)
    *   [[Champion 2 Name]](#2-champion-2-name)
*   **Formula Reference**: [Glossary & Formula Index](#-glossary--formula-index)

---

## 📊 Summary Tables

### Table 1: Baseline (Unequipped) DPS Summary
*Consolidated comparison across 1★, 2★, and 3★ star levels with NO items equipped (applicable for both 15s and 30s fights unless specified).*

| Champion | Hero Archetype | Duration | Star | Base AD | AS | Auto Attack DPS | Spell DPS | Total DPS |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **[Champ 1]** | [Archetype, target density / range details] | Not specify | 1★<br>2★<br>3★ | [Val]<br>[Val]<br>[Val] | [Val] | [Val]<br>[Val]<br>[Val] | [Val]<br>[Val]<br>[Val] | **[Val]**<br>**[Val]**<br>**[Val]** |
| **[Champ 2]** | [Archetype, target density / range details] | Not specify | 1★<br>2★<br>3★ | [Val]<br>[Val]<br>[Val] | [Val] | [Val]<br>[Val]<br>[Val] | [Val]<br>[Val]<br>[Val] | **[Val]**<br>**[Val]**<br>**[Val]** |

### Table 2: Well-Equipped (3-Item) DPS Summary
*Consolidated comparison across 1★, 2★, and 3★ star levels with 3 core optimal items equipped (applicable for both 15s and 30s fights unless specified).*

| Champion | Hero Archetype | Duration | Star | AD | AS | Auto Attack DPS | Spell DPS | Total DPS |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **[Champ 1]** (Standard) | [Archetype, target density / range details] | Not specify | 1★<br>2★<br>3★ | [Val]<br>[Val]<br>[Val] | [Val] | [Val]<br>[Val]<br>[Val] | [Val]<br>[Val]<br>[Val] | **[Val]**<br>**[Val]**<br>**[Val]** |
| **[Champ 2]** (Standard) | [Archetype, target density / range details] | Not specify | 1★<br>2★<br>3★ | [Val]<br>[Val]<br>[Val] | [Val] | [Val]<br>[Val]<br>[Val] | [Val]<br>[Val]<br>[Val] | **[Val]**<br>**[Val]**<br>**[Val]** |

---

## 👥 Champion Carries Breakdown

### 1. [Champion 1 Name] [Emoji]

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: [Ability name and brief description of combat scaling]
*   **Mana Rules**: [Attacks required to cast, starting mana, and mana locking behavior]
*   **Cast Lockout**: [Cast animation lockout duration in seconds]
*   **Attacks to Cast**: [ATC value]

#### 🧮 DPS Output Summary
*   **Baseline (Unequipped)**:
    *   15s / 30s Fight: 1★: **[Val]** | 2★: **[Val]** | 3★: **[Val]**
*   **Well-Equipped ([Item 1] + [Item 2] + [Item 3])**:
    *   15s / 30s Fight: 1★: **[Val]** | 2★: **[Val]** | 3★: **[Val]**
*   **Detailed DPS Calculations**: [[Champion 1] DPS Calculations](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/calculations/[champion-1-kebab]-dps-calculation.md)

#### 🔍 Analyze DPS
*   **[Key Insight 1]**: [Details about item scaling or comp interactions]
*   **[Key Insight 2]**: [Details about combat duration ramping, positioning, or tier power level]

---

### 2. [Champion 2 Name] [Emoji]

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: [Ability name and brief description of combat scaling]
*   **Mana Rules**: [Attacks required to cast, starting mana, and mana locking behavior]
*   **Cast Lockout**: [Cast animation lockout duration in seconds]
*   **Attacks to Cast**: [ATC value]

#### 🧮 DPS Output Summary
*   **Baseline (Unequipped)**:
    *   15s / 30s Fight: 1★: **[Val]** | 2★: **[Val]** | 3★: **[Val]**
*   **Well-Equipped ([Item 1] + [Item 2] + [Item 3])**:
    *   15s / 30s Fight: 1★: **[Val]** | 2★: **[Val]** | 3★: **[Val]**
*   **Detailed DPS Calculations**: [[Champion 2] DPS Calculations](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/calculations/[champion-2-kebab]-dps-calculation.md)

#### 🔍 Analyze DPS
*   **[Key Insight 1]**: [Details about item scaling or comp interactions]
*   **[Key Insight 2]**: [Details about combat duration ramping, positioning, or tier power level]

---

## 📖 Glossary & Formula Index

*   **AD (Attack Damage)**: The physical damage dealt by a basic auto-attack. AD scales by 1.5x per star level (2★ = 1.5x, 3★ = 2.25x).
*   **AS (Attack Speed)**: The rate of basic attacks per second. Attack Speed remains flat across star levels.
*   **AP (Ability Power)**: The scaling multiplier applied to base spell damage (base is 100 AP / 1.0x).
*   **Attacks to Cast**: The number of basic attacks required to generate the mana pool needed for a cast (each attack generates 10 mana by default, except with Shojin/Blue Buff).
*   **Cast Lockout (s)**: The duration spent executing the spell cast animation, during which basic attacks are paused. Lockout time is fixed and does not scale with AS:
    
    `Cycle Duration = (Attacks to Cast) / (AS) + Cast Lockout + [Steroid Uptime, if mana locked]`

*   **Auto Attack DPS**: The average damage per second contributed strictly by basic auto-attacks over a cycle:
    
    `Auto Attack DPS = (Total Attacks * AD) / (Cycle Duration) * Crit * Amp`

*   **Spell Base**: The unmitigated baseline damage of the spell before accounting for multi-target averages, piercing, or physical scaling ratios.
*   **Spell Damage**: The final output damage of the spell per cycle, applying target averages and modifiers:
    
    `Spell Damage = Spell Base * AP_mult * Target_density * [Splash/Pierce Multipliers] * Crit * Amp`

*   **Spell DPS**: The average damage per second contributed strictly by spell casts over a cycle:
    
    `Spell DPS = Spell Damage / Cycle Duration`

*   **Total DPS**: The unified DPS of the champion combining basic attacks and spell outputs:
    
    `Total DPS = Auto Attack DPS + Spell DPS`

*   **Crit (Critical Strike Multiplier)**: The scaling factor applied to damage to account for crit rate and damage:
    
    `Crit = 1 + Crit Chance * (Crit Damage - 1)`

*   **Amp (Amplification Multiplier)**: The flat damage amplification factor contributed by item passives (e.g. Giant Slayer):
    
    `Amp = Amp_1 * Amp_2 * ...`
