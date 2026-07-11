# ADR-001: UI Toolkit as the Standard UI Framework

**Status:** Accepted

## Context

The project had no recorded UI-framework standard. `technical-preferences.md` listed UI Toolkit (`.uxml`/`.uss`) and "Canvas prefabs" under the same `ui-programmer` routing line with no distinction between them, and its Architecture Decisions Log was empty. In practice this produced silent drift between two systems:

- **UI Toolkit** (`UIDocument` + `.uxml`/`.uss`): `MainMenuController.cs`, `SchoolHUD.cs`, `BattleHUD.cs`
- **Legacy uGUI** (`UnityEngine.UI`, built at runtime via `AddComponent<Text>()` with `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`): `SceneLoader.cs` (fade overlay), `TraitHUDController.cs`, `HeroInfoPanel.cs`, `BattleBoardManager.cs`

uGUI's actual differentiator from UI Toolkit is world-space/Transform-anchored UI (billboarded panels, floating health bars that follow a GameObject in 3D space). None of the uGUI usage above needs that: `HeroInfoPanel` is a fixed "right-docked hero inspector" and `TraitHUDController` is a fixed screen-space trait-count list — both functionally identical in role to BattleHUD's UI Toolkit panels. The split was not a deliberate design choice; the uGUI code predates the project's move to UI Toolkit and was never migrated.

## Decision

UI Toolkit is the standard for all screen-space HUD, menu, and panel UI in this project. uGUI (`Canvas` / `UnityEngine.UI`) is permitted only for genuine world-space/Transform-anchored UI — a case that does not currently exist anywhere in this project.

**Named exception:** `SceneLoader.cs`'s fade `CanvasGroup` stays on uGUI. It is a single full-screen alpha overlay, not a widget, and migrating it buys nothing.

## Consequences

- `HeroInfoPanel.cs`, `TraitHUDController.cs`, and `BattleBoardManager.cs` are tech debt. Migrate each to UI Toolkit the next time it's touched for other reasons — this ADR does not migrate them.
- `technical-preferences.md`'s File Extension Routing table no longer treats `.uxml`/`.uss` and "Canvas prefabs" as parallel options — UI Toolkit is the default, Canvas is legacy/exception-only.
- Any new screen-space UI work should start from `UIDocument` + `.uxml`/`.uss`, following `BattleHUD.cs`/`SchoolHUD.cs`/`MainMenuController.cs` as the reference pattern.
