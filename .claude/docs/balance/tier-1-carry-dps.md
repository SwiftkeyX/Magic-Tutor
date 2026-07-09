# TFT Set 9 Tier 1 Carry DPS Analysis (Unified)

This document provides a unified mathematical breakdown of baseline (unequipped) and well-equipped (3-item) DPS scaling for all 1-Gold carry champions in Teamfight Tactics (Set 9). 

## 📊 Master DPS Summary Tables

### 📘 15-Second Combat DPS Tables
*Consolidated comparison across 1★, 2★, and 3★ star levels for a 15-second fight.*

#### Table 1a: Baseline (Unequipped) DPS - 15s Fight
*With NO items equipped.*

| Champion | Hero Archetype | Star | Base AD | AS | Auto Attack DPS | Spell DPS | Total DPS |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Cassiopeia** | AP backliner, single target focus | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 25.1<br>37.6<br>56.4 | 43.5<br>65.2<br>97.8 | **68.6**<br>**102.8**<br>**154.2** |
| **Jhin** | AD backliner, Big pierce AOE (avg to 2 target) | 1★<br>2★<br>3★ | 54<br>81<br>122 | 0.70 | 34.6<br>51.9<br>78.1 | 35.5<br>53.2<br>80.1 | **70.1**<br>**105.1**<br>**158.2** |
| **Malzahar** | AP backliner, 1x3 hex AOE (avg to 1.5 target) | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 25.2<br>37.8<br>56.7 | 41.5<br>62.3<br>94.4 | **66.8**<br>**100.1**<br>**151.1** |
| **Samira** | AD backliner, single-target focus | 1★<br>2★<br>3★ | 45<br>68<br>101 | 0.70 | 26.5<br>40.1<br>59.6 | 16.8<br>25.4<br>39.7 | **43.3**<br>**65.5**<br>**99.3** |
| **Tristana** | AD backliner, splash AOE (avg to 2 targets), single-focus | 1★<br>2★<br>3★ | 45<br>68<br>101 | 0.70 | 40.6<br>61.3<br>91.1 | 13.2<br>20.0<br>29.7 | **53.8**<br>**81.3**<br>**120.8** |
| **Viego** | AP melee carry, single-target focus, stack scaling | 1★<br>2★<br>3★ | 45<br>68<br>101 | 0.75 | 30.1<br>45.5<br>67.6 | 21.4<br>32.1<br>48.6 | **51.5**<br>**77.6**<br>**116.2** |

#### Table 1b: Well-Equipped (3-Item) DPS - 15s Fight
*With 3 optimal core items equipped (ramping effects averaged over 15s).*

| Champion | Hero Archetype | Star | AD | AS | Auto Attack DPS | Spell DPS | Total DPS |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Cassiopeia** (170 AP) | AP backliner, single target focus | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 30.5<br>45.8<br>68.6 | 134.8<br>202.2<br>303.4 | **165.3**<br>**248.0**<br>**372.0** |
| **Jhin** | AD backliner, Big pierce AOE (avg to 2 target) | 1★<br>2★<br>3★ | 114<br>171<br>257 | 0.77 | 99.5<br>149.3<br>224.3 | 87.6<br>131.4<br>197.4 | **187.1**<br>**280.7**<br>**421.7** |
| **Malzahar** | AP backliner, 1x3 hex AOE (avg to 1.5 target) | 1★<br>2★<br>3★ | 46<br>69<br>104 | 0.70 | 36.2<br>54.2<br>81.7 | 139.4<br>209.1<br>316.8 | **175.6**<br>**263.3**<br>**398.5** |
| **Samira** | AD backliner, single-target focus | 1★<br>2★<br>3★ | 65<br>98<br>146 | 0.77 | 47.8<br>72.9<br>107.4 | 45.4<br>69.2<br>107.4 | **93.2**<br>**142.1**<br>**214.8** |
| **Tristana** (1.26 AS) | AD backliner, splash AOE (avg to 2 targets), single-focus | 1★<br>2★<br>3★ | 65<br>98<br>146 | 1.26 | 141.7<br>215.9<br>318.4 | 59.0<br>89.8<br>132.4 | **200.7**<br>**305.7**<br>**450.8** |
| **Viego** (Standard - 0.5 stacks avg) | AP melee carry, single-target focus, stack scaling | 1★<br>2★<br>3★ | 58<br>88<br>131 | 0.75 | 49.7<br>75.4<br>112.3 | 45.3<br>68.0<br>102.9 | **95.0**<br>**143.4**<br>**215.2** |
| **Viego** (RFC × 3 - 1.33 stacks avg) | AP melee carry, single-target focus, stack scaling | 1★<br>2★<br>3★ | 45<br>68<br>101 | 1.49 | 83.8<br>126.6<br>188.1 | 90.5<br>135.7<br>204.6 | **174.3**<br>**262.3**<br>**392.7** |

---

### ⚔️ 30-Second Combat DPS Tables
*Consolidated comparison across 1★, 2★, and 3★ star levels for a 30-second fight.*

#### Table 2a: Baseline (Unequipped) DPS - 30s Fight
*With NO items equipped.*

| Champion | Hero Archetype | Star | Base AD | AS | Auto Attack DPS | Spell DPS | Total DPS |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Cassiopeia** | AP backliner, single target focus | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 25.1<br>37.6<br>56.4 | 43.5<br>65.2<br>97.8 | **68.6**<br>**102.8**<br>**154.2** |
| **Jhin** | AD backliner, Big pierce AOE (avg to 2 target) | 1★<br>2★<br>3★ | 54<br>81<br>122 | 0.70 | 34.6<br>51.9<br>78.1 | 35.5<br>53.2<br>80.1 | **70.1**<br>**105.1**<br>**158.2** |
| **Malzahar** | AP backliner, 1x3 hex AOE (avg to 1.5 target) | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 25.2<br>37.8<br>56.7 | 41.5<br>62.3<br>94.4 | **66.8**<br>**100.1**<br>**151.1** |
| **Samira** | AD backliner, single-target focus | 1★<br>2★<br>3★ | 45<br>68<br>101 | 0.70 | 26.5<br>40.1<br>59.6 | 16.8<br>25.4<br>39.7 | **43.3**<br>**65.5**<br>**99.3** |
| **Tristana** | AD backliner, splash AOE (avg to 2 targets), single-focus | 1★<br>2★<br>3★ | 45<br>68<br>101 | 0.70 | 40.6<br>61.3<br>91.1 | 13.2<br>20.0<br>29.7 | **53.8**<br>**81.3**<br>**120.8** |
| **Viego** | AP melee carry, single-target focus, stack scaling | 1★<br>2★<br>3★ | 45<br>68<br>101 | 0.75 | 30.1<br>45.5<br>67.6 | 34.8<br>52.2<br>78.7 | **64.9**<br>**97.7**<br>**146.3** |

#### Table 2b: Well-Equipped (3-Item) DPS - 30s Fight
*With 3 optimal core items equipped (ramping effects averaged over 30s).*

| Champion | Hero Archetype | Star | AD | AS | Auto Attack DPS | Spell DPS | Total DPS |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Cassiopeia** (200 AP) | AP backliner, single target focus | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 30.5<br>45.8<br>68.6 | 158.6<br>237.9<br>356.9 | **189.1**<br>**283.7**<br>**425.5** |
| **Jhin** | AD backliner, Big pierce AOE (avg to 2 target) | 1★<br>2★<br>3★ | 114<br>171<br>257 | 0.77 | 99.5<br>149.3<br>224.3 | 87.6<br>131.4<br>197.4 | **187.1**<br>**280.7**<br>**421.7** |
| **Malzahar** | AP backliner, 1x3 hex AOE (avg to 1.5 target) | 1★<br>2★<br>3★ | 46<br>69<br>104 | 0.70 | 36.2<br>54.2<br>81.7 | 139.4<br>209.1<br>316.8 | **175.6**<br>**263.3**<br>**398.5** |
| **Samira** | AD backliner, single-target focus | 1★<br>2★<br>3★ | 65<br>98<br>146 | 0.77 | 47.8<br>72.9<br>107.4 | 45.4<br>69.2<br>107.4 | **93.2**<br>**142.1**<br>**214.8** |
| **Tristana** (1.63 AS) | AD backliner, splash AOE (avg to 2 targets), single-focus | 1★<br>2★<br>3★ | 65<br>98<br>146 | 1.63 | 175.5<br>267.3<br>394.2 | 76.3<br>116.3<br>171.4 | **251.8**<br>**383.6**<br>**565.6** |
| **Viego** (Standard - 1.5 stacks avg) | AP melee carry, single-target focus, stack scaling | 1★<br>2★<br>3★ | 58<br>88<br>131 | 0.75 | 49.7<br>75.4<br>112.3 | 62.5<br>93.7<br>141.4 | **112.2**<br>**169.1**<br>**253.7** |
| **Viego** (RFC × 3 - 3.11 stacks avg) | AP melee carry, single-target focus, stack scaling | 1★<br>2★<br>3★ | 45<br>68<br>101 | 1.49 | 83.8<br>126.6<br>188.1 | 156.8<br>235.2<br>353.7 | **240.6**<br>**361.8**<br>**541.8** |

---

## 📖 Glossary & Formula Index

*   **AD (Attack Damage)**: The physical damage dealt by a basic auto-attack. AD scales by 1.5x per star level (2★ = 1.5x, 3★ = 2.25x).
*   **AS (Attack Speed)**: The rate of basic attacks per second. Attack Speed remains flat across star levels.
*   **AP (Ability Power)**: The scaling multiplier applied to base spell damage (base is 100 AP / 1.0x).
*   **Attacks to Cast**: The number of basic attacks required to generate the mana pool needed for a cast (each attack generates 10 mana by default, except with Shojin).
*   **Cast Lockout (s)**: The duration spent executing the spell cast animation, during which basic attacks are paused. **Lockout time is fixed and does not scale with AS**, matching the Google Sheet formulas:
    
 `Cycle Duration = (Attacks to Cast) / (AS) + Cast Lockout`

*   **Auto Attack DPS**: The average damage per second contributed strictly by basic auto-attacks over a cycle:
    
 `Auto Attack DPS = (Attacks to Cast * AD) / (Cycle Duration) * Crit * Amp`

*   **Spell DPS**: The average damage per second contributed strictly by the spell over a cycle:
    
 `Spell DPS = (Spell Damage) / (Cycle Duration) * Crit * Amp`

*   **Total DPS**: The combined damage output over a cycle:
    
 `Total DPS = Auto Attack DPS + Spell DPS`

*   **Crit Multiplier (Crit)**: The average multiplier applied by Critical strike chance and damage:
    
 `Average Crit Multiplier = 1 + Crit Chance * (Crit Damage - 1)`

    *   *Baseline (No Crit Items)*: Base auto-attacks can crit (1.10x multiplier) but baseline spells cannot crit. In the Google Sheet, the 1.10x crit multiplier is omitted entirely for baseline unequipped calculations to show direct stat scaling.
    *   *Jeweled Gauntlet (JG)*: 40% Crit Chance, 170% Crit Damage (`1.28x` multiplier). Spells can crit.
    *   *Infinity Edge (IE)*: 60% Crit Chance, 140% Crit Damage (`1.24x` multiplier). Spells can crit.
    *   *IE + Last Whisper (LW)*: 70% Crit Chance, 140% Crit Damage (`1.28x` multiplier).
*   **AP vs. AD-ratio spells**: AP multiplies a spell's flat/base damage component only. Pure AD-ratio components (e.g. Jhin's 7.44 × AD) do not scale with AP — matching TFT's per-star ability data.

---

## 🔍 Detailed Champion Breakdowns

### 1. Cassiopeia 🐍

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Fires a blast of energy dealing magic damage. If the target is already Wounded (reduced healing), deals 30% bonus damage. Since her Wound lasts 5 seconds and she casts roughly every 3-4 seconds, every cast after the first one gains the 30% damage boost.
*   **Cast Lockout**: 0.5 seconds base.
*   **Attacks to Cast**: 3 (baseline unequipped) / 1 (equipped with Blue Buff).

#### 🧮 DPS Output Summary
*   **Baseline (Unequipped)**:
    *   1★: **68.6** | 2★: **102.8** | 3★: **154.2**
*   **Well-Equipped (Blue Buff + Jeweled Gauntlet + Archangel's Staff)**:
    *   1★: **189.1 Avg** | 2★: **283.7 Avg** | 3★: **425.5 Avg**
*   **Detailed DPS Calculations**: [Cassiopeia DPS Calculations](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/calculations/cassiopeia-dps-calculation.md)

#### 🔍 Analyze DPS
*   **Tank Melt Specialization**: Cassiopeia is a backliner who only focuses on 1 hero at a time. This concentrated, single-target magic damage output results in exceptionally high sustain DPS, making her highly effective at burning down frontline tanks.

---

### 2. Jhin 🏹

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Channels and fires a piercing physical line spell scaling with AD.
*   **Cast Lockout**: 1.6 seconds base.
*   **Attacks to Cast**: 12 attacks.
*   **Line Pierce Falloff**: Spell pierces through multiple enemies, dealing reduced damage per target: Target 1 (100%), Target 2 (44%), Target 3 (19.4%), Target 4 (8.5%).

#### 🧮 DPS Output Summary
*   **Baseline (Unequipped, 2 Targets Avg)**:
    *   1★: **70.1** | 2★: **105.1** | 3★: **158.2**
*   **Well-Equipped (Deathblade + Infinity Edge + Last Whisper, 2 Targets Avg)**:
    *   1★: **193.9** | 2★: **290.8** | 3★: **436.3**
*   **Detailed DPS Calculations**: [Jhin DPS Calculations](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/calculations/jhin-dps-calculation.md)

#### 🔍 Analyze DPS
*   **Burst AoE Distribution**: Jhin is a backliner who does big burst AoE damage. While his raw single-target DPS is limited by his long 1.6s cast lockout, Jhin's DPS can match other carries when his skill hits at least 2 people. This is highly consistent in real combat due to his large skill hitbox.

---

### 3. Malzahar 🌌

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Spawns two portals dealing magic damage in a rectangular area. Portal hits instantly destroy 50% of active shields on all hit targets.
*   **Cast Lockout**: 0.8 seconds base.
*   **Attacks to Cast**: 5 attacks (baseline unequipped) / 4 attacks (equipped with Spear of Shojin).

#### 🧮 DPS Output Summary
*   **Baseline (Unequipped, 1.5 Targets Avg)**:
    *   1★: **66.8** | 2★: **100.1** | 3★: **151.1**
*   **Well-Equipped (Spear of Shojin + Jeweled Gauntlet + Rabadon's Deathcap, 2 Targets Avg)**:
    *   1★: **238.9** | 2★: **358.3** | 3★: **542.2**
*   **Detailed DPS Calculations**: [Malzahar DPS Calculations](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/calculations/malzahar-dps-calculation.md)

#### 🔍 Analyze DPS
*   **High-Impact Small-AoE Portals**: Malzahar is a backliner who does AoE damage. Similar to Jhin, his DPS is not high when his skill only hits 1 person. However, when it hits at least 2 targets, his DPS skyrockets even higher than Jhin's. Note that his portal hitbox is relatively small for an AoE spell, meaning his skill typically hits only 1–2 enemies at a time.

---

### 4. Samira 🌹

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Fires a quick physical spell that deals AD-scaling damage and permanently shreds target Armor by flat values.
*   **Cast Lockout**: 0.8 seconds base.
*   **Attacks to Cast**: 3 attacks (baseline unequipped) / 2 attacks (equipped with Blue Buff).

#### 🧮 DPS Output Summary
*   **Baseline (Unequipped)**:
    *   1★: **59.9** | 2★: **90.2** | 3★: **134.8**
*   **Well-Equipped (Blue Buff + Infinity Edge + Last Whisper)**:
    *   1★: **91.1** | 2★: **136.6** | 3★: **204.9**
*   **Detailed DPS Calculations**: [Samira DPS Calculations](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/calculations/samira-dps-calculation.md)

#### 🔍 Analyze DPS
*   **Utility & Armor Shredding**: Samira is a backliner who focuses on 1 target and shreds their armor quickly. Although focusing on a single target (similar to Cassiopeia) might suggest high DPS, Samira's personal DPS is super low when you only look at the numbers. However, this raw DPS does not account for the permanent armor reduction from her skill, which significantly increases the damage output of her entire team against her target.

---

### 5. Tristana 🔫

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Active grants +70% Attack Speed for 4 seconds. During the buff, her attacks explode dealing 60% AD bonus physical splash damage to adjacent hexes.
*   **Mana Rules**: She cannot gain mana while the buff is active.
*   **Cast Lockout**: 0 0 seconds base (buff starts instantly).
*   **Attacks to Cast**: 4 attacks (charges 40 max mana).

#### 🧮 DPS Output Summary
*   **Baseline (Unequipped)**:
    *   1 Splash Target Avg: 1★: **53.8** | 2★: **81.3** | 3★: **120.8**
*   **Well-Equipped (Guinsoo's + Last Whisper + Infinity Edge)**:
    *   15s Fight (1.26 AS avg): 1★: **200.7** | 2★: **305.7** | 3★: **450.8**
    *   30s Fight (1.63 AS avg): 1★: **251.8** | 2★: **383.6** | 3★: **565.6**
*   **Detailed DPS Calculations**: [Tristana DPS Calculations](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/calculations/tristana-dps-calculation.md)

#### 🔍 Analyze DPS
*   **Super-Linear Attack Speed Scaling**: Tristana is a backline carry whose baseline DPS appears low because she lacks flat spell damage.
*   **Why normal carries scale linearly (1:1)**: For a standard carry (e.g., Cassiopeia), their cycle duration is modeled as `Cycle Duration = (N + L) / (AS)` (where `N` is attacks to cast, and `L` is cast lockout). Since their damage per cycle `D` (autos + spell) is constant with respect to AS, their DPS formula is:
    
 `DPS = (D) / (Cycle Duration) = AS * (D) / (N + L) = Constant * AS`

    This represents a mathematically perfect linear (1:1) relationship: doubling AS doubles DPS.
*   **Why Tristana scales super-linearly (better than 1:1)**: Because Tristana's active steroid phase is a *fixed* 4.0 seconds (which does not scale down with AS), her cycle duration is `(4) / (AS) + 4.0`. However, her damage per cycle *increases* with AS because she squeezes more splash attacks into that 4s window: `D(AS) = [4 + (6.8 + 4.08 * S) * AS] * AD` (where `S` is splash targets). Her DPS formula is:
    
 `DPS(AS) = AS * AD * (4 + (6.8 + 4.08 * S) * AS) / (4 + 4 * AS)`

    Because both the numerator and denominator scale with AS, her DPS scaling curve is concave up (super-linear). For instance, tripling her AS from 0.70 to 2.10 yields a **4.03x** DPS increase (climbing from 73.7 to 292.2 with 2 splash targets), showing that she scales far better with AS than any linear champion.

---

### 6. Viego 👑

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Strikes his target, dealing magic damage. Auto-attacks permanently gain bonus stacking magic damage on-hit for the rest of combat.
*   **Cast Lockout**: 0.8 seconds base.
*   **Attacks to Cast**: 5 attacks.

#### 🧮 DPS Output Summary
*   **Baseline (Unequipped)**:
    *   15s Fight (0.5 stacks avg): 1★: **51.5** | 2★: **77.6** | 3★: **116.2**
    *   30s Fight (1.5 stacks avg): 1★: **64.9** | 2★: **97.7** | 3★: **146.3**
*   **Well-Equipped (Standard: Gunblade + Titan's + Jeweled Gauntlet)**:
    *   15s Fight (0.5 stacks avg): 1★: **95.0** | 2★: **143.4** | 3★: **215.2**
    *   30s Fight (1.5 stacks avg): 1★: **112.2** | 2★: **169.1** | 3★: **253.7**
*   **Well-Equipped (RFC × 3)**:
    *   15s Fight (1.33 stacks avg): 1★: **174.3** | 2★: **262.3** | 3★: **392.7**
    *   30s Fight (3.11 stacks avg): 1★: **240.6** | 2★: **361.8** | 3★: **541.8**
*   **Detailed DPS Calculations**: [Viego DPS Calculations](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/calculations/viego-dps-calculation.md)

#### 🔍 Analyze DPS
*   **Close-Range Melee Scaling**: Viego is a close-ranged carry who also focuses mainly on 1 target.
*   **Engagement Scaling**: Stacking magic damage on-hit over combat duration rewards long melee engagements, raising his baseline DPS from 51.5 (15s average) to 64.9 (30s average).
*   **Rapid Firecannon Ramping**: Equipping 3 Rapid Firecannons spikes Viego's Attack Speed to 1.49, allowing him to attack 36 times in 30 seconds and ramp his stacking damage to an average of 3.11 stacks, yielding 541.8 DPS at 3★.
