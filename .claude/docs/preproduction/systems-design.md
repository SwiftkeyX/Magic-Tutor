# Systems Design

> List every system in the game, its single responsibility, what it depends on, and which tier it belongs to. Fill this out as part of Phase 1 before writing any code.

## Systems Table

| System | Responsibility | Depends On | Tier |
|---|---|---|---|
| GameManager | Singleton; owns game state enum (MainMenu, InRun, RunEnd) and active run data | — | 1 |
| SceneLoader | All scene transitions; no scene loads happen outside this system | GameManager | 1 |
| InputHandler | Reads Unity Input System; exposes input events to all other systems | — | 1 |
| SaveSystem | Serializes/loads persistent data (teacher roster, run stats) to disk | — | 1 |
| RunManager | Drives the semester loop (recruit→train→battle→year-end, 3 years); transitions phases and years | GameManager, SceneLoader | 2 |
| StudentRoster | Manages active student list; handles random recruitment draw, stat storage, and removal | RunManager | 2 |
| TrainingSystem | Training-action allocation mechanic; modifies student stats per allocated training sessions | StudentRoster | 2 |
| TraitSystem | Assigns traits to students, tracks team trait counts, fires threshold events, applies trait abilities | StudentRoster | 2 |
| AutoBattleResolver | Simulates auto-battle (student stats + trait abilities vs enemy stats); returns win/lose result | TraitSystem, EnemyDatabase | 2 |
| ActiveSkillSystem | Defines mana, spellcasting, and active abilities for all 30 champions | AutoBattleResolver | 2 |
| EnemyDatabase | Defines enemy types and scales their stats per year; feeds data to AutoBattleResolver | RunManager | 2 |
| PromotionSystem | Year-end screen logic; player chooses promotions → creates teacher records, discards remaining students | StudentRoster, TeacherRoster | 2 |
| TeacherRoster | Manages persistent teacher collection across runs; computes and exposes aggregate teacher buffs | SaveSystem | 2 |
| AudioSystem | Music and SFX playback; AudioSource pooling, clip management | GameManager | 3 |
| MainMenuController | Main menu screen logic (new run, settings, quit) | GameManager, SceneLoader | 3 |
| SchoolHUD | School management phase UI (student list, training actions, trait display, year progress) | StudentRoster, TrainingSystem, TraitSystem | 3 |
| BattleHUD | Auto-battle view UI (HP bars, damage numbers, outcome screen) | AutoBattleResolver, BattleBoardManager | 3 |
| BattleBoardManager | Renders hex grid, handles pre-battle drag-drop placement, drives unit visual movement on board | AutoBattleResolver | 3 |
| YearEndHUD | Promotion/discard screen UI | PromotionSystem | 3 |
| MetaProgressionHUD | Teacher collection and run history display | TeacherRoster | 3 |

## Tier Definitions

| Tier | Label | Must work before… |
|---|---|---|
| 1 | Foundation | Any gameplay can be tested |
| 2 | Core Loop | Win/lose is reachable end-to-end |
| 3 | Supporting | Content is complete and game is shippable |
