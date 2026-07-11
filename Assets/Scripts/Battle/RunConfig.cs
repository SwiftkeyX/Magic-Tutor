using UnityEngine;

namespace MagicSchool.Battle
{
    [CreateAssetMenu(fileName = "RunConfig", menuName = "MagicSchool/RunConfig")]
    public class RunConfig : ScriptableObject
    {
        public int MaxYears = 3;
        public int TrainingActionsPerSemester = 5;
        /// <summary>
        /// Enables the teacher promotion/roster system. Default false for stability.
        /// Moved here from RunManager.EnableTeacherSystem (L1 — was a static field).
        /// </summary>
        public bool EnableTeacherSystem = false;
    }
}
