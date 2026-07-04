# Game Balance Hub

Welcome to the balancing documentation hub for the Custom Auto-Battler. This directory contains our math, simulations, and reference data. 

Use this guide to find the right document depending on your task.

---

## 🗺️ Documentation Map

Start here if you want to understand how our balancing system is structured:

```mermaid
graph TD
    hub[Game Balance Hub - README.md] --> framework[balance-framework.md<br>'Live implementation, math & scoring']
    hub --> sources[data-sources.md<br>'Google Sheets registry & sync']
    hub --> analysis[tft-balancing-analysis.md<br>'TFT Math: EHP, mana formulas']
    hub --> rules[tft-balancing-rules.md<br>'TFT Reference tables & values']
    hub --> roadmap[tft-balancing-research-plan.md<br>'Balance Roadmap & Research tasks']
    hub --> t1dps[tier-1-carry-dps.md<br>'Tier 1 carry DPS breakdowns']
    hub --> t2dps[tier-2-carry-dps.md<br>'Tier 2 carry DPS breakdowns']
    
    style hub fill:#1a365d,stroke:#3182ce,stroke-width:2px,color:#fff
    style framework fill:#2d3748,stroke:#4a5568,color:#fff
    style sources fill:#2d3748,stroke:#4a5568,color:#fff
    style analysis fill:#2d3748,stroke:#4a5568,color:#fff
    style rules fill:#2d3748,stroke:#4a5568,color:#fff
    style roadmap fill:#2d3748,stroke:#4a5568,color:#fff
    style t1dps fill:#2d3748,stroke:#4a5568,color:#fff
    style t2dps fill:#2d3748,stroke:#4a5568,color:#fff
```

### 📄 Document Directory

| Document | Purpose / What's inside | When to read it |
| :--- | :--- | :--- |
| **[Balance Framework](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/balance-framework.md)** | **Live design truth.** Explains the 3-step balance process, the Attack Speed Accumulator, the Mana-on-Hit system, and the target Stat Budget Scores. | Read this before making stat or mechanical adjustments to any champion. |
| **[TFT Balancing Rules & Reference Guidelines](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/tft-balancing-rules.md)** | **Compact reference tables.** Contains target durability/EHP ranges by gold tier, attack speed splits, and star-level scaling multipliers from Teamfight Tactics. | Read this for quick numeric benchmarks when designing new units or traits. |
| **[TFT Stats and Balancing Analysis](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/tft-balancing-analysis.md)** | **The mathematical "why."** A deep dive into League of Legends/TFT math: EHP calculations, pre-mitigation mana-on-hit formulas, tick quantization, and cost scaling. | Read this to understand the underlying theory and reasons behind our math. |
| **[TFT Set 9 Tier 1 Carry DPS Analysis](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/tier-1-carry-dps.md)** | **In-depth Carry breakdowns.** Details base auto-attacks and spell-cycle DPS (normal vs. spell contribution) for all 6 Tier-1 carry units across star upgrades. | Read this when reviewing raw power scaling curves and caster vs. physical carrying dynamics. |
| **[TFT Set 9 Tier 2 Carry DPS Analysis](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/tier-2-carry-dps.md)** | **In-depth Carry breakdowns.** Details base auto-attacks and spell-cycle DPS (normal vs. spell contribution) for all 4 Tier-2 carry units across star upgrades. | Read this when reviewing combo multipliers, clone duplicates, and splitting projectiles. |
| **[TFT Balancing Research Roadmap](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/tft-balancing-research-plan.md)** | **Long-term research plan.** Tracks our progress across topics like Trait Synergy, Ability Scaling, Team Combat Simulation, Itemization, and Economy. | Read this to check what balance tasks are coming next or need research. |
| **[Balance Data Sources](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/balance/data-sources.md)** | **Google Sheets registry.** Outlines our live sheets (`auto-battler`, `tft-set9`, `tft-set10`) and details the CLI commands to sync or dump data. | Read this when you need to sync Google Sheet data or work with offline snapshots. |

---

## 🔄 Balancing Workflow

Our game uses a combination of **static math** (spreadsheet-driven) and **dynamic simulation** (combat runner) to keep the roster balanced. 

```mermaid
flowchart LR
    A[1. Google Sheet<br>Design Stats] -->|Sync| B[2. ChampionRoster.cs<br>Game Code]
    B -->|Layer 2 Math| C[3. Balance Report<br>Stat Budget Check]
    C -->|Layer 3 Sim| D[4. Win-Rate Validator<br>Matchup Matrix]
    D -->|If unbalanced| A
```

### Useful CLI Commands & Workflows
All commands are run using Claude Code (or helper scripts) and the `auto-battler` Google Sheet:
*   **`/check-balance`** — Checks the consistency between the Google Sheet and game code, runs Layer 2 Stat Budget Score check, and flags out-of-band champions.
*   **`/tune-champion`** — Suggests stat changes for an out-of-band champion until their math fits their cost tier.
*   **`/validate-balance`** — Runs 200 rapid 1v1 battles inside Unity for all matchup pairs to detect win-rate outliers.
*   **`/push-champion-stats`** — Syncs local stat changes back up to the Google Sheet.
