using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.IO;

namespace MagicSchool.Editor
{
    /// One-shot editor script to create the Battle.unity scene.
    /// Run via: Tools > Magic School > Create Battle Scene
    public static class BattleSceneSetup
    {
        [MenuItem("Tools/Magic School/Create Battle Scene")]
        public static void Execute()
        {
            const string scenePath = "Assets/Scenes/Battle.unity";
            Directory.CreateDirectory("Assets/Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── Camera ───────────────────────────────────────────────────────────
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic     = true;
            cam.orthographicSize = 5.5f;
            cam.backgroundColor  = new Color(0.08f, 0.08f, 0.12f);
            cam.clearFlags       = CameraClearFlags.SolidColor;
            camGO.transform.position = new Vector3(3.3f, 3.3f, -10f);

            // ── BattleBoard root (holds all the logical components) ───────────────
            var boardGO = new GameObject("BattleBoard");
            boardGO.AddComponent<MagicSchool.Battle.HexGrid>();
            var resolver = boardGO.AddComponent<MagicSchool.Battle.AutoBattleResolver>();
            var mgr      = boardGO.AddComponent<MagicSchool.Battle.BattleBoardManager>();
            var roster   = boardGO.AddComponent<MagicSchool.Battle.StudentRosterStub>();
            var enemies  = boardGO.AddComponent<MagicSchool.Battle.EnemyDatabaseStub>();

            // Load existing prefabs
            const string tilePrefabPath = "Assets/Prefabs/Battle/HexTile.prefab";
            const string unitPrefabPath = "Assets/Prefabs/Battle/BattleUnit.prefab";
            var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(tilePrefabPath);
            var unitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(unitPrefabPath);

            if (tilePrefab == null) { Debug.LogError("[BattleSceneSetup] HexTile prefab not found. Run BattleTestSetup first."); return; }
            if (unitPrefab == null) { Debug.LogError("[BattleSceneSetup] BattleUnit prefab not found. Run BattleTestSetup first."); return; }

            // ── Canvas ────────────────────────────────────────────────────────────
            var canvasGO = new GameObject("Canvas");
            var canvas   = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // EventSystem — InputSystemUIInputModule handles both legacy and new Input System
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            EnsureInputHandlingBoth();

            // Bench panel (bottom strip)
            var benchGO  = new GameObject("BenchPanel");
            benchGO.transform.SetParent(canvasGO.transform, false);
            var benchImg = benchGO.AddComponent<Image>();
            benchImg.color = new Color(0f, 0f, 0f, 0.6f);
            var benchRT  = benchGO.GetComponent<RectTransform>();
            benchRT.anchorMin  = new Vector2(0f, 0f);
            benchRT.anchorMax  = new Vector2(1f, 0f);
            benchRT.pivot      = new Vector2(0.5f, 0f);
            benchRT.sizeDelta  = new Vector2(0f, 120f);
            benchRT.anchoredPosition = Vector2.zero;
            var hLayout = benchGO.AddComponent<HorizontalLayoutGroup>();
            hLayout.childAlignment     = TextAnchor.MiddleCenter;
            hLayout.spacing            = 10f;
            hLayout.padding            = new RectOffset(10, 10, 10, 10);
            hLayout.childControlWidth  = false;
            hLayout.childControlHeight = false;

            // Start Battle button
            var btnGO = new GameObject("StartBattleButton");
            btnGO.transform.SetParent(canvasGO.transform, false);
            var btn    = btnGO.AddComponent<Button>();
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.7f, 0.2f, 1f);
            var btnRT  = btnGO.GetComponent<RectTransform>();
            btnRT.anchorMin  = new Vector2(0.5f, 0f);
            btnRT.anchorMax  = new Vector2(0.5f, 0f);
            btnRT.pivot      = new Vector2(0.5f, 0f);
            btnRT.sizeDelta  = new Vector2(160f, 50f);
            btnRT.anchoredPosition = new Vector2(0f, 130f);

            var btnLabelGO = new GameObject("Label");
            btnLabelGO.transform.SetParent(btnGO.transform, false);
            var btnTxt = btnLabelGO.AddComponent<Text>();
            btnTxt.text      = "Start Battle";
            btnTxt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnTxt.fontSize  = 18;
            btnTxt.alignment = TextAnchor.MiddleCenter;
            btnTxt.color     = Color.white;
            var btnLabelRT   = btnLabelGO.GetComponent<RectTransform>();
            btnLabelRT.anchorMin = Vector2.zero; btnLabelRT.anchorMax = Vector2.one;
            btnLabelRT.offsetMin = btnLabelRT.offsetMax = Vector2.zero;

            // Outcome overlay
            var overlayGO = new GameObject("OutcomePanel");
            overlayGO.transform.SetParent(canvasGO.transform, false);
            var overlayImg = overlayGO.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.75f);
            var overlayRT  = overlayGO.GetComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero; overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = overlayRT.offsetMax = Vector2.zero;

            var outcomeLabelGO = new GameObject("OutcomeText");
            outcomeLabelGO.transform.SetParent(overlayGO.transform, false);
            var outcomeTxt = outcomeLabelGO.AddComponent<Text>();
            outcomeTxt.text      = "VICTORY!";
            outcomeTxt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            outcomeTxt.fontSize  = 48;
            outcomeTxt.alignment = TextAnchor.MiddleCenter;
            outcomeTxt.color     = Color.white;
            var outcomeLabelRT   = outcomeLabelGO.GetComponent<RectTransform>();
            outcomeLabelRT.anchorMin = Vector2.zero; outcomeLabelRT.anchorMax = Vector2.one;
            outcomeLabelRT.offsetMin = outcomeLabelRT.offsetMax = Vector2.zero;

            // ── Wire SerializedFields ─────────────────────────────────────────────
            var so = new SerializedObject(mgr);
            so.FindProperty("_resolver").objectReferenceValue          = resolver;
            so.FindProperty("_hexTilePrefab").objectReferenceValue     = tilePrefab;
            so.FindProperty("_battleUnitPrefab").objectReferenceValue  = unitPrefab;
            so.FindProperty("_studentRoster").objectReferenceValue     = roster;
            so.FindProperty("_enemyDatabase").objectReferenceValue     = enemies;
            so.FindProperty("_benchPanel").objectReferenceValue        = benchRT;
            so.FindProperty("_startBattleButton").objectReferenceValue = btn;
            so.FindProperty("_outcomePanel").objectReferenceValue      = overlayGO;
            so.FindProperty("_outcomeText").objectReferenceValue       = outcomeTxt;
            so.ApplyModifiedProperties();

            // ── Save ──────────────────────────────────────────────────────────────
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();

            // Add to build settings
            var buildScenes = EditorBuildSettings.scenes;
            bool alreadyIn = false;
            foreach (var s in buildScenes) if (s.path == scenePath) { alreadyIn = true; break; }
            if (!alreadyIn)
            {
                System.Array.Resize(ref buildScenes, buildScenes.Length + 1);
                buildScenes[buildScenes.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
                EditorBuildSettings.scenes = buildScenes;
            }

            Debug.Log("[BattleSceneSetup] Battle.unity created. Open it and press Play to test.");
        }

        private static void EnsureInputHandlingBoth()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets == null || assets.Length == 0) return;
            var so   = new SerializedObject(assets[0]);
            var prop = so.FindProperty("activeInputHandler");
            if (prop == null || prop.intValue == 2) return;
            prop.intValue = 2; // 0=InputManager 1=InputSystem 2=Both
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("[BattleSceneSetup] Set Active Input Handling to 'Both'.");
        }
    }
}
