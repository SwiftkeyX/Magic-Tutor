using System;
using System.Collections.Generic;

namespace MagicSchool.Battle
{
    public enum DamageType { Physical, Magic }

    public enum BattleBehaviorFlag
    {
        FirstHitDouble,
        AOEAttack,
        TakesReducedDamage,
        ShadowSurge,
        MagicAttack,    // when present, unit uses MG/MR instead of ATK/DEF
    }

    public struct BattleResult
    {
        public bool Won;
        public int TicksElapsed;
        public bool TimedOut;
    }

    // Minimal data contract for a student entering battle.
    // Filled from StudentRoster (when implemented).
    [Serializable]
    public class StudentCombatData
    {
        public string Id;
        public string DisplayName;
        public int MaxHP;
        public int ATK;
        public int DEF;
        public int MG;
        public int MR;
        public float AttackSpeed;       // attacks per second — interval derived as round(1 / (AS × 0.6))
        public int CRIT;
        public int Range = 1;   // 1 = melee, 2 = ranged
        public List<BattleBehaviorFlag> Flags = new List<BattleBehaviorFlag>();
    }

    // Minimal data contract for an enemy.
    // Filled from EnemyDatabase (when implemented).
    [Serializable]
    public class EnemyCombatData
    {
        public string Id;
        public string DisplayName;
        public int MaxHP;
        public int ATK;
        public int DEF;
        public int MG;
        public int MR;
        public float AttackSpeed;       // attacks per second
        public int CRIT;
        public int Range = 1;
        public List<BattleBehaviorFlag> Flags = new List<BattleBehaviorFlag>();
    }

    // Read-only snapshot exposed to BattleHUD and BattleBoardManager for initialization.
    public class CombatantSnapshot
    {
        public string Id;
        public string DisplayName;
        public bool IsStudent;
        public int MaxHP;
        public int CurrentHP;
        public HexCoord Position;
        public int Range;
        public List<BattleBehaviorFlag> Flags;
    }
}
