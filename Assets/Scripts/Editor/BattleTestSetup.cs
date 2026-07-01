using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

namespace MagicSchool.Editor
{
    /// One-shot editor setup script for battle prefabs (HexTile, BattleUnit).
    /// Run via: Execute BattleTestSetup.Execute()
    public class BattleTestSetup
    {
        public static void Execute()
        {
            // ── 1. Create or open BattleTest scene ──────────────────────────
            const string scenePath = "Assets/Scenes/BattleTest.unity";
            Directory.CreateDirectory("Assets/Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, scenePath);

            // ── 2. Camera setup ──────────────────────────────────────────────
            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGO.transform.position = new Vector3(3.3f, 3.3f, -10f);
            camGO.tag = "MainCamera";

            // ── 3. HexTile prefab ────────────────────────────────────────────
            Directory.CreateDirectory("Assets/Prefabs/Battle");

            // Create a regular hexagon mesh sprite via a 2D quad + custom material
            // For simplicity, use a Unity circle sprite scaled to look hex-like.
            // A proper hexagon can be set up using a custom sprite or polygon collider.
            var hexTileGO = new GameObject("HexTile");
            var tileSr = hexTileGO.AddComponent<SpriteRenderer>();
            tileSr.sprite = CreateHexSprite();
            tileSr.color = new Color(0.25f, 0.45f, 0.75f, 0.85f);

            // Add collider for raycast-based interactions
            var hexCollider = hexTileGO.AddComponent<PolygonCollider2D>();
            SetHexColliderPoints(hexCollider, 0.5f);

            // HexTileView component
            hexTileGO.AddComponent<MagicSchool.Battle.HexTileView>();

            string tilePrefabPath = "Assets/Prefabs/Battle/HexTile.prefab";
            PrefabUtility.SaveAsPrefabAsset(hexTileGO, tilePrefabPath);
            Object.DestroyImmediate(hexTileGO);

            // ── 4. BattleUnit prefab ─────────────────────────────────────────
            var unitGO = new GameObject("BattleUnit");
            var unitSr = unitGO.AddComponent<SpriteRenderer>();
            unitSr.sprite = CreateCircleSprite();
            unitSr.color = Color.white;
            unitSr.sortingOrder = 1;   // render on top of tiles

            unitGO.AddComponent<MagicSchool.Battle.BattleUnit>();

            string unitPrefabPath = "Assets/Prefabs/Battle/BattleUnit.prefab";
            PrefabUtility.SaveAsPrefabAsset(unitGO, unitPrefabPath);
            Object.DestroyImmediate(unitGO);

            // ── 5. Save scene ────────────────────────────────────────────────
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();

            // Add to build settings
            var buildScenes = EditorBuildSettings.scenes;
            System.Array.Resize(ref buildScenes, buildScenes.Length + 1);
            buildScenes[buildScenes.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = buildScenes;

            Debug.Log("[BattleTestSetup] Scene created at: " + scenePath);
            Debug.Log("[BattleTestSetup] HexTile prefab:    " + tilePrefabPath);
            Debug.Log("[BattleTestSetup] BattleUnit prefab: " + unitPrefabPath);
        }

        private static Sprite CreateHexSprite()
        {
            // Use Unity's built-in circle sprite as a placeholder for hex tiles.
            // Replace with a proper hexagon sprite asset for production.
            return Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd")
                   ?? CreateFallbackSprite(new Color(1, 1, 1, 1));
        }

        private static Sprite CreateCircleSprite()
        {
            return Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd")
                   ?? CreateFallbackSprite(Color.white);
        }

        private static Sprite CreateFallbackSprite(Color color)
        {
            // 32×32 single-color texture as a last resort
            var tex = new Texture2D(32, 32);
            var pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        }

        private static void SetHexColliderPoints(PolygonCollider2D col, float radius)
        {
            var points = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.PI / 2f + i * Mathf.PI / 3f;
                points[i] = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            }
            col.SetPath(0, points);
        }
    }
}
