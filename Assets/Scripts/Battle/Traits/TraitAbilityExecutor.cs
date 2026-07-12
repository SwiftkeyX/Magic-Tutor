using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool.Battle
{
    // Externalized static helpers for trait-reactive ability logic that fires during
    // battle resolution. Mirrors SkillArchetypeExecutor's pattern: receives ctx as the
    // first parameter and calls back into AutoBattleResolver's internal helpers so all
    // state mutation and event firing remain owned by AutoBattleResolver.
    //
    // This class handles *trait-reactive* abilities — those triggered by game-state
    // transitions (kills, HP thresholds) — distinct from SkillArchetypeExecutor which
    // handles active mana-cast skills.
    internal static class TraitAbilityExecutor
    {
        // ── Elementalist explosion ─────────────────────────────────────────
        // Triggered in HandleKill when the actor has ElementalistExplosionPct > 0.
        // Radiates % of the killed unit's MaxHP as AoE damage to adjacent enemies.
        // Chain-kills are deferred (secondary list) to avoid mutating _combatants
        // mid-iteration — same pattern as the original inline method.
        public static void TriggerElementalistExplosion(AutoBattleResolver ctx, Combatant actor, Combatant killed)
        {
            var adjacent  = ctx.Grid.GetInRange(killed.Position, 1);
            var secondary = new List<(Combatant a, Combatant t)>();

            foreach (var coord in adjacent)
            {
                if (coord.Equals(killed.Position)) continue;

                var hit = ctx.GetOccupantAt(coord);
                if (hit == null || hit.IsPlayer == actor.IsPlayer) continue;

                int dmg = (int)(killed.MaxHP * actor.Elementalist.ExplosionPct);
                if (!actor.Elementalist.TrueDamage)
                    dmg = Math.Max(1, (int)(dmg * (100f / (100 + hit.MR))));

                // Explosion damage has always bypassed shields — preserved exactly (bypassShield: true).
                // "EXPLOSION" tag passed so ApplyDamageAndCheckKill fires OnCombatantActed internally,
                // replacing the separate OnCombatantActed?.Invoke call in the original inline version.
                dmg = ctx.ApplyDamageAndCheckKill(actor, hit, dmg, out _, tags: new List<string> { "EXPLOSION" }, bypassShield: true, autoHandleKill: false);

                Debug.Log($"[Trait] Elementalist explosion: {hit.DisplayName} -{dmg}");

                if (hit.IsDefeated) secondary.Add((actor, hit));
            }

            foreach (var (a, t) in secondary) ctx.HandleKill(a, t);
        }

        // ── Trickster dash ─────────────────────────────────────────────────
        // Triggered in BattleLoop Phase 3 when a Trickster unit's HP falls below 50%.
        // Dashes to the deepest backline enemy hex, becomes untargetable briefly,
        // and arms the bleed-on-next-attack flag.
        public static void ExecuteTricksterDash(AutoBattleResolver ctx, Combatant c)
        {
            c.Trickster.DashTriggered     = true;
            c.Trickster.Untargetable      = true;
            c.Trickster.UntargetableTicks = 12;  // 1.2s at 0.1s/tick (was 2 at 0.6s/tick)
            c.Trickster.BleedNextAttack   = c.Trickster.BleedEnabled;

            // Find deepest backline enemy target
            Combatant backlineTarget = null;
            int best = c.IsPlayer ? int.MinValue : int.MaxValue;
            foreach (var x in ctx.AllCombatants)
            {
                if (x.IsDefeated || x.IsPlayer == c.IsPlayer) continue;
                if ( c.IsPlayer && x.Position.Row > best) { best = x.Position.Row; backlineTarget = x; }
                if (!c.IsPlayer && x.Position.Row < best) { best = x.Position.Row; backlineTarget = x; }
            }
            if (backlineTarget == null) return;

            // Find open neighbor adjacent to backline target
            HexCoord? dashDest = null;
            foreach (var nb in HexCoord.GetNeighbors(backlineTarget.Position, HexGrid.Cols, HexGrid.Rows))
                if (!ctx.Grid.IsOccupied(nb)) { dashDest = nb; break; }

            if (!dashDest.HasValue)
            {
                c.Trickster.UntargetableTicks = 0;
                c.Trickster.Untargetable      = false;
                return;
            }

            var from = c.Position;
            ctx.MoveUnit(c, dashDest.Value);
            Debug.Log($"[Trait] {c.DisplayName} Trickster Dash: {from} → {c.Position}");
        }

        // ── Dreadknight low-HP shield ──────────────────────────────────────
        // Ticked once per BattleLoop tick (AutoBattleResolver.TickDreadknightShield),
        // once per combatant. One-time per combat, guarded by DreadknightShieldGranted.
        public static void TickDreadknightShield(AutoBattleResolver ctx, Combatant c)
        {
            if (c.IsDefeated || !c.Dreadknight.ShieldEnabled || c.Dreadknight.ShieldGranted) return;
            if (c.CurrentHP >= c.MaxHP * AutoBattleResolver.DreadknightShieldHpThresholdPct) return;

            c.Shield += (int)(c.MaxHP * 0.25f);
            c.Dreadknight.ShieldGranted = true;
            Debug.Log($"[Trait] {c.DisplayName} Dreadknight Shield: +{(int)(c.MaxHP * 0.25f)}");
        }

        // ── Trickster untargetable countdown ───────────────────────────────
        // Ticked once per BattleLoop tick (AutoBattleResolver.TickTricksterUntargetable),
        // once per combatant. Counts down the post-dash untargetable window armed by
        // ExecuteTricksterDash above.
        public static void TickTricksterUntargetable(AutoBattleResolver ctx, Combatant c)
        {
            if (!c.Trickster.Untargetable) return;
            c.Trickster.UntargetableTicks--;
            if (c.Trickster.UntargetableTicks <= 0) c.Trickster.Untargetable = false;
        }

        // ── Kinetic mana tick ───────────────────────────────────────────────
        // Ticked once per BattleLoop tick (AutoBattleResolver.TickKineticMana), across
        // all combatants at once (every 3s = 30 ticks at 0.1s/tick). Mutates the
        // resolver-owned kinetic counter and pending-bonus-action list via ctx.
        public static void TickKineticMana(AutoBattleResolver ctx)
        {
            if (ctx.KineticEnabled)
            {
                ctx.KineticTickCounter++;
                if (ctx.KineticTickCounter >= 30)
                {
                    ctx.KineticTickCounter = 0;
                    foreach (var c in ctx.AllCombatants)
                    {
                        if (c.IsDefeated || !c.IsPlayer) continue;
                        int gain = ctx.KineticManaPerInterval;
                        if (ctx.KineticSupportExtraBonus && c.Role == ChampionRole.Support) gain += 10;
                        ctx.SetMana(c, c.Mana + gain);
                        Debug.Log($"[Trait][Kinetic] {c.DisplayName} +{gain} mana → {c.Mana}/{c.MaxMana}");
                        if (c.Mana >= c.MaxMana) { ctx.SetMana(c, 0); ctx.PendingBonusActions.Add(c); }
                    }
                }
            }
        }

        // ── Striker max-stack speed bonus ──────────────────────────────────
        // Applied per-combatant during BattleLoop's action-progress accumulation.
        // BP6 + Carry role: ActionInterval effectively −30% once Striker reaches 8 stacks.
        public static float ApplyStrikerSpeedBonus(Combatant c, float gain)
        {
            if (c.Striker.MaxStackSpeedBonus && c.Role == ChampionRole.Carry && c.Striker.Stacks >= 8)
                gain /= 0.70f;  // +42.9% at max Striker stacks
            return gain;
        }
    }
}
