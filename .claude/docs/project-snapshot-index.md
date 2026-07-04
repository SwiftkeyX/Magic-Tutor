# Project Snapshot Index

Generated: 2026-07-04 (manually maintained — `GenerateProjectSnapshot.Execute()` referenced by rule-read-write-unity.md does not exist in this project; AudioSystem added 2026-07-02; TrainingSystem added 2026-07-02; SchoolHUD added 2026-07-02; BattleHUD added 2026-07-02; AutoBattleResolver.OnCombatantsSet event + BattleHUD event-driven card build added 2026-07-03; BattleHUD's combatant HP-card/damage-number system removed 2026-07-04 — it now only owns the outcome overlay, Continue button, and speed-up indicator; in-battle visual feedback is BattleBoardManager's alone; 2026-07-04 fixed StudentId/ChampionId identity mismatch — ChampionId now flows through StudentCombatData/Combatant/CombatantSnapshot, BattleBoardManager._championDataLookup is keyed by placed-unit ID (fixes Hero Info Panel selection + TraitTracker placement registration), added BattleBoardManager.OnCardClicked, BattleUnit.HeroSelectionId, HeroInfoPanel miss-fallback, and a "X/Y heroes placed" counter (Canvas/PlacementCountText in Battle.unity, wired to BattleBoardManager._placementCountText); also fixed a pre-existing TraitTracker crash (VerticalToTraitType/HorizontalToTraitType + Breakpoints dict now cover all 15 traits, not just the original 4+4); 2026-07-04 fixed two more duplicate copies of the same incomplete trait-mapping — TraitHUDController.Thresholds and HeroInfoPanel.ToTraitType() now also cover all 15 traits (previously showed no X/Y count, or in HeroInfoPanel's case silently mislabeled Astral/Wild/Shadow/Oracle/Guardian/Tech/Void as "Vanguard"); 2026-07-04 trait display cleanup — TraitHUDController now hides zero-count trait labels (only shows traits with ≥1 placed unit); HeroInfoPanel's Traits line simplified to plain trait names with no count/breakpoint (removed its now-unused TraitTracker reference))

## Scripts

- `Assets/Scripts/Audio/AudioClipLibrary.cs`
- `Assets/Scripts/Audio/AudioSystem.cs`
- `Assets/Scripts/Battle/AutoBattleResolver.cs`
- `Assets/Scripts/Battle/BattleBoardManager.cs`
- `Assets/Scripts/Battle/BattleData.cs`
- `Assets/Scripts/Battle/BattleUnit.cs`
- `Assets/Scripts/Battle/CameraController.cs`
- `Assets/Scripts/Battle/EnemyDatabaseStub.cs`
- `Assets/Scripts/Battle/HeroInfoPanel.cs`
- `Assets/Scripts/Battle/HeroSelection.cs`
- `Assets/Scripts/Battle/HexCoord.cs`
- `Assets/Scripts/Battle/HexGrid.cs`
- `Assets/Scripts/Battle/HexTileView.cs`
- `Assets/Scripts/Battle/Skills/SkillEnums.cs`
- `Assets/Scripts/Battle/Skills/SkillDefinition.cs`
- `Assets/Scripts/Battle/Skills/SkillTargetSelector.cs`
- `Assets/Scripts/Battle/Skills/SkillArchetypeExecutor.cs`
- `Assets/Scripts/Battle/TrainingConfig.cs`
- `Assets/Scripts/Battle/TrainingSystem.cs`
- `Assets/Scripts/Battle/StudentRosterStub.cs`
- `Assets/Scripts/Battle/Traits/ChampionData.cs`
- `Assets/Scripts/Battle/Traits/ChampionRoster.cs`
- `Assets/Scripts/Battle/Traits/TraitEffectApplier.cs`
- `Assets/Scripts/Battle/Traits/TraitHUDController.cs`
- `Assets/Scripts/Battle/Traits/TraitTracker.cs`
- `Assets/Scripts/Battle/Traits/TraitType.cs`
- `Assets/Scripts/Editor/BattleSceneSetup.cs`
- `Assets/Scripts/Editor/BattleTestSetup.cs`
- `Assets/Scripts/Editor/DisableCanvasGizmo.cs`
- `Assets/Scripts/UI/BattleHUD.cs`
- `Assets/Scripts/UI/MainMenuController.cs`
- `Assets/Scripts/UI/SchoolHUD.cs`

## Config Assets

- `Assets/Config/AudioClipLibrary.asset`
- `Assets/Config/TrainingConfig.asset`

## UI Toolkit Assets

- `Assets/UI/MainMenu.uxml`
- `Assets/UI/MainMenu.uss`
- `Assets/UI/MainMenuPanelSettings.asset`
- `Assets/UI/BattleHUD.uxml`
- `Assets/UI/BattleHUD.uss`
- `Assets/UI/BattleHUDPanelSettings.asset`
- `Assets/UI/SchoolHUD.uxml`
- `Assets/UI/SchoolHUD.uss`
- `Assets/UI/SchoolHUDPanelSettings.asset`

## Prefabs

- `Assets/Prefabs/Battle/BattleUnit.prefab`
- `Assets/Prefabs/Battle/HexTile.prefab`

## Scenes

- `Assets/Scenes/Battle.unity`
- `Assets/Scenes/BattleTest.unity`
- `Assets/Scenes/Bootstrap.unity`
- `Assets/Scenes/MainMenu.unity`
- `Assets/Scenes/SampleScene.unity`
- `Assets/Scenes/School.unity`
- `Assets/Scenes/YearEnd.unity`
