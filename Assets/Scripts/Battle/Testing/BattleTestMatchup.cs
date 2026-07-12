using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool.Battle
{
    // A reusable, named "Team A vs Team B" preset for BattleTestHarness — lets a specific
    // matchup be set up once and re-run indefinitely instead of rebuilt from scratch each session.
    [CreateAssetMenu(fileName = "BattleTestMatchup", menuName = "MagicSchool/BattleTestMatchup")]
    public class BattleTestMatchup : ScriptableObject
    {
        [Header("Team A — Player side (bench, drag-placed as usual)")]
        public List<string> PlayerChampionIds = new List<string>();

        [Header("Team B — Enemy side (auto-placed as usual)")]
        public List<string> EnemyChampionIds = new List<string>();

        [SerializeField] private ChampionRoster _championRoster;

        private void OnValidate()
        {
            // Editor-time validation only; an unassigned asset skips the check silently
            // rather than throwing.
            if (_championRoster == null) return;

            var allIds = new HashSet<string>();
            foreach (var c in _championRoster.GetAllChampions()) allIds.Add(c.Id);

            CheckIds(PlayerChampionIds, "PlayerChampionIds", allIds);
            CheckIds(EnemyChampionIds, "EnemyChampionIds", allIds);
        }

        private void CheckIds(List<string> ids, string fieldName, HashSet<string> allIds)
        {
            if (ids == null) return;
            var seen = new HashSet<string>();
            foreach (var id in ids)
            {
                if (string.IsNullOrEmpty(id)) continue;
                if (!allIds.Contains(id))
                    Debug.LogWarning($"[BattleTestMatchup] '{name}'.{fieldName} contains unknown champion id: '{id}'");
                if (!seen.Add(id))
                    Debug.LogWarning($"[BattleTestMatchup] '{name}'.{fieldName} contains duplicate champion id: '{id}'");
            }
        }
    }
}
