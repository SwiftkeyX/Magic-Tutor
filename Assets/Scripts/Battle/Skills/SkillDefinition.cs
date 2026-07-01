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
        public TargetBaseFilter BaseFilter;
        public TargetPrioritySort[] Sorts = new TargetPrioritySort[0];
        public int Range;
        public int Radius;
        public int BounceCount;
        public int HitCount = 1;

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
        public float AttackSpeedBuffPct;
        public CcType CrowdControl;

        // Special-case fields (0/None/false = inactive; only set for the specific hero that uses them)
        public float DreadZoneDefShredPct;
        public int ZoneDurationTicks;  // separate from DurationTicks: Dread Overlord's zone (5 ticks) outlasts its stun (2 ticks)
        public float InterceptPct;
        public bool ReturnToOriginAfter;
    }
}
