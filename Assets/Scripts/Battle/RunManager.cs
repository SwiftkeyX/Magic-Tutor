using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool.Battle
{
    public enum RunPhase
    {
        None,
        Recruit,
        Train,
        Battle,
        YearEnd
    }

    public class RunManager : MonoBehaviour
    {
        public static RunManager Instance { get; private set; }

        [SerializeField] private RunConfig _config;
        [SerializeField] private EnemyDatabase _enemyDatabase;

        public int CurrentYear { get; private set; } = 1;
        public RunPhase CurrentPhase { get; private set; } = RunPhase.None;
        public bool IsPaused { get; private set; } = false;

        public event Action<RunPhase> OnPhaseChanged;
        public event Action<int> OnYearChanged;
        public event Action<bool> OnPauseChanged;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"[RunManager] Duplicate instance found. Destroying {gameObject.name}.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scene loads
        }

        private void Start()
        {
            // Register active run on GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ActiveRun = this;
            }

            // Load configs if not assigned
            if (_config == null)
            {
                _config = Resources.Load<RunConfig>("RunConfig");
                if (_config == null)
                {
                    _config = ScriptableObject.CreateInstance<RunConfig>();
                    Debug.LogWarning("[RunManager] RunConfig not found in Resources. Using defaults.");
                }
            }

            if (_enemyDatabase == null)
            {
                _enemyDatabase = Resources.Load<EnemyDatabase>("EnemyDatabase");
                if (_enemyDatabase == null)
                {
                    Debug.LogError("[RunManager] EnemyDatabase not found in Resources!");
                }
            }

            // Subscribe to scene change events to hook scene-specific managers
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.OnSceneChanged += HandleSceneChanged;
            }

            StartRun();
        }

        private void OnDestroy()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.OnSceneChanged -= HandleSceneChanged;
            }
        }

        public void StartRun()
        {
            CurrentYear = 1;
            TransitionToPhase(RunPhase.Recruit);
        }

        public void CompleteRecruitPhase()
        {
            if (IsPaused) return;
            if (CurrentPhase != RunPhase.Recruit)
            {
                Debug.LogError($"[RunManager] CompleteRecruitPhase called during invalid phase: {CurrentPhase}");
                return;
            }

            TransitionToPhase(RunPhase.Train);
        }

        public void CompleteTrainPhase()
        {
            if (IsPaused) return;
            if (CurrentPhase != RunPhase.Train)
            {
                Debug.LogError($"[RunManager] CompleteTrainPhase called during invalid phase: {CurrentPhase}");
                return;
            }

            TransitionToPhase(RunPhase.Battle);
        }

        public void PauseRun()
        {
            if (IsPaused) return;
            IsPaused = true;
            OnPauseChanged?.Invoke(IsPaused);
        }

        public void ResumeRun()
        {
            if (!IsPaused) return;
            IsPaused = false;
            OnPauseChanged?.Invoke(IsPaused);
        }

        private void TransitionToPhase(RunPhase nextPhase)
        {
            CurrentPhase = nextPhase;
            OnPhaseChanged?.Invoke(CurrentPhase);

            switch (CurrentPhase)
            {
                case RunPhase.Recruit:
                    if (SceneLoader.Instance != null)
                    {
                        SceneLoader.Instance.LoadScene(SceneName.School);
                    }
                    break;

                case RunPhase.Train:
                    // Same scene, no reload needed. Just let UI know.
                    break;

                case RunPhase.Battle:
                    if (SceneLoader.Instance != null)
                    {
                        SceneLoader.Instance.LoadScene(SceneName.Battle);
                    }
                    break;

                case RunPhase.YearEnd:
                    if (SceneLoader.Instance != null)
                    {
                        SceneLoader.Instance.LoadScene(SceneName.YearEnd);
                    }
                    break;
            }
        }

        private void HandleSceneChanged(SceneName newScene)
        {
            StartCoroutine(SetupSceneManagersSequence(newScene));
        }

        private IEnumerator SetupSceneManagersSequence(SceneName newScene)
        {
            // Give the loaded scene one frame to initialize start-up objects
            yield return null;

            if (newScene == SceneName.School && CurrentPhase == RunPhase.Recruit)
            {
                if (StudentRoster.Instance != null)
                {
                    StudentRoster.Instance.GenerateStudents();
                }
                else
                {
                    Debug.LogError("[RunManager] StudentRoster.Instance is null when entering Recruit phase.");
                }
            }
            else if (newScene == SceneName.Battle && CurrentPhase == RunPhase.Battle)
            {
                var resolver = FindObjectOfType<AutoBattleResolver>();
                if (resolver != null)
                {
                    resolver.OnBattleComplete += HandleBattleComplete;

                    var students = StudentRoster.Instance != null 
                        ? StudentRoster.Instance.GetStudentsForBattle() 
                        : new List<StudentCombatData>();

                    var enemies = _enemyDatabase != null 
                        ? _enemyDatabase.GetEnemiesCombatDataForYear(CurrentYear) 
                        : new List<EnemyCombatData>();

                    resolver.SetCombatants(students, enemies);
                }
                else
                {
                    Debug.LogError("[RunManager] AutoBattleResolver not found in Battle scene.");
                }
            }
            else if (newScene == SceneName.YearEnd && CurrentPhase == RunPhase.YearEnd)
            {
                var promo = FindObjectOfType<PromotionSystem>();
                if (promo != null)
                {
                    promo.OnPromotionComplete += HandlePromotionComplete;
                }
                else
                {
                    Debug.LogError("[RunManager] PromotionSystem not found in YearEnd scene.");
                }
            }
        }

        private void HandleBattleComplete(BattleResult result)
        {
            var resolver = FindObjectOfType<AutoBattleResolver>();
            if (resolver != null)
            {
                resolver.OnBattleComplete -= HandleBattleComplete;
            }

            if (result.Won)
            {
                TransitionToPhase(RunPhase.YearEnd);
            }
            else
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.EndRun(won: false, yearReached: CurrentYear);
                }
                Destroy(gameObject);
            }
        }

        private void HandlePromotionComplete()
        {
            var promo = FindObjectOfType<PromotionSystem>();
            if (promo != null)
            {
                promo.OnPromotionComplete -= HandlePromotionComplete;
            }

            if (StudentRoster.Instance != null)
            {
                StudentRoster.Instance.Clear();
            }

            if (CurrentYear < _config.MaxYears)
            {
                CurrentYear++;
                OnYearChanged?.Invoke(CurrentYear);
                TransitionToPhase(RunPhase.Recruit);
            }
            else
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.EndRun(won: true, yearReached: 3);
                }
                Destroy(gameObject);
            }
        }
    }
}
