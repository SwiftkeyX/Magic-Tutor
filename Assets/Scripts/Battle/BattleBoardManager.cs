using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MagicSchool.Battle
{
    public class BattleBoardManager : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────
        [SerializeField] AutoBattleResolver   _resolver;
        [SerializeField] GameObject           _hexTilePrefab;
        [SerializeField] GameObject           _battleUnitPrefab;
        [SerializeField] RectTransform        _benchPanel;
        [SerializeField] Button               _startBattleButton;
        [SerializeField] GameObject           _outcomePanel;
        [SerializeField] Text                 _outcomeText;
        [SerializeField] private TraitTracker   _traitTracker;
        [SerializeField] private ChampionRoster _championRoster;

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
        private Dictionary<string, ChampionData> _championDataLookup = new Dictionary<string, ChampionData>();

        // ── Dragging state ───────────────────────────────────────────────────
        private string      _draggingStudentId;
        private GameObject  _dragGhost;
        private Camera      _cam;
        private HexTileView _hoveredTile;

        // ── Lifecycle ────────────────────────────────────────────────────────
        private void Awake()
        {
            _cam  = Camera.main;
            _grid = GetComponent<HexGrid>();
            if (_grid == null) { Debug.LogError("[BattleBoardManager] HexGrid missing", this); enabled = false; return; }
        }

        private void Start()
        {
            if (_resolver == null)          { Debug.LogError("[BattleBoardManager] AutoBattleResolver missing", this); enabled = false; return; }
            if (_startBattleButton == null) { Debug.LogError("[BattleBoardManager] StartBattleButton missing", this); enabled = false; return; }
            if (_outcomePanel == null)      { Debug.LogError("[BattleBoardManager] OutcomePanel missing", this); enabled = false; return; }

            BuildBoard();

            if (_championRoster != null)
                _championDataLookup = _championRoster.GetChampionLookup();

            var snapshots = _resolver.GetCombatantSnapshots();
            var students  = snapshots.Where(s => s.IsStudent).ToList();

            foreach (var s in students)
                _studentSnapshots[s.Id] = s;

            _resolver.OnCombatantMoved    += HandleMoved;
            _resolver.OnCombatantActed    += HandleActed;
            _resolver.OnCombatantDefeated += HandleDefeated;
            _resolver.OnBattleComplete    += HandleComplete;

            BuildBench(students);

            _startBattleButton.interactable = false;
            _startBattleButton.onClick.AddListener(OnStartBattle);
            _outcomePanel.SetActive(false);

#if UNITY_EDITOR
            if (_debugAutoStart) TestAutoPlace();
#endif
        }

        private void OnDestroy()
        {
            if (_resolver == null) return;
            _resolver.OnCombatantMoved    -= HandleMoved;
            _resolver.OnCombatantActed    -= HandleActed;
            _resolver.OnCombatantDefeated -= HandleDefeated;
            _resolver.OnBattleComplete    -= HandleComplete;
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
            foreach (var s in students)
            {
                var card = new GameObject($"Card_{s.Id}");
                card.transform.SetParent(_benchPanel, false);

                var img   = card.AddComponent<Image>();
                img.color = StudentColor(s.Id);
                var rt    = card.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(80, 100);

                var label    = new GameObject("Label");
                label.transform.SetParent(card.transform, false);
                var txt      = label.AddComponent<Text>();
                txt.text     = s.DisplayName;
                txt.font     = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.fontSize = 14;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color    = Color.white;
                var lrt      = label.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
                lrt.offsetMin = lrt.offsetMax = Vector2.zero;

                // Wire drag events
                var drag = card.AddComponent<BenchCardDrag>();
                drag.Init(s.Id, this);
            }
        }

        // ── Dragging (called by BenchCardDrag) ───────────────────────────────
        public void OnCardDragStart(string studentId, GameObject card)
        {
            if (_battleStarted) return;
            _draggingStudentId = studentId;
            _hoveredTile = null;

            // Highlight valid player tiles (exclude this student's own current tile)
            foreach (var kv in _tiles)
            {
                bool isOwnTile = _pendingPlacements.TryGetValue(studentId, out var own) && kv.Key == own;
                if (kv.Key.Row < HexGrid.PlayerRowCount && (!_grid.IsOccupied(kv.Key) || isOwnTile))
                    kv.Value.SetHighlight(true);
            }

            // Create a ghost — get sprite from the dragged card's SpriteRenderer if present;
            // card is a UI Image (no SpriteRenderer) so fall back to the procedural white sprite.
            _dragGhost = new GameObject("DragGhost");
            _dragGhost.transform.SetParent(transform, false);
            var sr     = _dragGhost.AddComponent<SpriteRenderer>();
            var cardSr = card.GetComponent<SpriteRenderer>();
            sr.sprite       = cardSr != null ? cardSr.sprite : GetFallbackSprite();
            sr.color        = new Color(StudentColor(studentId).r, StudentColor(studentId).g, StudentColor(studentId).b, 0.6f);
            sr.sortingOrder = 10;
            _dragGhost.transform.localScale = Vector3.one * 0.4f;
        }

        public void OnCardDrag(Vector2 screenPos)
        {
            if (_dragGhost == null) return;
            Vector3 world = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_cam.transform.position.z));
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
            Vector3 worldPos = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_cam.transform.position.z));
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
            var card = _benchPanel.Find($"Card_{studentId}");
            if (card != null)
            {
                var img = card.GetComponent<Image>();
                if (img != null) img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
            }
            _startBattleButton.interactable = _pendingPlacements.Count > 0;
        }

        private void PlaceStudent(string studentId, HexCoord coord)
        {
            if (!_studentSnapshots.TryGetValue(studentId, out var snap)) return;
            if (_grid.IsOccupied(coord)) return;

            // Remove existing placement if re-placing — restore card alpha first
            if (_pendingPlacements.TryGetValue(studentId, out var oldCoord))
            {
                _grid.ClearOccupant(oldCoord);
                if (_units.TryGetValue(studentId, out var oldUnit))
                {
                    Destroy(oldUnit.gameObject);
                    _units.Remove(studentId);
                }
                var existingCard = _benchPanel.Find($"Card_{studentId}");
                if (existingCard != null)
                {
                    var existingImg = existingCard.GetComponent<Image>();
                    if (existingImg != null)
                        existingImg.color = new Color(existingImg.color.r, existingImg.color.g, existingImg.color.b, 1f);
                }
            }

            _pendingPlacements[studentId] = coord;
            if (_traitTracker != null && _championDataLookup.TryGetValue(studentId, out var champData))
                _traitTracker.RegisterPlacement(studentId, coord, champData);
            _grid.SetOccupant(coord, studentId);

            // Spawn unit — MaxHP sourced from snapshot (B3)
            var go   = Instantiate(_battleUnitPrefab, CoordToWorld(coord), Quaternion.identity);
            var unit = go.GetComponent<BattleUnit>();
            unit.Init(studentId, coord);
            unit.InitHealthBar(snap.MaxHP, snap.MaxHP);
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (sr.sprite == null) sr.sprite = GetFallbackSprite();
                sr.color = StudentColor(studentId);
            }
            _units[studentId] = unit;

            // Dim bench card to show it's placed (kept active so it can be re-dragged)
            var card = _benchPanel.Find($"Card_{studentId}");
            if (card != null)
            {
                var img = card.GetComponent<Image>();
                if (img != null) img.color = new Color(img.color.r, img.color.g, img.color.b, 0.4f);
            }

            _startBattleButton.interactable = _pendingPlacements.Count > 0;
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
            _startBattleButton.gameObject.SetActive(false);
            _benchPanel.gameObject.SetActive(false);

            // Tell resolver player positions
            _resolver.SetUnitPositions(_pendingPlacements);

            // Apply trait bonuses before battle starts
            if (_traitTracker != null && _championRoster != null)
                TraitEffectApplier.Apply(
                    _traitTracker.GetActiveBreakpoints(),
                    _championDataLookup,
                    _pendingPlacements,
                    _resolver);

            // Auto-spawn enemy units from snapshots — no direct stub dependency
            var enemyPlacements = _resolver.GetAutoEnemyPlacements();
            var enemySnapshots  = _resolver.GetCombatantSnapshots().Where(s => !s.IsStudent).ToList();
            foreach (var e in enemySnapshots)
            {
                if (!enemyPlacements.TryGetValue(e.Id, out var coord)) continue;
                var go   = Instantiate(_battleUnitPrefab, CoordToWorld(coord), Quaternion.identity);
                var unit = go.GetComponent<BattleUnit>();
                unit.Init(e.Id, coord);
                unit.InitHealthBar(e.MaxHP, e.MaxHP);
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (sr.sprite == null) sr.sprite = GetFallbackSprite();
                    sr.color = EnemyColor(e.Id);
                }
                _units[e.Id] = unit;
            }

#if UNITY_EDITOR
            if (_debugPlayerStartHpPct < 1f) _resolver.DebugSetAllPlayerHp(_debugPlayerStartHpPct);
#endif
            _resolver.BeginBattle();
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

        private void HandleComplete(BattleResult result)
        {
            _outcomePanel.SetActive(true);
            _outcomeText.text = result.Won
                ? $"VICTORY!\n{result.TicksElapsed} ticks"
                : $"DEFEAT\n{result.TicksElapsed} ticks";
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

    // ── Drag helper component ─────────────────────────────────────────────────
    [RequireComponent(typeof(Image))]
    internal class BenchCardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private string             _studentId;
        private BattleBoardManager _board;

        public void Init(string studentId, BattleBoardManager board)
        {
            _studentId = studentId;
            _board     = board;
        }

        public void OnBeginDrag(PointerEventData e) => _board.OnCardDragStart(_studentId, gameObject);
        public void OnDrag(PointerEventData e)      => _board.OnCardDrag(e.position);
        public void OnEndDrag(PointerEventData e)   => _board.OnCardDragEnd(e.position);
    }
}
