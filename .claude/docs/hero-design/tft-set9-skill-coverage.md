# TFT Set 9 → SkillDefinition Coverage Audit

## Purpose

This doc checks how far our active-skill schema — [`SkillDefinition.cs`](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/Assets/Scripts/Battle/Skills/SkillDefinition.cs), [`SkillTargetSelector.cs`](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/Assets/Scripts/Battle/Skills/SkillTargetSelector.cs), and [`SkillArchetypeExecutor.cs`](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/Assets/Scripts/Battle/Skills/SkillArchetypeExecutor.cs) — can actually reproduce real champion kits, using all 67 champions on the `tft-set9` reference sheet's **Champions** tab as the test set. It is a schema/engine capability audit, not a request to implement any of these champions.

**Source data:** `.claude/reference/json/tft-set9_dump_20260711.json` (`dumped_at: 2026-07-11T14:43:25`), the `Champions` tab, `Skill Description` column. Re-run `python .claude/scripts/sheet_sync.py --sheet tft-set9 dump` and re-audit if that sheet changes.

**Per-champion results:** see [TFT Set 9 Skill Coverage — Full Table](file:///c:/Organized%20Files/Working/Unity/Unity%20Project/Magic%20School/.claude/docs/hero-design/tft-set9-skill-coverage-table.md), sorted by champion cost (1→5).

## Legend

- ✅ **Usable as-is** — maps cleanly onto one existing `SkillArchetype` + `SkillDefinition` fields, nothing meaningful lost.
- ⚠️ **Partial** — the core action (a hit, a shield, a stun, a heal) is representable, but a real part of the kit is dropped or approximated (a status effect, a conditional, a passive layer, a stacking/escalation mechanic, falloff, etc.).
- ❌ **Not representable** — the kit's defining mechanic needs something the schema/executor has no field or code path for at all (transform, summon, revive, displacement, stat-drain, row/column targeting, a fully passive kit with no real cast).

## Summary

| Verdict | Count |
|---|---|
| ✅ Usable as-is | 4 |
| ⚠️ Partial | 49 |
| ❌ Not representable | 14 |

### Recurring gap themes (most to least common)

1. **No passive / on-attack trigger system.** `SkillDefinition` models exactly one active cast per full mana bar. Any kit whose identity is a passive proc, an on-attack modifier, or a level-gated escalation (Kayle, Kled, Zeri, Maokai, and the passive half of Aphelios, Azir, Kalista, Nasus, Nilah, Sejuani, Taliyah, Urgot, Viego, Warwick) loses that layer entirely — this is the single biggest gap by champion count.
2. **`SelfBuff` (Template G) can't also hit enemies.** It only buffs the caster and, optionally, one ally target. Real kits that want "shield/buff self **and** damage or CC nearby enemies in the same cast" (Poppy, Sejuani, Vi, Warwick, Kassadin, and the two-phase Galio/Irelia/Neeko pattern) can't be expressed as one archetype call.
3. **No generic status effects beyond Stun/Silence.** Wound (heal reduction), Chill (AS reduction), Disarm, Sunder (Armor reduction), Shred (MR reduction, outside the hardcoded per-hero `DreadZone`), and "+X% damage taken" vulnerability all appear on real kits with no `CcType` or field to carry them. `Root`/`Taunt` exist in the `CcType` enum but are explicit no-ops per `SkillArchetypeExecutor.cs:364-368`.
4. **No conditional/branching damage formulas.** "If already Wounded, +30% damage", "if target is below 66% HP, deal true damage instead", "if only one enemy is grabbed, +50%" all require a runtime condition on the damage formula that doesn't exist — `ComputeSkillDamage` is a fixed linear formula.
5. **No damage-over-time / ticking zones.** `GroundAoE` and friends resolve instantly. Kits with an explicit "...over N seconds" damage window (Teemo, Silco) collapse to one instant hit.
6. **No per-hit damage falloff on multi-hit/pierce skills.** `LaserBeam` and the multi-hit loops deal full `ComputeSkillDamage` to every target hit; real kits (Jhin, Sona, Vel'Koz) explicitly reduce damage per hit or per pass.
7. **No target-max-HP-scaling damage term.** `ShieldPctOfMaxHP` exists for shields; there is no damage equivalent, so kits like Cho'Gath (`+12% target's max Health`) can't scale correctly.
8. **No true damage / mitigation-bypass flag.** `ComputeSkillDamage` always runs damage through the target's DEF/MR curve.
9. **No multi-cast counters.** "Every 3rd cast...", "every 2 casts..." escalations (Ahri, Karma, Kalista's passive, Sona-adjacent) need cast-count state that doesn't exist on `SkillDefinition` or `Combatant`.
10. **No displacement, summons, transforms, or revives.** The largest structural absences — several kits (Aatrox, Azir, K'Sante, Kled, Maokai, Nasus, Quinn, Rek'Sai, Silco, Sion's passive, Swain, Zed, Zeri) are built entirely around one of these and have no partial equivalent in the engine at all.
