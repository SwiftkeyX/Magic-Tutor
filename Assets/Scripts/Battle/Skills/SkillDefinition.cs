using System;

namespace MagicSchool.Battle
{
    // Flat, mostly-zero data record describing one champion's active skill.
    // Archetype.None means "no skill" (default for enemies and unpopulated champions).
    [Serializable]
    public class SkillDefinition
    {
        public SkillArchetype Archetype = SkillArchetype.None;
        public string SkillName;

        // Targeting
        public TargetTeam TargetTeam = TargetTeam.Enemy;  // Enemy = default (all 10 launch champions); Ally = same-team scope
        public TargetBaseFilter BaseFilter;
        public TargetPrioritySort[] Sorts = new TargetPrioritySort[0];
        public int Range;
        public int Radius;
        public int BounceCount;
        public int HitCount = 1;

        // Template G secondary ally target: if set, ExecuteSelfBuff resolves this
        // filter scoped to TargetTeam.Ally after self-buff and applies Ally-Support
        // Resolution to the first hit (e.g. Aegis: shields self AND lowest-HP%-adjacent-ally).
        public TargetBaseFilter? SecondaryFilter;
        public TargetPrioritySort[] SecondarySorts;

        // Casting
        public bool IsChannel;
        public int LockoutTicks = 1;

        // Damage / shielding scaling
        public float OffenseMultiplier;
        public float SecondaryMultiplier;
        public bool UsesMagicOffense;
        public float SplashPct;

        // Buffs / effects
        public int DurationTicks;
        public float FlatShieldAmount;
        public float ShieldPctOfMaxHP;
        public float ShieldDefMultiplier;     // shield += caster.DEF * ShieldDefMultiplier (same pattern as FlatShieldAmount)
        public float AttackSpeedBuffPct;
        public CcType CrowdControl;

        // Ally-Support Resolution (applied when TargetTeam = Ally, or via SecondaryFilter)
        public float HealMultiplier;          // heals target for caster.MG * HealMultiplier
        public int   ManaRestoreAmount;       // grants flat mana to target (also used for splash mana restore in ExplodingProjectile)
        public bool  CleansesStatus;          // if true, clears IsStunned / IsSilenced + their tick counters on target

        // Special-case fields (0/None/false = inactive; only set for the specific hero that uses them)
        public float DreadZoneDefShredPct;
        public int ZoneDurationTicks;  // separate from DurationTicks: Dread Overlord's zone (5 ticks) outlasts its stun (2 ticks)
        public float InterceptPct;
        public bool ReturnToOriginAfter;
    }
}
