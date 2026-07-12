using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool.Battle
{
    // The 9 per-tick phases BattleLoop() calls in sequence, extracted so each phase
    // is independently readable/jumpable-to instead of one long inline coroutine body.
    public partial class AutoBattleResolver
    {
        // Runs first so State is settled before anything else reads it this tick
        // (GDD: Stunned "action timers paused"; silence just gates mana/casting,
        // no state transition needed).
        private void TickStatusCounters()
        {
            foreach (var c in _combatants)
            {
                if (c.IsDefeated) continue;
                if (c.IsStunned)
                {
                    c.StunTicksRemaining--;
                    if (c.StunTicksRemaining <= 0)
                    {
                        c.IsStunned = false;
                        SetState(c, CastState.Idle);
                    }
                }
                if (c.IsSilenced)
                {
                    c.SilenceTicksRemaining--;
                    if (c.SilenceTicksRemaining <= 0) c.IsSilenced = false;
                }
                if (c.InterceptTicksRemaining > 0)
                {
                    c.InterceptTicksRemaining--;
                    if (c.InterceptTicksRemaining <= 0) c.InterceptPct = 0f;
                }
            }
        }

        // Same iterate-and-expire pattern as ApplyBleedTicks below.
        private void TickDreadZones()
        {
            for (int zi = _activeZones.Count - 1; zi >= 0; zi--)
            {
                var zone = _activeZones[zi];
                zone.TicksRemaining--;
                if (zone.TicksRemaining <= 0) _activeZones.RemoveAt(zi);
                else _activeZones[zi] = zone;
            }
        }

        private void ApplyBleedTicks()
        {
            var bleedTargets = new List<Combatant>();
            foreach (var c in _combatants)
                if (!c.IsDefeated && c.BleedTicksRemaining > 0) bleedTargets.Add(c);
            foreach (var c in bleedTargets)
            {
                ApplyDamage(c, c.BleedDamagePerTick);
                c.BleedTicksRemaining--;
                if (c.IsDefeated) HandleKill(null, c);
            }
        }

        private void TickDreadknightShield()
        {
            foreach (var c in _combatants)
                TraitAbilityExecutor.TickDreadknightShield(this, c);
        }

        private void TickTricksterDashTrigger()
        {
            var dashCandidates = new List<Combatant>();
            foreach (var c in _combatants)
                if (!c.IsDefeated && c.Trickster.DashEnabled && !c.Trickster.DashTriggered && c.CurrentHP < c.MaxHP * TricksterDashHpThresholdPct)
                    dashCandidates.Add(c);
            foreach (var c in dashCandidates) TraitAbilityExecutor.ExecuteTricksterDash(this, c);
        }

        private void TickTricksterUntargetable()
        {
            foreach (var c in _combatants)
                TraitAbilityExecutor.TickTricksterUntargetable(this, c);
        }

        // Every 3s = 30 ticks at 0.1s/tick.
        private void TickKineticMana() => TraitAbilityExecutor.TickKineticMana(this);

        private void ResolveKineticBonusActions()
        {
            foreach (var actor in _pendingBonusActions)
            {
                if (actor.IsDefeated) continue;
                var opp2 = new List<Combatant>();
                foreach (var x in _combatants)
                    if (!x.IsDefeated && x.IsPlayer != actor.IsPlayer) opp2.Add(x);
                var inRange2 = FindInRange(actor, opp2);
                if (inRange2 != null)
                {
                    Attack(actor, inRange2);
                    Debug.Log($"[Trait] {actor.DisplayName} Mana Bonus Action!");
                }
            }
            _pendingBonusActions.Clear();
        }

        private void TickCastChannels()
        {
            var castingUnits = new List<Combatant>();
            foreach (var c in _combatants)
                if (!c.IsDefeated && (c.State == CastState.Casting || c.State == CastState.Channeling))
                    castingUnits.Add(c);
            foreach (var c in castingUnits)
            {
                if (c.IsDefeated) continue;

                if (c.IsStunned)
                {
                    // Interrupted by stun: mana was already consumed at trigger time
                    // (never refunded), so "loses the consumed mana" falls out for free.
                    SetState(c, CastState.Stunned);
                    continue;
                }

                c.CastTicksRemaining--;
                if (c.CastTicksRemaining <= 0)
                {
                    ResolveCastPlaceholder(c);
                    if (!c.IsDefeated) SetState(c, CastState.Idle);
                }
            }
        }
    }
}
