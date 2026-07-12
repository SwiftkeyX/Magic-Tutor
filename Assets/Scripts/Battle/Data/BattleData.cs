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
        public string ChampionId;       // Bridge to ChampionData.Id — see StudentRoster.md
        public string DisplayName;
        public int MaxHP;
        public int ATK;
        public int DEF;
        public int MG;
        public int MR;
        public float AttackSpeed;       // attacks per second
        public int CRIT;
        public int Range = 1;   // 1 = melee, 2 = ranged
        public int MaxMana      = 100;  // mana threshold to cast ability
        public int StartingMana = 0;    // mana at battle start (before trait bonuses)
        public List<BattleBehaviorFlag> Flags = new List<BattleBehaviorFlag>();
        public SkillDefinition Skill = new SkillDefinition();
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
        public int MaxMana      = 100;
        public int StartingMana = 0;
        public List<BattleBehaviorFlag> Flags = new List<BattleBehaviorFlag>();
        public SkillDefinition Skill = new SkillDefinition();
    }

    // Read-only snapshot exposed to BattleHUD and BattleBoardManager for initialization.
    public class CombatantSnapshot
    {
        public string Id;
        public string ChampionId;       // Players only — bridge to ChampionData.Id, see BattleBoardManager.md
        public string DisplayName;
        public bool IsStudent;
        public int MaxHP;
        public int CurrentHP;
        public HexCoord Position;
        public int Range;
        public List<BattleBehaviorFlag> Flags;
        public int Mana;
        public int MaxMana;
    }
}
