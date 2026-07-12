using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicSchool.Battle
{
    // Targeting helpers, the shared damage/mana/state mutation choke points, and the
    // internal accessors Skills/*.cs and Traits/*.cs reach through via their ctx parameter.
    public partial class AutoBattleResolver
    {
        // Casting/Channeling/Stunned units cannot act (basic-attack lockout / CC freeze).
        private static bool IsActionLocked(Combatant c) =>
            c.State == CastState.Casting || c.State == CastState.Channeling || c.State == CastState.Stunned;

        private Combatant FindInRange(Combatant actor, List<Combatant> opponents)
        {
            Combatant nearest = null;
            int minDist = int.MaxValue;
            foreach (var o in opponents)
            {
                if (o.Trickster.Untargetable) continue;
                int d = HexCoord.Distance(actor.Position, o.Position);
                if (d <= actor.Range && d < minDist) { minDist = d; nearest = o; }
            }
            return nearest;
        }

        // Shared shield-absorb + HP-subtract + (optional) event + (optional) kill-check
        // choke point for all damage application. Callers that need to defer the kill
        // check (Attack does more work first; TriggerElementalistExplosion defers to
        // avoid mutating _combatants mid-iteration) pass autoHandleKill: false and
        // check target.IsDefeated themselves afterward. Returns the post-shield damage
        // actually applied to HP.
        internal int ApplyDamageAndCheckKill(Combatant actor, Combatant target, int damage, out int shieldAbsorbed,
            List<string> tags = null, bool bypassShield = false, bool autoHandleKill = true)
        {
            // Phalanx-style intercept: an adjacent ally with an active InterceptPct
            // (set by its own skill, ticked down like stun/silence) absorbs a % of
            // damage aimed at this target onto itself instead.
            var interceptor = _combatants.FirstOrDefault(c =>
                !c.IsDefeated && c != target && c.IsPlayer == target.IsPlayer &&
                c.InterceptTicksRemaining > 0 && c.InterceptPct > 0 &&
                HexCoord.Distance(c.Position, target.Position) <= 1);
            if (interceptor != null)
            {
                int redirected = (int)(damage * interceptor.InterceptPct);
                if (redirected > 0)
                {
                    damage -= redirected;
                    Debug.Log($"[Skill] {interceptor.DisplayName} intercepts {redirected} dmg meant for {target.DisplayName}");
                    ApplyDamageAndCheckKill(actor, interceptor, redirected, out _, tags: null, autoHandleKill: autoHandleKill);
                }
            }

            shieldAbsorbed = 0;
            if (!bypassShield && target.Shield > 0)
            {
                shieldAbsorbed = Math.Min(target.Shield, damage);
                target.Shield -= shieldAbsorbed;
                damage        -= shieldAbsorbed;
            }
            target.CurrentHP -= damage;

            if (tags != null)
                OnCombatantActed?.Invoke(actor?.Id, target.Id, damage, tags);

            if (autoHandleKill && target.IsDefeated)
                HandleKill(actor, target);

            return damage;
        }

        // Single choke point for all Mana mutations, so the UI always hears about
        // every change via one event (same philosophy as ApplyDamageAndCheckKill).
        // Internal (not private): called by TraitAbilityExecutor.TickKineticMana.
        internal void SetMana(Combatant c, int newValue)
        {
            c.Mana = Math.Max(0, newValue);
            OnManaChanged?.Invoke(c.Id, c.Mana, c.MaxMana);
        }

        private void SetState(Combatant c, CastState newState)
        {
            c.State = newState;
            OnCastStateChanged?.Invoke(c.Id, newState);
        }

        // ── Skill execution support (internal — called by SkillArchetypeExecutor) ────
        internal HexGrid Grid => _grid;

        // Exposes all combatants (both teams) as a read-only list so SkillArchetypeExecutor
        // can call SkillTargetSelector for secondary-target resolution (e.g. Aegis's
        // SecondaryFilter ally shield) without needing a separate lookup path.
        internal IReadOnlyList<Combatant> AllCombatants => _combatants;

        internal List<Combatant> GetOpponentsOf(Combatant c) =>
            _combatants.Where(x => !x.IsDefeated && x.IsPlayer != c.IsPlayer).ToList();

        internal List<Combatant> GetAlliesOf(Combatant c) =>
            _combatants.Where(x => !x.IsDefeated && x.IsPlayer == c.IsPlayer && x != c).ToList();

        internal Combatant GetOccupantAt(HexCoord hex) =>
            _combatants.FirstOrDefault(x => !x.IsDefeated && x.Position.Equals(hex));

        // Applies the strongest active Dread-Zone def/MR shred (if any) covering
        // target's current hex. Used at damage-mitigation time by every damage
        // formula (basic attacks, casts, skills) so a zone affects all incoming
        // damage, not just its caster's own hits.
        internal int GetZoneShreddedDefense(Combatant target, int baseDefense)
        {
            float shredPct = 0f;
            foreach (var zone in _activeZones)
                if (HexCoord.Distance(zone.Center, target.Position) <= zone.Radius)
                    shredPct = Math.Max(shredPct, zone.DefShredPct);
            return shredPct > 0f ? (int)(baseDefense * (1f - shredPct)) : baseDefense;
        }

        internal void GrantMana(Combatant c, int amount)
        {
            if (c.IsSilenced) return;
            SetMana(c, c.Mana + amount);
        }

        internal void MoveUnit(Combatant c, HexCoord dest)
        {
            _grid.ClearOccupant(c.Position);
            var from = c.Position;
            c.Position = dest;
            _grid.SetOccupant(c.Position, c.Id);
            OnCombatantMoved?.Invoke(c.Id, from, c.Position);
        }

        internal void AddDreadZone(HexCoord center, int radius, float defShredPct, int ticks)
        {
            _activeZones.Add(new ActiveZoneEffect { Center = center, Radius = radius, DefShredPct = defShredPct, TicksRemaining = ticks });
        }
    }
}
