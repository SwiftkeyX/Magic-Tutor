using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool.Battle
{
    [Serializable]
    public class EnemyData
    {
        public string EnemyId;                      // Designer-authored unique identifier
        public string DisplayName;                  // Shown in HUD
        public int BaseHP;
        public int BaseATK;
        public int BaseDEF;
        public int BaseMG;
        public int BaseMR;
        public int BaseSPD;
        public int BaseCRIT;
        public int Range = 1;                       // 1 = melee, 2+ = ranged
        public List<BattleBehaviorFlag> Flags = new List<BattleBehaviorFlag>();
        public Sprite Icon;

        public EnemyData Clone()
        {
            return new EnemyData
            {
                EnemyId = EnemyId,
                DisplayName = DisplayName,
                BaseHP = BaseHP,
                BaseATK = BaseATK,
                BaseDEF = BaseDEF,
                BaseMG = BaseMG,
                BaseMR = BaseMR,
                BaseSPD = BaseSPD,
                BaseCRIT = BaseCRIT,
                Range = Range,
                Flags = new List<BattleBehaviorFlag>(Flags),
                Icon = Icon
            };
        }

        public EnemyCombatData ToCombatData()
        {
            return new EnemyCombatData
            {
                Id = EnemyId,
                DisplayName = DisplayName,
                MaxHP = BaseHP,
                ATK = BaseATK,
                DEF = BaseDEF,
                MG = BaseMG,
                MR = BaseMR,
                AttackSpeed = 0.4f + BaseSPD * 0.05f, // Base 0.4 attacks/sec + 5% per SPD point
                CRIT = BaseCRIT,
                Range = Range,
                MaxMana = 100,
                StartingMana = 0,
                Flags = new List<BattleBehaviorFlag>(Flags),
                Skill = new SkillDefinition() // Enemies in v1 use basic attacks / flags, no active skills
            };
        }
    }

    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "MagicSchool/EnemyDatabase")]
    public class EnemyDatabase : ScriptableObject
    {
        public List<EnemyData> Year1Squad = new List<EnemyData>();
        public List<EnemyData> Year2Squad = new List<EnemyData>();
        public List<EnemyData> Year3Squad = new List<EnemyData>();

        public List<EnemyData> GetEnemiesForYear(int year)
        {
            List<EnemyData> source = year switch
            {
                1 => Year1Squad,
                2 => Year2Squad,
                3 => Year3Squad,
                _ => null
            };

            if (source == null)
            {
                Debug.LogError($"[EnemyDatabase] GetEnemiesForYear called with invalid year: {year}");
                return new List<EnemyData>();
            }

            return source.ConvertAll(e => e.Clone());
        }

        public List<EnemyCombatData> GetEnemiesCombatDataForYear(int year)
        {
            var squad = GetEnemiesForYear(year);
            return squad.ConvertAll(e => e.ToCombatData());
        }

        private void OnValidate()
        {
            var ids = new HashSet<string>();
            ValidateSquad(Year1Squad, 1, ids);
            ValidateSquad(Year2Squad, 2, ids);
            ValidateSquad(Year3Squad, 3, ids);
        }

        private void ValidateSquad(List<EnemyData> squad, int year, HashSet<string> ids)
        {
            if (squad == null) return;
            foreach (var enemy in squad)
            {
                if (string.IsNullOrEmpty(enemy.EnemyId))
                {
                    Debug.LogWarning($"[EnemyDatabase] Enemy in Year {year} squad has empty ID!");
                    continue;
                }

                if (!ids.Add(enemy.EnemyId))
                {
                    Debug.LogError($"[EnemyDatabase] Duplicate Enemy ID found: {enemy.EnemyId} in Year {year} squad.");
                }

                if (enemy.BaseHP <= 0 || enemy.BaseATK < 0 || enemy.BaseDEF < 0)
                {
                    Debug.LogError($"[EnemyDatabase] Enemy {enemy.EnemyId} has invalid stats (HP must be > 0, ATK/DEF must be >= 0).");
                }
            }
        }
    }
}
