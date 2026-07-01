using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicSchool.Battle
{
    using Combatant = AutoBattleResolver.Combatant;

    // Switch-dispatched per-archetype resolution, mirroring TraitEffectApplier's static-
    // class style. Mutation (HP, mana, position, events) stays owned by AutoBattleResolver
    // via its internal helpers — this class only decides what to hit and how much.
    internal static class SkillArchetypeExecutor
    {
        public static void Execute(AutoBattleResolver ctx, Combatant caster)
        {
            switch (caster.Skill?.Archetype ?? SkillArchetype.None)
            {
                case SkillArchetype.StandardProjectile:  ExecuteStandardProjectile(ctx, caster);  break;
                case SkillArchetype.ExplodingProjectile:  ExecuteExplodingProjectile(ctx, caster); break;
                case SkillArchetype.LaserBeam:             ExecuteLaserBeam(ctx, caster);          break;
                case SkillArchetype.GroundAoE:              ExecuteGroundAoE(ctx, caster);           break;
                case SkillArchetype.BouncingChain:           ExecuteBouncingChain(ctx, caster);       break;
                case SkillArchetype.BlinkStrike:              ExecuteBlinkStrike(ctx, caster);         break;
                case SkillArchetype.SelfBuff:                   ExecuteSelfBuff(ctx, caster);            break;
                case SkillArchetype.None: default: break;  // no skill (enemies, unpopulated champions)
            }
        }

        // ── Template A ────────────────────────────────────────────────────────
        private static void ExecuteStandardProjectile(AutoBattleResolver ctx, Combatant caster)
        {
            var target = ctx.GetOccupantAt(caster.PendingTargetHex);
            if (target == null || target.IsPlayer == caster.IsPlayer) return;

            int dmg = ComputeSkillDamage(ctx, caster, target);
            dmg = ctx.ApplyDamageAndCheckKill(caster, target, dmg, out _, tags: new List<string> { "SKILL" });
            Debug.Log($"[Skill] {caster.DisplayName} ({caster.Skill.SkillName}) hits {target.DisplayName}: {dmg} dmg (HP:{target.CurrentHP}/{target.MaxHP})");
            ApplyCrowdControl(target, caster.Skill);
        }

        // ── Template B ────────────────────────────────────────────────────────
        private static void ExecuteExplodingProjectile(AutoBattleResolver ctx, Combatant caster)
        {
            var primary = ctx.GetOccupantAt(caster.PendingTargetHex);
            if (primary == null || primary.IsPlayer == caster.IsPlayer) return;

            int dmg = ComputeSkillDamage(ctx, caster, primary);
            dmg = ctx.ApplyDamageAndCheckKill(caster, primary, dmg, out _, tags: new List<string> { "SKILL" });
            Debug.Log($"[Skill] {caster.DisplayName} ({caster.Skill.SkillName}) hits {primary.DisplayName}: {dmg} dmg (HP:{primary.CurrentHP}/{primary.MaxHP})");
            ApplyCrowdControl(primary, caster.Skill);

            int splashDmg = Math.Max(1, (int)(dmg * caster.Skill.SplashPct));
            foreach (var hex in HexCoord.GetNeighbors(caster.PendingTargetHex, HexGrid.Cols, HexGrid.Rows))
            {
                var splashTarget = ctx.GetOccupantAt(hex);
                if (splashTarget == null || splashTarget.IsPlayer == caster.IsPlayer || splashTarget == primary) continue;
                ctx.ApplyDamageAndCheckKill(caster, splashTarget, splashDmg, out _, tags: new List<string> { "SKILL" });
                Debug.Log($"[Skill] {caster.DisplayName} ({caster.Skill.SkillName}) splash hits {splashTarget.DisplayName}: {splashDmg} dmg (HP:{splashTarget.CurrentHP}/{splashTarget.MaxHP})");
            }
        }

        // ── Template C ────────────────────────────────────────────────────────
        private static void ExecuteLaserBeam(AutoBattleResolver ctx, Combatant caster)
        {
            var path = ctx.Grid.GetLinearPath(caster.Position, caster.PendingTargetHex, caster.Skill.Range);
            foreach (var hex in path)
            {
                var hit = ctx.GetOccupantAt(hex);
                if (hit == null || hit.IsPlayer == caster.IsPlayer) continue;

                int dmg = ComputeSkillDamage(ctx, caster, hit);
                dmg = ctx.ApplyDamageAndCheckKill(caster, hit, dmg, out _, tags: new List<string> { "SKILL" });
                Debug.Log($"[Skill] {caster.DisplayName} ({caster.Skill.SkillName}) pierces {hit.DisplayName}: {dmg} dmg (HP:{hit.CurrentHP}/{hit.MaxHP})");
                ApplyCrowdControl(hit, caster.Skill);
                // GDD: Laser also "shreds defensive stats" of units hit — no generic
                // timed-debuff mechanism exists for this yet (only the Dread Zone's
                // area-based shred is modeled). Known gap if a Laser hero needs it.
            }
        }

        // ── Template D ────────────────────────────────────────────────────────
        private static void ExecuteGroundAoE(AutoBattleResolver ctx, Combatant caster)
        {
            var inRadius = ctx.Grid.GetInRange(caster.PendingTargetHex, caster.Skill.Radius);
            foreach (var hex in inRadius)
            {
                var hit = ctx.GetOccupantAt(hex);
                if (hit == null || hit.IsPlayer == caster.IsPlayer) continue;

                int dmg = ComputeSkillDamage(ctx, caster, hit);
                dmg = ctx.ApplyDamageAndCheckKill(caster, hit, dmg, out _, tags: new List<string> { "SKILL" });
                Debug.Log($"[Skill] {caster.DisplayName} ({caster.Skill.SkillName}) hits {hit.DisplayName}: {dmg} dmg (HP:{hit.CurrentHP}/{hit.MaxHP})");
                ApplyCrowdControl(hit, caster.Skill);
            }

            if (caster.Skill.DreadZoneDefShredPct > 0)
            {
                ctx.AddDreadZone(caster.PendingTargetHex, caster.Skill.Radius, caster.Skill.DreadZoneDefShredPct, caster.Skill.ZoneDurationTicks);
                Debug.Log($"[Skill] {caster.DisplayName} ({caster.Skill.SkillName}) creates a Dread Zone at {caster.PendingTargetHex}: -{caster.Skill.DreadZoneDefShredPct:P0} DEF/MR for {caster.Skill.ZoneDurationTicks} ticks");
            }
        }

        // ── Template E ────────────────────────────────────────────────────────
        private static void ExecuteBouncingChain(AutoBattleResolver ctx, Combatant caster)
        {
            var current = ctx.GetOccupantAt(caster.PendingTargetHex);
            if (current == null || current.IsPlayer == caster.IsPlayer) return;

            var hitIds = new HashSet<string>();
            int bounceCount = Math.Max(1, caster.Skill.BounceCount);

            for (int i = 0; i < bounceCount && current != null; i++)
            {
                hitIds.Add(current.Id);

                int dmg = ComputeSkillDamage(ctx, caster, current);
                dmg = ctx.ApplyDamageAndCheckKill(caster, current, dmg, out _, tags: new List<string> { "SKILL" });
                Debug.Log($"[Skill] {caster.DisplayName} ({caster.Skill.SkillName}) bounce {i + 1} hits {current.DisplayName}: {dmg} dmg (HP:{current.CurrentHP}/{current.MaxHP})");
                ApplyCrowdControl(current, caster.Skill);

                var lastHitPos = current.Position;
                current = ctx.GetOpponentsOf(caster)
                    .Where(c => !hitIds.Contains(c.Id) && HexCoord.Distance(lastHitPos, c.Position) <= caster.Skill.Range)
                    .OrderBy(c => HexCoord.Distance(lastHitPos, c.Position))
                    .FirstOrDefault();
            }
        }

        // ── Template F ────────────────────────────────────────────────────────
        // Loops HitCount times (default 1, matching Shadowblade's single-hit case
        // unchanged). Phantom Assassin's HitCount=3 turns this into a sequential
        // teleport-and-strike flurry against successive lowest-HP% targets, per GDD.
        private static void ExecuteBlinkStrike(AutoBattleResolver ctx, Combatant caster)
        {
            var target = ctx.GetOccupantAt(caster.PendingTargetHex);
            if (target == null || target.IsPlayer == caster.IsPlayer) return;

            HexCoord originPosition = caster.Position;
            int hitCount = Math.Max(1, caster.Skill.HitCount);
            var hitIds = new HashSet<string>();
            var current = target;

            for (int i = 0; i < hitCount && current != null; i++)
            {
                hitIds.Add(current.Id);

                HexCoord? openAdjacent = null;
                foreach (var nb in HexCoord.GetNeighbors(current.Position, HexGrid.Cols, HexGrid.Rows))
                    if (!ctx.Grid.IsOccupied(nb)) { openAdjacent = nb; break; }
                if (openAdjacent.HasValue)
                    ctx.MoveUnit(caster, openAdjacent.Value);

                int dmg = ComputeSkillDamage(ctx, caster, current);
                dmg = ctx.ApplyDamageAndCheckKill(caster, current, dmg, out _, tags: new List<string> { "SKILL" });
                Debug.Log($"[Skill] {caster.DisplayName} ({caster.Skill.SkillName}) hit {i + 1}/{hitCount} strikes {current.DisplayName}: {dmg} dmg (HP:{current.CurrentHP}/{current.MaxHP})");
                ApplyCrowdControl(current, caster.Skill);

                current = ctx.GetOpponentsOf(caster)
                    .Where(c => !hitIds.Contains(c.Id))
                    .OrderBy(c => (float)c.CurrentHP / c.MaxHP)
                    .FirstOrDefault();
            }

            caster.CurrentTargetId = null;  // GDD: "drops current enemy threat (aggro reset)"

            if (caster.Skill.ReturnToOriginAfter && !ctx.Grid.IsOccupied(originPosition))
            {
                ctx.MoveUnit(caster, originPosition);
                Debug.Log($"[Skill] {caster.DisplayName} returns to {originPosition}");
            }
        }

        // ── Template G ────────────────────────────────────────────────────────
        private static void ExecuteSelfBuff(AutoBattleResolver ctx, Combatant caster)
        {
            var skill = caster.Skill;

            if (skill.FlatShieldAmount > 0 || skill.ShieldPctOfMaxHP > 0)
            {
                int shieldAmount = (int)(skill.FlatShieldAmount + caster.MaxHP * skill.ShieldPctOfMaxHP);
                caster.Shield += shieldAmount;
                Debug.Log($"[Skill] {caster.DisplayName} ({skill.SkillName}) gains {shieldAmount} shield → {caster.Shield} total");
            }

            if (skill.InterceptPct > 0)
            {
                // Phalanx's "Intercepts 30% of damage taken by adjacent allies" — tied
                // to the same DurationTicks window as the shield/Taunt, ticked down in
                // BattleLoop's Phase 0 alongside stun/silence.
                caster.InterceptPct = skill.InterceptPct;
                caster.InterceptTicksRemaining = skill.DurationTicks;
                Debug.Log($"[Skill] {caster.DisplayName} ({skill.SkillName}) now intercepts {skill.InterceptPct:P0} of adjacent allies' damage for {skill.DurationTicks} ticks");
            }

            if (skill.AttackSpeedBuffPct > 0)
            {
                // No generic timed-buff/restore framework exists yet (Trait System's
                // AttackSpeedMult is a permanent pre-battle modifier, not time-limited).
                // Applied as a standing multiplier; expiry is a known gap for a future pass.
                caster.AttackSpeed *= (1f + skill.AttackSpeedBuffPct);
            }

            // Taunt/Root self-buffs (e.g. Phalanx's radius Taunt) apply CC to nearby
            // enemies rather than self — but forced-attack-target CC isn't modeled
            // generically yet. Handled as Phalanx's own special-case pass (Step 16a)
            // for the intercept portion; Taunt's forced-targeting is a known gap.
        }

        // ── Shared helpers ────────────────────────────────────────────────────
        private static int ComputeSkillDamage(AutoBattleResolver ctx, Combatant caster, Combatant target)
        {
            var skill = caster.Skill;
            float rawOffense = (skill.UsesMagicOffense ? caster.MG : caster.ATK) * skill.OffenseMultiplier
                              + caster.DEF * skill.SecondaryMultiplier;
            int rawDefense = ctx.GetZoneShreddedDefense(target, skill.UsesMagicOffense ? target.MR : target.DEF);
            return Math.Max(1, (int)(rawOffense * (100f / (100 + rawDefense))));
        }

        private static void ApplyCrowdControl(Combatant target, SkillDefinition skill)
        {
            switch (skill.CrowdControl)
            {
                case CcType.Stun:
                    target.IsStunned = true;
                    target.StunTicksRemaining = Math.Max(target.StunTicksRemaining, skill.DurationTicks);
                    break;
                case CcType.Silence:
                    target.IsSilenced = true;
                    target.SilenceTicksRemaining = Math.Max(target.SilenceTicksRemaining, skill.DurationTicks);
                    break;
                case CcType.Root:
                case CcType.Taunt:
                    // Not modeled generically yet — Root/Taunt need movement/forced-
                    // targeting hooks that don't exist. Known gap.
                    break;
                case CcType.None:
                default:
                    break;
            }
        }
    }
}
