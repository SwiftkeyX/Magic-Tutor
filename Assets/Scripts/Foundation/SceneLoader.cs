using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MagicSchool.Battle
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [Header("Fade Settings")]
        [SerializeField] private float _fadeOutDuration = 0.3f;
        [SerializeField] private float _fadeInDuration = 0.3f;
        [SerializeField] private Color _fadeColor = Color.black;
        [SerializeField] private CanvasGroup _fadeCanvasGroup;

        public event Action<SceneName> OnSceneChanged;

        private Scene _activeScene;
        private bool _isLoading = false;
        private readonly Queue<SceneName> _loadQueue = new Queue<SceneName>();

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"[SceneLoader] Duplicate instance found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            // Check if active scene is Bootstrap
            var active = SceneManager.GetActiveScene();
            if (active.name == SceneName.Bootstrap.ToString())
            {
                _activeScene = active;
            }
        }

        private IEnumerator Start()
        {
            // Setup dynamic fade canvas if missing from inspector
            if (_fadeCanvasGroup == null)
            {
                SetupDynamicFadeCanvas();
            }

            // On startup, automatically load MainMenu additively
            yield return StartCoroutine(LoadSceneSequence(SceneName.MainMenu, useFade: false));
        }

        public void LoadScene(SceneName target)
        {
            if (target == SceneName.Bootstrap)
            {
                Debug.LogError("[SceneLoader] Attempted to load Bootstrap scene. Bootstrap must always remain resident.");
                return;
            }

            if (_isLoading)
            {
                Debug.Log($"[SceneLoader] Load in progress. Queueing request for {target}.");
                _loadQueue.Enqueue(target);
                return;
            }

            StartCoroutine(LoadSceneSequence(target, useFade: true));
        }

        private IEnumerator LoadSceneSequence(SceneName target, bool useFade)
        {
            _isLoading = true;

            // 1. Fade out if enabled
            if (useFade && _fadeCanvasGroup != null && _fadeOutDuration > 0)
            {
                yield return StartCoroutine(Fade(1f, _fadeOutDuration));
            }

            // 2. Load target scene additively
            string targetName = target.ToString();
            var asyncLoad = SceneManager.LoadSceneAsync(targetName, LoadSceneMode.Additive);
            if (asyncLoad == null)
            {
                Debug.LogError($"[SceneLoader] Failed to load scene '{targetName}'. Make sure it is added to the Build Settings.");
                _isLoading = false;
                yield break;
            }

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // 3. Set the newly loaded scene as active
            var newlyLoadedScene = SceneManager.GetSceneByName(targetName);
            if (newlyLoadedScene.IsValid())
            {
                SceneManager.SetActiveScene(newlyLoadedScene);

                // Disable duplicate AudioListeners in the additively loaded scene
                var rootObjects = newlyLoadedScene.GetRootGameObjects();
                foreach (var go in rootObjects)
                {
                    var listeners = go.GetComponentsInChildren<AudioListener>(includeInactive: true);
                    foreach (var listener in listeners)
                    {
                        listener.enabled = false;
                        Debug.Log($"[SceneLoader] Disabled duplicate AudioListener on {go.name}/{listener.gameObject.name} in loaded scene {newlyLoadedScene.name}");
                    }
                }
            }

            // 4. Unload previously active gameplay scene
            if (_activeScene.IsValid() && _activeScene.name != SceneName.Bootstrap.ToString())
            {
                var asyncUnload = SceneManager.UnloadSceneAsync(_activeScene);
                while (asyncUnload != null && !asyncUnload.isDone)
                {
                    yield return null;
                }
            }

            // 5. Update active scene reference
            _activeScene = newlyLoadedScene;

            // 6. Fade in if enabled
            if (useFade && _fadeCanvasGroup != null && _fadeInDuration > 0)
            {
                yield return StartCoroutine(Fade(0f, _fadeInDuration));
            }

            _isLoading = false;
            OnSceneChanged?.Invoke(target);

            // 7. Process queued requests
            if (_loadQueue.Count > 0)
            {
                var nextTarget = _loadQueue.Dequeue();
                StartCoroutine(LoadSceneSequence(nextTarget, useFade: true));
            }
        }

        private IEnumerator Fade(float targetAlpha, float duration)
        {
            if (_fadeCanvasGroup == null) yield break;

            float startAlpha = _fadeCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            _fadeCanvasGroup.alpha = targetAlpha;
        }

        private void SetupDynamicFadeCanvas()
        {
            var canvasGO = new GameObject("DynamicFadeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(transform, false);

            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // ensure it sits on top of all UI

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var imageGO = new GameObject("FadeImage", typeof(Image));
            imageGO.transform.SetParent(canvasGO.transform, false);
            var image = imageGO.GetComponent<Image>();
            image.color = _fadeColor;

            var rt = imageGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _fadeCanvasGroup = canvasGO.AddComponent<CanvasGroup>();
            _fadeCanvasGroup.alpha = 0f;
            _fadeCanvasGroup.blocksRaycasts = false;
        }
    }
}
