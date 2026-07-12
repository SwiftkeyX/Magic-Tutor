using System.Linq;
using UnityEngine;

namespace MagicSchool.Battle
{
    // Editor/QA-only debug hooks, split out of AutoBattleResolver.cs for file-length
    // hygiene — these don't participate in battle control flow, they're callable
    // shortcuts for testing.
    public partial class AutoBattleResolver
    {
#if UNITY_EDITOR
        public void DebugSetAllPlayerHp(float pct)
        {
            foreach (var c in _combatants)
                if (c.IsPlayer) c.CurrentHP = Mathf.Max(1, Mathf.RoundToInt(c.MaxHP * pct));
        }

        // Force-triggers a cast for the named combatant regardless of current mana,
        // bypassing battle RNG/positioning. QA hook for exercising targeting/archetype
        // resolution directly (see ActiveSkillSystem plan's AC4 verification approach).
        public void DebugForceCast(string combatantId)
        {
            var c = _combatants.FirstOrDefault(x => x.Id == combatantId);
            if (c != null && !c.IsDefeated && c.State == CastState.Idle) TriggerCast(c);
        }
#endif
    }
}
