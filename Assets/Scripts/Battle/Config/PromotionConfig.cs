using UnityEngine;

namespace MagicSchool.Battle
{
    [CreateAssetMenu(fileName = "PromotionConfig", menuName = "MagicSchool/PromotionConfig")]
    public class PromotionConfig : ScriptableObject
    {
        public int MinTeacherBuff = 1;
        public int MaxTeacherBuff = 5;
        public float BuffPerPoint = 0.5f;
    }
}
