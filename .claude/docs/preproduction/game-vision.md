# Game Vision Document

## Overview

| Field | Value |
|---|---|
| **Title** | Magic Tutor |
| **Genre** | Roguelike + Auto Battler |
| **Platform** | PC (Windows) |
| **Target audience** | Hardcore — players who enjoy deep strategy, build optimization, and permadeath tension |
| **Estimated scope** | Vertical slice → full release |

## Design Pillars

Three qualities that every design decision must serve. If a feature doesn't serve at least one pillar, cut it.

1. **Challenging** — Every run demands genuine decisions; mistakes have real consequences (permadeath per run)
2. **Strategic** — Teacher promotion choices and student training paths reward long-term thinking
3. **Empowering** — Watching a well-built team obliterate enemies in auto-battle is the payoff

## Core Loop

```
Recruit random students → Train students (stat management) → Auto-battle enemies
→ Win: survive the year → Graduate best students as Teachers (meta-progression)
→ Discard remaining students → New semester: recruit again
→ Repeat for 3 years → Run complete
→ Lose: team wiped in battle → Run ends (permadeath), teachers persist
```

**Session feel:** The player feels like a growing mastermind — every training decision and promotion choice compounds into a team that visibly crushes harder enemies over time.

## Mechanics

### Player (Principal)

| Mechanic | Description |
|---|---|
| Role | Non-combatant — Utility provider and meta-progression architect |
| Training action | Allocate training sessions to students to raise their stats (Uma Musume-style event system) |
| Promotion decision | At year-end, choose which students to graduate as Teachers |
| Teacher utility | Graduated teachers provide passive buffs/utility effects to future runs |

### Students

| Aspect | Description |
|---|---|
| Recruitment | Random pool of new students drawn each semester |
| Combat | Auto-battle against enemies as a team — no direct player input during fights |
| Growth | Stats improve through training actions during the semester |
| Year-end fate | Best students → Teachers (meta-progression); remaining students → discarded |

### Enemies / Obstacles

| Type | Behavior |
|---|---|
| Year 1 enemies | Base-level auto-battle opponents — introduces combat mechanics |
| Year 2 enemies | Stronger stats, possibly new abilities — forces better team composition |
| Year 3 enemies | Peak difficulty — tests the full accumulated training and teacher buffs |

### Trait System

Each Student has one or more Traits (e.g., Fire, Healer, Shield). Fielding multiple Students with the same Trait past a threshold unlocks that Trait's ability — mirroring TFT synergy breakpoints. This is the primary driver of build diversity and team combo potential.

| Aspect | Description |
|---|---|
| Trait assignment | Each Student is assigned Traits on recruitment (random draw) |
| Threshold activation | Reaching e.g. 2/4/6 of the same Trait activates escalating Trait abilities |
| Build diversity | Different Trait combos = different playstyles and win conditions |

### Progression

**Within-run (ephemeral):**
- Student stats rise through training each semester
- Team composition improves as you learn which student types synergize

**Cross-run (persistent meta):**
- Teachers accumulated from previous runs provide utility effects and buffs
- More teachers = easier future runs (roguelite meta-progression loop)

## Difficulty Curve

| Year | Enemy Power | Expected Team State | Pacing Target |
|---|---|---|---|
| Year 1 | Low — tutorial-level threat | Raw recruits, minimal training | Player learns training loop and student types |
| Year 2 | Medium — meaningful challenge | 1 graduating class of teachers accumulated | Player feels meta-progression benefits kick in |
| Year 3 | High — run-ending threat | 2 graduating classes + optimized students | Player must have built well to survive |

**Escalation pattern:** Enemy stats scale up each year. New enemy types may be introduced in Year 2–3 to force adaptation.
**Run length:** 3 years (semesters) per run.
**Permadeath trigger:** Any team loss ends the run. Teachers from completed years persist into future runs.
**Meta-progression curve:** Each failed run adds at most the teachers earned before failure — the ratchet effect keeps players making incremental progress.

## Win / Lose Conditions

| Condition | Trigger |
|---|---|
| **Win (run)** | Survive all 3 years — team defeats Year 3 enemies |
| **Lose (run)** | Team is wiped in any auto-battle — run ends, teachers earned before failure persist |

## Levels / Scenes

| Scene | Purpose |
|---|---|
| `Bootstrap` | Persistent singleton initialization (GameManager, SceneLoader, etc.) |
| `MainMenu` | Title screen — new run, continue, settings, quit |
| `School` | Main gameplay hub — semester loop, training, student management |
| `Battle` | Auto-battle arena — students vs enemies, watch and evaluate |
| `YearEnd` | Promotion screen — choose which students become teachers |

## Art Direction

| Aspect | Direction |
|---|---|
| **Style** | 2D — exact style (pixel art vs. flat illustration) TBD during production |
| **Palette** | Magical/school theme — warm, inviting with arcane accents |
| **Camera** | Orthographic 2D |
| **Resolution / aspect** | 1920×1080, 16:9 locked |

## Audio Direction

| Aspect | Direction |
|---|---|
| **Music style** | Whimsical/magical — light academy theme for school scenes, tense for battles |
| **SFX style** | Punchy spell effects in battle; satisfying UI clicks in management screens |
| **Key audio moments** | Student level-up, team victory fanfare, run-end defeat sting, teacher promotion chime |

## Out of Scope

Features explicitly excluded to prevent scope creep:

- Multiplayer
- Story mode / narrative campaign
- Controller support
- Teacher skill trees (teachers are simple persistent buffs in v1)
