# PIPELINE.md

## Phase 1 — Pre-production

- [x] Fill out `docs/preproduction/game-vision.md`
- [x] Fill out `docs/preproduction/design-decisions.md`
- [x] Fill out `docs/preproduction/technical-preferences.md` (engine, platform, performance budgets)
- [x] Fill out `docs/preproduction/systems-design.md` — list every system, tier, and dependencies
- [x] Fill out `docs/preproduction/architecture.md` with finalized script table
- [x] Fill out `docs/preproduction/best-practices.md` — add project-critical patterns section
- [x] Milestone 0 — vision complete, all systems tiered, architecture and tech stack finalized

## Phase 2 — Production

### Sub-phase A — Design (GDDs)

#### Tier 1 — Foundation GDDs
- [ ] Design `GameManager`
- [ ] Design `SceneLoader`
- [ ] Design `InputHandler`
- [ ] Design `SaveSystem`

#### Tier 2 — Core Loop GDDs
- [ ] Design `RunManager`
- [ ] Design `StudentRoster`
- [ ] Design `TrainingSystem`
- [ ] Design `TraitSystem`
- [ ] Design `AutoBattleResolver`
- [ ] Design `EnemyDatabase`
- [ ] Design `PromotionSystem`
- [ ] Design `TeacherRoster`

#### Tier 3 — Supporting GDDs
- [ ] Design `AudioSystem`
- [ ] Design `MainMenuController`
- [ ] Design `SchoolHUD`
- [ ] Design `BattleHUD`
- [ ] Design `YearEndHUD`
- [ ] Design `MetaProgressionHUD`

- [ ] Milestone 1 — all system GDDs written and approved

### Sub-phase B — Implementation

#### Tier 1 — Foundation
- [ ] Code `GameManager`
- [ ] Code `SceneLoader`
- [ ] Code `InputHandler`
- [ ] Code `SaveSystem`
- [ ] 🧪 Test Gate 1 — Foundation wired; game launches, Bootstrap scene loads, no console errors

#### Tier 2 — Core Loop
- [ ] Code `RunManager`
- [ ] Code `StudentRoster`
- [ ] Code `TrainingSystem`
- [ ] Code `TraitSystem`
- [ ] Code `AutoBattleResolver`
- [ ] Code `EnemyDatabase`
- [ ] Code `PromotionSystem`
- [ ] Code `TeacherRoster`
- [ ] Milestone 2 — core loop playable end-to-end (run starts, 3 years cycle, win/lose resolves)
- [ ] 🧪 Test Gate 2 — One full 3-year run completes without errors

#### Tier 3 — Supporting Systems
- [ ] Code `AudioSystem`
- [ ] Code `MainMenuController`
- [ ] Code `SchoolHUD`
- [ ] Code `BattleHUD`
- [ ] Code `YearEndHUD`
- [ ] Code `MetaProgressionHUD`
- [ ] Milestone 3 — all features in, content complete
- [ ] 🧪 Test Gate 3 — All systems integrated; full game playable start-to-finish

### Production Exit
- [ ] Architecture pass — code matches GDDs (via `/architecture-pass`)

## Phase 3 — Beta

- [ ] Juice pass — screen shake, particles, hit-stop, SFX, music, UI animations
- [ ] Feel tuning — tweak values via ScriptableObjects/Inspector
- [ ] Difficulty tuning — curve, pacing, escalation
- [ ] Bug pass — all known issues fixed (`docs/process/known-issues.md` clear)
- [ ] Performance pass — GC allocs and frame rate within budgets (`docs/technical/technical-preferences.md`)
- [ ] Ship — final build, smoke test, release (`docs/process/build-notes.md` checklist)
