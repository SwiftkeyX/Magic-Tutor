using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool.Battle
{
    [Serializable]
    public class StudentData
    {
        public string StudentId;        // GUID assigned at generation
        public string Name;             // drawn from name pool
        public string ChampionId;       // Link to ChampionData
        public List<TraitType> Traits = new List<TraitType>();

        // Base stats
        public int BaseHP;
        public int BaseATK;
        public int BaseDEF;
        public int BaseMG;
        public int BaseMR;
        public int BaseSPD;
        public int BaseCRIT;

        // Bonus stats
        public int BonusHP;
        public int BonusATK;
        public int BonusDEF;
        public int BonusMG;
        public int BonusMR;
        public int BonusSPD;
        public int BonusCRIT;

        public int TrainingPointsSpent; // total training actions used on this student

        // Derived properties
        public int TotalHP => BaseHP + BonusHP;
        public int TotalATK => BaseATK + BonusATK;
        public int TotalDEF => BaseDEF + BonusDEF;
        public int TotalMG => BaseMG + BonusMG;
        public int TotalMR => BaseMR + BonusMR;
        public int TotalSPD => BaseSPD + BonusSPD;
        public int TotalCRIT => Mathf.Clamp(BaseCRIT + BonusCRIT, 0, 100);
    }

    public class StudentRoster : MonoBehaviour
    {
        public static StudentRoster Instance { get; private set; }

        [SerializeField] private StudentConfig _config;
        private readonly List<StudentData> _students = new List<StudentData>();

        public event Action OnRosterChanged;
        public event Action<StudentData> OnStudentStatChanged;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"[StudentRoster] Duplicate instance found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void GenerateStudents()
        {
            if (_students.Count > 0)
            {
                Debug.LogError("[StudentRoster] GenerateStudents called while roster is non-empty.");
                return;
            }

            if (_config == null)
            {
                _config = Resources.Load<StudentConfig>("StudentConfig");
                if (_config == null)
                {
                    _config = ScriptableObject.CreateInstance<StudentConfig>();
                    Debug.LogWarning("[StudentRoster] StudentConfig not found. Using default scriptable object.");
                }
            }

            var allChampions = ChampionRoster.GetAllChampions();
            if (allChampions == null || allChampions.Count == 0)
            {
                Debug.LogError("[StudentRoster] No champions found in ChampionRoster.");
                return;
            }

            int count = _config.RecruitCountPerSemester;
            var namePool = new List<string>(_config.NamePool);
            if (namePool.Count == 0)
            {
                namePool.Add("Student");
            }

            for (int i = 0; i < count; i++)
            {
                // Choose a random champion definition to base this student on
                var champion = allChampions[UnityEngine.Random.Range(0, allChampions.Count)];

                // Choose a random name
                string name = namePool[UnityEngine.Random.Range(0, namePool.Count)];

                var student = new StudentData
                {
                    StudentId = Guid.NewGuid().ToString(),
                    Name = name,
                    ChampionId = champion.Id,
                    BaseHP = champion.MaxHP,
                    BaseATK = champion.ATK,
                    BaseDEF = champion.DEF,
                    BaseMG = champion.MG,
                    BaseMR = champion.MR,
                    BaseSPD = (int)(champion.AttackSpeed * 10f),
                    BaseCRIT = champion.CRIT
                };

                // Populate traits from champion's vertical/horizontal traits
                if (champion.VerticalTrait != VerticalTrait.None)
                {
                    if (Enum.TryParse<TraitType>(champion.VerticalTrait.ToString(), out var vt))
                    {
                        student.Traits.Add(vt);
                    }
                }
                if (champion.HorizontalTrait != HorizontalTrait.None)
                {
                    if (Enum.TryParse<TraitType>(champion.HorizontalTrait.ToString(), out var ht))
                    {
                        student.Traits.Add(ht);
                    }
                }

                _students.Add(student);
            }

            OnRosterChanged?.Invoke();
        }

        public List<StudentData> GetAll()
        {
            return new List<StudentData>(_students);
        }

        public StudentData GetById(string studentId)
        {
            var student = _students.Find(s => s.StudentId == studentId);
            if (student == null)
            {
                Debug.LogWarning($"[StudentRoster] Student with ID {studentId} not found.");
            }
            return student;
        }

        public void NotifyStatChanged(StudentData student)
        {
            if (student != null)
            {
                OnStudentStatChanged?.Invoke(student);
            }
        }

        public void RemoveStudent(string studentId)
        {
            var student = _students.Find(s => s.StudentId == studentId);
            if (student != null)
            {
                _students.Remove(student);
                OnRosterChanged?.Invoke();
            }
            else
            {
                Debug.LogWarning($"[StudentRoster] RemoveStudent failed. Unknown ID: {studentId}");
            }
        }

        public void Clear()
        {
            if (_students.Count > 0)
            {
                _students.Clear();
                OnRosterChanged?.Invoke();
            }
        }

        // Convert the active student roster to a list of combat-ready StudentCombatData
        public List<StudentCombatData> GetStudentsForBattle()
        {
            var list = new List<StudentCombatData>();
            var allChampions = ChampionRoster.GetAllChampions();

            foreach (var s in _students)
            {
                var champion = allChampions.Find(c => c.Id == s.ChampionId);
                if (champion == null) continue;

                var combatData = new StudentCombatData
                {
                    Id = s.StudentId,
                    DisplayName = $"{s.Name} ({champion.DisplayName})",
                    MaxHP = s.TotalHP,
                    ATK = s.TotalATK,
                    DEF = s.TotalDEF,
                    MG = s.TotalMG,
                    MR = s.TotalMR,
                    AttackSpeed = champion.AttackSpeed * (1f + s.TotalSPD * 0.05f), // Scale AS by 5% per point of total speed
                    CRIT = s.TotalCRIT,
                    Range = champion.Range,
                    MaxMana = champion.MaxMana,
                    StartingMana = champion.StartingMana,
                    Skill = champion.Skill,
                    Flags = champion.ToStudentCombatData().Flags
                };

                list.Add(combatData);
            }
            return list;
        }
    }
}
