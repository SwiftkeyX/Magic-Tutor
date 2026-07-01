# Known Issues

Bug tracker for issues logged via `/log-bug`, worked via `/debug` → `/fix-bug` or `/fix-all-bugs`. Fixed issues move to `known-issues-archive.md` with a resolution note.

## Open

### #1 — [Tooling] save_scene MCP tool writes to Assets/<name>.unity instead of the scene's real path, and re-points the open scene's saved location
- **Environment:** Unity 6000.4, Windows, `Assets/Scenes/Battle.unity`, Unity Editor (Edit Mode, via coplay MCP tool calls)
- **Description:** Any scene living outside the `Assets/` root gets silently duplicated/redirected on save, causing real work to land in the wrong file and (if unnoticed) look lost.
- **Steps to Reproduce:**
  1. Open a scene that lives in a subfolder, e.g. `Assets/Scenes/Battle.unity`, and confirm via `get_unity_editor_state` that `activeAssetPath` is `Assets/Scenes/Battle.unity`.
  2. Make an edit (e.g. add a GameObject).
  3. Call the `save_scene` MCP tool with `scene_name: "Battle"` (bare name, no path).
  4. Inspect disk: `Assets/Battle.unity` now exists/updates with the edit; `Assets/Scenes/Battle.unity` is unchanged.
  5. Call `execute_script` running `EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene())` (no explicit path) — it also writes to `Assets/Battle.unity`, confirming the open scene's tracked path was redirected, not just that one call.
- **Expected:** `save_scene` (and any path-less save) writes back to the scene's actual current path, `Assets/Scenes/Battle.unity`.
- **Actual:** Writes to `Assets/Battle.unity` at the Assets root and appears to change the in-memory scene's own tracked save path for the rest of the session, until the scene is reloaded via `open_scene` with the correct relative path.
- **Evidence:** File diff showed `Assets/Battle.unity` as an exact superset of `Assets/Scenes/Battle.unity` plus the new GameObject; mtimes confirmed which file was actually written on each save call; no console errors were logged — it fails silently. This is the confirmed root cause of the duplicate-scene issue previously patched twice (commits `b1ef742`, `41d9e81`).
- **Area:** Tooling (coplay MCP `save_scene`)
- **Severity:** Major

## Fixed

*(none yet)*

## Won't Fix

*(none yet)*
