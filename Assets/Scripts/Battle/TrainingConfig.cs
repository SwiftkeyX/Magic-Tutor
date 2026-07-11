using UnityEngine;

namespace MagicSchool.Battle
{
    [CreateAssetMenu(fileName = "TrainingConfig", menuName = "MagicSchool/TrainingConfig")]
    public class TrainingConfig : ScriptableObject
    {
        [Tooltip("Base HP bonus granted per training action.")]
        public int HPTrainingGain = 12;

        [Tooltip("Base physical attack bonus granted per training action.")]
        public int ATKTrainingGain = 3;

        [Tooltip("Base armor bonus granted per training action.")]
        public int DEFTrainingGain = 2;

        [Tooltip("Base magic power bonus granted per training action.")]
        public int MGTrainingGain = 3;

        [Tooltip("Base magic resistance bonus granted per training action.")]
        public int MRTrainingGain = 2;

        [Tooltip("Base speed bonus granted per training action.")]
        public int SPDTrainingGain = 1;

        [Tooltip("Base crit chance (%) bonus granted per training action.")]
        public int CRITTrainingGain = 5;
    }
}
