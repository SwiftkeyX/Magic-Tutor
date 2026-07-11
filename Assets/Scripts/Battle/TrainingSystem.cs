using System;
using UnityEngine;

namespace MagicSchool.Battle
{
    /// <summary>
    /// Manages the Training phase of a semester. Created and initialized by RunManager
    /// at the start of each RunPhase.Train. The player allocates a limited pool of training
    /// actions (set by RunConfig.TrainingActionsPerSemester) to raise student stats.
    /// Destroyed implicitly when the School scene unloads on transition to Battle.
    /// </summary>
    public class TrainingSystem : MonoBehaviour
    {
        public static TrainingSystem Instance { get; private set; }

        private TrainingConfig _config;
        private int _remainingActions;
        private bool _isActive;
        private bool _exhaustedFired;

        /// <summary>Read-only: training actions remaining this semester.</summary>
        public int RemainingActions => _remainingActions;

        /// <summary>Fires every time a training action is successfully consumed.</summary>
        public event Action OnTrainingActionUsed;

        /// <summary>Fires exactly once when RemainingActions reaches zero.</summary>
        public event Action OnTrainingExhausted;

        // ────────────────────────────────── Unity lifecycle ──────────────────────────────────

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"[TrainingSystem] Duplicate instance found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // ────────────────────────────────── Public API ───────────────────────────────────────

        /// <summary>
        /// Assigns the tuning config. Called by RunManager immediately after AddComponent,
        /// before InitializeSemester.
        /// </summary>
        public void SetConfig(TrainingConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Called by RunManager at the start of each Train phase.
        /// Resets all semester state and sets the fresh action budget.
        /// </summary>
        public void InitializeSemester(int actionBudget)
        {
            _remainingActions = actionBudget;
            _isActive = true;
            _exhaustedFired = false;
            Debug.Log($"[TrainingSystem] Semester initialized with {actionBudget} training actions.");
        }

        /// <summary>
        /// Allocates one training action to the given student, raising the specified stat.
        /// Returns the actual gain applied (>= 1 on success, 0 on any rejection).
        /// Costs exactly 1 action per call regardless of gain size.
        /// </summary>
        public int AllocateTraining(string studentId, StatType stat)
        {
            // Phase guard
            if (!_isActive)
            {
                Debug.LogError("[TrainingSystem] AllocateTraining called while TrainingSystem is inactive.");
                return 0;
            }

            if (RunManager.Instance == null || RunManager.Instance.CurrentPhase != RunPhase.Train)
            {
                Debug.LogError(
                    $"[TrainingSystem] AllocateTraining called outside Train phase " +
                    $"(current: {RunManager.Instance?.CurrentPhase}).");
                return 0;
            }

            // Budget guard
            if (_remainingActions <= 0)
            {
                Debug.LogWarning("[TrainingSystem] Training budget exhausted. AllocateTraining rejected.");
                return 0;
            }

            // Config guard
            if (_config == null)
            {
                Debug.LogError("[TrainingSystem] TrainingConfig is null. Cannot compute training gain.");
                return 0;
            }

            // Student lookup
            var student = StudentRoster.Instance != null
                ? StudentRoster.Instance.GetById(studentId)
                : null;

            if (student == null)
            {
                Debug.LogError($"[TrainingSystem] Student '{studentId}' not found in StudentRoster.");
                return 0;
            }

            // Compute gain: base + teacher buff, minimum 1
            int teacherBuff = TeacherRoster.Instance != null
                ? TeacherRoster.Instance.GetTrainingBuff(stat)
                : 0;
            int baseGain = GetBaseGain(stat);
            int rawGain = Mathf.Max(1, baseGain + teacherBuff);

            // Apply gain (CRIT path clamps so TotalCRIT <= 100)
            int actualGain = ApplyStatGain(student, stat, rawGain);

            // Update metadata and budget
            student.TrainingPointsSpent++;
            _remainingActions--;

            StudentRoster.Instance.NotifyStatChanged(student);
            OnTrainingActionUsed?.Invoke();

            Debug.Log(
                $"[TrainingSystem] Trained '{student.Name}' in {stat}: +{actualGain} " +
                $"(base={baseGain}, teacherBuff={teacherBuff}). Remaining actions: {_remainingActions}.");

            // Fire exhaustion event exactly once
            if (_remainingActions == 0 && !_exhaustedFired)
            {
                _exhaustedFired = true;
                OnTrainingExhausted?.Invoke();
                Debug.Log("[TrainingSystem] Training budget exhausted. OnTrainingExhausted fired.");
            }

            return actualGain;
        }

        /// <summary>
        /// Called by SchoolHUD "Proceed to Battle" button.
        /// May be called at any point during the Train phase — exhaustion is not required.
        /// </summary>
        public void ConfirmTrainingComplete()
        {
            if (RunManager.Instance == null || RunManager.Instance.CurrentPhase != RunPhase.Train)
            {
                Debug.LogError("[TrainingSystem] ConfirmTrainingComplete called outside Train phase.");
                return;
            }

            _isActive = false;
            Debug.Log("[TrainingSystem] Training confirmed complete. Delegating to RunManager.CompleteTrainPhase().");
            RunManager.Instance.CompleteTrainPhase();
        }

        // ────────────────────────────────── Private helpers ──────────────────────────────────

        private int GetBaseGain(StatType stat)
        {
            switch (stat)
            {
                case StatType.HP:   return _config.HPTrainingGain;
                case StatType.ATK:  return _config.ATKTrainingGain;
                case StatType.DEF:  return _config.DEFTrainingGain;
                case StatType.MG:   return _config.MGTrainingGain;
                case StatType.MR:   return _config.MRTrainingGain;
                case StatType.SPD:  return _config.SPDTrainingGain;
                case StatType.CRIT: return _config.CRITTrainingGain;
                default:
                    Debug.LogError($"[TrainingSystem] Unknown StatType '{stat}' in GetBaseGain.");
                    return 0;
            }
        }

        /// <summary>
        /// Writes the gain to the matching bonus stat field on <paramref name="student"/>.
        /// For CRIT, clamps so TotalCRIT never exceeds 100. Returns the actual delta applied.
        /// </summary>
        private int ApplyStatGain(StudentData student, StatType stat, int gain)
        {
            switch (stat)
            {
                case StatType.HP:
                    student.BonusHP += gain;
                    return gain;

                case StatType.ATK:
                    student.BonusATK += gain;
                    return gain;

                case StatType.DEF:
                    student.BonusDEF += gain;
                    return gain;

                case StatType.MG:
                    student.BonusMG += gain;
                    return gain;

                case StatType.MR:
                    student.BonusMR += gain;
                    return gain;

                case StatType.SPD:
                    student.BonusSPD += gain;
                    return gain;

                case StatType.CRIT:
                    // TotalCRIT = Mathf.Clamp(BaseCRIT + BonusCRIT, 0, 100).
                    // Compute headroom before writing so BonusCRIT never causes TotalCRIT > 100.
                    int headroom = 100 - student.TotalCRIT;
                    int actualCritGain = Mathf.Max(0, Mathf.Min(gain, headroom));
                    student.BonusCRIT += actualCritGain;
                    return actualCritGain;

                default:
                    Debug.LogError($"[TrainingSystem] Unknown StatType '{stat}' in ApplyStatGain.");
                    return 0;
            }
        }
    }
}
