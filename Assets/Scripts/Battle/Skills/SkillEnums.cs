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
}
