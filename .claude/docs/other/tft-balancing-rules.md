# TFT Balancing Rules & Reference Guidelines

This document serves as the project's official design reference for durability, damage, mana gating, and star-level scaling multipliers, mirroring the "TFT Balancing Rules" Google Sheet tab.

> **Scope**: compact TFT reference tables. For in-depth explanations, see `tft-balancing-analysis.md`. For this project's implementation, see `balance-framework.md`.

---

## 1. Durability & EHP (Effective Health Pool) Trends

Effective Health Pool (EHP) represents the actual amount of raw, pre-mitigation damage a unit must take before being defeated:

$$\text{EHP} = \text{HP} \times \left(1 + \frac{\text{Defense}}{100}\right)$$

### Tanks vs Carries:
Tanks consistently possess **$50\%$ to $65\%$ more base EHP** than carries of the same cost tier.

### EHP Progression by Gold Tier:

| Cost Tier | Tank Base EHP | Carry Base EHP | Growth Notes |
| :--- | :---: | :---: | :--- |
| **1-Gold** | 910 - 980 | 570 - 600 | Baseline starting durability (e.g. Cho'Gath vs Cassiopeia) |
| **2-Gold** | 1,200 | 720 | 20% - 30% jump from Tier 1 (e.g. Sett vs Jinx) |
| **3-Gold** | 1,200 | 875 | Taric gains shields, Karma AP carry scales |
| **4-Gold** | 1,600 | 937 - 975 | Hyper-carries and primary frontlines (e.g. Sejuani vs Zeri) |
| **5-Gold** | 1,600 | 1,190 | Ahri carry gains 25% EHP; tanks gain revives/knock-aways |

---

## 2. Offense & Attack Speed Splits

### Flat Base AD:
AD is kept flat between **$40\text{ and }80\text{ AD}$** across all tiers (at 1-star). Carries do not scale via huge base AD; they scale through spells, items, and attack speed.

### Attack Speed (AS) by Role:

| Role | Base AS (attacks/s) | Mana Generation Note | Examples |
| :--- | :---: | :--- | :--- |
| **Tanks** | 0.60 | Slow attacks, slower base mana generation | Cho'Gath, Sejuani, Sett |
| **AP Carries** | 0.70 | Medium attacks, standard mana cycle | Karma, Lux, Cassiopeia |
| **AD/Hyper Carries** | 0.75 - 0.85 | Fast attacks, rapid mana accumulation | Jinx, Zeri, Ahri |

---

## 3. Mana Pool Design (Ability Gating)

> Mana gain formula: see `tft-balancing-analysis.md §3 — Mana and the Ability Cycle` for derivation (10 mana per auto, 7% mana-on-hit capped at 42.5).

### Ability Casting Profiles:

| Caster Type | Max Mana Range | Starting Mana | Description & Usage |
| :--- | :---: | :---: | :--- |
| **Spam Carries** | 40 - 50 | 0 | Casts every 4-5 attacks. Fast cycle. |
| **Utility / CC Tanks** | 90 - 160 | 30 - 70 | Casts once early, difficult to cycle a second time. |

---

## 4. Star-Level Scaling Multipliers

When a unit is upgraded to 2-star and 3-star, only a few stats scale:

| Stat Type | 1-Star | 2-Star | 3-Star | Scaling Rule |
| :--- | :---: | :---: | :---: | :--- |
| **Health (HP)** | 1.0x | 1.8x | 3.24x | Compounding 1.8x multiplier |
| **Attack Damage (AD)** | 1.0x | 1.8x | 3.24x | Compounding 1.8x multiplier |
| **Armor & MR** | Flat | Flat | Flat | Does NOT scale with star level |
| **Attack Speed (AS)** | Flat | Flat | Flat | Does NOT scale with star level |
| **Range** | Flat | Flat | Flat | Does NOT scale with star level |
