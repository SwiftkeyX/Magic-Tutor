# TFT Set 9 Tier 2 Carry DPS Analysis (Unified)

This document provides a unified mathematical breakdown of baseline (unequipped) and well-equipped (3-item) DPS scaling for all 2-Gold carry champions in Teamfight Tactics (Set 9).

Putting baseline and itemized calculations in a single document guarantees that both calculations utilize identical, up-to-date mechanical assumptions (AS-scaling lockouts, targeting splits, and cast animation frames).

---

## 📊 Master DPS Summary Tables

### 📘 Table 1: Baseline (Unequipped) DPS Summary
*Consolidated comparison across 1★, 2★, and 3★ star levels with NO items equipped. AoE/multi-target spells are shown using typical combat averages (5-rocket sum for Jinx, 2 targets for Teemo, active + 1 self-trigger boulder for Taliyah, double slash on 1 target for Zed).*

| Champion | Star | Base AD | AS | Normal DPS | Spell DPS | Total DPS |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Jinx** (5 Rockets Sum) | 1★<br>2★<br>3★ | 50<br>75<br>113 | 0.75 | 36.1<br>54.1<br>81.2 | 46.4<br>70.3<br>110.9 | **82.5**<br>**124.4**<br>**192.1** |
| **Teemo** (2 Targets Avg) | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 26.6<br>39.8<br>59.7 | 69.0<br>103.5<br>155.3 | **95.6**<br>**143.3**<br>**215.0** |
| **Taliyah** (Self-Trigger, 1 Boulder) | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 27.2<br>40.8<br>61.2 | 38.5<br>57.8<br>86.6 | **65.7**<br>**98.6**<br>**147.8** |
| **Zed** (Slash x3, 2 Targets Avg) | 1★<br>2★<br>3★ | 55<br>83<br>124 | 0.70 | 42.4<br>63.9<br>95.5 | 33.7<br>51.5<br>77.9 | **76.1**<br>**115.4**<br>**173.4** |

### ⚔️ Table 2: Well-Equipped (3-Item) DPS Summary
*Consolidated comparison across 1★, 2★, and 3★ star levels with 3 core optimal items equipped. Ramping items (Archangel's for Taliyah) represent **[Start / End / Average]** over a 30s fight.*

| Champion | Star | AD (Equipped) | AS (Equipped) | Normal DPS | Spell DPS | Total DPS |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Jinx** (5 Rockets Sum) | 1★<br>2★<br>3★ | 98<br>147<br>221 | 0.90 | 132.0<br>198.0<br>297.0 | 103.9<br>158.9<br>248.7 | **235.9**<br>**356.9**<br>**545.7** |
| **Teemo** (2 Targets Avg) | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 29.9<br>44.8<br>67.2 | 194.1<br>291.2<br>436.8 | **224.0**<br>**336.0**<br>**504.0** |
| **Taliyah** (2 Passive Boulders) | 1★<br>2★<br>3★ | 40<br>60<br>90 | 0.70 | 31.6<br>47.4<br>71.1 | 100.0 / 172.7 / 130.3<br>150.0 / 259.1 / 195.5<br>225.0 / 388.6 / 293.2 | **131.6 / 204.3 / 161.9**<br>**197.4 / 306.5 / 242.9**<br>**296.1 / 459.7 / 364.3** |
| **Zed** (Slash x3, 2 Targets Avg) | 1★<br>2★<br>3★ | 122<br>183<br>275 | 0.70 | 105.9<br>158.9<br>238.3 | 72.8<br>110.2<br>171.8 | **178.7**<br>**269.1**<br>**410.1** |

---

## 📖 Glossary & Formula Index

*   **AD (Attack Damage)**: The physical damage dealt by a basic auto-attack. AD scales by 1.5x per star level (2★ = 1.5x, 3★ = 2.25x).
*   **AS (Attack Speed)**: The rate of basic attacks per second. Attack Speed remains flat across star levels.
*   **AP (Ability Power)**: The scaling multiplier applied to base spell damage (base is 100 AP / 1.0x).
*   **Attacks to Cast**: The number of basic attacks required to generate the mana pool needed for a cast (each attack generates 10 mana by default, except with Shojin).
*   **Cast Lockout (s)**: The duration spent executing the spell cast animation, during which basic attacks are paused. **Lockout time scales with Attack Speed**, modeled as:
    $$\text{Lockout}(\text{AS}) = \frac{\text{Base Lockout}}{\text{AS}}$$
*   **Cycle Duration (s)**: The total time of one full combat loop:
    $$\text{Cycle Duration} = \frac{\text{Attacks to Cast} + \text{Base Lockout}}{\text{AS}}$$
*   **Normal DPS**: The average damage per second contributed strictly by basic auto-attacks over a cycle:
    $$\text{Normal DPS} = \frac{\text{Attacks to Cast} \times \text{AD}}{\text{Cycle Duration}} \times \text{Crit} \times \text{Amp}$$
*   **Spell DPS**: The average damage per second contributed strictly by the spell over a cycle:
    $$\text{Spell DPS} = \frac{\text{Spell Damage}}{\text{Cycle Duration}} \times \text{Crit} \times \text{Amp}$$
*   **Total DPS**: The combined damage output over a cycle:
    $$\text{Total DPS} = \text{Normal DPS} + \text{Spell DPS}$$
*   **Crit Multiplier (Crit)**: The average multiplier applied by Critical strike chance and damage:
    $$\text{Average Crit Multiplier} = 1 + \text{Crit Chance} \times (\text{Crit Damage} - 1)$$
    *   *Baseline (No Crit Items)*: 25% Crit Chance, 140% Crit Damage (\(1.10\text{x}\) multiplier) — TFT base crit damage is 140%.
    *   *Jeweled Gauntlet (JG)*: 40% Crit Chance, 170% Crit Damage (\(1.28\text{x}\) multiplier). Spells can crit.
    *   *Infinity Edge (IE)*: 60% Crit Chance, 140% Crit Damage (\(1.24\text{x}\) multiplier). Spells can crit.
    *   *Last Whisper only (Jinx build)*: 35% Crit Chance, 140% Crit Damage (\(1.14\text{x}\) multiplier).
    *   *Modeling note*: as a simplification, these docs apply the average crit multiplier to spell damage in baseline rows too (strictly, TFT abilities only crit with JG/IE equipped).
*   **AP vs. AD-ratio spells**: AP multiplies a spell's flat/base damage component only (e.g. Jinx's 15/20/35 rocket flat, Zed's 25/40/50 slash flat). Pure AD-ratio components do not scale with AP — matching TFT's per-star ability data.

---

## 🔍 Detailed Champion Breakdowns

### 1. Jinx 🚀

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Fires 5 rockets at random enemies within a 2-hex radius of her current target. Each rocket deals physical damage: \(\text{AD} \times 1.50/1.55/1.60 + 15/20/35\) per star.
*   **Cast Lockout**: 1.0 second base. While casting she does not auto-attack.
*   **Attacks to Cast**: 7 attacks (mana is 10 / 70).
*   **Combat Role**: A **semi-AoE cleanup tool** — rockets disperse among nearby enemies, excellent at finishing multiple low-health targets.

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 50 | AS = 0.75 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Rocket = \(50 \times 1.50 + 15 = 90\); 5 rockets = 450)**:
    *   Cycle Duration = \((7 + 1.0) / 0.75 = \mathbf{10.67\text{ seconds}}\)
    *   Normal DPS = \((7 \times 50) / 10.67 \times 1.10 = \mathbf{36.1}\)
    *   Spell DPS = \(450 / 10.67 \times 1.10 = \mathbf{46.4}\)
    *   Total DPS = \(36.1 + 46.4 = \mathbf{82.5}\)
*   **Star Scaling (Unequipped; rocket ratio 1.50/1.55/1.60, flat 15/20/35)**:
    *   **1★**: AD = 50 | Rockets = 450 | **Total DPS: 82.5**
    *   **2★**: AD = 75 | Rockets = 681.3 | **Total DPS: 124.4**
    *   **3★**: AD = 113 | Rockets = 1075 | **Total DPS: 192.1**

#### 🧮 Equipped Calculations (Deathblade + Runaan's Hurricane + Last Whisper)
*   **Stats**: AD = 98 (+96% AD) | AS = 0.90 | Crit Chance = 35% (1.14x avg multiplier for all physical damage)
*   **Cycle & DPS (Runaan's bolt adds 50% AD to another enemy = 1.5x auto scaling)**:
    *   Cycle Duration = \((7 + 1.0) / 0.90 = \mathbf{8.89\text{ seconds}}\)
    *   Normal DPS = \((7 \times 98 \times 1.5) / 8.89 \times 1.14 = \mathbf{132.0}\)
    *   Rocket Damage = \((98 \times 1.50 + 15) \times 5 = 810\)
    *   Spell DPS = \(810 / 8.89 \times 1.14 = \mathbf{103.9}\)
    *   Total DPS = \(132.0 + 103.9 = \mathbf{235.9}\)

#### 🔍 Analyze DPS
*   **Backline Splash Damage**: Jinx is a backliner who does splash damage. Her base and equipped DPS numbers are naturally higher than Tier 1 carries (such as Tristana's single-splash baseline), which aligns with her higher cost tier and clean rocket targeting.

---

### 2. Teemo 🍄

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Throws an explosive mushroom. On detonation, enemies in a 1-hex radius are Wounded (-33% healing) and dealt magic damage over 3 seconds.
*   **Cast Lockout**: 0.8 seconds base.
*   **Attacks to Cast**: 5 attacks (mana is 0 / 50) / 4 attacks (equipped with Blue Buff).
*   **Combat Role**: An **anti-healing AoE burner** — poison applies fully to all units in the circle, scaling linearly with target density.

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 40 | AS = 0.70 | AP = 100 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Assuming 2 Targets Hit Avg; spell base 260/390/585 per star)**:
    *   Cycle Duration = \((5 + 0.8) / 0.70 = \mathbf{8.29\text{ seconds}}\)
    *   Normal DPS = \((5 \times 40) / 8.29 \times 1.10 = \mathbf{26.6}\)
    *   Spell DPS (2 Tgt = 520) = \(520 / 8.29 \times 1.10 = \mathbf{69.0}\)
    *   Total DPS = \(26.6 + 69.0 = \mathbf{95.6}\)
*   **Star Scaling (Unequipped, 2 Targets)**:
    *   **1★**: Normal = 26.6 | Spell = 69.0 | **Total DPS: 95.6**
    *   **2★**: Normal = 39.8 | Spell = 103.5 | **Total DPS: 143.3**
    *   **3★**: Normal = 59.7 | Spell = 155.3 | **Total DPS: 215.0**

#### 🧮 Equipped Calculations (Blue Buff + Jeweled Gauntlet + Rabadon's Deathcap)
*   **Stats**: AD = 40 | AS = 0.70 | AP = 200 (100 + BB 10 + JG 20 + Rabadon 70) | Crit Chance = 40% (1.28x avg multiplier, spell crits enabled)
*   **Cycle & DPS (Blue Buff requires 4 attacks to cast; 2 Targets Avg)**:
    *   Cycle Duration = \((4 + 0.8) / 0.70 = \mathbf{6.86\text{ seconds}}\)
    *   Normal DPS = \((4 \times 40) / 6.86 \times 1.28 = \mathbf{29.9}\)
    *   Spell Damage (1 Target) = \(260 \times 2.00 \text{ AP} \times 1.28 = 665.6\)
    *   Spell DPS (2 Targets) = \(1331.2 / 6.86 = \mathbf{194.1}\)
    *   Total DPS = \(29.9 + 194.1 = \mathbf{224.0}\)

#### 🔍 Analyze DPS
*   **Massive Area Poison & Delay**: Teemo is a backliner who does AoE damage. His baseline and equipped DPS are higher than Tier 1 carries, and his total damage output can escalate dramatically in crowded fights due to the large circular area of his mushroom blast.
*   **Why is he not OP?**: Despite high theoretical DPS, Teemo's damage is a Damage-over-Time (DoT) effect applied over 3 seconds. This delay allows enemies to heal through it, and often leads to "wasted" damage on units that die before the full duration ticks out, balancing his large AoE threat.

---

### 3. Taliyah 🪨

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: **Active** — deals magic damage to the current target and knocks them up (2s stun). **Passive** — whenever *any* enemy is knocked up or back by *anything*, she throws a boulder dealing magic damage.
*   **Cast Lockout**: 0.8 seconds base.
*   **Attacks to Cast**: 6 attacks for sustain casts (mana is 20 / 60).
*   **Combat Role**: A **synergistic combo carry** — with knockup allies (Sett, Vi, Poppy), every external knockup adds a free boulder. Spell bases per star: active 220/330/495, boulder 120/180/270.

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 40 | AS = 0.70 | AP = 100 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Self-trigger only: active + 1 boulder = 340)**:
    *   Cycle Duration = \((6 + 0.8) / 0.70 = \mathbf{9.71\text{ seconds}}\)
    *   Normal DPS = \((6 \times 40) / 9.71 \times 1.10 = \mathbf{27.2}\)
    *   Spell DPS = \(340 / 9.71 \times 1.10 = \mathbf{38.5}\)
    *   Total DPS = \(27.2 + 38.5 = \mathbf{65.7}\)
*   **Star Scaling (Unequipped, Self-Trigger)**:
    *   **1★**: Spell = 340 | **Total DPS: 65.7**
    *   **2★**: Spell = 510 | **Total DPS: 98.6**
    *   **3★**: Spell = 765 | **Total DPS: 147.8**
*   **External-knockup bonus**: each external knockup during a cycle adds \(120 \times 1.10 / 9.71 = 13.6\) DPS at 1★.

#### 🧮 Equipped Calculations (Jeweled Gauntlet + Archangel's Staff + Hextech Gunblade)
*   **Stats**: AD = 40 | AS = 0.70 | Crit Chance = 40% (1.28x avg multiplier, spell crits enabled)
    *   Start AP (0s) = 165 AP (100 + JG 20 + Gunblade 25 + AA 20)
    *   End AP (30s) = 285 AP
    *   Average AP = 215 AP — Archangel's time-averages +70 AP over a 30s fight
*   **Cycle & DPS (Assuming 2 passive boulders per cast: 220 + 240 = 460 base)**:
    *   Cycle Duration = \((6 + 0.8) / 0.70 = \mathbf{9.71\text{ seconds}}\)
    *   Normal DPS = \((6 \times 40) / 9.71 \times 1.28 = \mathbf{31.6}\)
*   **Ramping Outputs**:
    | Phase | AP Mod | Spell Damage / Cast | Spell DPS | Total DPS |
    | :--- | :---: | :---: | :---: | :---: |
    | **Start (0s)** | 1.65x | 971.5 | 100.0 | **131.6** |
    | **End (30s)** | 2.85x | 1678.1 | 172.7 | **204.3** |
    | **Average** | 2.15x | 1266.1 | 130.3 | **161.9** |

#### 🔍 Analyze DPS
*   **Synergy-Dependent Combo Carry**: Taliyah is a backliner who scales her damage with knockups. Her unequipped baseline DPS is lower than Tier 1 carries, which is expected because she is balanced around her passive, which requires external knockups from allies to trigger bonus damage boulders.
*   **Comp Limitation**: Without allies triggering stuns/knockups, her DPS is very low. She can only function as a main carry in highly specific team compositions (such as the Sett + Taliyah double-knockup combo).

---

### 4. Zed 🥷

#### 🛡️ Mechanics & Combat Nuance
*   **Ability**: Creates a shadow clone at the furthest enemy within 2 hexes. Zed and his shadow slash adjacent targets: \(\text{AD} \times 1.40/1.40/1.50 + 25/40/50\) physical per slash.
*   **Cast Lockout**: none (instant slashes).
*   **Attacks to Cast**: 7 attacks (mana is 0 / 70).
*   **Combat Role**: A **critical strike assassin** — his Ionia bonus (+15% Crit Chance / +15% Crit Damage) and crit items multiply both autos and slashes.

#### 🧮 Baseline (Unequipped) Calculations
*   **Stats**: AD = 55 | AS = 0.70 | Crit Chance = 25% (1.10x avg multiplier)
*   **Cycle & DPS (Zed + clone slash 2 targets avg — 3 slashes total: \((55 \times 1.40 + 25) \times 3 = 306\))**:
    *   Cycle Duration = \(7 / 0.70 = \mathbf{10.00\text{ seconds}}\)
    *   Normal DPS = \(55 \times 0.70 \times 1.10 = \mathbf{42.4}\)
    *   Spell DPS (3 hits total) = \(306 / 10.00 \times 1.10 = \mathbf{33.7}\)
    *   Total DPS (3 hits total) = \(42.4 + 33.7 = \mathbf{76.1}\)
*   **Star & Target Scaling (Unequipped; ratio 1.40/1.40/1.50, flat 25/40/50)**:
    *   *Slash x2 (1 Target double-hit)*:
        *   **1★**: Normal = 42.4 | Spell = 22.4 | **Total DPS: 64.8**
        *   **2★**: Normal = 63.9 | Spell = 34.2 | **Total DPS: 98.1**
        *   **3★**: Normal = 95.5 | Spell = 51.9 | **Total DPS: 147.4**
    *   *Slash x3 (2 Targets Avg, 3 hits total)*:
        *   **1★**: Normal = 42.4 | Spell = 33.7 | **Total DPS: 76.1**
        *   **2★**: Normal = 63.9 | Spell = 51.5 | **Total DPS: 115.4**
        *   **3★**: Normal = 95.5 | Spell = 77.9 | **Total DPS: 173.4**
    *   *Slash x4 (2 Targets Max, 4 hits total)*:
        *   **1★**: Normal = 42.4 | Spell = 44.9 | **Total DPS: 87.3**
        *   **2★**: Normal = 63.9 | Spell = 68.7 | **Total DPS: 132.6**
        *   **3★**: Normal = 95.5 | Spell = 103.8 | **Total DPS: 199.3**

#### 🧮 Equipped Calculations (Infinity Edge + Deathblade + Bloodthirster)
*   **Stats**: AD = 122 (+121% AD) | AS = 0.70 | Crit Chance = 60% (1.24x avg multiplier, spell crits enabled)
*   **Cycle & DPS (Zed and clone slash 2 adjacent targets — 3 slashes avg total)**:
    *   Cycle Duration = \(7 / 0.70 = \mathbf{10.00\text{ seconds}}\)
    *   Normal DPS = \(122 \times 0.70 \times 1.24 = \mathbf{105.9}\)
    *   Single Slash = \((122 \times 1.40 + 25) \times 1.24 = 242.8\)
    *   Spell DPS (3 slashes avg) = \(728.4 / 10.00 = \mathbf{72.8}\)
    *   Total DPS (3 slashes avg) = \(105.9 + 72.8 = \mathbf{178.7}\)
*   **AoE Multi-Target Outputs**:
    | Target Count / Slashes | Spell Damage | Spell DPS | Total DPS |
    | :--- | :---: | :---: | :---: |
    | **1 Target (2 slashes)** | 485.6 | 48.6 | **154.5** |
    | **2 Targets Avg (3 slashes)**| 728.4 | 72.8 | **178.7** |
    | **2 Targets Max (4 slashes)**| 971.2 | 97.1 | **203.0** |

#### 🔍 Analyze DPS
*   **Backline Infiltration & Cleave**: Zed is a melee assassin who can jump directly to the enemy backline to assassinate carries.
*   **AoE Hit Density & Jinx Comparison**: To ensure balanced scaling, Zed's DPS should not exceed Jinx's (the primary Tier 2 backline splash carry). By assuming his skill hits 2 targets on average but does not trigger a full double-slash (averaging 3 slashes total across all targets, rather than 4), his unequipped DPS is centered at **76.1** and equipped at **178.7** (both safely below Jinx's **82.5** / **235.9**), while his maximum cleave potential (Slash x4) can still reach **87.3** unequipped and **203.0** equipped.
