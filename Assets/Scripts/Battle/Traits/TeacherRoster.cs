using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool.Battle
{
    [Serializable]
    public class TeacherData
    {
        public string TeacherId;
        public string Name;
        public TraitType Specialty;
        public StatType TrainingFocus;
        public int TrainingBuff;
    }

    public class TeacherRoster : MonoBehaviour
    {
        public static TeacherRoster Instance { get; private set; }

        private readonly List<TeacherData> _teachers = new List<TeacherData>();

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"[TeacherRoster] Duplicate instance found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            LoadFromSaveSystem();
        }

        public void LoadFromSaveSystem()
        {
            _teachers.Clear();

            if (SaveSystem.Instance == null)
            {
                Debug.LogError("[TeacherRoster] SaveSystem.Instance is null. Cannot load teachers.");
                return;
            }

            var save = SaveSystem.Instance.Load();
            if (save == null || save.Teachers == null)
            {
                return;
            }

            foreach (var saveData in save.Teachers)
            {
                try
                {
                    var specialty = Enum.Parse<TraitType>(saveData.Specialty);
                    var focus = Enum.Parse<StatType>(saveData.TrainingFocus);

                    var teacher = new TeacherData
                    {
                        TeacherId = saveData.TeacherId,
                        Name = saveData.Name,
                        Specialty = specialty,
                        TrainingFocus = focus,
                        TrainingBuff = saveData.TrainingBuff
                    };

                    _teachers.Add(teacher);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[TeacherRoster] Failed to parse teacher data: {saveData.Name}. Exception: {e}");
                }
            }
        }

        public void AddTeacher(TeacherData teacher)
        {
            if (teacher == null) return;

            _teachers.Add(teacher);

            if (SaveSystem.Instance != null)
            {
                var save = SaveSystem.Instance.Load();
                
                var saveData = new TeacherSaveData
                {
                    TeacherId = teacher.TeacherId,
                    Name = teacher.Name,
                    Specialty = teacher.Specialty.ToString(),
                    TrainingFocus = teacher.TrainingFocus.ToString(),
                    TrainingBuff = teacher.TrainingBuff
                };

                save.Teachers.Add(saveData);
                SaveSystem.Instance.Save(save);
            }
            else
            {
                Debug.LogError("[TeacherRoster] SaveSystem.Instance is null. Cannot persist new teacher.");
            }
        }

        public int GetTrainingBuff(StatType stat)
        {
            int total = 0;
            foreach (var t in _teachers)
            {
                if (t.TrainingFocus == stat)
                {
                    total += t.TrainingBuff;
                }
            }
            return total;
        }

        public IReadOnlyList<TeacherData> GetAll()
        {
            return _teachers.AsReadOnly();
        }

        public int GetTeacherCountByFocus(StatType stat)
        {
            int count = 0;
            foreach (var t in _teachers)
            {
                if (t.TrainingFocus == stat)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
