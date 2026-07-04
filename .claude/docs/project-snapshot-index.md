# Project Snapshot Index

Generated: 2026-07-04 (manually maintained — `GenerateProjectSnapshot.Execute()` referenced by rule-read-write-unity.md does not exist in this project; 2026-07-04 Phase 1 of ADR-001 uGUI→UI Toolkit migration — HeroInfoPanel.cs migrated to UIDocument (Assets/UI/HeroInfoPanel.uxml/.uss), shares Assets/UI/BattleHUDPanelSettings.asset with BattleHUD via new Assets/Scripts/UI/BattleUISortOrder.cs sort-order constants; scene GameObject reparented out of Canvas to scene root, old uGUI Image/CanvasRenderer removed; 2026-07-04 Phase 2 — TraitHUDController.cs migrated to UIDocument (Assets/UI/TraitHUD.uxml/.uss), same shared BattleHUDPanelSettings.asset pattern; 2026-07-04 Phase 3 — BattleBoardManager's Start Battle button + placement counter migrated to UIDocument (Assets/UI/BattleBoardHUD.uxml/.uss, shared BattleHUDPanelSettings.asset); redundant _outcomePanel/_outcomeText/HandleComplete() deleted (BattleHUD's overlay is sole outcome display); 2026-07-04 Phase 4 (final) — bench cards migrated to C# VisualElements (mirrors SchoolHUD's dynamic-card pattern) with new Assets/Scripts/Battle/BenchCardDragManipulator.cs (UI Toolkit PointerManipulator) replacing the old uGUI BenchCardDrag/IBeginDragHandler et al.; native UI Toolkit ScrollView replaces the hand-built ScrollRect/Viewport/RectMask2D (SetupBenchScrollView() deleted); 2026-07-04 Phase 5 (final) — empty Canvas GameObject removed from Battle.unity entirely; EventSystem kept as a defensive no-op (SceneLoader's fade canvas still carries a GraphicRaycaster component, avoiding a console warning even though nothing in Battle.unity needs raycasting anymore) — ADR-001 uGUI→UI Toolkit migration complete for HeroInfoPanel, TraitHUDController, and BattleBoardManager; AudioSystem added 2026-07-02; TrainingSystem added 2026-07-02; SchoolHUD added 2026-07-02; BattleHUD added 2026-07-02; AutoBattleResolver.OnCombatantsSet event + BattleHUD event-driven card build added 2026-07-03; BattleHUD's combatant HP-card/damage-number system removed 2026-07-04 — it now only owns the outcome overlay, Continue button, and speed-up indicator; in-battle visual feedback is BattleBoardManager's alone; 2026-07-04 fixed StudentId/ChampionId identity mismatch — ChampionId now flows through StudentCombatData/Combatant/CombatantSnapshot, BattleBoardManager._championDataLookup is keyed by placed-unit ID (fixes Hero Info Panel selection + TraitTracker placement registration), added BattleBoardManager.OnCardClicked, BattleUnit.HeroSelectionId, HeroInfoPanel miss-fallback, and a "X/Y heroes placed" counter (Canvas/PlacementCountText in Battle.unity, wired to BattleBoardManager._placementCountText); also fixed a pre-existing TraitTracker crash (VerticalToTraitType/HorizontalToTraitType + Breakpoints dict now cover all 15 traits, not just the original 4+4); 2026-07-04 fixed two more duplicate copies of the same incomplete trait-mapping — TraitHUDController.Thresholds and HeroInfoPanel.ToTraitType() now also cover all 15 traits (previously showed no X/Y count, or in HeroInfoPanel's case silently mislabeled Astral/Wild/Shadow/Oracle/Guardian/Tech/Void as "Vanguard"); 2026-07-04 trait display cleanup — TraitHUDController now hides zero-count trait labels (only shows traits with ≥1 placed unit); HeroInfoPanel's Traits line simplified to plain trait names with no count/breakpoint (removed its now-unused TraitTracker reference); 2026-07-04 added BattleTestHarness — Assets/Scenes/BattleTest.unity rebuilt from scratch (its old BattleTestController GameObject referenced a script deleted in commit 1f776d1; scene was fully derelict). New scene is a copy of Battle.unity's proven hierarchy plus a new BattleTestHarness GameObject: new BattleTestMatchup ScriptableObject (Assets/Scripts/Battle/BattleTestMatchup.cs) holds a reusable "Team A vs Team B" champion-ID preset; new BattleTestHarness.cs pre-seeds AutoBattleResolver.SetCombatants() in Awake() (before BattleBoardManager.Start()'s lazy-init fallback can fire) so a specific matchup's champions populate both the bench and the enemy side instead of the default all-30-champions-vs-stub-trio. No changes to ChampionRoster/AutoBattleResolver/BattleBoardManager. Two example assets at Assets/Config/Matchup_Example.asset and Matchup_Example2.asset.)

## Scripts

- `Assets/Scripts/Audio/AudioClipLibrary.cs`
- `Assets/Scripts/Audio/AudioSystem.cs`
- `Assets/Scripts/Battle/AutoBattleResolver.cs`
- `Assets/Scripts/Battle/BattleBoardManager.cs`
- `Assets/Scripts/Battle/BenchCardDragManipulator.cs`
- `Assets/Scripts/Battle/BattleData.cs`
- `Assets/Scripts/Battle/BattleTestHarness.cs`
- `Assets/Scripts/Battle/BattleTestMatchup.cs`
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
- `Assets/Scripts/UI/BattleUISortOrder.cs`
- `Assets/Scripts/UI/MainMenuController.cs`
- `Assets/Scripts/UI/SchoolHUD.cs`

## Config Assets

- `Assets/Config/AudioClipLibrary.asset`
- `Assets/Config/TrainingConfig.asset`
- `Assets/Config/Matchup_Example.asset`
- `Assets/Config/Matchup_Example2.asset`

## UI Toolkit Assets

- `Assets/UI/MainMenu.uxml`
- `Assets/UI/MainMenu.uss`
- `Assets/UI/MainMenuPanelSettings.asset`
- `Assets/UI/BattleHUD.uxml`
- `Assets/UI/BattleHUD.uss`
- `Assets/UI/BattleHUDPanelSettings.asset`
- `Assets/UI/HeroInfoPanel.uxml`
- `Assets/UI/HeroInfoPanel.uss`
- `Assets/UI/TraitHUD.uxml`
- `Assets/UI/TraitHUD.uss`
- `Assets/UI/BattleBoardHUD.uxml`
- `Assets/UI/BattleBoardHUD.uss`
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
