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
        // When TargetTeam = Ally (e.g. Novice Cleric), heals/cleanses the ally at
        // PendingTargetHex instead of dealing damage.
        private static void ExecuteStandardProjectile(AutoBattleResolver ctx, Combatant caster)
        {
            var skill = caster.Skill;
            var occupant = ctx.GetOccupantAt(caster.PendingTargetHex);

            if (skill.TargetTeam == TargetTeam.Ally)
            {
                // Ally path — target must be on the same team (set by TriggerCast via TargetTeam.Ally)
                if (occupant == null || occupant.IsPlayer != caster.IsPlayer) return;
                ApplyAllySupportResolution(ctx, caster, occupant);
                return;
            }

            // Original enemy-damage path (all 10 launch champions, TargetTeam.Enemy default)
            if (occupant == null || occupant.IsPlayer == caster.IsPlayer) return;
            int dmg = ComputeSkillDamage(ctx, caster, occupant);
            dmg = ctx.ApplyDamageAndCheckKill(caster, occupant, dmg, out _, tags: new List<string> { "SKILL" });
            Debug.Log($"[Skill] {caster.DisplayName} ({skill.SkillName}) hits {occupant.DisplayName}: {dmg} dmg (HP:{occupant.CurrentHP}/{occupant.MaxHP})");
            ApplyCrowdControl(occupant, skill);
        }

        // ── Template B ────────────────────────────────────────────────────────
        // Primary target is always an enemy (damage unchanged).
        // If ManaRestoreAmount > 0, the splash radius also restores mana to adjacent
        // allies instead of dealing splash damage (Cosmic Sprite GDD #13).
        private static void ExecuteExplodingProjectile(AutoBattleResolver ctx, Combatant caster)
        {
            var primary = ctx.GetOccupantAt(caster.PendingTargetHex);
            if (primary == null || primary.IsPlayer == caster.IsPlayer) return;

            int dmg = ComputeSkillDamage(ctx, caster, primary);
            dmg = ctx.ApplyDamageAndCheckKill(caster, primary, dmg, out _, tags: new List<string> { "SKILL" });
            Debug.Log($"[Skill] {caster.DisplayName} ({caster.Skill.SkillName}) hits {primary.DisplayName}: {dmg} dmg (HP:{primary.CurrentHP}/{primary.MaxHP})");
            ApplyCrowdControl(primary, caster.Skill);

            // Enemy splash damage — gated on SplashPct > 0 to avoid 1-damage ghost hits
            // when a champion has no splash (e.g. Cosmic Sprite).
            if (caster.Skill.SplashPct > 0)
            {
                int splashDmg = Math.Max(1, (int)(dmg * caster.Skill.SplashPct));
                foreach (var hex in HexCoord.GetNeighbors(caster.PendingTargetHex, HexGrid.Cols, HexGrid.Rows))
                {
                    var splashTarget = ctx.GetOccupantAt(hex);
                    if (splashTarget == null || splashTarget.IsPlayer == caster.IsPlayer || splashTarget == primary) continue;
                    ctx.ApplyDamageAndCheckKill(caster, splashTarget, splashDmg, out _, tags: new List<string> { "SKILL" });
                    Debug.Log($"[Skill] {caster.DisplayName} ({caster.Skill.SkillName}) splash hits {splashTarget.DisplayName}: {splashDmg} dmg (HP:{splashTarget.CurrentHP}/{splashTarget.MaxHP})");
                }
            }

            // Ally mana-restore splash (GDD #13 Cosmic Sprite: restores ManaRestoreAmount
            // mana to adjacent allies after the enemy explosion).
            if (caster.Skill.ManaRestoreAmount > 0)
            {
                foreach (var hex in HexCoord.GetNeighbors(caster.PendingTargetHex, HexGrid.Cols, HexGrid.Rows))
                {
                    var allyTarget = ctx.GetOccupantAt(hex);
                    if (allyTarget == null || allyTarget.IsPlayer != caster.IsPlayer) continue;
                    ctx.GrantMana(allyTarget, caster.Skill.ManaRestoreAmount);
                    Debug.Log($"[Skill] {caster.DisplayName} ({caster.Skill.SkillName}) restores {caster.Skill.ManaRestoreAmount} mana to ally {allyTarget.DisplayName} → {allyTarget.Mana}/{allyTarget.MaxMana}");
                }
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
        // AttackSpeedBuffPct (if set) is applied after all hits — Wildcat's post-blink AS buff.
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

            // Post-blink AS buff (mirrors ExecuteSelfBuff's standing-multiplier pattern).
            // No generic timed-buff expiry framework exists yet — known gap for a future pass.
            if (caster.Skill.AttackSpeedBuffPct > 0)
            {
                caster.AttackSpeed *= (1f + caster.Skill.AttackSpeedBuffPct);
                Debug.Log($"[Skill] {caster.DisplayName} ({caster.Skill.SkillName}) gains +{caster.Skill.AttackSpeedBuffPct:P0} Attack Speed → {caster.AttackSpeed:F3}/s");
            }
        }

        // ── Template G ────────────────────────────────────────────────────────
        // Self-buff: applies shield / intercept / AS buff to caster.
        // If SecondaryFilter is set, also resolves an ally target and calls
        // ApplyAllySupportResolution on it (e.g. Aegis shields self AND adjacent ally).
        private static void ExecuteSelfBuff(AutoBattleResolver ctx, Combatant caster)
        {
            var skill = caster.Skill;

            // Shield self — combines all three shield-source fields (flat, %MaxHP, DEF-based)
            if (skill.FlatShieldAmount > 0 || skill.ShieldPctOfMaxHP > 0 || skill.ShieldDefMultiplier > 0)
            {
                int shieldAmount = (int)(skill.FlatShieldAmount
                    + caster.MaxHP * skill.ShieldPctOfMaxHP
                    + caster.DEF   * skill.ShieldDefMultiplier);
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

            // Optional secondary ally target (Template G, e.g. Aegis: "shields self AND
            // lowest-HP%-adjacent-ally"). SecondaryFilter is always resolved against
            // TargetTeam.Ally (the design intent — secondary targets are always own-team).
            if (skill.SecondaryFilter.HasValue)
            {
                var secondaryHexes = SkillTargetSelector.SelectTargetHexes(
                    ctx.Grid, caster, ctx.AllCombatants,
                    skill.SecondaryFilter.Value,
                    skill.SecondarySorts ?? new TargetPrioritySort[0],
                    skill.Range, skill.Radius,
                    TargetTeam.Ally);

                if (secondaryHexes.Count > 0)
                {
                    var ally = ctx.GetOccupantAt(secondaryHexes[0]);
                    // Must be a living ally and not the caster itself (caster was already buffed above)
                    if (ally != null && ally.IsPlayer == caster.IsPlayer && ally != caster)
                        ApplyAllySupportResolution(ctx, caster, ally);
                }
            }

            // Taunt/Root self-buffs (e.g. Phalanx's radius Taunt) apply CC to nearby
            // enemies rather than self — but forced-attack-target CC isn't modeled
            // generically yet. Handled as Phalanx's own special-case pass (Step 16a)
            // for the intercept portion; Taunt's forced-targeting is a known gap.
        }

        // ── Ally-Support Resolution ───────────────────────────────────────────
        // Mirrors ComputeSkillDamage's shape but for healing/support effects.
        // Called when TargetTeam = Ally (ExecuteStandardProjectile) or when a
        // SecondaryFilter is set (ExecuteSelfBuff).
        private static void ApplyAllySupportResolution(AutoBattleResolver ctx, Combatant caster, Combatant target)
        {
            var skill = caster.Skill;

            // Shield ally — same three-source combination as self-shield in ExecuteSelfBuff,
            // but targeting the ally (ShieldPctOfMaxHP uses target.MaxHP, as per GDD:
            // "apply to the target ally instead of the caster").
            int shieldAmt = (int)(caster.DEF * skill.ShieldDefMultiplier
                                + skill.FlatShieldAmount
                                + target.MaxHP * skill.ShieldPctOfMaxHP);
            if (shieldAmt > 0)
            {
                target.Shield += shieldAmt;
                Debug.Log($"[Skill] {caster.DisplayName} ({skill.SkillName}) shields {target.DisplayName} for {shieldAmt} → {target.Shield} total shield");
            }

            // Heal ally — caster.MG × HealMultiplier, capped at MaxHP
            if (skill.HealMultiplier > 0)
            {
                int healAmt = (int)(caster.MG * skill.HealMultiplier);
                target.CurrentHP = Math.Min(target.MaxHP, target.CurrentHP + healAmt);
                Debug.Log($"[Skill] {caster.DisplayName} ({skill.SkillName}) heals {target.DisplayName} for {healAmt} → HP:{target.CurrentHP}/{target.MaxHP}");
            }

            // Mana restore — flat grant (GrantMana respects IsSilenced and fires OnManaChanged)
            if (skill.ManaRestoreAmount > 0)
            {
                ctx.GrantMana(target, skill.ManaRestoreAmount);
                Debug.Log($"[Skill] {caster.DisplayName} ({skill.SkillName}) restores {skill.ManaRestoreAmount} mana to {target.DisplayName} → {target.Mana}/{target.MaxMana}");
            }

            // Cleanse — removes IsStunned / IsSilenced + their tick counters
            if (skill.CleansesStatus)
            {
                bool wasCleansed = target.IsStunned || target.IsSilenced;
                target.IsStunned            = false;
                target.StunTicksRemaining   = 0;
                target.IsSilenced           = false;
                target.SilenceTicksRemaining = 0;
                Debug.Log(wasCleansed
                    ? $"[Skill] {caster.DisplayName} ({skill.SkillName}) cleanses stun/silence from {target.DisplayName}"
                    : $"[Skill] {caster.DisplayName} ({skill.SkillName}) cleanse on {target.DisplayName}: no active CC to remove");
            }
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
