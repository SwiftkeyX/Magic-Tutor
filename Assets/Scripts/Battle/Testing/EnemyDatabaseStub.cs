using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool.Battle
{
    public class EnemyDatabaseStub : MonoBehaviour
    {
        [SerializeField] private List<EnemyCombatData> _enemies = new List<EnemyCombatData>();

        public List<EnemyCombatData> GetEnemies() => new List<EnemyCombatData>(_enemies);

        public EnemyCombatData GetEnemyById(string id) => _enemies.Find(e => e.Id == id);

        private void Reset()
        {
            _enemies = new List<EnemyCombatData>
            {
                new EnemyCombatData { Id = "brute",  DisplayName = "Brute",  MaxHP = 600, ATK = 55, DEF = 40, MG = 0,  MR = 30, AttackSpeed = 0.55f, Range = 1, Flags = new List<BattleBehaviorFlag>() },
                new EnemyCombatData { Id = "witch",  DisplayName = "Witch",  MaxHP = 450, ATK = 0,  DEF = 20, MG = 50, MR = 20, AttackSpeed = 0.60f, Range = 2, Flags = new List<BattleBehaviorFlag> { BattleBehaviorFlag.MagicAttack } },
                new EnemyCombatData { Id = "sniper", DisplayName = "Sniper", MaxHP = 500, ATK = 60, DEF = 15, MG = 0,  MR = 15, AttackSpeed = 0.65f, Range = 3, Flags = new List<BattleBehaviorFlag>() },
            };
        }
    }
}
