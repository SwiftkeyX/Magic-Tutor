# PIPELINE.md

## Phase 1 — Pre-production

- [x] Fill out `docs/preproduction/game-vision.md`
- [x] Fill out `docs/preproduction/design-decisions.md`
- [x] Fill out `docs/preproduction/technical-preferences.md` (engine, platform, performance budgets)
- [x] Fill out `docs/preproduction/systems-design.md` — list every system, tier, and dependencies
- [x] Fill out `docs/preproduction/architecture.md` with finalized script table
- [x] Fill out `docs/preproduction/best-practices.md` — add project-critical patterns section
- [x] Milestone 0 — vision complete, all systems tiered, architecture and tech stack finalized

## Prototype — Playable Proof of Concept

Rapid prototype on feature branches to validate the core loop before committing to full GDD-gated production. Each milestone ships independently.

- [x] **Milestone 1 — Battle Grid** — hex board, drag-drop placement, AutoBattleResolver tick simulation, VICTORY/DEFEAT overlay (`feat/battle-grid-milestone1`)
- [x] **Milestone 2 — Trait System** — 4 traits wired into AutoBattleResolver (MG/MR/CRIT/Flags); requires deep research first
- [ ] **Milestone 3 — Training System** — deferred; design TBD
- [ ] **Prototype Exit** — all 3 milestones playable; core loop validated → Phase 2 unlocks

---

## Phase 2 — Production

### Sub-phase A — Design (GDDs)

#### Tier 1 — Foundation GDDs
- [x] Design `GameManager`
- [x] Design `SceneLoader`
- [x] Design `InputHandler`
- [x] Design `SaveSystem`

#### Tier 2 — Core Loop GDDs
- [x] Design `RunManager`
- [x] Design `StudentRoster`
- [x] Design `TrainingSystem` _(retroactive — document what was built in Prototype)_
- [x] Design `TraitSystem` _(retroactive — document what was built in Prototype)_
- [x] Design `AutoBattleResolver` _(retroactive — document what was built in Prototype)_
- [x] Design `EnemyDatabase`
- [x] Design `PromotionSystem`
- [x] Design `TeacherRoster`

#### Tier 3 — Supporting GDDs
- [x] Design `AudioSystem`
- [x] Design `MainMenuController`
- [x] Design `SchoolHUD`
- [x] Design `BattleHUD`
- [x] Design `YearEndHUD`
- [x] Design `MetaProgressionHUD`

- [x] Milestone 1 — all system GDDs written and approved

### Sub-phase B — Implementation

#### Tier 1 — Foundation
- [x] Code `GameManager`
- [x] Code `SceneLoader`
- [x] Code `InputHandler`
- [x] Code `SaveSystem`
- [x] 🧪 Test Gate 1 — Foundation wired; game launches, Bootstrap scene loads, no console errors

#### Tier 2 — Core Loop

> `AutoBattleResolver`, `TraitSystem`, and `TrainingSystem` were built during the Prototype phase. Their GDDs (Sub-phase A) must be written before the Architecture pass.

- [x] Code `RunManager`
- [x] Code `StudentRoster`
- [x] Code `EnemyDatabase`
- [x] Code `PromotionSystem`
- [x] Code `TeacherRoster`
- [x] Code `ActiveSkillSystem`
  - [x] Base system (7 templates, mana, lockout)
  - [x] Champion roster wiring: Batch 1 [x], Batch 2 [x], Batch 3 [x], Batch 4 [x], Batch 5 [x]
- [x] Milestone 2 — core loop playable end-to-end (run starts, 3 years cycle, win/lose resolves)
- [x] 🧪 Test Gate 2 — One full 3-year run completes without errors

#### Tier 3 — Supporting Systems
- [x] Code `AudioSystem`
- [x] Code `MainMenuController`
- [x] Code `SchoolHUD`
- [x] Code `BattleHUD`
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
