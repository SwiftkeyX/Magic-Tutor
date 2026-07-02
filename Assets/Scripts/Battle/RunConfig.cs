using UnityEngine;

namespace MagicSchool.Battle
{
    [CreateAssetMenu(fileName = "RunConfig", menuName = "MagicSchool/RunConfig")]
    public class RunConfig : ScriptableObject
    {
        public int MaxYears = 3;
        public int TrainingActionsPerSemester = 5;
    }
}
