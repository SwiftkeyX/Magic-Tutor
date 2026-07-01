namespace MagicSchool.Battle
{
    public enum SkillArchetype
    {
        None,
        StandardProjectile,
        ExplodingProjectile,
        LaserBeam,
        GroundAoE,
        BouncingChain,
        BlinkStrike,
        SelfBuff,
    }

    public enum TargetBaseFilter
    {
        Adjacent,
        WithinRange,
        Global,
        LinearPath,
    }

    public enum TargetPrioritySort
    {
        CurrentTarget,
        Furthest,
        Closest,
        LowestHpPct,
        LargestCluster,
    }

    public enum CastState
    {
        Idle,
        Moving,
        Attacking,
        Casting,
        Channeling,
        Stunned,
    }

    public enum CcType
    {
        None,
        Root,
        Stun,
        Taunt,
        Silence,
    }

    // Whether a skill's Base Filter scopes to the caster's opponents (Enemy, default)
    // or the caster's own team (Ally). All 10 launch champions default to Enemy.
    public enum TargetTeam
    {
        Enemy,
        Ally,
    }
}
