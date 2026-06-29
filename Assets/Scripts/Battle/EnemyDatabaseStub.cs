using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool.Battle
{
    public class EnemyDatabaseStub : MonoBehaviour
    {
        public List<EnemyCombatData> GetEnemies() => new List<EnemyCombatData>
        {
            new EnemyCombatData { Id = "brute", DisplayName = "Brute",
                MaxHP = 55, ATK = 10, DEF = 3, MG = 0, MR = 1, SPD = 3, CRIT = 0, Range = 1 },
            new EnemyCombatData { Id = "witch", DisplayName = "Witch",
                MaxHP = 35, ATK = 5, DEF = 1, MG = 12, MR = 4, SPD = 5, CRIT = 8, Range = 2 },
            new EnemyCombatData { Id = "sniper", DisplayName = "Sniper",
                MaxHP = 40, ATK = 9, DEF = 2, MG = 0, MR = 2, SPD = 6, CRIT = 20, Range = 3 },
        };
    }
}
