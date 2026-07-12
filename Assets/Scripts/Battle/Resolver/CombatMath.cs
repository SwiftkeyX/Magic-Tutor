using System;

namespace MagicSchool.Battle
{
    // Shared damage-mitigation math. Previously reimplemented independently in
    // AutoBattleResolver.Attack(), AutoBattleResolver.ResolveCastPlaceholder(),
    // and SkillArchetypeExecutor.ComputeSkillDamage() — consolidated here so the
    // core LoL-style mitigation formula can't drift between call sites again.
    // Caller-specific pre-mitigation modifiers (Striker stacks, Ranger per-hex
    // bonus, skill offense/secondary multipliers) stay owned by each caller and
    // are applied to rawOffense/rawDefense before calling this.
    public static class CombatMath
    {
        public static int ApplyMitigation(float rawOffense, int rawDefense)
        {
            return Math.Max(1, (int)(rawOffense * (100f / (100 + rawDefense))));
        }
    }
}
