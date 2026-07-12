# Project Snapshot Index

Generated: 2026-07-04 (manually maintained — `GenerateProjectSnapshot.Execute()` referenced by rule-read-write-unity.md does not exist in this project; 2026-07-04 Phase 1 of ADR-001 uGUI→UI Toolkit migration — HeroInfoPanel.cs migrated to UIDocument (Assets/UI/HeroInfoPanel.uxml/.uss), shares Assets/UI/BattleHUDPanelSettings.asset with BattleHUD via new Assets/Scripts/UI/BattleUISortOrder.cs sort-order constants; scene GameObject reparented out of Canvas to scene root, old uGUI Image/CanvasRenderer removed; 2026-07-04 Phase 2 — TraitHUDController.cs migrated to UIDocument (Assets/UI/TraitHUD.uxml/.uss), same shared BattleHUDPanelSettings.asset pattern; 2026-07-04 Phase 3 — BattleBoardManager's Start Battle button + placement counter migrated to UIDocument (Assets/UI/BattleBoardHUD.uxml/.uss, shared BattleHUDPanelSettings.asset); redundant _outcomePanel/_outcomeText/HandleComplete() deleted (BattleHUD's overlay is sole outcome display); 2026-07-04 Phase 4 (final) — bench cards migrated to C# VisualElements (mirrors SchoolHUD's dynamic-card pattern) with new Assets/Scripts/Battle/BenchCardDragManipulator.cs (UI Toolkit PointerManipulator) replacing the old uGUI BenchCardDrag/IBeginDragHandler et al.; native UI Toolkit ScrollView replaces the hand-built ScrollRect/Viewport/RectMask2D (SetupBenchScrollView() deleted); 2026-07-04 Phase 5 (final) — empty Canvas GameObject removed from Battle.unity entirely; EventSystem kept as a defensive no-op (SceneLoader's fade canvas still carries a GraphicRaycaster component, avoiding a console warning even though nothing in Battle.unity needs raycasting anymore) — ADR-001 uGUI→UI Toolkit migration complete for HeroInfoPanel, TraitHUDController, and BattleBoardManager; AudioSystem added 2026-07-02; TrainingSystem added 2026-07-02; SchoolHUD added 2026-07-02; BattleHUD added 2026-07-02; AutoBattleResolver.OnCombatantsSet event + BattleHUD event-driven card build added 2026-07-03; BattleHUD's combatant HP-card/damage-number system removed 2026-07-04 — it now only owns the outcome overlay, Continue button, and speed-up indicator; in-battle visual feedback is BattleBoardManager's alone; 2026-07-04 fixed StudentId/ChampionId identity mismatch — ChampionId now flows through StudentCombatData/Combatant/CombatantSnapshot, BattleBoardManager._championDataLookup is keyed by placed-unit ID (fixes Hero Info Panel selection + TraitTracker placement registration), added BattleBoardManager.OnCardClicked, BattleUnit.HeroSelectionId, HeroInfoPanel miss-fallback, and a "X/Y heroes placed" counter (Canvas/PlacementCountText in Battle.unity, wired to BattleBoardManager._placementCountText); also fixed a pre-existing TraitTracker crash (VerticalToTraitType/HorizontalToTraitType + Breakpoints dict now cover all 15 traits, not just the original 4+4); 2026-07-04 fixed two more duplicate copies of the same incomplete trait-mapping — TraitHUDController.Thresholds and HeroInfoPanel.ToTraitType() now also cover all 15 traits (previously showed no X/Y count, or in HeroInfoPanel's case silently mislabeled Astral/Wild/Shadow/Oracle/Guardian/Tech/Void as "Vanguard"); 2026-07-04 trait display cleanup — TraitHUDController now hides zero-count trait labels (only shows traits with ≥1 placed unit); HeroInfoPanel's Traits line simplified to plain trait names with no count/breakpoint (removed its now-unused TraitTracker reference); 2026-07-04 added BattleTestHarness — Assets/Scenes/BattleTest.unity rebuilt from scratch (its old BattleTestController GameObject referenced a script deleted in commit 1f776d1; scene was fully derelict). New scene is a copy of Battle.unity's proven hierarchy plus a new BattleTestHarness GameObject: new BattleTestMatchup ScriptableObject (Assets/Scripts/Battle/BattleTestMatchup.cs) holds a reusable "Team A vs Team B" champion-ID preset; new BattleTestHarness.cs pre-seeds AutoBattleResolver.SetCombatants() in Awake() (before BattleBoardManager.Start()'s lazy-init fallback can fire) so a specific matchup's champions populate both the bench and the enemy side instead of the default all-30-champions-vs-stub-trio. No changes to ChampionRoster/AutoBattleResolver/BattleBoardManager. Two example assets at Assets/Config/Matchup_Example.asset and Matchup_Example2.asset. 2026-07-11 M4 architecture-review migration — ChampionRoster.cs converted from an in-scene MonoBehaviour with a hardcoded static 30-champion list to a ScriptableObject (Assets/Config/ChampionRoster.asset, [CreateAssetMenu(menuName = "MagicSchool/ChampionRoster")]); AutoBattleResolver, BattleBoardManager, BattleTestHarness, HeroInfoPanel (all in both BattleTest.unity and Battle.unity), Bootstrap.unity's GameManager, and Matchup_Example/Matchup_Example2 assets all now hold a direct [SerializeField] reference to the shared asset instead of an in-scene component or static call; verified via play-test in BattleTest.unity (BattleTestHarness pre-seeded 5 vs 3 combatants correctly from the new asset); 2026-07-11 AutoBattleResolver decomposition step 1 — new Assets/Scripts/Battle/CombatMath.cs (static ApplyMitigation) consolidates the damage-mitigation formula previously duplicated in AutoBattleResolver.Attack(), AutoBattleResolver.ResolveCastPlaceholder(), and Skills/SkillArchetypeExecutor.ComputeSkillDamage() — pure structural extraction, no behavior change; verified via play-test in BattleTest.unity plus an in-editor script cross-checking ApplyMitigation against the GDD's documented expected outputs; 2026-07-11 AutoBattleResolver decomposition steps 2-7 completed — Attack()'s mana/cast-trigger tail extracted to GrantAttackMana/CheckCastTriggers; GetCombatantSnapshots' dev fallback moved to new public EnsureCombatantsInitialized() (called explicitly by BattleBoardManager.Start(), since investigation showed this fallback also backs standalone no-RunManager play, not just tests); BattleLoop's 9 inline phases extracted to named methods (TickStatusCounters/TickDreadZones/ApplyBleedTicks/TickDreadknightShield/TickTricksterDashTrigger/TickTricksterUntargetable/TickKineticMana/ResolveKineticBonusActions/TickCastChannels); Dreadknight-shield/Trickster-untargetable/Kinetic-mana-tick/Striker-speed-bonus logic delegated to Traits/TraitAbilityExecutor.cs (new internal accessors added to AutoBattleResolver: KineticEnabled/KineticManaPerInterval/KineticSupportExtraBonus/KineticTickCounter/PendingBonusActions, SetMana widened to internal, DreadknightShieldHpThresholdPct widened to internal); Combatant's 6-trait flat field bag split into 5 per-trait structs (DreadknightState/StrikerState/TricksterState/ElementalistState/RangerState — no separate KineticState needed, Kinetic's tick state lives entirely on AutoBattleResolver and Combatant.Mana is a universal shared resource not Kinetic-exclusive; Bleed fields left flat since bleed lives on the victim, trait-agnostic); UNITY_EDITOR debug hooks (DebugSetAllPlayerHp/DebugForceCast) split into new AutoBattleResolver.Debug.cs partial-class file. AutoBattleResolver.cs is ~908 lines after all 7 steps (net line count did not shrink — extracting to named methods/files adds signatures and spacing even though logic moved out; the win is responsibility separation, not raw line count: BattleLoop's control flow, damage math, mana/cast triggers, and 4 trait mechanics are now each in their own named method or file instead of inlined together). Every step verified via check_compile_errors plus an end-to-end battle run in BattleTest.unity (direct AutoBattleResolver.BeginBattle() call via execute_script, bypassing the placement UI) — identical "PLAYERS WIN in 201 ticks" outcome confirmed after steps 4, 5, and 6; 2026-07-11 AutoBattleResolver file split (user requested <=300 lines) — the nested Combatant class moved to a new top-level Assets/Scripts/Battle/Combatant.cs (Skills/SkillArchetypeExecutor.cs and Traits/TraitAbilityExecutor.cs had their "using Combatant = AutoBattleResolver.Combatant;" alias deleted; Skills/SkillTargetSelector.cs had its ~9 fully-qualified "AutoBattleResolver.Combatant" references shortened to "Combatant"); remaining logic split across 4 new partial-class files following the AutoBattleResolver.Debug.cs precedent — AutoBattleResolver.CombatHelpers.cs (targeting, damage/mana/state mutation choke points, Skills/Traits internal accessors), AutoBattleResolver.Phases.cs (the 9 tick-phase methods), AutoBattleResolver.Attack.cs (Attack/GrantAttackMana/CheckCastTriggers/TriggerCast/ResolveCastPlaceholder/ApplyDamage/HandleKill/MoveTowardNearest), AutoBattleResolver.Setup.cs (SetCombatants/SetUnitPositions/GetAutoEnemyPlacements/EnsureCombatantsInitialized/GetCombatantSnapshots/GetCurrentHP-MaxHP-CurrentMana-MaxMana/ApplyPreBattleTraitModifiers) — AutoBattleResolver.cs itself now holds only events/constants/fields/Awake/BeginBattle/BattleLoop at 221 lines (down from 908, target was <=300). Pure file-reorganization, zero behavior change — verified via check_compile_errors plus 5 consecutive end-to-end battle runs in BattleTest.unity (one per file moved), all producing the identical "PLAYERS WIN in 201 ticks" outcome; 2026-07-11 Battle folder reorganization (user requested — root had 33 loose files) — moved into 7 new subfolders by architectural role via AssetDatabase.MoveAsset (GUIDs preserved, zero scene/prefab reference breakage): Resolver/ (the AutoBattleResolver partial-class family + Combatant.cs + CombatMath.cs), Managers/ (RunManager, BattleBoardManager, StudentRoster, TrainingSystem, PromotionSystem, HexGrid, BattlePlacementController), Config/ (RunConfig, StudentConfig, TrainingConfig, PromotionConfig, EnemyDatabase), Views/ (BattleUnit, HexTileView, CameraController, HexSpriteGenerator), UI/ (HeroInfoPanel, BenchCardDragManipulator), Data/ (BattleData, HexCoord, HeroSelection), Testing/ (BattleTestHarness, BattleTestMatchup, StudentRosterStub, EnemyDatabaseStub). Skills/ and Traits/ untouched. Verified via check_compile_errors plus the same BattleTest.unity end-to-end battle run — identical "PLAYERS WIN in 201 ticks" outcome, confirming all component references survived the move.)

## Scripts

- `Assets/Scripts/Audio/AudioClipLibrary.cs`
- `Assets/Scripts/Audio/AudioSystem.cs`
- `Assets/Scripts/Battle/Resolver/AutoBattleResolver.cs`
- `Assets/Scripts/Battle/Resolver/AutoBattleResolver.Attack.cs`
- `Assets/Scripts/Battle/Resolver/AutoBattleResolver.CombatHelpers.cs`
- `Assets/Scripts/Battle/Resolver/AutoBattleResolver.Debug.cs`
- `Assets/Scripts/Battle/Resolver/AutoBattleResolver.Phases.cs`
- `Assets/Scripts/Battle/Resolver/AutoBattleResolver.Setup.cs`
- `Assets/Scripts/Battle/Resolver/Combatant.cs`
- `Assets/Scripts/Battle/Resolver/CombatMath.cs`
- `Assets/Scripts/Battle/Managers/BattleBoardManager.cs`
- `Assets/Scripts/Battle/Managers/HexGrid.cs`
- `Assets/Scripts/Battle/Managers/TrainingSystem.cs`
- `Assets/Scripts/Battle/Config/TrainingConfig.cs`
- `Assets/Scripts/Battle/UI/BenchCardDragManipulator.cs`
- `Assets/Scripts/Battle/UI/HeroInfoPanel.cs`
- `Assets/Scripts/Battle/Data/BattleData.cs`
- `Assets/Scripts/Battle/Data/HeroSelection.cs`
- `Assets/Scripts/Battle/Data/HexCoord.cs`
- `Assets/Scripts/Battle/Testing/BattleTestHarness.cs`
- `Assets/Scripts/Battle/Testing/BattleTestMatchup.cs`
- `Assets/Scripts/Battle/Testing/EnemyDatabaseStub.cs`
- `Assets/Scripts/Battle/Testing/StudentRosterStub.cs`
- `Assets/Scripts/Battle/Views/BattleUnit.cs`
- `Assets/Scripts/Battle/Views/CameraController.cs`
- `Assets/Scripts/Battle/Views/HexTileView.cs`
- `Assets/Scripts/Battle/Skills/SkillEnums.cs`
- `Assets/Scripts/Battle/Skills/SkillDefinition.cs`
- `Assets/Scripts/Battle/Skills/SkillTargetSelector.cs`
- `Assets/Scripts/Battle/Skills/SkillArchetypeExecutor.cs`
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
- `Assets/Config/ChampionRoster.asset`

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
