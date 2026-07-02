using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicSchool.Battle
{
    /// <summary>
    /// Renders the School scene UI for both the Recruit and Train phases.
    /// Attached to the UIDocument GameObject in School.unity.
    ///
    /// Owns: year/phase header, student card grid, trait synergy panel,
    ///       training action area (stat buttons + counter), phase action bar.
    ///
    /// Never mutates game state directly — all player actions route through
    /// TrainingSystem.AllocateTraining() or RunManager phase-transition methods.
    /// </summary>
    public class SchoolHUD : MonoBehaviour
    {
        // ── Cached UI element references (populated once in Awake) ──────────
        private UIDocument    _document;
        private VisualElement _root;

        // Header
        private Label _yearLabel;
        private Label _phaseLabel;

        // Student grid
        private VisualElement _studentGrid;

        // Trait panel
        private VisualElement _traitList;

        // Training action area
        private VisualElement _trainingArea;
        private Label         _actionsLabel;
        private Label         _selectHint;
        private readonly Dictionary<StatType, Button> _statButtons = new Dictionary<StatType, Button>();

        // Action bar
        private Button _beginTrainingButton;
        private Button _proceedBattleButton;

        // ── Local runtime state ─────────────────────────────────────────────
        private string _selectedStudentId;

        /// <summary>studentId → card VisualElement</summary>
        private readonly Dictionary<string, VisualElement> _studentCards =
            new Dictionary<string, VisualElement>();

        /// <summary>studentId → (StatType → value Label) for O(1) partial updates</summary>
        private readonly Dictionary<string, Dictionary<StatType, Label>> _statLabels =
            new Dictionary<string, Dictionary<StatType, Label>>();

        /// <summary>Empty-roster placeholder — created once and re-parented as needed.</summary>
        private Label _emptyLabel;

        private bool      _trainingSystemSubscribed;
        private int       _maxTrainingActions = 5;   // read from TrainingSystem.RemainingActions after init
        private Coroutine _trainInitCoroutine;

        // ── Lifecycle ────────────────────────────────────────────────────────

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            if (_document == null)
            {
                Debug.LogError("[SchoolHUD] UIDocument component not found on this GameObject.");
                return;
            }

            _root = _document.rootVisualElement;

            // Cache all static element references — Q<> must never be called in Update
            _yearLabel  = _root.Q<Label>("year-label");
            _phaseLabel = _root.Q<Label>("phase-label");
            _studentGrid  = _root.Q<VisualElement>("student-grid");
            _traitList    = _root.Q<VisualElement>("trait-list");
            _trainingArea = _root.Q<VisualElement>("training-area");
            _actionsLabel = _root.Q<Label>("actions-label");
            _selectHint   = _root.Q<Label>("select-hint");

            _beginTrainingButton = _root.Q<Button>("begin-training-button");
            _proceedBattleButton = _root.Q<Button>("proceed-battle-button");

            // Stat buttons
            _statButtons[StatType.HP]   = _root.Q<Button>("train-hp");
            _statButtons[StatType.ATK]  = _root.Q<Button>("train-atk");
            _statButtons[StatType.DEF]  = _root.Q<Button>("train-def");
            _statButtons[StatType.MG]   = _root.Q<Button>("train-mg");
            _statButtons[StatType.MR]   = _root.Q<Button>("train-mr");
            _statButtons[StatType.SPD]  = _root.Q<Button>("train-spd");
            _statButtons[StatType.CRIT] = _root.Q<Button>("train-crit");

            // Wire button callbacks in code — never rely on Inspector wiring
            if (_beginTrainingButton != null)
                _beginTrainingButton.clicked += OnBeginTrainingClicked;
            if (_proceedBattleButton != null)
                _proceedBattleButton.clicked += OnProceedBattleClicked;

            // Stat button callbacks (capture stat value for lambda)
            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            {
                if (_statButtons.TryGetValue(stat, out var btn) && btn != null)
                {
                    var captured = stat;
                    btn.clicked += () => OnStatButtonClicked(captured);
                }
            }

            // Create the empty-roster placeholder once; add/remove as needed
            _emptyLabel = new Label("Waiting for students...");
            _emptyLabel.AddToClassList("empty-label");
        }

        private void OnEnable()
        {
            if (RunManager.Instance != null)
            {
                RunManager.Instance.OnPhaseChanged += OnPhaseChanged;
                RunManager.Instance.OnYearChanged  += OnYearChanged;
            }
            else
            {
                Debug.LogWarning("[SchoolHUD] RunManager.Instance is null in OnEnable — " +
                                 "phase/year events will not be received.");
            }

            if (StudentRoster.Instance != null)
            {
                StudentRoster.Instance.OnRosterChanged     += OnRosterChanged;
                StudentRoster.Instance.OnStudentStatChanged += OnStudentStatChanged;
            }
            else
            {
                Debug.LogWarning("[SchoolHUD] StudentRoster.Instance is null in OnEnable — " +
                                 "roster events will not be received.");
            }

            // TrainingSystem may not exist yet (created by RunManager at Train phase entry).
            // Try immediate subscription; phase-change handler retries if null now.
            SubscribeToTrainingSystem();
        }

        private void OnDisable()
        {
            if (_trainInitCoroutine != null)
            {
                StopCoroutine(_trainInitCoroutine);
                _trainInitCoroutine = null;
            }

            if (RunManager.Instance != null)
            {
                RunManager.Instance.OnPhaseChanged -= OnPhaseChanged;
                RunManager.Instance.OnYearChanged  -= OnYearChanged;
            }

            if (StudentRoster.Instance != null)
            {
                StudentRoster.Instance.OnRosterChanged     -= OnRosterChanged;
                StudentRoster.Instance.OnStudentStatChanged -= OnStudentStatChanged;
            }

            UnsubscribeFromTrainingSystem();
        }

        private void Start()
        {
            // Sync header with current state (RunManager is a DontDestroyOnLoad singleton)
            if (RunManager.Instance != null)
            {
                UpdateYearLabel(RunManager.Instance.CurrentYear);
                ApplyPhaseMode(RunManager.Instance.CurrentPhase);
            }
            else
            {
                Debug.LogWarning("[SchoolHUD] RunManager.Instance is null on Start.");
                ApplyPhaseMode(RunPhase.None);
            }

            RebuildCards();
            RecomputeTraitPanel();
        }

        // ── TrainingSystem subscription helpers ──────────────────────────────

        private void SubscribeToTrainingSystem()
        {
            if (_trainingSystemSubscribed) return;
            if (TrainingSystem.Instance == null) return;

            TrainingSystem.Instance.OnTrainingActionUsed += OnTrainingActionUsed;
            TrainingSystem.Instance.OnTrainingExhausted  += OnTrainingExhausted;
            _trainingSystemSubscribed = true;
            Debug.Log("[SchoolHUD] Subscribed to TrainingSystem events.");
        }

        private void UnsubscribeFromTrainingSystem()
        {
            if (!_trainingSystemSubscribed) return;
            _trainingSystemSubscribed = false;

            if (TrainingSystem.Instance == null) return;

            TrainingSystem.Instance.OnTrainingActionUsed -= OnTrainingActionUsed;
            TrainingSystem.Instance.OnTrainingExhausted  -= OnTrainingExhausted;
        }

        // ── Phase management ─────────────────────────────────────────────────

        private void OnPhaseChanged(RunPhase phase)
        {
            // When Train fires, RunManager creates/initializes TrainingSystem in the SAME call
            // AFTER invoking this event. Defer subscription by one frame so Instance is ready.
            if (phase == RunPhase.Train)
            {
                if (_trainInitCoroutine != null) StopCoroutine(_trainInitCoroutine);
                _trainInitCoroutine = StartCoroutine(SubscribeTrainingSystemNextFrame());
            }

            // Phase → Battle: hide HUD (SceneLoader transitions away)
            if (phase == RunPhase.Battle || phase == RunPhase.YearEnd)
            {
                if (_root != null)
                    _root.style.display = DisplayStyle.None;
                return;
            }

            ApplyPhaseMode(phase);
        }

        private IEnumerator SubscribeTrainingSystemNextFrame()
        {
            yield return null; // one frame — RunManager finishes InitializeSemester

            SubscribeToTrainingSystem();

            if (TrainingSystem.Instance != null)
                _maxTrainingActions = TrainingSystem.Instance.RemainingActions;

            // Refresh UI now that training budget is known
            UpdateActionsLabel();
            UpdateStatButtonsAndHint();
            _trainInitCoroutine = null;
        }

        private void ApplyPhaseMode(RunPhase phase)
        {
            if (_root != null)
                _root.style.display = DisplayStyle.Flex;

            bool isRecruit = phase == RunPhase.Recruit;
            bool isTrain   = phase == RunPhase.Train;

            // Phase label
            if (_phaseLabel != null)
                _phaseLabel.text = phase == RunPhase.None ? "—" : phase.ToString();

            // Recruit: Begin Training visible; Proceed + training area hidden
            SetElementDisplay(_beginTrainingButton, isRecruit);
            SetElementDisplay(_proceedBattleButton, isTrain);
            SetElementDisplay(_trainingArea,        isTrain);

            if (isTrain)
            {
                UpdateActionsLabel();
                UpdateStatButtonsAndHint();
            }
        }

        // ── Year label ───────────────────────────────────────────────────────

        private void OnYearChanged(int year) => UpdateYearLabel(year);

        private void UpdateYearLabel(int year)
        {
            if (_yearLabel != null)
                _yearLabel.text = $"Year {year} of 3";
        }

        // ── Student card building ────────────────────────────────────────────

        private void OnRosterChanged()
        {
            RebuildCards();
            RecomputeTraitPanel();
        }

        private void RebuildCards()
        {
            if (_studentGrid == null) return;

            _studentGrid.Clear();
            _studentCards.Clear();
            _statLabels.Clear();
            _selectedStudentId = null;

            var students = StudentRoster.Instance != null
                ? StudentRoster.Instance.GetAll()
                : null;

            if (students == null || students.Count == 0)
            {
                _studentGrid.Add(_emptyLabel);
                return;
            }

            foreach (var student in students)
            {
                var card = BuildStudentCard(student);
                _studentGrid.Add(card);
                _studentCards[student.StudentId] = card;
            }
        }

        private VisualElement BuildStudentCard(StudentData student)
        {
            var card = new VisualElement();
            card.AddToClassList("student-card");
            card.userData = student.StudentId;

            // Name
            var nameLabel = new Label(student.Name);
            nameLabel.AddToClassList("card-name");
            card.Add(nameLabel);

            // Traits
            var traitsRow = new VisualElement();
            traitsRow.AddToClassList("card-traits");
            foreach (var trait in student.Traits)
            {
                var badge = new Label(trait.ToString());
                badge.AddToClassList("trait-badge");
                traitsRow.Add(badge);
            }
            if (student.Traits.Count == 0)
            {
                var noTrait = new Label("—");
                noTrait.AddToClassList("trait-badge");
                traitsRow.Add(noTrait);
            }
            card.Add(traitsRow);

            // 7 stat rows
            var statsSection = new VisualElement();
            statsSection.AddToClassList("card-stats");

            var labelMap = new Dictionary<StatType, Label>();
            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            {
                var row = new VisualElement();
                row.AddToClassList("stat-row");

                var nameEl = new Label(stat.ToString());
                nameEl.AddToClassList("stat-name");
                row.Add(nameEl);

                var valueEl = new Label(GetStatDisplay(student, stat));
                valueEl.AddToClassList("stat-value");
                row.Add(valueEl);

                statsSection.Add(row);
                labelMap[stat] = valueEl;
            }
            card.Add(statsSection);
            _statLabels[student.StudentId] = labelMap;

            // Training pips
            var pipsLabel = new Label(FormatPips(student.TrainingPointsSpent));
            pipsLabel.AddToClassList("training-pips");
            card.Add(pipsLabel);

            // Click to select
            card.RegisterCallback<ClickEvent>(_ => OnCardClicked(student.StudentId));

            return card;
        }

        // ── Partial stat update (no full rebuild) ────────────────────────────

        private void OnStudentStatChanged(StudentData student)
        {
            if (!_statLabels.TryGetValue(student.StudentId, out var labelMap))
            {
                Debug.LogWarning(
                    $"[SchoolHUD] OnStudentStatChanged: student '{student.StudentId}' " +
                    "not found in rendered cards.");
                return;
            }

            // Update every stat label for this student
            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            {
                if (labelMap.TryGetValue(stat, out var lbl))
                    lbl.text = GetStatDisplay(student, stat);
            }

            // Update training pips label
            if (_studentCards.TryGetValue(student.StudentId, out var card))
            {
                var pipsLabel = card.Q<Label>(className: "training-pips");
                if (pipsLabel != null)
                    pipsLabel.text = FormatPips(student.TrainingPointsSpent);
            }

            // Refresh stat-button state if this is the currently selected student
            if (student.StudentId == _selectedStudentId)
                UpdateStatButtonsAndHint();

            RecomputeTraitPanel();
        }

        // ── Card selection ───────────────────────────────────────────────────

        private void OnCardClicked(string studentId)
        {
            if (_selectedStudentId == studentId) return; // already selected

            // Deselect old
            if (_selectedStudentId != null &&
                _studentCards.TryGetValue(_selectedStudentId, out var oldCard))
            {
                oldCard.RemoveFromClassList("selected");
            }

            _selectedStudentId = studentId;

            if (_studentCards.TryGetValue(studentId, out var newCard))
                newCard.AddToClassList("selected");

            UpdateStatButtonsAndHint();
        }

        // ── Training action events ───────────────────────────────────────────

        private void OnTrainingActionUsed()
        {
            UpdateActionsLabel();
            UpdateStatButtonsAndHint();
        }

        private void OnTrainingExhausted()
        {
            UpdateActionsLabel();
            UpdateStatButtonsAndHint();

            // Highlight Proceed button to draw attention
            _proceedBattleButton?.AddToClassList("proceed-highlight");
        }

        private void UpdateActionsLabel()
        {
            if (_actionsLabel == null) return;
            int remaining = TrainingSystem.Instance?.RemainingActions ?? 0;
            _actionsLabel.text = $"Actions: {remaining} / {_maxTrainingActions}";
        }

        private void UpdateStatButtonsAndHint()
        {
            bool hasSelection = !string.IsNullOrEmpty(_selectedStudentId);
            bool budgetLeft   = TrainingSystem.Instance != null &&
                                TrainingSystem.Instance.RemainingActions > 0;
            bool canTrain     = hasSelection && budgetLeft;

            foreach (var pair in _statButtons)
            {
                if (pair.Value != null)
                    pair.Value.SetEnabled(canTrain);
            }

            // Show hint when in train mode but nothing selected
            bool showHint = !hasSelection &&
                            RunManager.Instance?.CurrentPhase == RunPhase.Train;
            SetElementDisplay(_selectHint, showHint);
        }

        // ── Button handlers ──────────────────────────────────────────────────

        private void OnStatButtonClicked(StatType stat)
        {
            if (string.IsNullOrEmpty(_selectedStudentId))
            {
                Debug.LogWarning("[SchoolHUD] Stat button clicked but no student is selected.");
                return;
            }

            if (TrainingSystem.Instance == null)
            {
                Debug.LogWarning("[SchoolHUD] Stat button clicked but TrainingSystem.Instance is null.");
                return;
            }

            if (TrainingSystem.Instance.RemainingActions <= 0)
            {
                Debug.LogWarning("[SchoolHUD] Stat button clicked but training budget is exhausted.");
                return;
            }

            // Forward to TrainingSystem — never write StudentData directly (GDD Rule 2)
            TrainingSystem.Instance.AllocateTraining(_selectedStudentId, stat);
            // StudentRoster.NotifyStatChanged fires → OnStudentStatChanged updates the display
        }

        private void OnBeginTrainingClicked()
        {
            if (RunManager.Instance == null)
            {
                Debug.LogError("[SchoolHUD] OnBeginTrainingClicked: RunManager.Instance is null.");
                return;
            }

            // Disable to prevent double-click
            if (_beginTrainingButton != null)
                _beginTrainingButton.SetEnabled(false);

            RunManager.Instance.CompleteRecruitPhase();
        }

        private void OnProceedBattleClicked()
        {
            if (TrainingSystem.Instance == null)
            {
                Debug.LogWarning("[SchoolHUD] OnProceedBattleClicked: TrainingSystem.Instance is null.");
                return;
            }

            if (_proceedBattleButton != null)
                _proceedBattleButton.SetEnabled(false);

            TrainingSystem.Instance.ConfirmTrainingComplete();
        }

        // ── Trait synergy panel (self-computed per GDD Core Rule 6) ─────────

        private void RecomputeTraitPanel()
        {
            if (_traitList == null) return;
            _traitList.Clear();

            var students = StudentRoster.Instance?.GetAll();
            if (students == null || students.Count == 0) return;

            // Tally TraitType occurrences — no TraitTracker dependency (GDD Core Rule 6)
            var counts = students
                .SelectMany(s => s.Traits)
                .GroupBy(t => t)
                .OrderByDescending(g => g.Count());

            foreach (var group in counts)
            {
                var row = new VisualElement();
                row.AddToClassList("trait-row");

                var label = new Label($"{group.Key}: {group.Count()}");
                label.AddToClassList("trait-count-label");
                row.Add(label);

                _traitList.Add(row);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static string GetStatDisplay(StudentData s, StatType stat)
        {
            switch (stat)
            {
                case StatType.HP:   return s.TotalHP.ToString();
                case StatType.ATK:  return s.TotalATK.ToString();
                case StatType.DEF:  return s.TotalDEF.ToString();
                case StatType.MG:   return s.TotalMG.ToString();
                case StatType.MR:   return s.TotalMR.ToString();
                case StatType.SPD:  return s.TotalSPD.ToString();
                case StatType.CRIT: return $"{s.TotalCRIT}%";
                default:            return "?";
            }
        }

        private static string FormatPips(int pointsSpent)
        {
            return pointsSpent == 0
                ? "Training: none"
                : $"Training: {pointsSpent} pt{(pointsSpent == 1 ? "" : "s")}";
        }

        private static void SetElementDisplay(VisualElement el, bool visible)
        {
            if (el != null)
                el.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
