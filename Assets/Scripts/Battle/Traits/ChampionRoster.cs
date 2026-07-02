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
            new ChampionData { Id="aegis",            DisplayName="Aegis",              Cost=1, Role=ChampionRole.Tank,    VerticalTrait=VerticalTrait.Vanguard,     HorizontalTrait=HorizontalTrait.Guardian,    MaxHP=650,  ATK=50, DEF=45, MG=0,  MR=45, AttackSpeed=0.60f, CRIT=2,  Range=1, MaxMana=80,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.SelfBuff, SkillName = "Shield Cover", ShieldDefMultiplier = 1.5f, DurationTicks = 3, SecondaryFilter = TargetBaseFilter.Adjacent, SecondarySorts = new[] { TargetPrioritySort.LowestHpPct }, LockoutTicks = 1 } },

            // #12 — Wildcat: BlinkStrike (Template F). Blinks to current target, physical
            // damage (1.4×ATK), then gains +20% AS for 4 ticks (Claw Dash GDD #12).
            new ChampionData { Id="wildcat",           DisplayName="Wildcat",            Cost=1, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Wild,         HorizontalTrait=HorizontalTrait.Trickster,   MaxHP=500,   ATK=55, DEF=20,  MG=0,  MR=20,  AttackSpeed=0.75f, CRIT=12, Range=1, MaxMana=60,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.BlinkStrike, SkillName = "Claw Dash", BaseFilter = TargetBaseFilter.Global, Sorts = new[] { TargetPrioritySort.CurrentTarget }, OffenseMultiplier = 1.4f, UsesMagicOffense = false, AttackSpeedBuffPct = 0.20f, DurationTicks = 4, LockoutTicks = 1 } },

            // #13 — Cosmic Sprite: ExplodingProjectile (Template B). Magic damage (1.8×MG) to
            // hit enemy; restores +15 mana to adjacent allies via ManaRestoreAmount (Starlight
            // Spark GDD #13). No SplashPct — enemy splash is not part of this skill.
            new ChampionData { Id="cosmicsprite",      DisplayName="Cosmic Sprite",      Cost=1, Role=ChampionRole.Support, VerticalTrait=VerticalTrait.Astral,       HorizontalTrait=HorizontalTrait.Kinetic,     MaxHP=480,   ATK=40,  DEF=20,  MG=65, MR=20,  AttackSpeed=0.70f, CRIT=8,  Range=2, MaxMana=80,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.ExplodingProjectile, SkillName = "Starlight Spark", BaseFilter = TargetBaseFilter.Global, Sorts = new[] { TargetPrioritySort.CurrentTarget }, OffenseMultiplier = 1.8f, UsesMagicOffense = true, ManaRestoreAmount = 15, LockoutTicks = 2 } },

            // #14 — Novice Cleric: StandardProjectile (Template A) with TargetTeam=Ally.
            // Fires a healing spark at the lowest-HP%-ally for 2.0×MG healing, plus cleanse
            // (Holy Blessing GDD #14). TargetTeam.Ally flips targeting and resolution to ally path.
            new ChampionData { Id="novicecleric",      DisplayName="Novice Cleric",      Cost=1, Role=ChampionRole.Support, VerticalTrait=VerticalTrait.Astral,       HorizontalTrait=HorizontalTrait.Oracle,      MaxHP=480,   ATK=40,  DEF=20,  MG=65, MR=20,  AttackSpeed=0.70f, CRIT=5,  Range=2, MaxMana=65,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.StandardProjectile, SkillName = "Holy Blessing", TargetTeam = TargetTeam.Ally, BaseFilter = TargetBaseFilter.Global, Sorts = new[] { TargetPrioritySort.LowestHpPct }, HealMultiplier = 2.0f, CleansesStatus = true, LockoutTicks = 1 } },

            // #15 — Tech Scrapper: SelfBuff (Template G). Physical damage (1.3×ATK) in a
            // frontal cone — cone geometry is not modeled by the generic SelfBuff executor,
            // matching the existing Ironclad precedent (Scrap Cleave GDD #15). Known gap.
            new ChampionData { Id="techscrapper",      DisplayName="Tech Scrapper",      Cost=1, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Striker,      HorizontalTrait=HorizontalTrait.Tech,        MaxHP=500,   ATK=55, DEF=20,  MG=0,  MR=20,  AttackSpeed=0.75f, CRIT=10, Range=1, MaxMana=70,  StartingMana=0,
                // GDD's frontal-cone damage (1.3×ATK) and DEF-shred have no generic hook in SelfBuff
                // yet — known gap matching the Ironclad precedent for cone/area self-buff effects.
                Skill = new SkillDefinition { Archetype = SkillArchetype.SelfBuff, SkillName = "Scrap Cleave", OffenseMultiplier = 1.3f, UsesMagicOffense = false, LockoutTicks = 1 } },

            // ── Batch 2 additions (champions #16–#19) ──────────────────────────
            // Stats are raw (unscaled) from TraitSystem.md roster table; intentionally weaker
            // than the existing 10 champions — explicitly accepted known gap, no rescaling.
            // Mana and skill design from ActiveSkillSystem.md Hero Skill Definitions #16–#19.

            // #16 — Sun Warden: SelfBuff (Template G) with secondary filter and hits all.
            // Solar Flare deals magic damage (1.0×MG) to adjacent enemies (unsupported, known gap)
            // and shields adjacent allies for 1.5×DEF for 3 ticks (Solar Flare GDD #16).
            new ChampionData { Id="sunwarden",         DisplayName="Sun Warden",         Cost=2, Role=ChampionRole.Tank,    VerticalTrait=VerticalTrait.Astral,       HorizontalTrait=HorizontalTrait.Warden,      MaxHP=800,  ATK=60, DEF=50, MG=0,  MR=50, AttackSpeed=0.60f, CRIT=2,  Range=1, MaxMana=90,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.SelfBuff, SkillName = "Solar Flare", OffenseMultiplier = 1.0f, UsesMagicOffense = true, SecondaryFilter = TargetBaseFilter.Adjacent, SecondaryHitsAll = true, ShieldDefMultiplier = 1.5f, DurationTicks = 3, LockoutTicks = 1 } },

            // #17 — Venom Stalker: SelfBuff (Template G). Bites target for 1.5×ATK physical damage
            // and inflicts magic DoT (0.3×MG) for 5 ticks (Toxic Bite GDD #17). Active damage and DoT
            // are unsupported in SelfBuff executor — known gap.
            new ChampionData { Id="venomstalker",      DisplayName="Venom Stalker",      Cost=2, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Shadow,       HorizontalTrait=HorizontalTrait.Void,        MaxHP=600,   ATK=65, DEF=20,  MG=0,  MR=20,  AttackSpeed=0.75f, CRIT=14, Range=1, MaxMana=75,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.SelfBuff, SkillName = "Toxic Bite", OffenseMultiplier = 1.5f, UsesMagicOffense = false, DurationTicks = 5, LockoutTicks = 1 } },

            // #18 — Forest Sentinel: SelfBuff (Template G). Gains Barkshield. Gains +40 DEF/MR,
            // Taunts adjacent enemies within 2 hexes, and adds +15 physical damage on-hit (Barkshield GDD #18).
            // Taunts are supported via CcType.Taunt. DEF/MR buff and on-hit damage are known gaps.
            new ChampionData { Id="forestsentinel",    DisplayName="Forest Sentinel",    Cost=2, Role=ChampionRole.Tank,    VerticalTrait=VerticalTrait.Wild,         HorizontalTrait=HorizontalTrait.Guardian,    MaxHP=800,  ATK=60, DEF=50, MG=0,  MR=50, AttackSpeed=0.60f, CRIT=2,  Range=1, MaxMana=100, StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.SelfBuff, SkillName = "Barkshield", CrowdControl = CcType.Taunt, Radius = 2, DurationTicks = 3, LockoutTicks = 1 } },

            // #19 — Void Mage: ExplodingProjectile (Template B). Fires a dark orb that explodes on
            // target, dealing magic damage (1.6×MG) to all units hit (SplashPct=1.0) and reducing MR
            // by 30% for 4 ticks (Void Sphere GDD #19). MR shred is a known gap.
            new ChampionData { Id="voidmage",          DisplayName="Void Mage",          Cost=2, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Elementalist, HorizontalTrait=HorizontalTrait.Void,        MaxHP=600,   ATK=50,  DEF=20,  MG=75, MR=20,  AttackSpeed=0.70f, CRIT=8,  Range=2, MaxMana=80,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.ExplodingProjectile, SkillName = "Void Sphere", BaseFilter = TargetBaseFilter.Global, Sorts = new[] { TargetPrioritySort.CurrentTarget }, OffenseMultiplier = 1.6f, UsesMagicOffense = true, SplashPct = 1.0f, Radius = 1, LockoutTicks = 2 } },

            // ── Batch 3 additions (champions #20–#23) ──────────────────────────
            // Stats are raw (unscaled) from TraitSystem.md roster table; intentionally weaker
            // than the existing 10 champions — explicitly accepted known gap, no rescaling.
            // Mana and skill design from ActiveSkillSystem.md Hero Skill Definitions #20–#23.

            // #20 — Starweaver: StandardProjectile (Template A) with TargetTeam=Ally.
            // Channels and heals the lowest HP % ally for 1.2×MG and grants a 100 HP Shield
            // (Astral Cascade GDD #20). Every-tick channel is a known gap (single-burst fallback).
            new ChampionData { Id="starweaver",        DisplayName="Starweaver",         Cost=3, Role=ChampionRole.Support, VerticalTrait=VerticalTrait.Astral,       HorizontalTrait=HorizontalTrait.Oracle,      MaxHP=700,  ATK=50, DEF=25,  MG=65, MR=25, AttackSpeed=0.70f, CRIT=5,  Range=3, MaxMana=100, StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.StandardProjectile, SkillName = "Astral Cascade", TargetTeam = TargetTeam.Ally, BaseFilter = TargetBaseFilter.Global, Sorts = new[] { TargetPrioritySort.LowestHpPct }, HealMultiplier = 1.2f, FlatShieldAmount = 100f, IsChannel = true, LockoutTicks = 3 } },

            // #21 — Rust Colossus: StandardProjectile (Template A). Slams the ground, dealing
            // physical damage (1.4×ATK + 0.5×DEF) to current target and stuns for 2 ticks
            // (Iron Fist GDD #21).
            new ChampionData { Id="rustcolossus",      DisplayName="Rust Colossus",      Cost=3, Role=ChampionRole.Tank,    VerticalTrait=VerticalTrait.Wild,         HorizontalTrait=HorizontalTrait.Tech,        MaxHP=800,  ATK=60, DEF=50, MG=0,  MR=50, AttackSpeed=0.60f, CRIT=3,  Range=1, MaxMana=90,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.StandardProjectile, SkillName = "Iron Fist", BaseFilter = TargetBaseFilter.WithinRange, Sorts = new[] { TargetPrioritySort.CurrentTarget }, Range = 1, OffenseMultiplier = 1.4f, SecondaryMultiplier = 0.5f, UsesMagicOffense = false, CrowdControl = CcType.Stun, DurationTicks = 2, LockoutTicks = 2 } },

            // #22 — Night Stalker: BlinkStrike (Template F). Teleports behind lowest-health enemy,
            // dealing physical damage (1.5×ATK) and dropping threat (Shadow Shroud GDD #22).
            // Guaranteed crit and threat shed details are known gaps of the BlinkStrike template.
            new ChampionData { Id="nightstalker",      DisplayName="Night Stalker",      Cost=3, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Shadow,       HorizontalTrait=HorizontalTrait.Trickster,   MaxHP=700,  ATK=60, DEF=25,  MG=0,  MR=25,  AttackSpeed=0.75f, CRIT=18, Range=1, MaxMana=60,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.BlinkStrike, SkillName = "Shadow Shroud", BaseFilter = TargetBaseFilter.Global, Sorts = new[] { TargetPrioritySort.LowestHpPct }, OffenseMultiplier = 1.5f, UsesMagicOffense = false, LockoutTicks = 1 } },

            // #23 — Storm Ranger: LaserBeam (Template C). Fires a linear beam dealing physical
            // damage (1.5×ATK) to all enemies hit (Overcharge Beam GDD #23). Shock Attack Speed
            // debuff is a known gap of the laser beam archetype.
            new ChampionData { Id="stormranger",      DisplayName="Storm Ranger",       Cost=3, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Ranger,       HorizontalTrait=HorizontalTrait.Tech,        MaxHP=700,   ATK=60, DEF=25,  MG=0,  MR=25,  AttackSpeed=0.75f, CRIT=10, Range=3, MaxMana=80,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.LaserBeam, SkillName = "Overcharge Beam", BaseFilter = TargetBaseFilter.LinearPath, Sorts = new[] { TargetPrioritySort.CurrentTarget }, Range = 3, OffenseMultiplier = 1.5f, UsesMagicOffense = false, LockoutTicks = 2 } },

            // ── Batch 4 additions (champions #24–#27) ──────────────────────────
            // Stats are raw (unscaled) from TraitSystem.md roster table.
            // Mana and skill design from ActiveSkillSystem.md Hero Skill Definitions #24–#27.

            // #24 — Grave Knight: SelfBuff (Template G). Emits a shadow aura for 4 ticks, dealing magic
            // damage and stealing Armor/MR (Soul Drain GDD #24). Aura damage and stat steal are known gaps.
            new ChampionData { Id="graveknight",      DisplayName="Grave Knight",       Cost=4, Role=ChampionRole.Tank,    VerticalTrait=VerticalTrait.Shadow,       HorizontalTrait=HorizontalTrait.Dreadknight, MaxHP=1000,  ATK=80, DEF=60, MG=0,  MR=60, AttackSpeed=0.65f, CRIT=4,  Range=1, MaxMana=110, StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.SelfBuff, SkillName = "Soul Drain", Radius = 1, DurationTicks = 4, LockoutTicks = 2 } },

            // #25 — Arcane Sage: GroundAoE (Template D). Deals magic damage (2.5×MG) to largest enemy
            // cluster (Chrono Shift GDD #25). Action progress slow/acceleration field is a known gap.
            new ChampionData { Id="arcanesage",       DisplayName="Arcane Sage",        Cost=4, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Elementalist, HorizontalTrait=HorizontalTrait.Tech,        MaxHP=750,  ATK=60, DEF=30,  MG=90, MR=30, AttackSpeed=0.80f, CRIT=18, Range=2, MaxMana=95,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.GroundAoE, SkillName = "Chrono Shift", BaseFilter = TargetBaseFilter.Global, Sorts = new[] { TargetPrioritySort.LargestCluster }, Radius = 2, OffenseMultiplier = 2.5f, UsesMagicOffense = true, LockoutTicks = 2 } },

            // #26 — Void Ranger: LaserBeam (Template C). Fires a linear arrow that pierces enemies
            // for physical damage (2.2×ATK) (Nether Arrow GDD #26). Execute below 20% health is a known gap.
            new ChampionData { Id="voidranger",       DisplayName="Void Ranger",        Cost=4, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Ranger,       HorizontalTrait=HorizontalTrait.Void,        MaxHP=750,  ATK=90, DEF=30,  MG=0,  MR=30,  AttackSpeed=0.80f, CRIT=16, Range=3, MaxMana=90,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.LaserBeam, SkillName = "Nether Arrow", BaseFilter = TargetBaseFilter.LinearPath, Sorts = new[] { TargetPrioritySort.CurrentTarget }, Range = 3, OffenseMultiplier = 2.2f, UsesMagicOffense = false, LockoutTicks = 2 } },

            // #27 — Divine Paladin: SelfBuff (Template G). Taunts nearby enemies, shields self for 3.5×DEF,
            // and heals adjacent allies for 1.0×MG (Guardian Shield GDD #27). Healing every-tick is a known
            // gap (applied once at cast time).
            new ChampionData { Id="divinepaladin",    DisplayName="Divine Paladin",     Cost=4, Role=ChampionRole.Support, VerticalTrait=VerticalTrait.Vanguard,     HorizontalTrait=HorizontalTrait.Oracle,      MaxHP=750,  ATK=60, DEF=30, MG=90, MR=30, AttackSpeed=0.80f, CRIT=4,  Range=1, MaxMana=100, StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.SelfBuff, SkillName = "Guardian Shield", ShieldDefMultiplier = 3.5f, DurationTicks = 3, CrowdControl = CcType.Taunt, Radius = 2, SecondaryFilter = TargetBaseFilter.Adjacent, SecondarySorts = new[] { TargetPrioritySort.LowestHpPct }, SecondaryHitsAll = true, HealMultiplier = 1.0f, LockoutTicks = 1 } },

            // ── Batch 5 additions (champions #28–#30) ──────────────────────────
            // Stats are raw (unscaled) from TraitSystem.md roster table.
            // Mana and skill design from ActiveSkillSystem.md Hero Skill Definitions #28–#30.

            // #28 — Cosmic Leviathan: GroundAoE (Template D). Crashes on the largest enemy cluster,
            // dealing magic damage (2.5×MG) and stunning all targets hit for 2 ticks (Supernova GDD #28).
            // Pulling hit targets toward center hex is a known gap.
            new ChampionData { Id="cosmicleviathan",  DisplayName="Cosmic Leviathan",   Cost=5, Role=ChampionRole.Tank,    VerticalTrait=VerticalTrait.Astral,       HorizontalTrait=HorizontalTrait.Guardian,    MaxHP=1000,  ATK=80, DEF=60, MG=0, MR=60, AttackSpeed=0.65f, CRIT=4,  Range=1, MaxMana=150, StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.GroundAoE, SkillName = "Supernova", BaseFilter = TargetBaseFilter.Global, Sorts = new[] { TargetPrioritySort.LargestCluster }, Radius = 2, OffenseMultiplier = 2.5f, UsesMagicOffense = true, CrowdControl = CcType.Stun, DurationTicks = 2, LockoutTicks = 2 } },

            // #29 — Reaper: BlinkStrike (Template F). Teleports to lowest HP % enemy and slashes,
            // dealing ATK-scaling physical damage (3.5×ATK) (Death's Scythe GDD #29). True damage
            // scaling and mana reset on kill are known gaps.
            new ChampionData { Id="reaper",           DisplayName="Reaper",             Cost=5, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Shadow,       HorizontalTrait=HorizontalTrait.Void,        MaxHP=900,  ATK=80, DEF=30, MG=0,  MR=30, AttackSpeed=0.80f, CRIT=22, Range=1, MaxMana=80,  StartingMana=0,
                Skill = new SkillDefinition { Archetype = SkillArchetype.BlinkStrike, SkillName = "Death's Scythe", BaseFilter = TargetBaseFilter.Global, Sorts = new[] { TargetPrioritySort.LowestHpPct }, OffenseMultiplier = 3.5f, UsesMagicOffense = false, LockoutTicks = 1 } },

            // #30 — Beastmaster: SelfBuff (Template G). Board-wide stun and AD/AP/AS buff to allies
            // (Primal Roar GDD #30). Board-wide stun and AD/AP stat buffs are known gaps.
            new ChampionData { Id="beastmaster",      DisplayName="Beastmaster",        Cost=5, Role=ChampionRole.Tank,    VerticalTrait=VerticalTrait.Wild,         HorizontalTrait=HorizontalTrait.Warden,      MaxHP=900,  ATK=80, DEF=30, MG=0,  MR=30, AttackSpeed=0.80f, CRIT=4,  Range=1, MaxMana=150, StartingMana=50,
                Skill = new SkillDefinition { Archetype = SkillArchetype.SelfBuff, SkillName = "Primal Roar", CrowdControl = CcType.Stun, DurationTicks = 2, Radius = 99, AttackSpeedBuffPct = 0.30f, LockoutTicks = 2 } },
        };

        public static List<ChampionData> GetAllChampions() => _all;

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
