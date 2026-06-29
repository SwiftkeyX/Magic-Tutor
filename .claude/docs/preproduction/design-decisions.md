# Design Decisions

> Core mechanic constraints, explicit non-goals, and scope boundaries derived from `game-vision.md`. These constrain how Claude names and implements things in code. Only add an entry when a decision is finalized AND it affects code structure or naming.

## Named Terms (Glossary)

| Term | Definition |
|---|---|
| **Principal** | The player character — the school director. Non-combatant; manages training and promotion. |
| **Student** | A combat unit trained during a semester. Ephemeral — resets or is promoted at year end. |
| **Teacher** | A promoted Student. Persistent meta-progression. Provides passive utility, never fights. |
| **Semester** | One in-run year cycle: recruit → train → battle → promote. 3 semesters = 1 full run. |
| **Run** | A single playthrough from Year 1 to Year 3 (or until team loss). |
| **Auto-battle** | Combat phase where Students fight enemies with no real-time player input. |
| **Training action** | The primary gameplay verb: allocating training sessions to Students to raise their stats. |
| **Trait** | A tag attached to each Student (e.g., Fire, Healer, Shield). Accumulating Students with the same Trait past a threshold unlocks a Trait ability for the team. |
| **Trait threshold** | The minimum number of same-Trait Students required to activate a Trait's ability (mirrors TFT synergy breakpoints). |

## Core Mechanic Constraints

| Decision | Constraint | Why |
|---|---|---|
| Principal is non-combatant | The Principal character never directly fights enemies | Keeps management and combat as distinct layers — fighting would blur the strategy/payoff separation |
| Auto-battle only | Players have zero input during the battle phase | Battle is a payoff moment: you watch what your training produced, not a reflex challenge |
| Permadeath per run | Any team loss ends the run immediately | Gives every training decision real stakes; preserves roguelike tension |
| Teachers persist across runs | Teachers earned before a run's end carry over to future runs | Creates the ratchet effect — failed runs still contribute to long-term progress |
| Students reset each run | All untrained/undrafted Students are discarded when a run ends | Ensures each run starts fresh with new random recruits; prevents indefinite power accumulation |
| Teachers never fight | In v1, Teachers provide passive buffs/utility only — no combat role | Keeps the meta-progression layer simple and distinct from the in-run combat layer |
| Fixed 3-year run | Runs are exactly 3 Semesters; there is no open-ended survival mode | Gives players a concrete goal and allows tuned difficulty pacing per year |
| Random student pool | Each Semester draws a random set of new Students | Forces adaptation and replayability; players cannot plan around a fixed roster |
| Training is the primary verb | Core player action = allocating Training actions to Students | Grounds the game in management feel (not reflex or real-time skill) |
| Trait system drives team synergy | Each Student carries one or more Traits; fielding enough same-Trait Students past a threshold unlocks that Trait's ability (TFT-style) | Auto-battle teams need a mechanic for inter-unit interaction; Traits enable diverse builds, combos, and emergent strategies without requiring real-time control |

## Scope Boundaries (In vs. Out)

| Feature | Status | Note |
|---|---|---|
| Auto-battle combat | **IN** | Core loop |
| Student stat training (Uma Musume-style) | **IN** | Primary gameplay verb |
| Teacher meta-progression (passive buffs) | **IN** | Cross-run persistence |
| Random student recruitment per semester | **IN** | Core roguelite variance |
| Year-end promotion/discard decision | **IN** | Main strategic choice |
| Trait system (TFT-style synergies) | **IN** | Core team-building mechanic — enables build diversity and combos |
| 2D art | **IN** | All assets are 2D |
| Multiplayer | **OUT** | Explicitly excluded v1 |
| Story mode / narrative campaign | **OUT** | Explicitly excluded v1 |
| Controller support | **OUT** | Keyboard/mouse only v1 |
| Teacher combat roles | **OUT** | Teachers are utility-only in v1 |
| Teacher skill trees | **OUT** | Teachers are simple passive buffs in v1 |
| 3D assets | **OUT** | 2D throughout |

## Non-Goals

- This game is **not** a real-time strategy game — the player never controls units directly in combat
- This game is **not** a narrative RPG — there is no story campaign or dialogue system
- This game is **not** a multiplayer game — solo experience only
