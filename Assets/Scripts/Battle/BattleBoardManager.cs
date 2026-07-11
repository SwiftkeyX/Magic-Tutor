using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicSchool.Battle
{
    public class BattleBoardManager : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────
        [SerializeField] AutoBattleResolver   _resolver;
        [SerializeField] GameObject           _hexTilePrefab;
        [SerializeField] GameObject           _battleUnitPrefab;
        [SerializeField] private UnityEngine.UIElements.UIDocument _uiDocument;
        [SerializeField] private TraitTracker   _traitTracker;
        [SerializeField] private ChampionRoster _championRoster;

        private UnityEngine.UIElements.VisualElement _root;
        private UnityEngine.UIElements.Button        _startBattleButton;
        private UnityEngine.UIElements.Label         _placementCountText;
        private ScrollView _benchScrollView;
        private UnityEngine.UIElements.VisualElement _benchContainer;
        private readonly Dictionary<string, UnityEngine.UIElements.VisualElement> _benchCardsById = new();

        // ── Hex constants ────────────────────────────────────────────────────
        private const float HexWidth  = 1.1f;
        private const float HexHeight = 0.95f;
        private const float HexOffset = 0.55f;

        // ── State ────────────────────────────────────────────────────────────
        private readonly Dictionary<HexCoord, HexTileView>      _tiles             = new();
        private readonly Dictionary<string,   BattleUnit>        _units             = new();
        private readonly Dictionary<string,   HexCoord>          _pendingPlacements = new();
        private readonly Dictionary<string,   CombatantSnapshot> _studentSnapshots  = new();

        private HexGrid _grid;
        private bool    _battleStarted;
        private int     _maxSquadSize        = int.MaxValue; // unlimited in standalone; set by BeginPlacement() in production
        private bool    _placementPhaseActive;               // true once BeginPlacement() fires; prevents double-call
        private Dictionary<string, ChampionData> _championDataLookup = new Dictionary<string, ChampionData>();

        // ── Dragging state ───────────────────────────────────────────────────
        private string      _draggingStudentId;
        private GameObject  _dragGhost;
        private Camera      _cam;
        private HexTileView _hoveredTile;

        private Camera Cam
        {
            get
            {
                if (_cam == null) _cam = Camera.main;
                return _cam;
            }
        }

        // ── Lifecycle ────────────────────────────────────────────────────────
        private void Awake()
        {
            _grid = GetComponent<HexGrid>();
            if (_grid == null) { Debug.LogError("[BattleBoardManager] HexGrid missing", this); enabled = false; return; }
            // H2: Register with RunManager so it can reach us without FindObjectOfType.
            RunManager.Instance?.RegisterBattleBoardManager(this);
        }

        private void Start()
        {
            if (_resolver == null)    { Debug.LogError("[BattleBoardManager] AutoBattleResolver missing", this); enabled = false; return; }
            if (_uiDocument == null)  { Debug.LogError("[BattleBoardManager] UIDocument missing", this); enabled = false; return; }

            _uiDocument.sortingOrder = BattleUISortOrder.BoardBenchHUD;
            _root = _uiDocument.rootVisualElement;
            _startBattleButton  = _root.Q<UnityEngine.UIElements.Button>("start-battle-button");
            _placementCountText = _root.Q<UnityEngine.UIElements.Label>("placement-count-label");
            _benchScrollView = _root.Q<ScrollView>("bench-scroll");
            _benchContainer = _benchScrollView.contentContainer;

            if (_startBattleButton == null) { Debug.LogError("[BattleBoardManager] start-battle-button not found in UXML", this); enabled = false; return; }

            BuildBoard();

            _resolver.OnCombatantMoved    += HandleMoved;
            _resolver.OnCombatantActed    += HandleActed;
            _resolver.OnCombatantDefeated += HandleDefeated;
            _resolver.OnSkillCast         += HandleSkillCast;
            _resolver.OnManaChanged       += HandleManaChanged;
            _resolver.OnCastStateChanged  += HandleCastStateChanged;

            _startBattleButton.SetEnabled(false);
            _startBattleButton.clicked += OnStartBattle;

            if (RunManager.Instance == null)
            {
                // Standalone (BattleTest.unity, no RunManager): auto-start Placement Phase immediately.
                // Reads GetCombatantSnapshots() — lazy-init fallback in AutoBattleResolver supplies
                // ChampionRoster data in this context (per AutoBattleResolver.md). Uncapped squad size.
                _maxSquadSize = int.MaxValue;
                var snapshots = _resolver.GetCombatantSnapshots();
                var students  = snapshots.Where(s => s.IsStudent).ToList();
                foreach (var s in students)
                {
                    _studentSnapshots[s.Id] = s;
                    var champ = _championRoster != null ? _championRoster.GetChampionById(s.ChampionId) : null;
                    if (champ != null) _championDataLookup[s.Id] = champ;
                }
                BuildBench(students);
                UpdatePlacementCountText();
#if UNITY_EDITOR
                if (_debugAutoStart) TestAutoPlace();
#endif
            }
            // Production (Battle.unity with RunManager): Placement Phase begins when
            // RunManager calls BeginPlacement() after AutoBattleResolver.SetCombatants().
        }

        private void OnDestroy()
        {
            if (_resolver == null) return;
            _resolver.OnCombatantMoved    -= HandleMoved;
            _resolver.OnCombatantActed    -= HandleActed;
            _resolver.OnCombatantDefeated -= HandleDefeated;
            _resolver.OnSkillCast         -= HandleSkillCast;
            _resolver.OnManaChanged       -= HandleManaChanged;
            _resolver.OnCastStateChanged  -= HandleCastStateChanged;
        }

        /// <summary>
        /// Called by RunManager after AutoBattleResolver.SetCombatants() to begin the
        /// Placement Phase with real roster data. Only valid in the production context
        /// (Battle.unity with RunManager present). No-ops with an error if called twice.
        /// </summary>
        public void BeginPlacement(List<StudentCombatData> students, List<EnemyCombatData> enemies, int maxSquadSize)
        {
            if (_placementPhaseActive)
            {
                Debug.LogError("[BattleBoardManager] BeginPlacement called while Placement Phase is already active.");
                return;
            }
            _placementPhaseActive = true;
            _maxSquadSize = maxSquadSize;

            // RunManager already called AutoBattleResolver.SetCombatants() with the real roster.
            // Reading GetCombatantSnapshots() here is now guaranteed to return student data, not stale
            // ChampionRoster fallback data — that fallback is only reachable when _combatants is empty.
            var snapshots    = _resolver.GetCombatantSnapshots();
            var studentSnaps = snapshots.Where(s => s.IsStudent).ToList();
            foreach (var s in studentSnaps)
            {
                _studentSnapshots[s.Id] = s;
                var champ = _championRoster != null ? _championRoster.GetChampionById(s.ChampionId) : null;
                if (champ != null) _championDataLookup[s.Id] = champ;
            }

            BuildBench(studentSnaps);
            UpdatePlacementCountText();
            Debug.Log($"[BattleBoardManager] BeginPlacement: {studentSnaps.Count} students on bench, maxSquadSize={maxSquadSize}");
        }

        // ── Board construction ───────────────────────────────────────────────
        // Full visual height of a pointy-top hex whose centre-to-centre column width = HexWidth
        private static readonly float HexVisualHeight = HexWidth * 2f / Mathf.Sqrt(3f);

        private void BuildBoard()
        {
            var hexSprite = GetHexSprite();
            var scale     = new Vector3(HexWidth, HexVisualHeight, 1f);

            for (int row = 0; row < HexGrid.Rows; row++)
            for (int col = 0; col < HexGrid.Cols; col++)
            {
                var coord = new HexCoord(col, row);
                var go    = Instantiate(_hexTilePrefab, CoordToWorld(coord), Quaternion.identity, transform);
                go.transform.localScale = scale;
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = hexSprite;
                var view = go.GetComponent<HexTileView>();
                view.Init(coord);
                _tiles[coord] = view;
            }
        }

        // ── Hex sprite (pointy-top, 128px, dark border ring) ─────────────────
        private static Sprite _hexSprite;
        private static Sprite GetHexSprite()
        {
            if (_hexSprite != null) return _hexSprite;

            const int size   = 128;
            const int border = 5;

            float cx   = (size - 1) / 2f;
            float cy   = (size - 1) / 2f;
            float rOut = cx - 1f;
            float rIn  = rOut - border;

            var outer  = HexVerts(cx, cy, rOut);
            var inner  = HexVerts(cx, cy, rIn);
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2(x, y);
                if (PointInHex(p, outer))
                    pixels[y * size + x] = PointInHex(p, inner)
                        ? new Color32(255, 255, 255, 255)   // body (tinted by SpriteRenderer.color)
                        : new Color32(15,  15,  15,  220);  // border ring
                // else stays default Color32(0,0,0,0) — transparent
            }

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.SetPixels32(pixels);
            tex.Apply();

            _hexSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _hexSprite;
        }

        private static Vector2[] HexVerts(float cx, float cy, float r)
        {
            var v = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float rad = (90f - 60f * i) * Mathf.Deg2Rad; // pointy-top: first vertex at 12 o'clock
                v[i] = new Vector2(cx + r * Mathf.Cos(rad), cy + r * Mathf.Sin(rad));
            }
            return v;
        }

        private static bool PointInHex(Vector2 p, Vector2[] verts)
        {
            for (int i = 0; i < verts.Length; i++)
            {
                Vector2 a = verts[i];
                Vector2 b = verts[(i + 1) % verts.Length];
                // Vertices are CW, so interior points have cross < 0; exclude when > 0.
                if ((b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x) > 0)
                    return false;
            }
            return true;
        }

        // Kept for drag-ghost (card sprite fallback, not a hex)
        private static Sprite _fallbackSprite;
        private static Sprite GetFallbackSprite()
        {
            if (_fallbackSprite != null) return _fallbackSprite;
            var tex    = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            var pixels = new Color32[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            _fallbackSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
            return _fallbackSprite;
        }

        private void BuildBench(List<CombatantSnapshot> students)
        {
            _benchContainer.Clear();
            _benchCardsById.Clear();

            foreach (var s in students)
            {
                var card = new VisualElement();
                card.name = $"Card_{s.Id}";
                card.AddToClassList("bench-card");
                card.style.backgroundColor = new StyleColor(StudentColor(s.Id));

                var label = new Label(s.DisplayName);
                label.AddToClassList("bench-card-label");
                card.Add(label);

                card.AddManipulator(new BenchCardDragManipulator(this, s.Id));

                _benchContainer.Add(card);
                _benchCardsById[s.Id] = card;
            }
        }

        // ── Hero selection (called by BenchCardDrag on click) ────────────────
        public void OnCardClicked(string studentId)
        {
            var championId = _championDataLookup.TryGetValue(studentId, out var champ) ? champ.Id : studentId;
            HeroSelection.Select(championId);
        }

        // ── Dragging (called by BenchCardDragManipulator) ────────────────────
        public void OnCardDragStart(string studentId)
        {
            if (_battleStarted) return;
            _draggingStudentId = studentId;
            _hoveredTile = null;

            // Highlight valid player tiles — skip entirely when squad cap is full for a new student.
            // A student being re-placed (already in _pendingPlacements) is always allowed to move.
            bool isReplacing = _pendingPlacements.ContainsKey(studentId);
            bool squadFull   = !isReplacing && _pendingPlacements.Count >= _maxSquadSize;

            if (!squadFull)
            {
                foreach (var kv in _tiles)
                {
                    bool isOwnTile = _pendingPlacements.TryGetValue(studentId, out var own) && kv.Key == own;
                    if (kv.Key.Row < HexGrid.PlayerRowCount && (!_grid.IsOccupied(kv.Key) || isOwnTile))
                        kv.Value.SetHighlight(true);
                }
            }

            // Create a ghost — a world-space SpriteRenderer, independent of the bench
            // card's UI framework (the bench card has no sprite of its own to reuse).
            _dragGhost = new GameObject("DragGhost");
            _dragGhost.transform.SetParent(transform, false);
            var sr     = _dragGhost.AddComponent<SpriteRenderer>();
            sr.sprite       = GetFallbackSprite();
            sr.color        = new Color(StudentColor(studentId).r, StudentColor(studentId).g, StudentColor(studentId).b, 0.6f);
            sr.sortingOrder = 10;
            _dragGhost.transform.localScale = Vector3.one * 0.4f;
        }

        public void OnCardDrag(Vector2 screenPos)
        {
            if (_dragGhost == null) return;
            Vector3 world = Cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -Cam.transform.position.z));
            world.z = 0f;
            _dragGhost.transform.position = world;

            // Update hover highlight — brightest tile nearest the cursor
            bool isReplacing = _pendingPlacements.TryGetValue(_draggingStudentId, out var ownCoord);
            HexTileView nearestView = null;
            float minDist = float.MaxValue;
            foreach (var kv in _tiles)
            {
                if (kv.Key.Row >= HexGrid.PlayerRowCount) continue;
                bool ownTile = isReplacing && kv.Key == ownCoord;
                if (_grid.IsOccupied(kv.Key) && !ownTile) continue;
                float d = Vector3.Distance(CoordToWorld(kv.Key), world);
                if (d < HexWidth * 0.75f && d < minDist) { minDist = d; nearestView = kv.Value; }
            }

            if (nearestView != _hoveredTile)
            {
                _hoveredTile?.SetHover(false);
                _hoveredTile = nearestView;
                _hoveredTile?.SetHover(true);
            }
        }

        public void OnCardDragEnd(Vector2 screenPos)
        {
            // Clear hover then all highlights
            _hoveredTile?.SetHover(false);
            _hoveredTile = null;
            foreach (var kv in _tiles) kv.Value.SetHighlight(false);

            if (_dragGhost != null) { Destroy(_dragGhost); _dragGhost = null; }

            if (_draggingStudentId == null) return;

            // Hit-test world pos → nearest valid tile
            Vector3 worldPos = Cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -Cam.transform.position.z));
            worldPos.z = 0f;

            bool hasExisting = _pendingPlacements.TryGetValue(_draggingStudentId, out var existingCoord);
            HexCoord? closest = null;
            float     minDist = float.MaxValue;
            foreach (var kv in _tiles)
            {
                if (kv.Key.Row >= HexGrid.PlayerRowCount) continue;
                bool ownTile = hasExisting && kv.Key == existingCoord;
                if (_grid.IsOccupied(kv.Key) && !ownTile) continue;
                float d = Vector3.Distance(CoordToWorld(kv.Key), worldPos);
                if (d < minDist && d < HexWidth * 0.75f) { minDist = d; closest = kv.Key; }
            }

            if (closest.HasValue)
                PlaceStudent(_draggingStudentId, closest.Value);
            else if (_pendingPlacements.ContainsKey(_draggingStudentId))
                UnplaceStudent(_draggingStudentId);

            _draggingStudentId = null;
        }

        private void UnplaceStudent(string studentId)
        {
            if (_pendingPlacements.TryGetValue(studentId, out var coord))
            {
                _grid.ClearOccupant(coord);
                _pendingPlacements.Remove(studentId);
                _traitTracker?.UnregisterPlacement(studentId);
            }
            if (_units.TryGetValue(studentId, out var unit))
            {
                Destroy(unit.gameObject);
                _units.Remove(studentId);
            }
            if (_benchCardsById.TryGetValue(studentId, out var card))
                card.style.opacity = 1f;

            _startBattleButton.SetEnabled(_pendingPlacements.Count >= 1 && _pendingPlacements.Count <= _maxSquadSize);
            UpdatePlacementCountText();
        }

        private void UpdatePlacementCountText()
        {
            if (_placementCountText != null)
                _placementCountText.text = $"{_pendingPlacements.Count}/{_maxSquadSize} heroes placed";
        }

        private void PlaceStudent(string studentId, HexCoord coord)
        {
            if (!_studentSnapshots.TryGetValue(studentId, out var snap)) return;
            if (_grid.IsOccupied(coord)) return;

            // Squad cap: reject placement of a new (currently unplaced) student when cap is reached.
            // Re-placing an already-placed student (moving it to a different tile) is always allowed.
            bool isReplacing = _pendingPlacements.ContainsKey(studentId);
            if (!isReplacing && _pendingPlacements.Count >= _maxSquadSize) return;

            // Remove existing placement if re-placing — restore card alpha first
            if (_pendingPlacements.TryGetValue(studentId, out var oldCoord))
            {
                _grid.ClearOccupant(oldCoord);
                if (_units.TryGetValue(studentId, out var oldUnit))
                {
                    Destroy(oldUnit.gameObject);
                    _units.Remove(studentId);
                }
                if (_benchCardsById.TryGetValue(studentId, out var existingCard))
                    existingCard.style.opacity = 1f;
            }

            _pendingPlacements[studentId] = coord;
            _championDataLookup.TryGetValue(studentId, out var champData);
            if (_traitTracker != null && champData != null)
                _traitTracker.RegisterPlacement(studentId, coord, champData);
            _grid.SetOccupant(coord, studentId);

            // Spawn unit — MaxHP sourced from snapshot (B3)
            var go   = Instantiate(_battleUnitPrefab, CoordToWorld(coord), Quaternion.identity);
            var unit = go.GetComponent<BattleUnit>();
            unit.Init(studentId, coord, champData?.Id);
            unit.InitHealthBar(snap.MaxHP, snap.MaxHP);
            unit.InitManaBar(snap.Mana, snap.MaxMana);
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (sr.sprite == null) sr.sprite = GetFallbackSprite();
                sr.color = StudentColor(studentId);
            }
            _units[studentId] = unit;

            // Dim bench card to show it's placed (kept in the bench so it can be re-dragged)
            if (_benchCardsById.TryGetValue(studentId, out var card))
                card.style.opacity = 0.4f;

            _startBattleButton.SetEnabled(_pendingPlacements.Count >= 1 && _pendingPlacements.Count <= _maxSquadSize);
            UpdatePlacementCountText();
        }

        // ── Test helper (editor/QA only) ─────────────────────────────────────
#if UNITY_EDITOR
        [SerializeField] private bool  _debugAutoStart;
        [SerializeField] private float _debugPlayerStartHpPct = 1f;

        public void TestAutoPlace()
        {
            // Covers all 4 horizontal trait breakpoints at BP2:
            // Dreadknight BP2: bloodhound + phalanx
            // Kinetic BP2:     pyromancer + windrunner
            // Striker BP2:     bloodhound + shadowblade
            // Trickster BP2:   shadowblade + phantomassassin
            PlaceStudent("ironclad",        new HexCoord(0, 0));
            PlaceStudent("bloodhound",      new HexCoord(1, 0));
            PlaceStudent("pyromancer",      new HexCoord(2, 0));
            PlaceStudent("windrunner",      new HexCoord(3, 0));
            PlaceStudent("shadowblade",     new HexCoord(0, 1));
            PlaceStudent("phalanx",         new HexCoord(1, 1));
            PlaceStudent("phantomassassin", new HexCoord(2, 1));
            OnStartBattle();
        }
#endif

        // ── Start Battle ─────────────────────────────────────────────────────
        private void OnStartBattle()
        {
            if (_battleStarted || _pendingPlacements.Count == 0) return;
            _battleStarted = true;
            _startBattleButton.style.display = UnityEngine.UIElements.DisplayStyle.None;
            _benchScrollView.style.display = UnityEngine.UIElements.DisplayStyle.None;

            // Snapshot enemy data BEFORE RunManager may re-call SetCombatants with the filtered squad.
            // Both paths spawn enemy GOs here (visual layer — BattleBoardManager's job).
            var enemyPlacements = _resolver.GetAutoEnemyPlacements();
            var enemySnapshots  = _resolver.GetCombatantSnapshots().Where(s => !s.IsStudent).ToList();
            foreach (var e in enemySnapshots)
            {
                if (!enemyPlacements.TryGetValue(e.Id, out var coord)) continue;
                var go   = Instantiate(_battleUnitPrefab, CoordToWorld(coord), Quaternion.identity);
                var unit = go.GetComponent<BattleUnit>();
                unit.Init(e.Id, coord, e.Id);
                unit.InitHealthBar(e.MaxHP, e.MaxHP);
                unit.InitManaBar(e.Mana, e.MaxMana);
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (sr.sprite == null) sr.sprite = GetFallbackSprite();
                    sr.color = EnemyColor(e.Id);
                }
                _units[e.Id] = unit;
            }

            if (RunManager.Instance != null)
            {
                // Production: hand off fielded IDs + positions to RunManager, which filters combatants
                // to only the placed subset, applies trait modifiers via ApplyTraitModifiers(), and
                // calls resolver.BeginBattle(). Trait application must happen AFTER RunManager's final
                // SetCombatants() call — see GDD Core Rule 4 and RunManager.ConfirmSquadPlacement().
                var fieldedStudentIds = _pendingPlacements.Keys.ToList();
                RunManager.Instance.ConfirmSquadPlacement(fieldedStudentIds, _pendingPlacements);
            }
            else
            {
                // Standalone (BattleTest.unity): call resolver directly.
                // Trait application happens here because there is no RunManager to own the sequencing.
                _resolver.SetUnitPositions(_pendingPlacements);
                ApplyTraitModifiers(_resolver, _pendingPlacements);
#if UNITY_EDITOR
                if (_debugPlayerStartHpPct < 1f) _resolver.DebugSetAllPlayerHp(_debugPlayerStartHpPct);
#endif
                _resolver.BeginBattle();
            }
        }

        /// <summary>
        /// Applies trait-derived stat modifiers to the resolver's current combatant list.
        /// Must be called after SetCombatants() and SetUnitPositions(), before BeginBattle().
        /// On the RunManager path this is called by RunManager.ConfirmSquadPlacement() after
        /// its final SetCombatants() — see GDD Core Rule 4 (TraitSystem before AutoBattleResolver).
        /// On the standalone path it is called directly from OnStartBattle()'s else branch.
        /// </summary>
        public void ApplyTraitModifiers(AutoBattleResolver resolver, Dictionary<string, HexCoord> placements)
        {
            if (_traitTracker == null || _championRoster == null) return;
            TraitEffectApplier.Apply(
                _traitTracker.GetActiveBreakpoints(),
                _championDataLookup,
                placements,
                resolver);
        }

        // ── Event handlers ───────────────────────────────────────────────────
        private void HandleMoved(string id, HexCoord from, HexCoord to)
        {
            if (!_units.TryGetValue(id, out var unit)) return;
            unit.MoveTo(CoordToWorld(to), to);
        }

        private void HandleActed(string actorId, string targetId, int damage, System.Collections.Generic.List<string> flags)
        {
            if (_units.TryGetValue(actorId, out var actor) && _units.TryGetValue(targetId, out var target))
            {
                actor.PlayAttackAnim(target.transform.position);
                Debug.DrawLine(actor.transform.position, target.transform.position, Color.red, 0.5f);
                int cur = _resolver.GetCurrentHP(targetId);
                int max = _resolver.GetMaxHP(targetId);
                target.UpdateHP(cur, max);
            }
        }

        private void HandleDefeated(string id)
        {
            if (!_units.TryGetValue(id, out var unit)) return;
            unit.PlayDeathAnim();
            _units.Remove(id);
        }

        // GDD AC: "Visual log output or floating text notifies the board when a
        // skill is cast." Satisfied via the log branch — no floating-text widget
        // exists yet (that's BattleHUD's stated home per ActiveSkillSystem.md).
        private void HandleSkillCast(string casterId, string skillName)
        {
            string displayName = _studentSnapshots.TryGetValue(casterId, out var snap) ? snap.DisplayName : casterId;
            Debug.Log($"[Skill] {displayName} casts {skillName}!");

            if (_units.TryGetValue(casterId, out var unit))
                unit.PlayCastText(skillName);
        }

        private void HandleManaChanged(string id, int current, int max)
        {
            if (_units.TryGetValue(id, out var unit))
                unit.UpdateMana(current, max);
        }

        private void HandleCastStateChanged(string id, CastState state)
        {
            if (_units.TryGetValue(id, out var unit))
                unit.SetCastingVisual(state == CastState.Casting || state == CastState.Channeling);
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        private Vector3 CoordToWorld(HexCoord c) =>
            new Vector3(c.Col * HexWidth + (c.Row % 2 == 1 ? HexOffset : 0f),
                        c.Row * HexHeight, 0f);

        private static Color StudentColor(string id) => id switch
        {
            "warrior"         => new Color(0.2f,  0.5f,  1.0f),
            "mage"            => new Color(0.7f,  0.2f,  1.0f),
            "archer"          => new Color(0.2f,  0.8f,  0.3f),
            "ironclad"        => new Color(0.35f, 0.45f, 0.55f),
            "bloodhound"      => new Color(0.65f, 0.10f, 0.10f),
            "pyromancer"      => new Color(0.85f, 0.35f, 0.05f),
            "windrunner"      => new Color(0.10f, 0.65f, 0.65f),
            "grovekeeper"     => new Color(0.15f, 0.50f, 0.20f),
            "shadowblade"     => new Color(0.40f, 0.05f, 0.55f),
            "phalanx"         => new Color(0.10f, 0.20f, 0.60f),
            "stormbringer"    => new Color(0.25f, 0.35f, 0.75f),
            "phantomassassin" => new Color(0.60f, 0.05f, 0.60f),
            "dreadoverlord"   => new Color(0.45f, 0.00f, 0.10f),
            _                 => new Color(0.3f,  0.3f,  0.3f),
        };

        private static Color EnemyColor(string id) => id switch
        {
            "brute"  => new Color(1.0f, 0.2f, 0.2f),
            "witch"  => new Color(1.0f, 0.5f, 0.1f),
            "sniper" => new Color(1.0f, 0.9f, 0.1f),
            _        => Color.gray,
        };
    }
}
