# Viego DPS Calculation Details 👑

This document provides the step-by-step mathematical calculations for Viego's baseline (unequipped) and well-equipped DPS across 1★, 2★, and 3★ star levels.

> **Source of truth**: All base stats are sourced from the **TFT Set 9 Basic data** spreadsheet.
> DPS is calculated from first principles using the formulas below. No external script output is used.

---

## ⚙️ Core Variables & Mechanics

### 1. Base Stats (from TFT Set 9 Basic data)

**Scaling stats**
| Stat | 1★ | 2★ | 3★ |
| :--- | :---: | :---: | :---: |
| **Base AD** | 45 | 68 | 101 |
| **Base Spell Damage** | 110 | 165 | 250 |
| **Stacking Magic Damage (Base)** | 20 | 30 | 45 |

**Fixed stats** *(do not scale with star level)*
| Stat | Value |
| :--- | :---: |
| **Attack Speed (AS)** | 0.75 |
| **Max Mana** | 50 |
| **Cast Lockout** | 0.8s |

### 3. Skill Description & Mechanics
*   **Skill**: Strikes his target dealing magic damage. Auto-attacks permanently gain bonus stacking magic damage on-hit for the rest of combat.
*   **Stacking Magic Damage**: We model this by assuming an average stack count active across the combat duration.
*   **Target Multiplier**: Viego is a single-target melee carry (**1.00× target multiplier**).

---

## 🧮 Stacking Mechanics Proof & Average Stack Derivation

To derive the average active stack count over a combat duration of $T$ seconds, we calculate the total attacks completed and the resulting stacking math.

### 1. Attack & Cast Cycle Timeline (AS = 0.75)
Viego's cycle consists of 5 attacks (ATC = 5) followed by a cast lockout of 0.8s.
- **Cycle Duration**:
  $$\text{Cycle Duration} = \frac{\text{ATC}}{\text{AS}} + \text{Lockout} = \frac{5}{0.75} + 0.8 = 7.467 \text{ seconds}$$

By tracing this timeline from the start of combat ($t = 0$):
- **First 5 attacks**: Occur at $1.33\text{s}$, $2.67\text{s}$, $4.00\text{s}$, $5.33\text{s}$, and $6.67\text{s}$. (No stacks are active during these first 5 attacks).
- **First Spell Cast**: Cast lockout runs from $6.67\text{s}$ to $7.47\text{s}$ (no attacks are fired). After this cast, the passive is active and subsequent attacks start stacking magic damage on-hit.
- **Next attacks**: Occur at $8.80\text{s}$, $10.13\text{s}$, $11.47\text{s}$, $12.80\text{s}$, $14.13\text{s}$, etc.

- **In 15 seconds**: Viego completes exactly **10 attacks** ($N = 10$).
- **In 30 seconds**: Viego completes exactly **20 attacks** ($N = 20$).

### 2. Average Stacks Formula (On-Attack Stacking)
Since the buff is only active after the first cast (requiring 5 attacks):
- The first 5 attacks deal **0** stacks of bonus damage.
- The remaining $N - 5$ attacks deal **$1, 2, 3, \dots, N-5$** stacks of bonus damage.

The total stacks of damage dealt over $N$ attacks is given by:
$$\text{Total Stacks} = \sum_{i=1}^{N-5} i = \frac{(N - 5) \times (N - 4)}{2}$$

To find the average stacks active per attack over the fight, we divide by the total number of attacks ($N$):
$$\text{Average Stacks} = \frac{\text{Total Stacks}}{N} = \frac{(N - 5) \times (N - 4)}{2N}$$

### 3. Case Derivations (AS = 0.75)
- **15-Second Fight ($N = 10$ attacks)**:
  $$\text{Average Stacks} = \frac{(10 - 5) \times (10 - 4)}{20} = \frac{5 \times 6}{20} = 1.5 \text{ stacks} \quad (\text{Total Stacks} = 15)$$
- **30-Second Fight ($N = 20$ attacks)**:
  $$\text{Average Stacks} = \frac{(20 - 5) \times (20 - 4)}{40} = \frac{15 \times 16}{40} = 6.0 \text{ stacks} \quad (\text{Total Stacks} = 120)$$

---

## 🧮 Baseline (Unequipped) Calculations

### Calculations

| Step | Formula | Calculation | 1★ | 2★ | 3★ |
| :--- | :--- | :--- | :---: | :---: | :---: |
| ATC | `ceil(Max Mana / 10)` | `ceil(50 / 10)` | 5 | 5 | 5 |
| Cycle Duration | `ATC / AS + Lockout` | `5 / 0.75 + 0.8` | 7.467s | 7.467s | 7.467s |
| Auto Attack DPS | `(ATC × AD) / Cycle` | `(5 × [45, 68, 101]) / 7.467s` | 30.1 | 45.5 | 67.6 |
| Spell Base (1 Target) | `Spell` | `[110.0, 165.0, 250.0]` | 110.0 | 165.0 | 250.0 |
| **Case 1: 15s Fight** | | | | | |
| Stacking Magic Dmg (15s) | `ATC × Avg Stacks (15s) × Stacking_base` | `5 × 1.5 × [20, 30, 45]` | 150.0 | 225.0 | 337.5 |
| Spell Damage (15s) | `Spell Base + Stacking Magic Dmg (15s)` | `[110.0, 165.0, 250.0] + [150.0, 225.0, 337.5]` | 260.0 | 390.0 | 587.5 |
| Spell DPS (15s) | `Spell Damage (15s) / Cycle` | `[260.0, 390.0, 587.5] / 7.467s` | 34.8 | 52.2 | 78.7 |
| **Total DPS (15s)** | `Auto DPS + Spell DPS (15s)` | `Auto DPS + Spell DPS (15s)` | **64.9** | **97.7** | **146.3** |
| **Case 2: 30s Fight** | | | | | |
| Stacking Magic Dmg (30s) | `ATC × Avg Stacks (30s) × Stacking_base` | `5 × 6.0 × [20, 30, 45]` | 600.0 | 900.0 | 1350.0 |
| Spell Damage (30s) | `Spell Base + Stacking Magic Dmg (30s)` | `[110.0, 165.0, 250.0] + [600.0, 900.0, 1350.0]` | 710.0 | 1065.0 | 1600.0 |
| Spell DPS (30s) | `Spell Damage (30s) / Cycle` | `[710.0, 1065.0, 1600.0] / 7.467s` | 95.1 | 142.6 | 214.3 |
| **Total DPS (30s)** | `Auto DPS + Spell DPS (30s)` | `Auto DPS + Spell DPS (30s)` | **125.2** | **188.1** | **281.9** |

---

## 🧮 Equipped Calculations (Hextech Gunblade + Titan's Resolve + Jeweled Gauntlet)

### 1. Item Stats & Effects
| Item | Effect |
| :--- | :--- |
| **Hextech Gunblade** | +10% AD, +25 AP. |
| **Titan's Resolve** | +20% AD, +50 AP (at max 25 stacks). |
| **Jeweled Gauntlet** | +20 AP, +15% Crit Chance, +30% Crit Damage, spells can crit. |

### 2. Calculations

| Step | Formula | Calculation | 1★ | 2★ | 3★ |
| :--- | :--- | :--- | :---: | :---: | :---: |
| AD Mult | `1.00 + Titan_ad + Gunblade_ad` | `1.00 + 0.20 + 0.10` | 1.30× | 1.30× | 1.30× |
| Equipped AD | `round(AD_base × AD_Mult)` | `round([45, 68, 101] × 1.30)` | 58 | 88 | 131 |
| AS Equipped | `AS_base` | `0.75` | 0.75 | 0.75 | 0.75 |
| AP Total | `AP_base + AP_items` | `100 + 50 + 25 + 20` | 195 | 195 | 195 |
| Crit Chance | `Crit_base + JG_crit` | `25% + 15%` | 40% | 40% | 40% |
| Crit Damage | `CritDmg_base + JG_critdmg` | `140% + 30%` | 170% | 170% | 170% |
| Crit Multiplier | `1 + Crit Chance × (Crit Damage − 1)` | `1 + 0.40 × 0.70` | 1.28 | 1.28 | 1.28 |
| ATC | `ceil(Max Mana / 10)` | `ceil(50 / 10)` | 5 | 5 | 5 |
| Cycle Duration | `ATC / AS_equipped + Lockout` | `5 / 0.75 + 0.8` | 7.467s | 7.467s | 7.467s |
| Auto Attack DPS | `(ATC × AD_equipped × Crit) / Cycle` | `(5 × [58, 88, 131] × 1.28) / 7.467s` | 49.7 | 75.4 | 112.3 |
| Spell Base (1 Target) | `Spell` | `[110.0, 165.0, 250.0]` | 110.0 | 165.0 | 250.0 |
| **Case 1: 15s Fight** | | | | | |
| Stacking Magic Dmg (15s) | `ATC × Avg Stacks (15s) × Stacking_base` | `5 × 1.5 × [20, 30, 45]` | 150.0 | 225.0 | 337.5 |
| Spell Damage (15s) | `(Spell Base × AP + Stacking Magic Dmg (15s)) × Crit` | `([110.0, 165.0, 250.0] × 1.95 + [150.0, 225.0, 337.5]) × 1.28` | 466.6 | 699.8 | 1056.0 |
| Spell DPS (15s) | `Spell Damage (15s) / Cycle` | `[466.6, 699.8, 1056.0] / 7.467s` | 62.5 | 93.7 | 141.4 |
| **Total DPS (15s)** | `Auto DPS + Spell DPS (15s)` | `Auto DPS + Spell DPS (15s)` | **112.2** | **169.1** | **253.7** |
| **Case 2: 30s Fight** | | | | | |
| Stacking Magic Dmg (30s) | `ATC × Avg Stacks (30s) × Stacking_base` | `5 × 6.0 × [20, 30, 45]` | 600.0 | 900.0 | 1350.0 |
| Spell Damage (30s) | `(Spell Base × AP + Stacking Magic Dmg (30s)) × Crit` | `([110.0, 165.0, 250.0] × 1.95 + [600.0, 900.0, 1350.0]) × 1.28` | 1042.6 | 1563.8 | 2352.0 |
| Spell DPS (30s) | `Spell Damage (30s) / Cycle` | `[1042.6, 1563.8, 2352.0] / 7.467s` | 139.6 | 209.4 | 315.0 |
| **Total DPS (30s)** | `Auto DPS + Spell DPS (30s)` | `Auto DPS + Spell DPS (30s)` | **189.3** | **284.8** | **427.3** |

---

## 🧮 Equipped Calculations (Rapid Firecannon × 3)

### 1. Item Stats & Effects
| Item | Effect |
| :--- | :--- |
| **Rapid Firecannon (RFC)** | +33% AS, +12% damage amplification, +1 Attack Range. Stacks multiplicatively for damage amplification ($1.12 \times 1.12 \times 1.12 = 1.405\text{x}$). |

### 2. Timeline and Stack Derivation (RFC × 3)
- **AS Equipped**: $0.75 \times (1.00 + 3 \times 0.33) = 0.75 \times 1.99 = 1.4925$ attacks/sec.
- **Cycle Duration**:
  $$\text{Cycle Duration} = \frac{5}{1.4925} + 0.8 = 3.350 + 0.8 = 4.150 \text{ seconds}$$
- **Total Attacks in 15s**: $N = 18$ attacks.
  - Average Stacks (15s):
    $$\text{Average Stacks} = \frac{(18 - 5) \times (18 - 4)}{36} = 5.06 \text{ stacks} \quad (\text{Total Stacks} = 91)$$
- **Total Attacks in 30s**: $N = 36$ attacks.
  - Average Stacks (30s):
    $$\text{Average Stacks} = \frac{(36 - 5) \times (36 - 4)}{72} = 13.78 \text{ stacks} \quad (\text{Total Stacks} = 496)$$

### 3. Calculations

| Step | Formula | Calculation | 1★ | 2★ | 3★ |
| :--- | :--- | :--- | :---: | :---: | :---: |
| AD Mult | `1.00` | `1.00` | 1.00× | 1.00× | 1.00× |
| Equipped AD | `round(AD_base × AD_Mult)` | `round([45, 68, 101] × 1.00)` | 45 | 68 | 101 |
| AS Equipped | `AS_base × (1.00 + 3 × 0.33)` | `0.75 × 1.99` | 1.4925 | 1.4925 | 1.4925 |
| AP Total | `AP_base` | `100` | 100 | 100 | 100 |
| Crit Multiplier | `1 + Crit Chance × (Crit Damage − 1)` | `1 + 0.25 × 0.40` | 1.10 | 1.10 | 1.10 |
| Damage Amp | `1.12 × 1.12 × 1.12` | `1.405` | 1.405 | 1.405 | 1.405 |
| ATC | `ceil(Max Mana / 10)` | `ceil(50 / 10)` | 5 | 5 | 5 |
| Cycle Duration | `ATC / AS_equipped + Lockout` | `5 / 1.4925 + 0.8` | 4.150s | 4.150s | 4.150s |
| Auto Attack DPS | `(ATC × AD_equipped × Crit × Amp) / Cycle` | `(5 × [45, 68, 101] × 1.10 × 1.405) / 4.150s` | 83.8 | 126.6 | 188.1 |
| Spell Base (1 Target) | `Spell` | `[110.0, 165.0, 250.0]` | 110.0 | 165.0 | 250.0 |
| **Case 1: 15s Fight** | | | | | |
| Stacking Magic Dmg (15s) | `ATC × Avg Stacks (15s) × Stacking_base` | `5 × 5.06 × [20, 30, 45]` | 506.0 | 759.0 | 1138.5 |
| Spell Damage (15s) | `(Spell Base × AP + Stacking Magic Dmg (15s)) × Crit × Amp` | `([110.0, 165.0, 250.0] × 1.00 + [506.0, 759.0, 1138.5]) × 1.10 × 1.405` | 952.0 | 1428.0 | 2145.9 |
| Spell DPS (15s) | `Spell Damage (15s) / Cycle` | `[952.0, 1428.0, 2145.9] / 4.150s` | 229.4 | 344.1 | 517.1 |
| **Total DPS (15s)** | `Auto DPS + Spell DPS (15s)` | `Auto DPS + Spell DPS (15s)` | **313.2** | **470.7** | **705.2** |
| **Case 2: 30s Fight** | | | | | |
| Stacking Magic Dmg (30s) | `ATC × Avg Stacks (30s) × Stacking_base` | `5 × 13.78 × [20, 30, 45]` | 1378.0 | 2067.0 | 3100.5 |
| Spell Damage (30s) | `(Spell Base × AP + Stacking Magic Dmg (30s)) × Crit × Amp` | `([110.0, 165.0, 250.0] × 1.00 + [1378.0, 2067.0, 3100.5]) × 1.10 × 1.405` | 2299.7 | 3449.6 | 5178.2 |
| Spell DPS (30s) | `Spell Damage (30s) / Cycle` | `[2299.7, 3449.6, 5178.2] / 4.150s` | 554.1 | 831.2 | 1247.8 |
| **Total DPS (30s)** | `Auto DPS + Spell DPS (30s)` | `Auto DPS + Spell DPS (30s)` | **637.9** | **957.8** | **1435.9** |

---

## ⚠️ Script Reference (champion_db.py)

> [!WARNING]
> `champion_db.py` is **not the source of truth** and should not be used to drive calculations. Stats in that file may drift from the sheet. The calculations above are authoritative.
>
> The script file is retained for historical reference only. See the header comment in [champion_db.py](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/scripts/champion_db.py) for details.
