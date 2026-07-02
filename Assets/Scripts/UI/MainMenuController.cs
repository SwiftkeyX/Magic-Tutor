using UnityEngine;
using UnityEngine.UIElements;
using MagicSchool.Audio;

namespace MagicSchool.Battle
{
    /// <summary>
    /// Controls the Main Menu UI panel.
    /// Attached to the UIDocument GameObject in MainMenu.unity.
    /// Owns: New Run, Continue, Settings, and Quit buttons; Settings overlay with
    /// Music/SFX sliders; footer teacher-count label.
    /// Never calls SceneLoader directly — scene transitions go through GameManager.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        // ── Cached element references (populated once in Awake) ────────────────
        private UIDocument     _document;
        private VisualElement  _root;

        private Button         _newRunButton;
        private Button         _continueButton;
        private Button         _settingsButton;
        private Button         _quitButton;

        private VisualElement  _settingsPanel;
        private Button         _settingsCloseButton;
        private Slider         _musicSlider;
        private Slider         _sfxSlider;

        private Label          _teacherCountLabel;

        // ── Local state ────────────────────────────────────────────────────────
        private bool _settingsOpen;

        // ── Lifecycle ──────────────────────────────────────────────────────────

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            if (_document == null)
            {
                Debug.LogError("[MainMenuController] UIDocument component not found on this GameObject.");
                return;
            }

            _root = _document.rootVisualElement;

            // Cache all element references — never call Q<> outside Awake
            _newRunButton        = _root.Q<Button>("new-run-button");
            _continueButton      = _root.Q<Button>("continue-button");
            _settingsButton      = _root.Q<Button>("settings-button");
            _quitButton          = _root.Q<Button>("quit-button");
            _settingsPanel       = _root.Q<VisualElement>("settings-panel");
            _settingsCloseButton = _root.Q<Button>("settings-close-button");
            _musicSlider         = _root.Q<Slider>("music-slider");
            _sfxSlider           = _root.Q<Slider>("sfx-slider");
            _teacherCountLabel   = _root.Q<Label>("teacher-count-label");

            // Wire button callbacks in code — never rely on Inspector wiring
            if (_newRunButton        != null) _newRunButton.clicked        += OnNewRunClicked;
            if (_continueButton      != null) _continueButton.clicked      += OnContinueClicked;
            if (_settingsButton      != null) _settingsButton.clicked      += OnSettingsToggled;
            if (_quitButton          != null) _quitButton.clicked          += OnQuitClicked;
            if (_settingsCloseButton != null) _settingsCloseButton.clicked += OnSettingsCloseClicked;

            // Slider value-change callbacks
            // TODO: Wire to AudioSystem.Instance once AudioSystem is implemented
            if (_musicSlider != null)
                _musicSlider.RegisterValueChangedCallback(OnMusicVolumeChanged);
            if (_sfxSlider != null)
                _sfxSlider.RegisterValueChangedCallback(OnSfxVolumeChanged);

            // Settings panel is hidden until the Settings button is pressed
            SetSettingsPanelVisible(false);

#if UNITY_WEBGL && !UNITY_EDITOR
            // Application.Quit() is a no-op in WebGL — hide the Quit button
            if (_quitButton != null)
                _quitButton.style.display = DisplayStyle.None;
#endif
        }

        private void Start()
        {
            // Edge case: GameManager missing (Bootstrap scene did not load)
            if (GameManager.Instance == null)
            {
                Debug.LogError("[MainMenuController] GameManager.Instance is null on Start. " +
                               "Bootstrap scene may not have been loaded. All buttons disabled.");
                DisableAllButtons();
                return;
            }

            ConfigureButtonVisibility(GameManager.Instance.CurrentState);

            // Footer: teacher count (TeacherRoster is created by GameManager.Awake — guard defensively)
            int count = TeacherRoster.Instance != null
                ? TeacherRoster.Instance.GetAll().Count
                : 0;
            if (_teacherCountLabel != null)
                _teacherCountLabel.text = $"Teachers: {count}";
        }

        // ── Button Visibility ──────────────────────────────────────────────────

        private void ConfigureButtonVisibility(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    SetButtonVisible(_newRunButton, true);
                    SetButtonVisible(_continueButton, false);
                    break;

                case GameState.InRun:
                    // Show both; Continue is a stub — no mid-run resume feature in MVP
                    SetButtonVisible(_newRunButton, true);
                    SetButtonVisible(_continueButton, true);
                    if (_continueButton != null)
                        _continueButton.SetEnabled(false);
                    break;

                default:
                    // RunEnd or unknown state — show New Run only
                    SetButtonVisible(_newRunButton, true);
                    SetButtonVisible(_continueButton, false);
                    break;
            }
        }

        // ── Button Handlers ────────────────────────────────────────────────────

        private void OnNewRunClicked()
        {
            // Transition guard: disable immediately to prevent double-click (GDD Edge Case)
            if (_newRunButton != null)
                _newRunButton.SetEnabled(false);

            // Close Settings panel before transitioning (GDD Edge Case)
            if (_settingsOpen)
                SetSettingsPanelVisible(false);

            GameManager.Instance.StartNewRun();
        }

        private void OnContinueClicked()
        {
            // No continue-run feature built yet — stub for MVP
            Debug.Log("[MainMenuController] Continue clicked — feature not yet implemented.");
        }

        private void OnSettingsToggled()
        {
            SetSettingsPanelVisible(!_settingsOpen);
        }

        private void OnSettingsCloseClicked()
        {
            SetSettingsPanelVisible(false);
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ── Slider Handlers ────────────────────────────────────────────────────

        private void OnMusicVolumeChanged(ChangeEvent<float> evt)
        {
            AudioSystem.Instance?.SetMusicVolume(evt.newValue);
        }

        private void OnSfxVolumeChanged(ChangeEvent<float> evt)
        {
            AudioSystem.Instance?.SetSfxVolume(evt.newValue);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void SetSettingsPanelVisible(bool visible)
        {
            _settingsOpen = visible;
            if (_settingsPanel != null)
                _settingsPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void DisableAllButtons()
        {
            if (_newRunButton   != null) _newRunButton.SetEnabled(false);
            if (_continueButton != null) _continueButton.SetEnabled(false);
            if (_settingsButton != null) _settingsButton.SetEnabled(false);
            if (_quitButton     != null) _quitButton.SetEnabled(false);
        }

        private static void SetButtonVisible(Button button, bool visible)
        {
            if (button == null) return;
            button.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
