# [Champion Name] DPS Calculation Details [Emoji]

This document provides the step-by-step mathematical calculations for [Champion Name]'s baseline (unequipped) and well-equipped (3-item) DPS across 1★, 2★, and 3★ star levels.

> **Source of truth**: All base stats are sourced from the **TFT Set 9 Basic data** spreadsheet.
> DPS is calculated from first principles using the formulas below. No external script output is used.

---

## ⚙️ Core Variables & Mechanics

### 1. Base Stats (from TFT Set 9 Basic data)

**Scaling stats**
| Stat | 1★ | 2★ | 3★ |
| :--- | :---: | :---: | :---: |
| **Base AD** | [Value] | [Value] | [Value] |
| **Base Spell Damage** | [Value] | [Value] | [Value] |

**Fixed stats** *(do not scale with star level)*
| Stat | Value |
| :--- | :---: |
| **Attack Speed (AS)** | [Value] |
| **Max Mana** | [Value] |
| **Cast Lockout** | [Value]s |

### 3. Skill Description & Mechanics
*   **Skill**: [Brief description of the skill behavior].
*   **[Unique Mechanic]**: [Explain any target density, damage splits, wound multipliers, or duration refreshes here].

---

## 🧮 Baseline (Unequipped) Calculations

### Calculations

| Step | Formula | Calculation | 1★ | 2★ | 3★ |
| :--- | :--- | :--- | :---: | :---: | :---: |
| ATC | `ceil(Max Mana / 10)` | `ceil([Max] / 10)` | [Val] | [Val] | [Val] |
| Cycle Duration | `ATC / AS + Lockout` | `[ATC] / [AS] + [Lockout]` | [Val]s | [Val]s | [Val]s |
| Auto Attack DPS | `(ATC × AD) / Cycle` | `([ATC] × [Base AD Array]) / [Cycle]s` | [Val] | [Val] | [Val] |
| Spell Base (1 Target) | `[Derivation if AD-scaling, otherwise flat Spell]` | `[Derivation if AD-scaling, otherwise flat Spell]` | [Val] | [Val] | [Val] |
| Spell Damage | `Spell Base × [Modifiers]` | `[Spell Base Array] × [Modifiers]` | [Val] | [Val] | [Val] |
| Spell DPS | `Spell Damage / Cycle` | `[Spell Damage Array] / [Cycle]s` | [Val] | [Val] | [Val] |
| **Total DPS** | `Auto DPS + Spell DPS` | `Auto DPS + Spell DPS` | **[Val]** | **[Val]** | **[Val]** |

---

## 🧮 Equipped Calculations ([Item 1] + [Item 2] + [Item 3])

### 1. Item Stats & Effects
| Item | Effect |
| :--- | :--- |
| **[Item 1]** | [Details of stat buffs or passive effects relevant to DPS] |
| **[Item 2]** | [Details of stat buffs or passive effects relevant to DPS] |
| **[Item 3]** | [Details of stat buffs or passive effects relevant to DPS] |

### 2. Stats & Multipliers

| Stat | Formula | Calculation | 1★ | 2★ | 3★ |
| :--- | :--- | :--- | :---: | :---: | :---: |
| AP Total | `AP_base + AP_items` | `100 + [Item AP]` | [Val] | [Val] | [Val] |
| Crit Chance | `Crit_base + Crit_items` | `25% + [Item Crit]` | [Val] | [Val] | [Val] |
| Crit Damage | `CritDmg_base + CritDmg_items` | `140% + [Item CritDmg]` | [Val] | [Val] | [Val] |
| Crit Multiplier | `1 + Crit Chance × (Crit Damage − 1)` | `1 + [Chance] × [Damage - 1]` | [Val] | [Val] | [Val] |

### 3. DPS Calculations

| Step | Formula | Calculation | 1★ | 2★ | 3★ |
| :--- | :--- | :--- | :---: | :---: | :---: |
| ATC | `ceil((Max Mana − [Mana Reductions]) / [Mana Per Attack])` | `ceil(([Max] − [BB]) / [MPA])` | [Val] | [Val] | [Val] |
| Cycle Duration | `ATC / AS_equipped + Lockout` | `[ATC] / [AS_eq] + [Lockout]` | [Val]s | [Val]s | [Val]s |
| Auto Attack DPS | `(ATC × AD_equipped × Crit) / Cycle` | `([ATC] × [Equipped AD Array] × [Crit]) / [Cycle]s` | [Val] | [Val] | [Val] |
| Spell Base (1 Target) | `[Derivation if AD-scaling, otherwise flat Spell]` | `[Derivation if AD-scaling, otherwise flat Spell]` | [Val] | [Val] | [Val] |
| Spell Damage | `Spell Base × [Mod] × AP × Crit` | `[Spell Base Array] × [Mod] × [AP] × [Crit]` | [Val] | [Val] | [Val] |
| Spell DPS | `Spell Damage / Cycle` | `[Spell Damage Array] / [Cycle]s` | [Val] | [Val] | [Val] |
| **Total DPS** | `Auto DPS + Spell DPS` | `Auto DPS + Spell DPS` | **[Val]** | **[Val]** | **[Val]** |

---

## ⚠️ Script Reference (champion_db.py)

> [!WARNING]
> `champion_db.py` is **not the source of truth** and should not be used to drive calculations. Stats in that file may drift from the sheet. The calculations above are authoritative.
>
> The script file is retained for historical reference only. See the header comment in [champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py) for details.
