using System;
using UnityEngine;

namespace MagicSchool.Battle
{
    [Serializable]
    public struct RunSnapshot
    {
        public bool Won;
        public int YearReached;
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private RunConfig _runConfig;
        [SerializeField] private EnemyDatabase _enemyDatabase;
        [SerializeField] private PromotionConfig _promotionConfig;
        [SerializeField] private StudentConfig _studentConfig;

        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public RunSnapshot LastRunResult { get; private set; }
        
        // Let the active RunManager register itself here so we can track mid-run values
        public MonoBehaviour ActiveRun { get; set; } 
        
        public int TotalRunsStarted { get; private set; }
        public int TotalRunsCompleted { get; private set; }

        public event Action<GameState> OnGameStateChanged;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"[GameManager] Duplicate instance found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Debug.Log("[GameManager] Awake initialized.");

            // Dynamically instantiate persistent roster singletons if not already present
            if (FindObjectOfType<StudentRoster>() == null)
            {
                var rosterGO = new GameObject("StudentRoster");
                var comp = rosterGO.AddComponent<StudentRoster>();
                comp.Initialize(_studentConfig);
                Debug.Log($"[GameManager] Dynamically created StudentRoster. Component Null: {comp == null}");
                DontDestroyOnLoad(rosterGO);
            }
            else
            {
                Debug.Log("[GameManager] StudentRoster already exists in scene.");
            }

            if (FindObjectOfType<TeacherRoster>() == null)
            {
                var teacherRosterGO = new GameObject("TeacherRoster");
                var comp = teacherRosterGO.AddComponent<TeacherRoster>();
                Debug.Log($"[GameManager] Dynamically created TeacherRoster. Component Null: {comp == null}");
                DontDestroyOnLoad(teacherRosterGO);
            }
            else
            {
                Debug.Log("[GameManager] TeacherRoster already exists in scene.");
            }
        }

        private void Start()
        {
            // Retrieve stats from the loaded save on startup
            if (SaveSystem.Instance != null)
            {
                var save = SaveSystem.Instance.Load();
                TotalRunsStarted = save.TotalRunsStarted;
                TotalRunsCompleted = save.TotalRunsCompleted;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            gameObject.AddComponent<MagicSchool.Tests.DebugHUD>();
#endif
        }

        public void StartNewRun()
        {
            if (CurrentState == GameState.InRun)
            {
                Debug.LogError("[GameManager] Attempted to start a new run while a run is already active.");
                return;
            }

            TotalRunsStarted++;
            
            // Persist the started run counter
            if (SaveSystem.Instance != null)
            {
                var save = SaveSystem.Instance.Load();
                save.TotalRunsStarted = TotalRunsStarted;
                SaveSystem.Instance.Save(save);
            }

            CurrentState = GameState.InRun;
            OnGameStateChanged?.Invoke(CurrentState);

            // Instantiate dynamic RunManager to orchestrate the active gameplay run
            var runManagerGO = new GameObject("RunManager");
            var runManager = runManagerGO.AddComponent<RunManager>();
            runManager.Initialize(_runConfig, _enemyDatabase, _promotionConfig);
        }

        public void EndRun(bool won, int yearReached)
        {
            if (CurrentState == GameState.MainMenu)
            {
                Debug.LogError("[GameManager] Attempted to end a run while in MainMenu state.");
                return;
            }

            LastRunResult = new RunSnapshot { Won = won, YearReached = yearReached };

            if (won)
            {
                TotalRunsCompleted++;
            }

            // Persist run stats
            if (SaveSystem.Instance != null)
            {
                var save = SaveSystem.Instance.Load();
                save.TotalRunsCompleted = TotalRunsCompleted;
                SaveSystem.Instance.Save(save);
            }

            CurrentState = GameState.RunEnd;
            OnGameStateChanged?.Invoke(CurrentState);

            ActiveRun = null;

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(SceneName.MainMenu);
            }
        }

        public void ReturnToMainMenu()
        {
            CurrentState = GameState.MainMenu;
            OnGameStateChanged?.Invoke(CurrentState);

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(SceneName.MainMenu);
            }
        }

        private void OnApplicationQuit()
        {
            // If the player quits mid-run, count it as a run loss
            if (CurrentState == GameState.InRun)
            {
                int yearReached = 1;
                if (ActiveRun != null)
                {
                    // Use reflection or dynamic property retrieval to get the current year from RunManager
                    var type = ActiveRun.GetType();
                    var prop = type.GetProperty("CurrentYear");
                    if (prop != null)
                    {
                        yearReached = (int)prop.GetValue(ActiveRun);
                    }
                }

                EndRun(won: false, yearReached: yearReached);
            }
        }
    }
}
