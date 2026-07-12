using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool.Battle
{
    // LEGACY: Replaced by ChampionRoster.cs (Milestone 2+). Retained only as fallback if ChampionRoster is absent.
    public class StudentRosterStub : MonoBehaviour
    {
        public List<StudentCombatData> GetStudents() => new List<StudentCombatData>
        {
            new StudentCombatData { Id = "warrior", DisplayName = "Warrior",
                MaxHP = 60, ATK = 12, DEF = 5, MG = 0, MR = 2, AttackSpeed = 0.28f, CRIT = 5, Range = 1 },
            new StudentCombatData { Id = "mage", DisplayName = "Mage",
                MaxHP = 40, ATK = 4, DEF = 1, MG = 14, MR = 5, AttackSpeed = 0.24f, CRIT = 10, Range = 2 },
            new StudentCombatData { Id = "archer", DisplayName = "Archer",
                MaxHP = 45, ATK = 10, DEF = 2, MG = 0, MR = 3, AttackSpeed = 0.42f, CRIT = 15, Range = 3 },
        };
    }
}
