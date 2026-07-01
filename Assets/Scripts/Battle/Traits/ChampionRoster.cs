using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicSchool.Battle
{
    public class ChampionRoster : MonoBehaviour
    {
        private static readonly List<ChampionData> _all = new List<ChampionData>
        {
            // TFT-aligned stats. AttackSpeed = attacks/second (float accumulator, 0.1s tick).
            // MaxMana/StartingMana and Skill are taken from ActiveSkillSystem.md's Hero Skill
            // Definitions table (per-hero, not a tier-based rule of thumb).
            new ChampionData { Id="ironclad",         DisplayName="Ironclad",          Cost=1, Role=ChampionRole.Tank,    VerticalTrait=VerticalTrait.Vanguard,     HorizontalTrait=HorizontalTrait.Warden,      MaxHP=650,  ATK=50, DEF=45, MG=0,  MR=45, AttackSpeed=0.60f, CRIT=2,  Range=1, MaxMana=80,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.SelfBuff, SkillName = "Iron Shield Slam", OffenseMultiplier = 1.2f, SecondaryMultiplier = 0.4f, UsesMagicOffense = false, FlatShieldAmount = 200, DurationTicks = 3, LockoutTicks = 1 } },
            new ChampionData { Id="bloodhound",        DisplayName="Bloodhound",         Cost=1, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Striker,      HorizontalTrait=HorizontalTrait.Dreadknight, MaxHP=500,  ATK=55, DEF=20, MG=0,  MR=20, AttackSpeed=0.75f, CRIT=12, Range=1, MaxMana=60,  StartingMana=0,
                // GDD's "doubles Dreadknight Omnivamp" and "+2 Striker stacks per attack during Frenzy"
                // are bespoke trait-interaction effects with no generic hook yet — known gap.
                Skill = new SkillDefinition { Archetype = SkillArchetype.SelfBuff, SkillName = "Blood Frenzy", AttackSpeedBuffPct = 0.40f, DurationTicks = 5, LockoutTicks = 1 } },
            new ChampionData { Id="pyromancer",        DisplayName="Pyromancer",         Cost=1, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Elementalist, HorizontalTrait=HorizontalTrait.Kinetic,     MaxHP=480,  ATK=40, DEF=20, MG=65, MR=20, AttackSpeed=0.70f, CRIT=8,  Range=2, MaxMana=90,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.ExplodingProjectile, SkillName = "Fireball Blast", BaseFilter = TargetBaseFilter.Global, Sorts = new[] { TargetPrioritySort.CurrentTarget }, OffenseMultiplier = 2.2f, UsesMagicOffense = true, SplashPct = 0.5f, LockoutTicks = 2 } },
            new ChampionData { Id="windrunner",        DisplayName="Windrunner",         Cost=2, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Ranger,       HorizontalTrait=HorizontalTrait.Kinetic,     MaxHP=600,  ATK=65, DEF=20, MG=0,  MR=20, AttackSpeed=0.80f, CRIT=14, Range=3, MaxMana=100, StartingMana=30,
                // GDD's "+8% dmg per hex distance" per-hit scaling has no generic hook yet — known gap.
                Skill = new SkillDefinition { Archetype = SkillArchetype.LaserBeam, SkillName = "Gale Piercer", BaseFilter = TargetBaseFilter.LinearPath, Sorts = new[] { TargetPrioritySort.CurrentTarget }, Range = 6, OffenseMultiplier = 1.8f, UsesMagicOffense = false, IsChannel = true, LockoutTicks = 2 } },
            new ChampionData { Id="grovekeeper",       DisplayName="Grove Keeper",       Cost=2, Role=ChampionRole.Support, VerticalTrait=VerticalTrait.Elementalist, HorizontalTrait=HorizontalTrait.Warden,      MaxHP=600,  ATK=40, DEF=25, MG=55, MR=25, AttackSpeed=0.65f, CRIT=4,  Range=2, MaxMana=120, StartingMana=0,
                // GDD's "over 3 ticks" DoT (vs. instant) has no generic hook yet — known gap.
                Skill = new SkillDefinition { Archetype = SkillArchetype.GroundAoE, SkillName = "Verdant Roots", BaseFilter = TargetBaseFilter.Global, Sorts = new[] { TargetPrioritySort.LargestCluster }, Radius = 1, OffenseMultiplier = 1.2f, UsesMagicOffense = true, DurationTicks = 3, CrowdControl = CcType.Root, LockoutTicks = 1 } },
            new ChampionData { Id="shadowblade",       DisplayName="Shadowblade",        Cost=2, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Striker,      HorizontalTrait=HorizontalTrait.Trickster,   MaxHP=580,  ATK=65, DEF=25, MG=0,  MR=25, AttackSpeed=0.80f, CRIT=20, Range=1, MaxMana=50,  StartingMana=0,
                // GDD's CRIT-multiplier bonus and bleed-over-4-ticks have no generic hook yet — known gap.
                Skill = new SkillDefinition { Archetype = SkillArchetype.BlinkStrike, SkillName = "Shadow Strike", BaseFilter = TargetBaseFilter.Global, Sorts = new[] { TargetPrioritySort.Furthest }, OffenseMultiplier = 1.5f, UsesMagicOffense = false, LockoutTicks = 1 } },
            new ChampionData { Id="phalanx",           DisplayName="Phalanx",            Cost=3, Role=ChampionRole.Tank,    VerticalTrait=VerticalTrait.Vanguard,     HorizontalTrait=HorizontalTrait.Dreadknight, MaxHP=800,  ATK=60, DEF=50, MG=0,  MR=50, AttackSpeed=0.60f, CRIT=3,  Range=1, MaxMana=100, StartingMana=0,
                // InterceptPct data is set here; the actual ally-damage redirect mechanic is Step 16a.
                Skill = new SkillDefinition { Archetype = SkillArchetype.SelfBuff, SkillName = "Unbreakable Bastion", ShieldPctOfMaxHP = 0.25f, Radius = 2, DurationTicks = 3, CrowdControl = CcType.Taunt, InterceptPct = 0.30f, LockoutTicks = 1 } },
            new ChampionData { Id="stormbringer",      DisplayName="Stormbringer",       Cost=3, Role=ChampionRole.Support, VerticalTrait=VerticalTrait.Ranger,       HorizontalTrait=HorizontalTrait.Warden,      MaxHP=700,  ATK=50, DEF=25, MG=65, MR=25, AttackSpeed=0.70f, CRIT=7,  Range=3, MaxMana=80,  StartingMana=0,
                // GDD's ambiguous "Shields hit allies for 150 HP" clause was dropped per
                // design decision — Chain Lightning is damage + Silence only (see GDD).
                Skill = new SkillDefinition { Archetype = SkillArchetype.BouncingChain, SkillName = "Chain Lightning", BaseFilter = TargetBaseFilter.WithinRange, Sorts = new[] { TargetPrioritySort.CurrentTarget }, Range = 3, BounceCount = 4, OffenseMultiplier = 1.4f, UsesMagicOffense = true, CrowdControl = CcType.Silence, DurationTicks = 2, LockoutTicks = 1 } },
            new ChampionData { Id="phantomassassin",   DisplayName="Phantom Assassin",   Cost=4, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Elementalist, HorizontalTrait=HorizontalTrait.Trickster,   MaxHP=750,  ATK=65, DEF=30, MG=90, MR=30, AttackSpeed=0.80f, CRIT=22, Range=2, MaxMana=110, StartingMana=0,
                // HitCount/ReturnToOriginAfter data is set here; the 3-hit sequential
                // teleport-and-return mechanic is Step 16c.
                Skill = new SkillDefinition { Archetype = SkillArchetype.BlinkStrike, SkillName = "Phantom Flurry", BaseFilter = TargetBaseFilter.Global, Sorts = new[] { TargetPrioritySort.LowestHpPct }, HitCount = 3, ReturnToOriginAfter = true, OffenseMultiplier = 3.2f, UsesMagicOffense = true, LockoutTicks = 1 } },
            new ChampionData { Id="dreadoverlord",     DisplayName="Dread Overlord",     Cost=5, Role=ChampionRole.Tank,    VerticalTrait=VerticalTrait.Vanguard,     HorizontalTrait=HorizontalTrait.Trickster,   MaxHP=1000, ATK=80, DEF=60, MG=0,  MR=60, AttackSpeed=0.65f, CRIT=4,  Range=1, MaxMana=150, StartingMana=50,
                Skill = new SkillDefinition { Archetype = SkillArchetype.GroundAoE, SkillName = "Dread Cataclysm", BaseFilter = TargetBaseFilter.Global, Sorts = new[] { TargetPrioritySort.LargestCluster }, Radius = 2, OffenseMultiplier = 2.0f, UsesMagicOffense = true, CrowdControl = CcType.Stun, DurationTicks = 2, DreadZoneDefShredPct = 0.40f, ZoneDurationTicks = 5, LockoutTicks = 1 } },

            // ── Batch 1 additions (champions #11–#15) ──────────────────────────
            // Stats are raw (unscaled) from TraitSystem.md roster table; intentionally weaker
            // than the existing 10 champions — explicitly accepted known gap, no rescaling.
            // Mana and skill design from ActiveSkillSystem.md Hero Skill Definitions #11–#15.

            // #11 — Aegis: SelfBuff (Template G). Shields self AND the lowest-HP%-adjacent-ally
            // for 1.5×DEF each (Shield Cover GDD #11). Secondary ally target via SecondaryFilter.
            new ChampionData { Id="aegis",            DisplayName="Aegis",              Cost=1, Role=ChampionRole.Tank,    VerticalTrait=VerticalTrait.Vanguard,     HorizontalTrait=HorizontalTrait.Guardian,    MaxHP=100,  ATK=11, DEF=15, MG=0,  MR=10, AttackSpeed=0.21f, CRIT=2,  Range=1, MaxMana=80,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.SelfBuff, SkillName = "Shield Cover", ShieldDefMultiplier = 1.5f, DurationTicks = 3, SecondaryFilter = TargetBaseFilter.Adjacent, SecondarySorts = new[] { TargetPrioritySort.LowestHpPct }, LockoutTicks = 1 } },

            // #12 — Wildcat: BlinkStrike (Template F). Blinks to current target, physical
            // damage (1.4×ATK), then gains +20% AS for 4 ticks (Claw Dash GDD #12).
            new ChampionData { Id="wildcat",           DisplayName="Wildcat",            Cost=1, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Wild,         HorizontalTrait=HorizontalTrait.Trickster,   MaxHP=70,   ATK=15, DEF=5,  MG=0,  MR=5,  AttackSpeed=0.33f, CRIT=12, Range=1, MaxMana=60,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.BlinkStrike, SkillName = "Claw Dash", BaseFilter = TargetBaseFilter.Global, Sorts = new[] { TargetPrioritySort.CurrentTarget }, OffenseMultiplier = 1.4f, UsesMagicOffense = false, AttackSpeedBuffPct = 0.20f, DurationTicks = 4, LockoutTicks = 1 } },

            // #13 — Cosmic Sprite: ExplodingProjectile (Template B). Magic damage (1.8×MG) to
            // hit enemy; restores +15 mana to adjacent allies via ManaRestoreAmount (Starlight
            // Spark GDD #13). No SplashPct — enemy splash is not part of this skill.
            new ChampionData { Id="cosmicsprite",      DisplayName="Cosmic Sprite",      Cost=1, Role=ChampionRole.Support, VerticalTrait=VerticalTrait.Astral,       HorizontalTrait=HorizontalTrait.Kinetic,     MaxHP=60,   ATK=7,  DEF=5,  MG=18, MR=8,  AttackSpeed=0.28f, CRIT=8,  Range=2, MaxMana=80,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.ExplodingProjectile, SkillName = "Starlight Spark", BaseFilter = TargetBaseFilter.Global, Sorts = new[] { TargetPrioritySort.CurrentTarget }, OffenseMultiplier = 1.8f, UsesMagicOffense = true, ManaRestoreAmount = 15, LockoutTicks = 2 } },

            // #14 — Novice Cleric: StandardProjectile (Template A) with TargetTeam=Ally.
            // Fires a healing spark at the lowest-HP%-ally for 2.0×MG healing, plus cleanse
            // (Holy Blessing GDD #14). TargetTeam.Ally flips targeting and resolution to ally path.
            new ChampionData { Id="novicecleric",      DisplayName="Novice Cleric",      Cost=1, Role=ChampionRole.Support, VerticalTrait=VerticalTrait.Astral,       HorizontalTrait=HorizontalTrait.Oracle,      MaxHP=65,   ATK=6,  DEF=5,  MG=18, MR=8,  AttackSpeed=0.25f, CRIT=5,  Range=2, MaxMana=65,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.StandardProjectile, SkillName = "Holy Blessing", TargetTeam = TargetTeam.Ally, BaseFilter = TargetBaseFilter.Global, Sorts = new[] { TargetPrioritySort.LowestHpPct }, HealMultiplier = 2.0f, CleansesStatus = true, LockoutTicks = 1 } },

            // #15 — Tech Scrapper: SelfBuff (Template G). Physical damage (1.3×ATK) in a
            // frontal cone — cone geometry is not modeled by the generic SelfBuff executor,
            // matching the existing Ironclad precedent (Scrap Cleave GDD #15). Known gap.
            new ChampionData { Id="techscrapper",      DisplayName="Tech Scrapper",      Cost=1, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Striker,      HorizontalTrait=HorizontalTrait.Tech,        MaxHP=75,   ATK=16, DEF=6,  MG=0,  MR=5,  AttackSpeed=0.30f, CRIT=10, Range=1, MaxMana=70,  StartingMana=0,
                // GDD's frontal-cone damage (1.3×ATK) and DEF-shred have no generic hook in SelfBuff
                // yet — known gap matching the Ironclad precedent for cone/area self-buff effects.
                Skill = new SkillDefinition { Archetype = SkillArchetype.SelfBuff, SkillName = "Scrap Cleave", OffenseMultiplier = 1.3f, UsesMagicOffense = false, LockoutTicks = 1 } },
        };

        public List<StudentCombatData> GetStudents()
            => _all.Select(c => c.ToStudentCombatData()).ToList();

        public ChampionData GetChampionById(string id)
            => _all.Find(c => c.Id == id);

        public Dictionary<string, ChampionData> GetChampionLookup()
        {
            var dict = new Dictionary<string, ChampionData>();
            foreach (var c in _all) dict[c.Id] = c;
            return dict;
        }
    }
}
