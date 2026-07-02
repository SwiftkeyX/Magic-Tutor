using UnityEngine;

namespace MagicSchool.Audio
{
    /// <summary>
    /// Holds all AudioClip references for the AudioSystem.
    /// Assign clips in the Inspector on the AudioClipLibrary asset.
    /// All fields are optional — null clips are silent no-ops (AudioSystem Core Rule #8).
    /// </summary>
    [CreateAssetMenu(fileName = "AudioClipLibrary", menuName = "MagicSchool/Audio Clip Library")]
    public class AudioClipLibrary : ScriptableObject
    {
        [Header("Music Tracks")]
        public AudioClip MenuMusic;
        public AudioClip TrainingMusic;   // Used for both Recruit and Train run phases
        public AudioClip BattleMusic;
        public AudioClip YearEndMusic;
        public AudioClip VictoryMusic;
        public AudioClip DefeatMusic;

        [Header("SFX Clips")]
        public AudioClip BattleWinSting;
        public AudioClip BattleLoseSting;
        public AudioClip PromotionFanfare;
        public AudioClip RecruitChime;
    }
}
