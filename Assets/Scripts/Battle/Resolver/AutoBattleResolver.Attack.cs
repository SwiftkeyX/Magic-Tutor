using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicSchool.Battle
{
    // Basic-attack resolution, the skill-cast lifecycle (trigger/resolve), and the
    // kill/death handoff to Traits/TraitAbilityExecutor.
    public partial class AutoBattleResolver
    {
        private void Attack(Combatant actor, Combatant target)
        {
            actor.CurrentTargetId = target.Id;

            bool isMagic   = actor.Flags != null && actor.Flags.Contains(BattleBehaviorFlag.MagicAttack);
            int rawOffense = isMagic ? actor.MG  : actor.ATK;
            int rawDefense = GetZoneShreddedDefense(target, isMagic ? target.MR : target.DEF);
            int preMitDmg  = rawOffense;  // captured before mitigation for mana-on-hit

            if (!isMagic && actor.Striker.PctPerStack > 0)
            {
                float stackMult = 1f + actor.Striker.Stacks * actor.Striker.PctPerStack;
                rawOffense = (int)(rawOffense * stackMult);
                if (actor.Striker.BypassArmor) rawDefense = (int)(rawDefense * 0.6f);
            }

            int damage = CombatMath.ApplyMitigation(rawOffense, rawDefense);

            if (!isMagic && actor.Ranger.BonusDmgPerHex > 0)
            {
                int dist = HexCoord.Distance(actor.Position, target.Position);
                damage = (int)(damage * (1f + actor.Ranger.BonusDmgPerHex * dist));
            }

            if (!isMagic && actor.Striker.PctPerStack > 0)
            {
                actor.Striker.Stacks = Math.Min(8, actor.Striker.Stacks + 1);
                Debug.Log($"[Trait][Striker] {actor.DisplayName} stack {actor.Striker.Stacks}/8");
            }

            damage = ApplyDamageAndCheckKill(actor, target, damage, out int shieldAbsorbed, tags: null, autoHandleKill: false);

            int totalRaw = damage + shieldAbsorbed;
            if (actor.OmnivampPct > 0 && totalRaw > 0)
                actor.CurrentHP = Math.Min(actor.MaxHP, actor.CurrentHP + (int)(totalRaw * actor.OmnivampPct));

            if (actor.Trickster.BleedNextAttack && actor.Trickster.BleedEnabled)
            {
                target.BleedDamagePerTick  = Math.Max(1, (int)(target.MaxHP * 0.25f / 42));
                target.BleedTicksRemaining = 42;  // 4.2s at 0.1s/tick (was 7 at 0.6s/tick)
                actor.Trickster.BleedNextAttack = false;
                Debug.Log($"[Trait] {target.DisplayName} afflicted with bleed by {actor.DisplayName}");
            }

            Debug.Log($"[AutoBattle] {actor.DisplayName} → {target.DisplayName}: {damage} dmg (HP:{target.CurrentHP}/{target.MaxHP})");
            OnCombatantActed?.Invoke(actor.Id, target.Id, damage, new List<string>());

            GrantAttackMana(actor, target, preMitDmg);
            CheckCastTriggers(actor, target);

            if (target.IsDefeated)
                HandleKill(actor, target);
        }

        // Mana-on-attack (attacker, +10/hit) and mana-on-hit (defender, GDD formula:
        // max(1, (preMitDmg * 0.08) / MaxHP) * 100, capped at 40). Silenced units
        // generate no mana at all (GDD: "cannot cast skills and cannot generate mana").
        private void GrantAttackMana(Combatant actor, Combatant target, int preMitDmg)
        {
            if (!actor.IsSilenced)
            {
                SetMana(actor, actor.Mana + 10);
                Debug.Log($"[Mana] {actor.DisplayName} +10 atk → {actor.Mana}/{actor.MaxMana}");
            }
            if (!target.IsSilenced)
            {
                int manaGain = Mathf.Min(40, Mathf.Max(1, Mathf.RoundToInt(preMitDmg * 0.08f / target.MaxHP * 100f)));
                SetMana(target, target.Mana + manaGain);
                Debug.Log($"[Mana] {target.DisplayName} +{manaGain} def (preMitDmg:{preMitDmg}, MaxHP:{target.MaxHP}) → {target.Mana}/{target.MaxMana}");
            }
        }

        // Cast trigger — consume mana (reset to 0, plus any overflow past MaxMana per
        // GDD) before entering the Casting state, so a unit already mid-cast can't be
        // re-triggered by mana granted during its own attack resolution.
        private void CheckCastTriggers(Combatant actor, Combatant target)
        {
            if (actor.Mana >= actor.MaxMana && actor.State == CastState.Idle)
            {
                SetMana(actor, actor.Mana - actor.MaxMana);
                TriggerCast(actor);
            }
            if (!target.IsDefeated && target.Mana >= target.MaxMana && target.State == CastState.Idle)
            {
                SetMana(target, target.Mana - target.MaxMana);
                TriggerCast(target);
            }
        }

        // Mana capped — lock the target (or aim hex) in now, using the real
        // SkillTargetSelector when the caster has skill data, so casts/projectiles
        // resolve against whoever occupies that hex later even if the original
        // target moved or died. Falls back to the original placeholder (first-in-
        // range) for combatants with no Skill data yet (e.g. enemies).
        private void TriggerCast(Combatant caster)
        {
            var skill = caster.Skill;
            HexCoord? aimHex;

            if (skill != null && skill.Archetype == SkillArchetype.SelfBuff)
            {
                aimHex = caster.Position;  // GDD: Self-Buff templates target self, not an enemy hex
            }
            else if (skill != null && skill.Archetype != SkillArchetype.None)
            {
                // Pass skill.TargetTeam so ally-scoped skills (e.g. Novice Cleric) aim at
                // the correct team. TargetTeam defaults to Enemy, preserving all existing behavior.
                var hexes = SkillTargetSelector.SelectTargetHexes(_grid, caster, _combatants, skill.BaseFilter, skill.Sorts, skill.Range, skill.Radius, skill.TargetTeam);
                aimHex = hexes.Count > 0 ? hexes[0] : (HexCoord?)null;
            }
            else
            {
                var opponents = _combatants.Where(c => !c.IsDefeated && c.IsPlayer != caster.IsPlayer).ToList();
                aimHex = opponents.Count > 0 ? FindInRange(caster, opponents)?.Position : null;
            }

            if (aimHex == null) return;

            caster.PendingTargetHex   = aimHex.Value;
            caster.CastTicksRemaining = (skill != null && skill.LockoutTicks > 0) ? skill.LockoutTicks : 1;
            SetState(caster, (skill != null && skill.IsChannel) ? CastState.Channeling : CastState.Casting);
            Debug.Log($"[Skill] {caster.DisplayName} begins {caster.State} (target hex {caster.PendingTargetHex})");
            OnSkillCast?.Invoke(caster.Id, skill?.SkillName ?? "Basic Ability");
        }

        // Resolves the cast via SkillArchetypeExecutor when the caster has skill data;
        // otherwise falls back to the original placeholder single-target damage stub.
        private void ResolveCastPlaceholder(Combatant caster)
        {
            if (caster.Skill != null && caster.Skill.Archetype != SkillArchetype.None)
            {
                SkillArchetypeExecutor.Execute(this, caster);
                return;
            }

            Combatant target = null;
            foreach (var c in _combatants)
                if (!c.IsDefeated && c.Position.Equals(caster.PendingTargetHex) && c.IsPlayer != caster.IsPlayer)
                { target = c; break; }
            if (target == null) return;

            bool isMagic   = caster.Flags != null && caster.Flags.Contains(BattleBehaviorFlag.MagicAttack);
            int rawOffense = isMagic ? caster.MG : caster.ATK;
            int rawDefense = GetZoneShreddedDefense(target, isMagic ? target.MR : target.DEF);
            int damage     = CombatMath.ApplyMitigation(rawOffense, rawDefense);

            damage = ApplyDamageAndCheckKill(caster, target, damage, out _, tags: null, autoHandleKill: false);

            Debug.Log($"[Ability] {caster.DisplayName} casts on {target.DisplayName}: {damage} dmg (HP:{target.CurrentHP}/{target.MaxHP})");
            OnCombatantActed?.Invoke(caster.Id, target.Id, damage, new List<string> { "ABILITY" });
            if (target.IsDefeated) HandleKill(caster, target);
        }

        private void ApplyDamage(Combatant target, int damage)
        {
            ApplyDamageAndCheckKill(null, target, damage, out _, tags: null, autoHandleKill: false);
        }

        internal void HandleKill(Combatant actor, Combatant target)
        {
            _grid.ClearOccupant(target.Position);
            Debug.Log($"[AutoBattle] {target.DisplayName} DEFEATED");
            OnCombatantDefeated?.Invoke(target.Id);
            if (actor != null && actor.Elementalist.ExplosionPct > 0)
                TraitAbilityExecutor.TriggerElementalistExplosion(this, actor, target);
        }

        private void MoveTowardNearest(Combatant actor, List<Combatant> opponents)
        {
            var oppPositions = opponents.Select(o => o.Position).ToList();
            var nearest      = _grid.FindNearest(actor.Position, oppPositions);
            if (nearest == null) return;

            var next = _grid.GetNextStep(actor.Position, nearest.Value, actor.Id);
            if (next == null) return;

            _grid.ClearOccupant(actor.Position);
            var from       = actor.Position;
            actor.Position = next.Value;
            _grid.SetOccupant(actor.Position, actor.Id);

            Debug.Log($"[AutoBattle] {actor.DisplayName} moves {from} → {actor.Position}");
            OnCombatantMoved?.Invoke(actor.Id, from, actor.Position);
        }
    }
}
