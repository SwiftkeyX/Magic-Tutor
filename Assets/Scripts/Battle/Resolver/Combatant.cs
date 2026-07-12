using System.Collections.Generic;

namespace MagicSchool.Battle
{
    // Per-unit runtime simulation state built by AutoBattleResolver.SetCombatants()
    // from StudentCombatData/EnemyCombatData. Never persisted.
    // internal (not private) so Assets/Scripts/Battle/Skills/*.cs and Traits/*.cs can operate on it.
    internal class Combatant
    {
        public string   Id;
        public string   ChampionId;     // Players only — bridge to ChampionData.Id
        public string   DisplayName;
        public bool     IsPlayer;
        public int      MaxHP;
        public int      CurrentHP;
        public int      ATK;
        public int      DEF;
        public float    AttackSpeed;
        public int      Range;
        public HexCoord Position;
        public float    ActionProgress;  // accumulates AttackSpeed × TickDelay each tick; fires at ≥ 1.0
        public int      MaxMana;
        public bool     IsDefeated => CurrentHP <= 0;

        // ── Trait fields ─────────────────────────────────────────────
        public int  MG;
        public int  MR;
        public List<BattleBehaviorFlag> Flags;
        public ChampionRole Role;
        public bool  IsFrontRow;
        public int   Shield;         // general shield-absorption pool (Warden and Dreadknight both grant into this)
        public float OmnivampPct;
        public int   Mana;
        public int   BleedDamagePerTick;  // lives on the victim — any attacker can apply it, not Trickster-exclusive
        public int   BleedTicksRemaining;

        // Per-trait state, grouped so each trait's fields can't be confused with
        // another's. Structs on a class field are directly mutable in place
        // (e.g. c.Striker.Stacks++) since Combatant is a reference type.
        public struct DreadknightState
        {
            public bool ShieldEnabled;
            public bool ShieldGranted;   // one-time guard per combat
        }
        public struct StrikerState
        {
            public int   Stacks;             // 0-8
            public float PctPerStack;         // 0 = trait not active on this unit
            public bool  BypassArmor;         // BP8: attacks ignore 40% of DEF
            public bool  MaxStackSpeedBonus;  // BP6 + Carry: ActionInterval -30% at max stacks
        }
        public struct TricksterState
        {
            public bool DashEnabled;          // BP2
            public bool BleedEnabled;         // BP4
            public bool DashTriggered;         // one-time per combat
            public bool Untargetable;          // true during post-dash window
            public int  UntargetableTicks;     // countdown
            public bool BleedNextAttack;       // next attack applies bleed
        }
        public struct ElementalistState
        {
            public float ExplosionPct;   // 0 = not active; 0.10 (BP6) or 0.20 (BP8)
            public bool  TrueDamage;      // BP8: explosion ignores MR
        }
        public struct RangerState
        {
            public float BonusDmgPerHex;  // 0 = not active; 0.05 (BP4) or 0.12 (BP6)
        }

        public DreadknightState  Dreadknight;
        public StrikerState      Striker;
        public TricksterState    Trickster;
        public ElementalistState Elementalist;
        public RangerState       Ranger;

        // ── Active Skill System fields ────────────────────────────────
        public CastState        State;                 // default Idle
        public int              CastTicksRemaining;
        public HexCoord         PendingTargetHex;       // locked in at cast-trigger time
        public bool             IsSilenced;
        public int              SilenceTicksRemaining;
        public bool             IsStunned;
        public int              StunTicksRemaining;
        public SkillDefinition  Skill;                  // Archetype.None = no skill
        public float            InterceptPct;           // Phalanx: % of ally damage redirected to self
        public int              InterceptTicksRemaining;
        public string           CurrentTargetId;        // last basic-attack target; used by the "Current Target" priority sort
    }
}
