using UnityEngine;
using MagicSchool.Battle;

namespace MagicSchool.Tests
{
    public class DebugHUD : MonoBehaviour
    {
        private bool _showDebug = true;
        private Rect _windowRect = new Rect(20, 20, 320, 500);

        private void OnGUI()
        {
            if (!_showDebug)
            {
                if (GUI.Button(new Rect(20, 20, 150, 30), "Show Debug Menu"))
                {
                    _showDebug = true;
                }
                return;
            }

            _windowRect = GUI.Window(0, _windowRect, DrawWindow, "Magic School Debug Menu");
        }

        private void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();

            // Status Display
            string stateStr = GameManager.Instance != null ? GameManager.Instance.CurrentState.ToString() : "Not Initialized";
            GUILayout.Label($"<b>Game State:</b> {stateStr}");
            
            var run = RunManager.Instance;
            if (run != null)
            {
                GUILayout.Label($"<b>Run Year:</b> {run.CurrentYear} / 3");
                GUILayout.Label($"<b>Run Phase:</b> {run.CurrentPhase}");
                RunManager.EnableTeacherSystem = GUILayout.Toggle(RunManager.EnableTeacherSystem, "Enable Teacher System (Milestone 2)");
            }
            else
            {
                GUILayout.Label("<i>No active run.</i>");
            }

            int teacherCount = TeacherRoster.Instance != null ? TeacherRoster.Instance.GetAll().Count : 0;
            GUILayout.Label($"<b>Permanent Teachers:</b> {teacherCount}");

            GUILayout.Space(10);
            GUILayout.Label("<b>Actions:</b>");

            // Game State transitions
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.MainMenu)
            {
                if (GUILayout.Button("Start New Run"))
                {
                    GameManager.Instance.StartNewRun();
                }
            }
            else if (GameManager.Instance != null)
            {
                if (GUILayout.Button("Return to Main Menu"))
                {
                    GameManager.Instance.ReturnToMainMenu();
                }
            }

            if (run != null)
            {
                GUILayout.Space(5);
                GUILayout.Label("<b>Run Controls:</b>");

                if (run.CurrentPhase == RunPhase.Recruit)
                {
                    if (GUILayout.Button("Complete Recruit Phase"))
                    {
                        run.CompleteRecruitPhase();
                    }
                }

                if (run.CurrentPhase == RunPhase.Train)
                {
                    if (GUILayout.Button("Complete Train Phase"))
                    {
                        run.CompleteTrainPhase();
                    }
                }

                if (run.CurrentPhase == RunPhase.Battle)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Force Battle WIN"))
                    {
                        MockBattleResult(true);
                    }
                    if (GUILayout.Button("Force Battle LOSS"))
                    {
                        MockBattleResult(false);
                    }
                    GUILayout.EndHorizontal();
                }

                if (run.CurrentPhase == RunPhase.YearEnd)
                {
                    var promo = FindObjectOfType<PromotionSystem>();
                    if (promo != null)
                    {
                        int candidateCount = promo.GetCandidates().Count;
                        GUILayout.Label($"Graduates: {candidateCount}");

                        if (candidateCount > 0)
                        {
                            if (GUILayout.Button("Promote 2 Random Graduates"))
                            {
                                var candidates = promo.GetCandidates();
                                var selectedList = (System.Collections.Generic.List<string>)promo.GetType().GetField("_selectedIds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(promo);
                                selectedList?.Clear();
                                
                                for (int i = 0; i < Mathf.Min(2, candidates.Count); i++)
                                {
                                    promo.ToggleSelection(candidates[i].StudentId);
                                }
                                promo.ConfirmPromotions();
                                Debug.Log("[DebugHUD] Selected and promoted 2 graduates.");
                            }
                        }
                        
                        if (GUILayout.Button("Confirm Promotions (0 selected)"))
                        {
                            promo.ConfirmPromotions();
                        }
                    }
                    else
                    {
                        GUILayout.Label("<i>Waiting for PromotionSystem...</i>");
                    }
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>Save System:</b>");
            if (GUILayout.Button("Wipe Save File"))
            {
                if (SaveSystem.Instance != null)
                {
                    SaveSystem.Instance.Save(new SaveData());
                    if (TeacherRoster.Instance != null)
                    {
                        TeacherRoster.Instance.LoadFromSaveSystem();
                    }
                    Debug.Log("[DebugHUD] Save wiped. Reloaded roster.");
                }
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Hide Debug Menu"))
            {
                _showDebug = false;
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void MockBattleResult(bool won)
        {
            var result = new BattleResult 
            { 
                Won = won, 
                TicksElapsed = 50, 
                TimedOut = false 
            };

            var resolver = FindObjectOfType<AutoBattleResolver>();
            var battleCompleteMethod = resolver != null ? resolver.GetType().GetMethod("HandleComplete", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) : null;
            if (battleCompleteMethod != null)
            {
                battleCompleteMethod.Invoke(resolver, new object[] { result });
            }
            else if (RunManager.Instance != null)
            {
                var handleBattleCompleteMethod = RunManager.Instance.GetType().GetMethod("HandleBattleComplete", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (handleBattleCompleteMethod != null)
                {
                    handleBattleCompleteMethod.Invoke(RunManager.Instance, new object[] { result });
                }
            }
        }
    }
}
