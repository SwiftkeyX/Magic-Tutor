using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicSchool.Battle;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace MagicSchool.Tests
{
    public class Milestone2TestRunner : MonoBehaviour
    {
#if UNITY_EDITOR
        private const string TestPrefsKey = "MagicSchool_RunMilestone2Test";

        [MenuItem("Tools/Magic School/Run Milestone 2 Play Mode Test")]
        public static void RunTestFromMenu()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[TestRunner] Cannot trigger test while already in Play Mode. Exiting Play Mode first.");
                EditorApplication.isPlaying = false;
                return;
            }

            // Set the flag to run the test once play mode starts
            EditorPrefs.SetBool(TestPrefsKey, true);

            // Open Bootstrap scene
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity");
            
            // Enter Play Mode
            EditorApplication.isPlaying = true;
        }

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (EditorPrefs.GetBool(TestPrefsKey, false))
                {
                    EditorPrefs.SetBool(TestPrefsKey, false);

                    // Create the runner in the running play mode
                    var go = new GameObject("Milestone2TestRunner");
                    go.AddComponent<Milestone2TestRunner>();
                }
            }
        }
#endif

        private void Start()
        {
            // Make sure this runner persists so it can monitor the scenes
            DontDestroyOnLoad(gameObject);
            StartCoroutine(TestSequence());
        }

        private IEnumerator TestSequence()
        {
            Debug.Log("[TestRunner] Starting Milestone 2 Automated Play Mode Test...");

            // Temporarily enable teacher system for play mode test
            RunManager.EnableTeacherSystem = true;

            try
            {
                // 1. Wipe Save Data first to ensure clean state
                if (SaveSystem.Instance != null)
                {
                    SaveSystem.Instance.Save(new SaveData());
                    Debug.Log("[TestRunner] Cleared save data for a fresh run.");
                }

                // 2. Start a new run
                if (GameManager.Instance == null)
                {
                    Debug.LogError("[TestRunner] GameManager.Instance is null!");
                    yield break;
                }

                GameManager.Instance.StartNewRun();
                Debug.Log("[TestRunner] Triggered StartNewRun.");

                // Wait for School scene to load additively and RunManager to initialize
                yield return new WaitForSeconds(1.5f);

                // Verify RunManager is active and in Recruit phase
                if (RunManager.Instance == null)
                {
                    Debug.LogError("[TestRunner] RunManager.Instance is null after scene load!");
                    yield break;
                }

                if (RunManager.Instance.CurrentPhase != RunPhase.Recruit || RunManager.Instance.CurrentYear != 1)
                {
                    Debug.LogError($"[TestRunner] Expected Year 1 Recruit phase, got Year {RunManager.Instance.CurrentYear} {RunManager.Instance.CurrentPhase}");
                    yield break;
                }

                // Verify student roster has been generated
                if (StudentRoster.Instance == null || StudentRoster.Instance.GetAll().Count != 5)
                {
                    Debug.LogError($"[TestRunner] StudentRoster.Instance null or doesn't have 5 students!");
                    yield break;
                }

                Debug.Log("[TestRunner] Verification successful: 5 students recruited for Year 1.");

                // 3. Complete Recruit Phase -> Transition to Train
                RunManager.Instance.CompleteRecruitPhase();
                yield return new WaitForSeconds(0.5f);

                if (RunManager.Instance.CurrentPhase != RunPhase.Train)
                {
                    Debug.LogError($"[TestRunner] Expected Train phase, got {RunManager.Instance.CurrentPhase}");
                    yield break;
                }

                Debug.Log("[TestRunner] Verification successful: transitioned to Train phase.");

                // Mock training points on some students
                var students = StudentRoster.Instance.GetAll();
                students[0].TrainingPointsSpent = 4;
                students[0].BonusATK = 10;
                StudentRoster.Instance.NotifyStatChanged(students[0]);

                students[1].TrainingPointsSpent = 8;
                students[1].BonusHP = 100;
                StudentRoster.Instance.NotifyStatChanged(students[1]);

                Debug.Log($"[TestRunner] Mocked training: {students[0].Name} spent 4 points (+10 ATK), {students[1].Name} spent 8 points (+100 HP).");

                // 4. Complete Train Phase -> Transition to Battle
                RunManager.Instance.CompleteTrainPhase();
                yield return new WaitForSeconds(1.5f);

                if (RunManager.Instance.CurrentPhase != RunPhase.Battle)
                {
                    Debug.LogError($"[TestRunner] Expected Battle phase, got {RunManager.Instance.CurrentPhase}");
                    yield break;
                }

                // Verify battle resolver combatants
                var resolver = FindObjectOfType<AutoBattleResolver>();
                if (resolver == null)
                {
                    Debug.LogError("[TestRunner] AutoBattleResolver not found in Battle scene!");
                    yield break;
                }

                // Mock battle complete - Won = true
                Debug.Log("[TestRunner] Intercepting AutoBattleResolver to force a WIN for Year 1.");
                var battleCompleteMethod = resolver.GetType().GetMethod("HandleComplete", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (battleCompleteMethod != null)
                {
                    // Force win
                    var result = new BattleResult { Won = true, TicksElapsed = 50, TimedOut = false };
                    battleCompleteMethod.Invoke(resolver, new object[] { result });
                }
                else
                {
                    // Fallback direct call on RunManager's handler via reflection
                    var handleBattleCompleteMethod = RunManager.Instance.GetType().GetMethod("HandleBattleComplete", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var result = new BattleResult { Won = true, TicksElapsed = 50, TimedOut = false };
                    handleBattleCompleteMethod.Invoke(RunManager.Instance, new object[] { result });
                }

                yield return new WaitForSeconds(1.5f);

                if (RunManager.Instance.CurrentPhase != RunPhase.YearEnd)
                {
                    Debug.LogError($"[TestRunner] Expected YearEnd phase after battle win, got {RunManager.Instance.CurrentPhase}");
                    yield break;
                }

                // Verify promotion candidates
                float elapsed1 = 0f;
                while (FindObjectOfType<PromotionSystem>() == null && elapsed1 < 3f)
                {
                    yield return new WaitForSeconds(0.1f);
                    elapsed1 += 0.1f;
                }
                var promo = FindObjectOfType<PromotionSystem>();
                if (promo == null)
                {
                    Debug.LogError("[TestRunner] PromotionSystem not found in YearEnd scene!");
                    yield break;
                }

                if (promo.GetCandidates().Count != 5)
                {
                    Debug.LogError($"[TestRunner] Expected 5 promotion candidates, got {promo.GetCandidates().Count}");
                    yield break;
                }

                // Promote students[0] and students[1]
                promo.ToggleSelection(students[0].StudentId);
                promo.ToggleSelection(students[1].StudentId);

                Debug.Log($"[TestRunner] Selected graduates for promotion: {students[0].Name} and {students[1].Name}.");

                promo.ConfirmPromotions();
                yield return new WaitForSeconds(1.5f);

                // Roster should be empty, and we should be in Year 2 Recruit phase
                if (RunManager.Instance.CurrentPhase != RunPhase.Recruit || RunManager.Instance.CurrentYear != 2)
                {
                    Debug.LogError($"[TestRunner] Expected Year 2 Recruit phase, got Year {RunManager.Instance.CurrentYear} {RunManager.Instance.CurrentPhase}");
                    yield break;
                }

                if (TeacherRoster.Instance == null || TeacherRoster.Instance.GetAll().Count != 2)
                {
                    Debug.LogError($"[TestRunner] Expected 2 teachers in TeacherRoster, got {TeacherRoster.Instance?.GetAll().Count}");
                    yield break;
                }

                Debug.Log("[TestRunner] Verification successful: Year 1 promotions complete. Promoted 2 teachers.");

                // 5. Complete Year 2 Recruit & Train
                RunManager.Instance.CompleteRecruitPhase();
                yield return new WaitForSeconds(0.5f);

                RunManager.Instance.CompleteTrainPhase();
                yield return new WaitForSeconds(1.5f);

                // Mock battle complete - Won = true for Year 2
                resolver = FindObjectOfType<AutoBattleResolver>();
                if (resolver != null)
                {
                    var result = new BattleResult { Won = true, TicksElapsed = 60, TimedOut = false };
                    resolver.GetType().GetMethod("HandleComplete", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(resolver, new object[] { result });
                }
                yield return new WaitForSeconds(1.5f);

                // YearEnd 2 - confirm promotions with 0 selected (valid case!)
                float elapsed2 = 0f;
                while (FindObjectOfType<PromotionSystem>() == null && elapsed2 < 3f)
                {
                    yield return new WaitForSeconds(0.1f);
                    elapsed2 += 0.1f;
                }
                promo = FindObjectOfType<PromotionSystem>();
                if (promo == null)
                {
                    Debug.LogError("[TestRunner] PromotionSystem not found in YearEnd scene (Year 2)!");
                    yield break;
                }
                promo.ConfirmPromotions();
                yield return new WaitForSeconds(1.5f);

                // Should be in Year 3 Recruit
                if (RunManager.Instance.CurrentPhase != RunPhase.Recruit || RunManager.Instance.CurrentYear != 3)
                {
                    Debug.LogError($"[TestRunner] Expected Year 3 Recruit, got Year {RunManager.Instance.CurrentYear} {RunManager.Instance.CurrentPhase}");
                    yield break;
                }

                Debug.Log("[TestRunner] Verification successful: Year 2 promotions complete (promoted 0 teachers).");

                // 6. Complete Year 3 Recruit & Train
                RunManager.Instance.CompleteRecruitPhase();
                yield return new WaitForSeconds(0.5f);

                RunManager.Instance.CompleteTrainPhase();
                yield return new WaitForSeconds(1.5f);

                // Mock battle complete - Won = true for Year 3 (Final Year)
                resolver = FindObjectOfType<AutoBattleResolver>();
                if (resolver != null)
                {
                    var result = new BattleResult { Won = true, TicksElapsed = 70, TimedOut = false };
                    resolver.GetType().GetMethod("HandleComplete", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(resolver, new object[] { result });
                }
                yield return new WaitForSeconds(1.5f);

                // YearEnd 3 - promote one more student
                float elapsed3 = 0f;
                while (FindObjectOfType<PromotionSystem>() == null && elapsed3 < 3f)
                {
                    yield return new WaitForSeconds(0.1f);
                    elapsed3 += 0.1f;
                }
                promo = FindObjectOfType<PromotionSystem>();
                if (promo == null)
                {
                    Debug.LogError("[TestRunner] PromotionSystem not found in YearEnd scene (Year 3)!");
                    yield break;
                }
                var finalCandidates = promo.GetCandidates();
                promo.ToggleSelection(finalCandidates[0].StudentId);
                promo.ConfirmPromotions();
                yield return new WaitForSeconds(1.5f);

                // Run should end with Victory, and GameManager state should transition to RunEnd
                if (GameManager.Instance.CurrentState != GameState.RunEnd || !GameManager.Instance.LastRunResult.Won)
                {
                    Debug.LogError($"[TestRunner] Expected RunEnd state and RunWon, got State: {GameManager.Instance.CurrentState}, Won: {GameManager.Instance.LastRunResult.Won}");
                    yield break;
                }

                if (TeacherRoster.Instance.GetAll().Count != 3)
                {
                    Debug.LogError($"[TestRunner] Expected 3 teachers in total, got {TeacherRoster.Instance.GetAll().Count}");
                    yield break;
                }

                Debug.Log("[TestRunner] Verification successful: Run completed with VICTORY! Save file contains 3 teachers.");

                // 7. Verify meta-progression load on next run
                GameManager.Instance.ReturnToMainMenu();
                yield return new WaitForSeconds(1.0f);

                // Simulate application reload of TeacherRoster
                TeacherRoster.Instance.LoadFromSaveSystem();
                if (TeacherRoster.Instance.GetAll().Count != 3)
                {
                    Debug.LogError($"[TestRunner] Next run failed to load teachers from disk! Got {TeacherRoster.Instance.GetAll().Count}");
                    yield break;
                }

                Debug.Log("[TestRunner] Verification successful: Teacher Roster loaded 3 teachers correctly from save file.");
                Debug.Log("=========================================");
                Debug.Log("  MILESTONE 2 AUTOMATED TEST PASSED !!!  ");
                Debug.Log("=========================================");
            }
            finally
            {
                RunManager.EnableTeacherSystem = false;
                Debug.Log("[TestRunner] Restored EnableTeacherSystem to false.");

#if UNITY_EDITOR
                // Clean up temporary runner
                Destroy(gameObject);
                EditorApplication.isPlaying = false;
#endif
            }
        }
    }
}
