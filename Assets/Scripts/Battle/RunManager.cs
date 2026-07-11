using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public static bool EnableTeacherSystem = false; // Disabled by default for stability

        [SerializeField] private RunConfig _config;
        [SerializeField] private EnemyDatabase _enemyDatabase;
        [SerializeField] private PromotionConfig _promotionConfig;
        [SerializeField] private TrainingConfig _trainingConfig;

        public void Initialize(RunConfig config, EnemyDatabase enemyDatabase, PromotionConfig promotionConfig, TrainingConfig trainingConfig)
        {
            if (config != null) _config = config;
            if (enemyDatabase != null) _enemyDatabase = enemyDatabase;
            if (promotionConfig != null) _promotionConfig = promotionConfig;
            if (trainingConfig != null) _trainingConfig = trainingConfig;
        }

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

            // Configs are normally supplied by GameManager.Initialize(); these are last-resort safety nets.
            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<RunConfig>();
                Debug.LogWarning("[RunManager] RunConfig not assigned via GameManager or Inspector. Using in-memory defaults.");
            }

            if (_enemyDatabase == null)
            {
                Debug.LogError("[RunManager] EnemyDatabase not assigned via GameManager or Inspector. Battles will resolve with no enemies.");
            }

            if (_trainingConfig == null)
            {
                _trainingConfig = ScriptableObject.CreateInstance<TrainingConfig>();
                Debug.LogWarning("[RunManager] TrainingConfig not assigned via GameManager or Inspector. Using in-memory defaults.");
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
                    // Same scene (School), no reload needed.
                    // Create or re-initialize TrainingSystem for this semester.
                    if (TrainingSystem.Instance != null)
                    {
                        // Reuse the existing instance (year 2+ with same scene still loaded).
                        TrainingSystem.Instance.SetConfig(_trainingConfig);
                        TrainingSystem.Instance.InitializeSemester(_config.TrainingActionsPerSemester);
                    }
                    else
                    {
                        var tsGO = new GameObject("TrainingSystem");
                        var ts = tsGO.AddComponent<TrainingSystem>();
                        ts.SetConfig(_trainingConfig);
                        ts.InitializeSemester(_config.TrainingActionsPerSemester);
                    }
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

                    // Store for ConfirmSquadPlacement (which re-calls SetCombatants with filtered subset)
                    _pendingStudents = students;
                    _pendingEnemies  = enemies;

                    // Seed resolver with the full selection pool — guarantees GetCombatantSnapshots()
                    // returns real student data when BattleBoardManager.BeginPlacement() reads it.
                    // This is the hard ordering invariant: SetCombatants BEFORE BeginPlacement.
                    resolver.SetCombatants(students, enemies);

                    // Begin Placement Phase — player selects up to MaxSquadSize students to field.
                    var board = FindObjectOfType<BattleBoardManager>();
                    if (board != null)
                    {
                        int maxSquadSize = StudentRoster.Instance != null
                            ? StudentRoster.Instance.MaxSquadSize
                            : 3;
                        _awaitingSquadConfirmation = true;
                        board.BeginPlacement(students, enemies, maxSquadSize);
                    }
                    else
                    {
                        Debug.LogError("[RunManager] BattleBoardManager not found in Battle scene.");
                    }
                }
                else
                {
                    Debug.LogError("[RunManager] AutoBattleResolver not found in Battle scene.");
                }
            }
            else if (newScene == SceneName.YearEnd && CurrentPhase == RunPhase.YearEnd)
            {
                var promo = FindObjectOfType<PromotionSystem>();
                if (promo == null)
                {
                    Debug.Log("[RunManager] PromotionSystem not found in scene. Instantiating dynamically.");
                    var promoGO = new GameObject("PromotionSystem");
                    promo = promoGO.AddComponent<PromotionSystem>();
                    promo.Initialize(_promotionConfig);
                }
                
                promo.OnPromotionComplete += HandlePromotionComplete;
            }
        }

        private BattleResult? _pendingBattleResult;

        // ── Placement sub-step state (internal to RunPhase.Battle) ───────────
        private bool                    _awaitingSquadConfirmation;
        private List<StudentCombatData> _pendingStudents = new List<StudentCombatData>();
        private List<EnemyCombatData>   _pendingEnemies  = new List<EnemyCombatData>();

        private void HandleBattleComplete(BattleResult result)
        {
            var resolver = FindObjectOfType<AutoBattleResolver>();
            if (resolver != null)
            {
                resolver.OnBattleComplete -= HandleBattleComplete;
            }

            // Do not advance yet — BattleHUD shows the outcome overlay and calls
            // CompleteBattlePhase() when the player clicks "Continue".
            _pendingBattleResult = result;
        }

        /// <summary>
        /// Called by BattleBoardManager when the player confirms their Placement Phase squad selection.
        /// Filters the full selection pool down to only the fielded students, re-establishes combatants
        /// on AutoBattleResolver with just that subset, then begins the battle simulation.
        /// Guards: must be called during Battle phase's Placement sub-step; count must be 1–MaxSquadSize.
        /// </summary>
        public void ConfirmSquadPlacement(List<string> fieldedStudentIds, Dictionary<string, HexCoord> positions)
        {
            if (CurrentPhase != RunPhase.Battle)
            {
                Debug.LogError($"[RunManager] ConfirmSquadPlacement called outside Battle phase (current: {CurrentPhase}).");
                return;
            }
            if (!_awaitingSquadConfirmation)
            {
                Debug.LogError("[RunManager] ConfirmSquadPlacement called before BeginPlacement() or called a second time.");
                return;
            }

            int maxSquadSize = StudentRoster.Instance != null ? StudentRoster.Instance.MaxSquadSize : 3;

            if (fieldedStudentIds == null || fieldedStudentIds.Count == 0 || fieldedStudentIds.Count > maxSquadSize)
            {
                Debug.LogError($"[RunManager] ConfirmSquadPlacement: invalid fielded count {fieldedStudentIds?.Count ?? 0}. Must be 1–{maxSquadSize}.");
                return;
            }

            _awaitingSquadConfirmation = false;

            var resolver = FindObjectOfType<AutoBattleResolver>();
            if (resolver == null)
            {
                Debug.LogError("[RunManager] AutoBattleResolver not found during ConfirmSquadPlacement.");
                return;
            }

            // Filter the full selection pool down to only the students the player placed on the board
            var filteredStudents = _pendingStudents
                .Where(s => fieldedStudentIds.Contains(s.Id))
                .ToList();

            Debug.Log($"[RunManager] ConfirmSquadPlacement: {filteredStudents.Count} fielded students, {_pendingEnemies.Count} enemies.");

            // Re-establish resolver with only the fielded subset (clears the full-pool data set earlier).
            // Ordering invariant: SetCombatants → SetUnitPositions → BeginBattle.
            resolver.SetCombatants(filteredStudents, _pendingEnemies);
            resolver.SetUnitPositions(positions);

            // TraitSystem.ResolveBattleBuffs() — wire here when TraitSystem is implemented.

            resolver.BeginBattle();
        }

        /// <summary>
        /// Called by BattleHUD's "Continue" button after the player has seen the outcome overlay.
        /// </summary>
        public void CompleteBattlePhase()
        {
            if (!_pendingBattleResult.HasValue)
            {
                Debug.LogError("[RunManager] CompleteBattlePhase called with no pending battle result.");
                return;
            }

            BattleResult result = _pendingBattleResult.Value;
            _pendingBattleResult = null;

            if (result.Won)
            {
                if (EnableTeacherSystem)
                {
                    TransitionToPhase(RunPhase.YearEnd);
                }
                else
                {
                    // Clear roster, advance year or end run directly
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
