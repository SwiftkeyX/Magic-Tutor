using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool.Battle
{
    // Pre-seeds AutoBattleResolver with a specific BattleTestMatchup before BattleBoardManager.Start()
    // reads GetCombatantSnapshots() — Unity guarantees all Awake()s run before any Start() in a scene,
    // so this wins the race against AutoBattleResolver's lazy-init fallback (which would otherwise
    // default to all ChampionRoster champions vs EnemyDatabaseStub's fixed trio). Leaving _matchup
    // unassigned falls through to that same default, unchanged, for ad-hoc testing.
    public class BattleTestHarness : MonoBehaviour
    {
        [SerializeField] private AutoBattleResolver _resolver;
        [SerializeField] private ChampionRoster _championRoster;
        [SerializeField] private BattleTestMatchup _matchup;

        private void Awake()
        {
            if (_matchup == null || _resolver == null || _championRoster == null) return;

            var students = new List<StudentCombatData>();
            foreach (var id in _matchup.PlayerChampionIds)
            {
                var champ = _championRoster.GetChampionById(id);
                if (champ == null) { Debug.LogError($"[BattleTestHarness] Unknown player champion id: '{id}'"); continue; }
                students.Add(champ.ToStudentCombatData());
            }

            var enemies = new List<EnemyCombatData>();
            foreach (var id in _matchup.EnemyChampionIds)
            {
                var champ = _championRoster.GetChampionById(id);
                if (champ == null) { Debug.LogError($"[BattleTestHarness] Unknown enemy champion id: '{id}'"); continue; }
                enemies.Add(ToEnemyCombatData(champ));
            }

            _resolver.SetCombatants(students, enemies);
            Debug.Log($"[BattleTestHarness] Pre-seeded '{_matchup.name}': {students.Count} vs {enemies.Count}.");
        }

        // EnemyCombatData mirrors StudentCombatData minus ChampionId/traits — traits never apply
        // to enemies (BattleBoardManager Core Rule 5), so nothing is lost mapping a champion this way.
        private static EnemyCombatData ToEnemyCombatData(ChampionData c) => new EnemyCombatData
        {
            Id = c.Id,
            DisplayName = c.DisplayName,
            MaxHP = c.MaxHP,
            ATK = c.ATK,
            DEF = c.DEF,
            MG = c.MG,
            MR = c.MR,
            AttackSpeed = c.AttackSpeed,
            CRIT = c.CRIT,
            Range = c.Range,
            MaxMana = c.MaxMana,
            StartingMana = c.StartingMana,
            Skill = c.Skill,
        };
    }
}
