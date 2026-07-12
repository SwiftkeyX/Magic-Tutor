using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicSchool.Battle
{
    // Migrated from a MonoBehaviour with a hardcoded static list to a ScriptableObject
    // asset, per best-practices.md's "Tunable Data" rule (mirrors EnemyDatabase). The 30
    // champion entries now live on the Champions field of the Assets/Config/ChampionRoster
    // asset instead of in this file — see the M4 migration plan for how that asset gets
    // populated (a one-time Editor script, not hand-authored per-champion assets).
    [CreateAssetMenu(fileName = "ChampionRoster", menuName = "MagicSchool/ChampionRoster")]
    public class ChampionRoster : ScriptableObject
    {
        public List<ChampionData> Champions = new List<ChampionData>();

        // No clone-on-read (unlike EnemyDatabase.GetEnemiesForYear): no consumer mutates
        // a returned ChampionData/SkillDefinition, so a defensive copy would be a pure
        // extra allocation with nothing that needs it.
        public List<ChampionData> GetAllChampions() => Champions;

        public List<StudentCombatData> GetStudents()
            => Champions.Select(c => c.ToStudentCombatData()).ToList();

        public ChampionData GetChampionById(string id)
            => Champions.Find(c => c.Id == id);

        public Dictionary<string, ChampionData> GetChampionLookup()
        {
            var dict = new Dictionary<string, ChampionData>();
            foreach (var c in Champions) dict[c.Id] = c;
            return dict;
        }

        private void OnValidate()
        {
            var ids = new HashSet<string>();
            foreach (var c in Champions)
            {
                if (string.IsNullOrEmpty(c.Id))
                {
                    Debug.LogWarning("[ChampionRoster] Champion entry has empty ID!");
                    continue;
                }

                if (!ids.Add(c.Id))
                    Debug.LogError($"[ChampionRoster] Duplicate champion ID found: {c.Id}");

                if (c.MaxHP <= 0 || c.ATK < 0 || c.DEF < 0)
                    Debug.LogError($"[ChampionRoster] Champion {c.Id} has invalid stats (MaxHP must be > 0, ATK/DEF must be >= 0).");
            }
        }
    }
}
