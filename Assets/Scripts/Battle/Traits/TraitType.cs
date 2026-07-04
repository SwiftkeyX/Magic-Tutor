using System;
using System.Collections.Generic;

namespace MagicSchool.Battle
{
    // Vertical traits: original 4 + 3 new (Astral, Wild, Shadow) for the 30-champion roster.
    public enum VerticalTrait   { None, Vanguard, Striker, Elementalist, Ranger, Astral, Wild, Shadow }
    // Horizontal traits: original 4 + 4 new (Oracle, Guardian, Tech, Void) for the 30-champion roster.
    public enum HorizontalTrait { None, Kinetic, Dreadknight, Warden, Trickster, Oracle, Guardian, Tech, Void }
    public enum ChampionRole    { Tank, Carry, Support }

    public enum TraitType
    {
        Vanguard, Striker, Elementalist, Ranger,
        Kinetic, Dreadknight, Warden, Trickster,
        // New traits (Batch 1 — counting/breakpoints wired in TraitTracker; combat effects not yet wired in TraitEffectApplier)
        Astral, Wild, Shadow, Oracle, Guardian, Tech, Void,
    }

    public struct CombatantTraitModifiers
    {
        public ChampionRole Role;
        public bool         IsFrontRow;
        public int          BonusDEF;
        public int          BonusMR;
        public int          BonusMG;
        public int          BonusATK;
        public int          BonusRange;
        public float        AttackSpeedMult;      // 1.0 = no change; multiplicative. Applied as c.AttackSpeed *= mult before deriving interval.
        public int          InitialShield;
        public int          InitialMana;
        public float        OmnivampPct;
        public bool         DreadknightShieldOnLowHP;
        public float        StrikerPctPerStack;
        public bool         StrikerBypassArmor;
        public bool         StrikerMaxStackSpeedBonus;
        public bool         TricksterDashEnabled;
        public bool         TricksterBleedEnabled;
        public float        ElementalistExplosionPct;
        public bool         ElementalistTrueDamage;
        public float        RangerBonusDmgPerHex;
    }

    public struct ResolverTraitSettings
    {
        public bool KineticEnabled;
        public int  KineticManaPerInterval;
        public bool KineticSupportExtraBonus;
    }
}
