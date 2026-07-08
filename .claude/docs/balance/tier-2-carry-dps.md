# TFT Set 9 Tier 2 Carry DPS Analysis (Unified)

This document provides a unified mathematical breakdown of baseline (unequipped) and well-equipped (3-item) DPS scaling for all 2-Gold carry champions in Teamfight Tactics (Set 9).

Putting baseline and itemized calculations in a single document guarantees that both calculations utilize identical, up-to-date mechanical assumptions (AS-scaling lockouts, targeting splits, and cast animation frames).

---

## 📊 Master DPS Summary Tables

### 📘 Table 1: Baseline (Unequipped) DPS Summary
*Consolidated comparison across 1★, 2★, and 3★ star levels with NO items equipped.*

| Champion | Star | Base AD | AS | Auto Attack DPS | Spell DPS | Total DPS |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Jinx** (5 Rockets Sum) | 1★<br>2★<br>3★ | 50<br>75<br>113 | 0.75 | 33.9<br>50.9<br>76.3 | 43.5<br>65.2<br>97.9 | **77.4**<br>**116.1**<br>**174.2** |
| **Teemo** (1.33 Targets Avg) | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 25.2<br>37.8<br>56.7 | 43.5<br>65.3<br>98.0 | **68.7**<br>**103.1**<br>**154.7** <br>*(1 Target Baseline: **57.9** / **86.9** / **130.3**)* |
| **Taliyah** (2 Passive Boulders) | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 28.0<br>42.0<br>63.0 | 67.7<br>101.6<br>152.3 | **95.7**<br>**143.6**<br>**215.3** <br>*(1 Target Baseline: **61.9** / **92.8** / **139.2**)* |
| **Zed** (Slash x3, 2 Targets Avg) | 1★<br>2★<br>3★ | 55<br>83<br>124 | 0.70 | 35.6<br>53.4<br>80.1 | 33.5<br>50.3<br>75.1 | **69.1**<br>**103.7**<br>**155.2** <br>*(1 Target Baseline: **54.5** / **82.1** / **122.9**)* |

### ⚔️ Table 2: Well-Equipped (3-Item) DPS Summary
*Consolidated comparison across 1★, 2★, and 3★ star levels with 3 core optimal items equipped. Shows time-averaged stats and DPS (no ramping slashes).*

| Champion | Star | AD | AS | Auto Attack DPS | Spell DPS | Total DPS |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Jinx** (5 Rockets Sum) | 1★<br>2★<br>3★ | 98<br>147<br>221 | 0.90 | 95.4<br>143.1<br>214.6 | 140.5<br>210.8<br>316.1 | **235.9**<br>**353.9**<br>**530.7** |
| **Teemo** (1.33 Targets Avg) | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 29.9<br>44.8<br>67.2 | 129.0<br>193.6<br>290.3 | **158.9**<br>**238.4**<br>**357.5** |
| **Taliyah** (2 Passive Boulders) | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 31.6<br>47.4<br>71.1 | 130.3<br>195.5<br>293.2 | **161.9**<br>**242.9**<br>**364.3** |
| **Zed** (Slash x3, 2 Targets Avg) | 1★<br>2★<br>3★ | 122<br>183<br>275 | 0.70 | 56.8<br>85.3<br>127.9 | 121.9<br>182.9<br>274.3 | **178.7**<br>**268.2**<br>**402.2** |

---

## 📖 Glossary & Formula Index

*   **AD (Attack Damage)**: The physical damage dealt by a basic auto-attack. AD scales by 1.5x per star level (2★ = 1.5x, 3★ = 2.25x).
*   **AS (Attack Speed)**: The rate of basic attacks per second. Attack Speed remains flat across star levels.
*   **AP (Ability Power)**: The scaling multiplier applied to base spell damage (base is 100 AP / 1.0x).
*   **Attacks to Cast**: The number of basic attacks required to generate the mana pool needed for a cast (each attack generates 10 mana by default, except with Shojin).
*   **Cast Lockout (s)**: The duration spent executing the spell cast animation, during which basic attacks are paused. **Lockout time scales with Attack Speed**, modeled as:
    
 `Lockout(AS) = (Base Lockout) / (AS)`

*   **Cycle Duration (s)**: The total time of one full combat loop:
    
 `Cycle Duration = (Attacks to Cast + Base Lockout) / (AS)`

*   **Auto Attack DPS**: The average damage per second contributed strictly by basic auto-attacks over a cycle:
    
 `Auto Attack DPS = (Attacks to Cast * AD) / (Cycle Duration) * Crit * Amp`

*   **Spell DPS**: The average damage per second contributed strictly by the spell over a cycle:
    
 `Spell DPS = (Spell Damage) / (Cycle Duration) * Crit * Amp`

*   **Total DPS**: The combined damage output over a cycle:
    
 `Total DPS = Auto Attack DPS + Spell DPS`

*   **Crit Multiplier (Crit)**: The average multiplier applied by Critical strike chance and damage:
    
 `Average Crit Multiplier = 1 + Crit Chance * (Crit Damage - 1)`

    *   *Baseline (No Crit Items)*: 25% Crit Chance, 140% Crit Damage (`1.10x` multiplier) — TFT base crit damage is 140%.
    *   *Jeweled Gauntlet (JG)*: 40% Crit Chance, 170% Crit Damage (`1.28x` multiplier). Spells can crit.
    *   *Infinity Edge (IE)*: 60% Crit Chance, 140% Crit Damage (`1.24x` multiplier). Spells can crit.
    *   *Last Whisper only (Jinx build)*: 35% Crit Chance, 140% Crit Damage (`1.14x` multiplier).
    *   *Modeling note*: as a simplification, these docs apply the average crit multiplier to spell damage in baseline rows too (strictly, TFT abilities only crit with JG/IE equipped).
*   **AP vs. AD-ratio spells**: AP multiplies a spell's flat/base damage component only (e.g. Jinx's 15/20/35 rocket flat, Zed's 25/40/50 slash flat). Pure AD-ratio components do not scale with AP — matching TFT's per-star ability data.

---

## 🔍 Detailed Champion Breakdowns

### 1. Jinx 🚀
*   **Detailed Math & Formula Proofs**: See [jinx-dps-calculation.md](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/calculations/jinx-dps-calculation.md)
*   **Combat Role & Mechanics**: A semi-AoE cleanup carry. Fires 5 rockets at random enemies within a 2-hex radius of her target, split-targeting physical damage.
*   **Key DPS Outputs**:
    *   *Baseline (Unequipped) Total DPS*: **77.4 / 116.1 / 174.6**
    *   *Well-Equipped (3-Item) Total DPS*: **235.9 / 356.9 / 545.7**
*   **Aesthetic Balance Note**: Jinx represents the benchmark for Tier 2 backline splash damage, with output scaling safely above Tier 1 carries due to higher baseline ratios.

---

### 2. Teemo 🍄
*   **Detailed Math & Formula Proofs**: See [teemo-dps-calculation.md](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/calculations/teemo-dps-calculation.md)
*   **Combat Role & Mechanics**: An anti-healing AoE magic damage burner. Poison ticks over 3 seconds in a circular blast area, scaling linearly with enemy density.
*   **Key DPS Outputs**:
    *   *Baseline (Unequipped) Total DPS (1 Target)*: **72.4 / 108.6 / 162.9** (2 Targets Avg: **100.8 / 151.1 / 226.6**)
    *   *Well-Equipped (3-Item) Total DPS (2 Targets Avg)*: **224.0 / 336.0 / 504.0**
*   **Aesthetic Balance Note**: While Teemo displays very high theoretical DPS numbers, his damage is damage-over-time (DoT) which allows healing counterplay, preventing burst-focused dominance.

---

### 3. Taliyah 🪨
*   **Detailed Math & Formula Proofs**: See [taliyah-dps-calculation.md](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/calculations/taliyah-dps-calculation.md)
*   **Combat Role & Mechanics**: A synergistic combo carry. Active stuns/knocks up target; passive launches boulders whenever *any* enemy is knocked up by *any* source (e.g. Sett, Vi, Poppy).
*   **Key DPS Outputs**:
    *   *Baseline (Unequipped) Total DPS (1 Target)*: **61.9 / 92.8 / 139.2** (Synergy 2 Boulders: **95.7 / 143.6 / 215.3**)
    *   *Well-Equipped (3-Item) Total DPS (Synergy 2 Boulders)*: **161.9 / 242.9 / 364.3**
*   **Aesthetic Balance Note**: Taliyah's standalone baseline output is low, but she scales exponentially when paired with knockup synergies, presenting a highly draft-dependent design.

---

### 4. Zed 🥷
*   **Detailed Math & Formula Proofs**: See [zed-dps-calculation.md](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/calculations/zed-dps-calculation.md)
*   **Combat Role & Mechanics**: Melee physical assassin with high critical strike scaling. Spawns shadow clone and double-slashes adjacent targets.
*   **Key DPS Outputs**:
    *   *Baseline (Unequipped) Total DPS (1 Target)*: **54.5 / 82.1 / 122.9** (2 Targets Avg: **69.1 / 103.7 / 155.2**)
    *   *Well-Equipped (3-Item) Total DPS (2 Targets Avg)*: **178.7 / 269.1 / 410.1**
*   **Aesthetic Balance Note**: Zed is balanced strictly below Jinx in multi-target conditions due to his access to backline infiltration, trading raw AoE output for direct carry access.
