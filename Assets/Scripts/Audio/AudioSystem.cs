using System.Collections;
using UnityEngine;
using MagicSchool.Battle;

namespace MagicSchool.Audio
{
    /// <summary>
    /// Persistent singleton audio manager. Lives on a dedicated GameObject in Bootstrap.unity.
    ///
    /// Music channel: two AudioSources (_musicA / _musicB) cross-fading per GDD formula:
    ///   outgoingVolume = musicVolume * (1 - t)
    ///   incomingVolume = musicVolume * t
    ///
    /// SFX pool: SfxPoolSize AudioSources; first idle source is used; if all busy,
    /// the one with the smallest remaining clip time is interrupted.
    ///
    /// Architecture constraints (GDD Acceptance Criteria):
    ///   - No DontDestroyOnLoad — Bootstrap scene never unloads.
    ///   - No FindObjectOfType — event routing uses static events or static Instance refs.
    ///   - Subscribe in OnEnable, unsubscribe in OnDisable (Core Rule #5).
    ///   - Null clips are silent no-ops, never exceptions (Core Rule #8).
    /// </summary>
    public class AudioSystem : MonoBehaviour
    {
        public static AudioSystem Instance { get; private set; }

        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Clip Library")]
        [SerializeField] private AudioClipLibrary _clipLibrary;

        [Header("Tuning Knobs")]
        [SerializeField, Range(1, 24)]    private int   _sfxPoolSize              = 12;
        [SerializeField, Range(0f, 3f)]   private float _musicFadeDuration        = 1.0f;
        [SerializeField, Range(0f, 1f)]   private float _musicVolume              = 0.7f;
        [SerializeField, Range(0f, 1f)]   private float _sfxVolume                = 1.0f;
        [SerializeField, Range(0f, 0.5f)] private float _oldestInterruptThreshold = 0.15f;

        // ── Audio sources ─────────────────────────────────────────────────────
        private AudioSource   _musicA;        // Currently playing track (A/B swap after each fade)
        private AudioSource   _musicB;        // Incoming track during cross-fade
        private AudioSource[] _sfxPool;
        private Coroutine     _fadeCoroutine;

        // ── Subscription tracking ─────────────────────────────────────────────
        private bool _gmSubscribed;           // GameManager.OnGameStateChanged
        private bool _rosterSubscribed;       // StudentRoster.OnRosterChanged
        private bool _runManagerSubscribed;   // RunManager.OnPhaseChanged (per-run, dynamic)

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"[AudioSystem] Duplicate instance found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Debug.Log("[AudioSystem] Awake initialized.");
            BuildAudioSources();
        }

        private void OnEnable()
        {
            // GameManager.Instance may not be set yet if Awake order placed AudioSystem before GameManager.
            // Start() provides a second-chance subscription for the initial run.
            TrySubscribeToGameManager();
            TrySubscribeToStudentRoster();

            // Static events require no instance — safe to subscribe immediately
            AutoBattleResolver.OnAnyBattleComplete += OnBattleComplete;
            PromotionSystem.OnAnyPromotionComplete  += OnPromotionComplete;
        }

        private void Start()
        {
            // Second-chance subscriptions if the instances were not available during OnEnable
            TrySubscribeToGameManager();
            TrySubscribeToStudentRoster();

            // Read current game state — all Awakes are guaranteed to have run by Start
            var initialState = GameManager.Instance != null
                ? GameManager.Instance.CurrentState
                : GameState.MainMenu;

            SelectMusicForState(initialState);
            Debug.Log($"[AudioSystem] Start — initial state: {initialState}, pool size: {_sfxPoolSize}");
        }

        private void OnDisable()
        {
            if (_gmSubscribed && GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
                _gmSubscribed = false;
            }

            if (_rosterSubscribed && StudentRoster.Instance != null)
            {
                StudentRoster.Instance.OnRosterChanged -= OnRosterChanged;
                _rosterSubscribed = false;
            }

            AutoBattleResolver.OnAnyBattleComplete -= OnBattleComplete;
            PromotionSystem.OnAnyPromotionComplete  -= OnPromotionComplete;

            UnsubscribeFromRunManager();
        }

        private void OnApplicationPause(bool paused)
        {
            // Mute music when the app loses focus; restore on resume
            if (_musicA != null) _musicA.volume = paused ? 0f : _musicVolume;
            // _musicB is 0 except during a cross-fade, so leave it alone
        }

        // ── Subscription helpers ───────────────────────────────────────────────

        private void TrySubscribeToGameManager()
        {
            if (!_gmSubscribed && GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
                _gmSubscribed = true;
            }
        }

        private void TrySubscribeToStudentRoster()
        {
            if (!_rosterSubscribed && StudentRoster.Instance != null)
            {
                StudentRoster.Instance.OnRosterChanged += OnRosterChanged;
                _rosterSubscribed = true;
            }
        }

        private void UnsubscribeFromRunManager()
        {
            if (_runManagerSubscribed)
            {
                if (RunManager.Instance != null)
                    RunManager.Instance.OnPhaseChanged -= OnPhaseChanged;

                _runManagerSubscribed = false;
                Debug.Log("[AudioSystem] Unsubscribed from RunManager.OnPhaseChanged.");
            }
        }

        // ── Audio source construction ─────────────────────────────────────────

        private void BuildAudioSources()
        {
            _musicA             = gameObject.AddComponent<AudioSource>();
            _musicA.loop        = true;
            _musicA.volume      = _musicVolume;
            _musicA.playOnAwake = false;

            _musicB             = gameObject.AddComponent<AudioSource>();
            _musicB.loop        = true;
            _musicB.volume      = 0f;
            _musicB.playOnAwake = false;

            _sfxPool = new AudioSource[_sfxPoolSize];
            for (int i = 0; i < _sfxPoolSize; i++)
            {
                var src         = gameObject.AddComponent<AudioSource>();
                src.loop        = false;
                src.volume      = _sfxVolume;
                src.playOnAwake = false;
                _sfxPool[i]     = src;
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Sets music volume in real time. Called by the Settings UI music slider.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (_musicA != null) _musicA.volume = _musicVolume;
        }

        /// <summary>
        /// Sets SFX volume in real time. Called by the Settings UI SFX slider.
        /// </summary>
        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            if (_sfxPool == null) return;
            foreach (var src in _sfxPool)
                if (src != null) src.volume = _sfxVolume;
        }

        /// <summary>
        /// Plays a one-shot SFX clip on the pool. A null clip is a silent no-op (Core Rule #8).
        /// </summary>
        public void PlayClip(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioSystem] PlayClip called with null clip — returning silently.");
                return;
            }

            var src    = ClaimSfxSource();
            src.clip   = clip;
            src.volume = _sfxVolume;
            src.Play();
        }

        // ── Music selection ───────────────────────────────────────────────────

        private void SelectMusicForState(GameState state)
        {
            if (_clipLibrary == null) return;

            switch (state)
            {
                case GameState.MainMenu:
                    CrossFadeTo(_clipLibrary.MenuMusic);
                    break;

                case GameState.InRun:
                    // Phase-driven music: TrainingMusic starts here.
                    // RunManager.OnPhaseChanged takes over from the next frame onward
                    // (RunManager is instantiated after this event fires).
                    CrossFadeTo(_clipLibrary.TrainingMusic);
                    break;

                case GameState.RunEnd:
                    // Read LastRunResult.Won to select victory or defeat music.
                    // The battle-result STING (SFX) is handled separately by OnBattleComplete.
                    bool won = GameManager.Instance != null && GameManager.Instance.LastRunResult.Won;
                    CrossFadeTo(won ? _clipLibrary.VictoryMusic : _clipLibrary.DefeatMusic);
                    break;
            }
        }

        private void SelectMusicForPhase(RunPhase phase)
        {
            if (_clipLibrary == null) return;

            switch (phase)
            {
                case RunPhase.Recruit:
                case RunPhase.Train:
                    CrossFadeTo(_clipLibrary.TrainingMusic);
                    break;

                case RunPhase.Battle:
                    CrossFadeTo(_clipLibrary.BattleMusic);
                    break;

                case RunPhase.YearEnd:
                    CrossFadeTo(_clipLibrary.YearEndMusic);
                    break;

                case RunPhase.None:
                    break;  // No music change for uninitialized phase
            }
        }

        // ── Cross-fade ────────────────────────────────────────────────────────

        private void CrossFadeTo(AudioClip incoming)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            if (_musicFadeDuration <= 0f)
            {
                // Snap instantly — no coroutine needed
                _musicA.Stop();
                _musicA.clip   = incoming;
                _musicA.volume = _musicVolume;
                if (incoming != null) _musicA.Play();

                _musicB.Stop();
                _musicB.volume = 0f;
                return;
            }

            _fadeCoroutine = StartCoroutine(FadeCoroutine(incoming));
        }

        private IEnumerator FadeCoroutine(AudioClip incoming)
        {
            // Start incoming on _musicB at volume 0
            _musicB.clip   = incoming;
            _musicB.volume = 0f;
            if (incoming != null) _musicB.Play();

            float elapsed = 0f;
            while (elapsed < _musicFadeDuration)
            {
                elapsed        += Time.unscaledDeltaTime;
                float t         = Mathf.Clamp01(elapsed / _musicFadeDuration);
                _musicA.volume  = _musicVolume * (1f - t);   // outgoing fades out
                _musicB.volume  = _musicVolume * t;           // incoming fades in
                yield return null;
            }

            // Fade complete: stop the outgoing track, snap volumes
            _musicA.Stop();
            _musicA.volume = _musicVolume;  // reset for next time it becomes the active track

            // Swap A and B so _musicA is always "the currently playing track"
            var temp = _musicA;
            _musicA  = _musicB;
            _musicB  = temp;

            _musicA.volume = _musicVolume;
            _musicB.volume = 0f;

            _fadeCoroutine = null;
        }

        // ── SFX pool ──────────────────────────────────────────────────────────

        private AudioSource ClaimSfxSource()
        {
            // Pass 1: first idle (not playing) source
            foreach (var src in _sfxPool)
                if (src != null && !src.isPlaying) return src;

            // Pass 2: all busy — interrupt the one with least remaining clip time
            AudioSource candidate    = null;
            float       minRemaining = float.MaxValue;
            foreach (var src in _sfxPool)
            {
                if (src == null) continue;
                float remaining = (src.clip != null) ? (src.clip.length - src.time) : 0f;
                if (remaining < minRemaining)
                {
                    minRemaining = remaining;
                    candidate    = src;
                }
            }

            if (candidate != null)
            {
                candidate.Stop();
                return candidate;
            }

            // Ultimate fallback — should never reach here
            Debug.LogError("[AudioSystem] SFX pool exhausted and interrupt failed — returning slot 0.");
            return _sfxPool[0];
        }

        // ── Event handlers ─────────────────────────────────────────────────────

        private void OnGameStateChanged(GameState newState)
        {
            SelectMusicForState(newState);

            if (newState == GameState.InRun)
            {
                // RunManager is instantiated synchronously AFTER this event fires
                // (see GameManager.StartNewRun). Wait one frame so RunManager.Awake
                // has set RunManager.Instance before we subscribe.
                StartCoroutine(SubscribeToRunManagerNextFrame());
            }
            else
            {
                // Run ended or returned to menu — drop the per-run RunManager subscription
                UnsubscribeFromRunManager();
            }
        }

        private IEnumerator SubscribeToRunManagerNextFrame()
        {
            yield return null;  // RunManager.Awake has now run; Instance is set
            if (!_runManagerSubscribed && RunManager.Instance != null)
            {
                RunManager.Instance.OnPhaseChanged += OnPhaseChanged;
                _runManagerSubscribed = true;
                Debug.Log("[AudioSystem] Subscribed to RunManager.OnPhaseChanged.");
            }
        }

        private void OnPhaseChanged(RunPhase phase)
        {
            SelectMusicForPhase(phase);
        }

        private void OnBattleComplete(BattleResult result)
        {
            // Plays the battle win/lose STING (SFX only).
            // Victory/defeat MUSIC track is handled separately by OnGameStateChanged(RunEnd)
            // to avoid playing the sting twice when the run ends mid-year.
            if (_clipLibrary == null) return;
            PlayClip(result.Won ? _clipLibrary.BattleWinSting : _clipLibrary.BattleLoseSting);
        }

        private void OnPromotionComplete()
        {
            PlayClip(_clipLibrary?.PromotionFanfare);
        }

        private void OnRosterChanged()
        {
            // NOTE: StudentRoster.OnRosterChanged carries no arguments and fires on both
            // GenerateStudents() (recruit) and Clear() (between-year wipe). AudioSystem
            // cannot distinguish the two — it plays the recruit chime on any roster change.
            // Accepted limitation: the chime also sounds when the roster clears between years.
            // Resolution: StudentRoster would need a typed event (e.g. OnStudentAdded) to fix this.
            PlayClip(_clipLibrary?.RecruitChime);
        }
    }
}
