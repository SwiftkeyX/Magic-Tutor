using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool.Battle
{
    public class PromotionSystem : MonoBehaviour
    {
        public static PromotionSystem Instance { get; private set; }

        [SerializeField] private PromotionConfig _config;

        private readonly List<StudentData> _candidates = new List<StudentData>();
        private readonly List<string> _selectedIds = new List<string>();

        public IReadOnlyList<string> SelectedIds => _selectedIds.AsReadOnly();

        public event Action OnPromotionComplete;

        // Static forwarding event — AudioSystem subscribes here so it does not need
        // FindObjectOfType to reach a non-persistent PromotionSystem instance.
        public static event Action OnAnyPromotionComplete;

        public void Initialize(PromotionConfig config)
        {
            if (config != null) _config = config;
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"[PromotionSystem] Duplicate instance found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            // Config is normally supplied by RunManager.Initialize(); this is a last-resort safety net.
            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<PromotionConfig>();
                Debug.LogWarning("[PromotionSystem] PromotionConfig not assigned via RunManager or Inspector. Using in-memory defaults.");
            }

            PopulateCandidates();
        }

        private void PopulateCandidates()
        {
            _candidates.Clear();
            _selectedIds.Clear();

            if (StudentRoster.Instance != null)
            {
                _candidates.AddRange(StudentRoster.Instance.GetAll());
            }
            else
            {
                Debug.LogError("[PromotionSystem] StudentRoster.Instance is null. Cannot load candidates.");
            }
        }

        public List<StudentData> GetCandidates()
        {
            return new List<StudentData>(_candidates);
        }

        public bool ToggleSelection(string studentId)
        {
            if (RunManager.Instance == null || RunManager.Instance.CurrentPhase != RunPhase.YearEnd)
            {
                Debug.LogError("[PromotionSystem] Cannot toggle selection: not in YearEnd phase.");
                return false;
            }

            var candidate = _candidates.Find(s => s.StudentId == studentId);
            if (candidate == null)
            {
                Debug.LogWarning($"[PromotionSystem] Student with ID {studentId} is not a valid candidate.");
                return false;
            }

            if (_selectedIds.Contains(studentId))
            {
                _selectedIds.Remove(studentId);
            }
            else
            {
                _selectedIds.Add(studentId);
            }

            return true;
        }

        public void ConfirmPromotions()
        {
            if (RunManager.Instance == null || RunManager.Instance.CurrentPhase != RunPhase.YearEnd)
            {
                Debug.LogError("[PromotionSystem] ConfirmPromotions called outside YearEnd phase.");
                return;
            }

            foreach (var id in _selectedIds)
            {
                var student = _candidates.Find(s => s.StudentId == id);
                if (student == null) continue;

                var teacher = ConvertStudentToTeacher(student);
                if (TeacherRoster.Instance != null)
                {
                    TeacherRoster.Instance.AddTeacher(teacher);
                }
                else
                {
                    Debug.LogError("[PromotionSystem] TeacherRoster.Instance is null. Cannot add teacher.");
                }

                if (StudentRoster.Instance != null)
                {
                    StudentRoster.Instance.RemoveStudent(id);
                }
            }

            OnPromotionComplete?.Invoke();
            OnAnyPromotionComplete?.Invoke();
        }

        private TeacherData ConvertStudentToTeacher(StudentData student)
        {
            TraitType specialty = student.Traits.Count > 0 ? student.Traits[0] : TraitType.Vanguard;
            StatType focus = GetStatFocusForTrait(specialty);

            int buffVal = Mathf.Max(
                _config.MinTeacherBuff, 
                Mathf.Min(
                    _config.MaxTeacherBuff, 
                    Mathf.FloorToInt(student.TrainingPointsSpent * _config.BuffPerPoint)
                )
            );

            return new TeacherData
            {
                TeacherId = Guid.NewGuid().ToString(),
                Name = student.Name,
                Specialty = specialty,
                TrainingFocus = focus,
                TrainingBuff = buffVal
            };
        }

        public static StatType GetStatFocusForTrait(TraitType trait)
        {
            switch (trait)
            {
                case TraitType.Vanguard:
                case TraitType.Warden:
                case TraitType.Guardian:
                    return StatType.DEF;

                case TraitType.Striker:
                case TraitType.Trickster:
                case TraitType.Void:
                    return StatType.ATK;

                case TraitType.Elementalist:
                case TraitType.Kinetic:
                case TraitType.Oracle:
                case TraitType.Tech:
                    return StatType.MG;

                case TraitType.Ranger:
                case TraitType.Astral:
                    return StatType.SPD;

                case TraitType.Shadow:
                    return StatType.CRIT;

                case TraitType.Dreadknight:
                case TraitType.Wild:
                    return StatType.HP;

                default:
                    return StatType.MR;
            }
        }
    }
}
