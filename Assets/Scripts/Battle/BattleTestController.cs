using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MagicSchool.Battle
{
    /// Self-contained battle-grid test: 3 player units vs 3 enemy units.
    /// No external systems required. Attach to a GameObject in any scene.
    public class BattleTestController : MonoBehaviour
    {
        // ── Inspector refs ─────────────────────────────────────────────────
        [Header("Prefabs")]
        [SerializeField] private GameObject hexTilePrefab;
        [SerializeField] private GameObject battleUnitPrefab;

        [Header("Battle Config")]
        [SerializeField] private float tickDelay = 0.6f;
        [SerializeField] private int baseActionInterval = 10;
        [SerializeField] private int maxBattleTicks = 200;

        // ── Grid world-space sizing ────────────────────────────────────────
        private const float HexWidth  = 1.1f;
        private const float HexHeight = 0.95f;
        private const float HexOffset = 0.55f;   // half-tile indent for odd rows

        // ── Internal state ─────────────────────────────────────────────────
        private HexGrid _grid;
        private readonly Dictionary<HexCoord, HexTileView> _tiles = new Dictionary<HexCoord, HexTileView>();
        private readonly Dictionary<string, BattleUnitHandle> _units = new Dictionary<string, BattleUnitHandle>();
        private readonly List<Combatant> _combatants = new List<Combatant>();
        private bool _battleRunning;

        // ── Debug gizmo state ──────────────────────────────────────────────
        private struct AttackLine { public Vector3 From, To; public float Time; }
        private readonly AttackLine[] _attackLines = new AttackLine[10];
        private int _attackLineHead;

        private const float GizmoHexRadius  = HexWidth * 0.48f;
        private const float AttackLineTTL   = 1.5f;
        private const float ArrowHeadRadius = 0.12f;

        // ── Hardcoded test roster ──────────────────────────────────────────
        // Player side (rows 0–3). Three archetypes: melee, mage, archer.
        private static readonly TestUnit[] PlayerUnits =
        {
            new TestUnit("p_warrior", "Warrior", isPlayer: true,
                         hp: 60, atk: 12, def: 5, spd: 4, range: 1,
                         startCell: new HexCoord(1, 0), color: new Color(0.2f, 0.6f, 1f)),

            new TestUnit("p_mage",    "Mage",    isPlayer: true,
                         hp: 40, atk: 10, def: 2, spd: 3, range: 2,
                         startCell: new HexCoord(3, 1), color: new Color(0.7f, 0.3f, 1f)),

            new TestUnit("p_archer",  "Archer",  isPlayer: true,
                         hp: 45, atk: 8,  def: 3, spd: 5, range: 3,
                         startCell: new HexCoord(5, 2), color: new Color(0.2f, 0.85f, 0.4f)),
        };

        // Enemy side (rows 4–7). Mirror positions on enemy half.
        private static readonly TestUnit[] EnemyUnits =
        {
            new TestUnit("e_brute",   "Brute",   isPlayer: false,
                         hp: 55, atk: 11, def: 4, spd: 3, range: 1,
                         startCell: new HexCoord(1, 7), color: new Color(1f, 0.3f, 0.2f)),

            new TestUnit("e_witch",   "Witch",   isPlayer: false,
                         hp: 35, atk: 9,  def: 1, spd: 4, range: 2,
                         startCell: new HexCoord(3, 6), color: new Color(1f, 0.6f, 0.1f)),

            new TestUnit("e_sniper",  "Sniper",  isPlayer: false,
                         hp: 40, atk: 7,  def: 2, spd: 6, range: 3,
                         startCell: new HexCoord(5, 5), color: new Color(1f, 0.9f, 0.2f)),
        };

        // ── Unity lifecycle ────────────────────────────────────────────────
        private void Awake()
        {
            _grid = GetComponent<HexGrid>();
            if (_grid == null) _grid = gameObject.AddComponent<HexGrid>();
        }

        private void Start()
        {
            SpawnGrid();
            SpawnUnits();
            StartCoroutine(BattleLoop());
        }

        // ── Grid spawning ──────────────────────────────────────────────────
        private void SpawnGrid()
        {
            if (hexTilePrefab == null)
            {
                Debug.LogError("[BattleTestController] hexTilePrefab is not assigned.");
                return;
            }

            for (int row = 0; row < HexGrid.Rows; row++)
            {
                for (int col = 0; col < HexGrid.Cols; col++)
                {
                    var coord = new HexCoord(col, row);
                    Vector3 worldPos = CoordToWorld(coord);
                    GameObject tileGO = Instantiate(hexTilePrefab, worldPos, Quaternion.identity, transform);
                    var view = tileGO.GetComponent<HexTileView>();
                    if (view == null) view = tileGO.AddComponent<HexTileView>();
                    view.Init(coord);
                    _tiles[coord] = view;
                }
            }
        }

        // ── Unit spawning ──────────────────────────────────────────────────
        private void SpawnUnits()
        {
            if (battleUnitPrefab == null)
            {
                Debug.LogError("[BattleTestController] battleUnitPrefab is not assigned.");
                return;
            }

            foreach (TestUnit def in PlayerUnits.Concat(EnemyUnits))
            {
                SpawnCombatant(def);
            }
        }

        private static Sprite _circleSprite;

        private static Sprite GetCircleSprite()
        {
            if (_circleSprite != null) return _circleSprite;

            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            float center = size / 2f;
            float outerR = size / 2f - 1f;
            float innerR = outerR - 4f;   // thin dark ring for outline

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center, dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > outerR)
                        pixels[y * size + x] = new Color32(0, 0, 0, 0);        // transparent
                    else if (dist > innerR)
                        pixels[y * size + x] = new Color32(20, 20, 20, 200);   // dark outline ring
                    else
                        pixels[y * size + x] = new Color32(255, 255, 255, 255); // white fill (tinted by sr.color)
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            // PPU=80 → sprite is 64/80 = 0.8 world-units wide, fits comfortably in a hex cell
            _circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 80f);
            return _circleSprite;
        }

        private void SpawnCombatant(TestUnit def)
        {
            Vector3 worldPos = CoordToWorld(def.StartCell);
            GameObject go = Instantiate(battleUnitPrefab, worldPos, Quaternion.identity, transform);

            var unit = go.GetComponent<BattleUnit>();
            if (unit == null) unit = go.AddComponent<BattleUnit>();
            unit.Init(def.Id, def.StartCell);

            // Circle sprite tinted per archetype
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetCircleSprite();
            sr.color = def.Color;
            sr.sortingOrder = 1;

            // Health bar above the label
            unit.InitHealthBar(def.HP, def.HP);

            // Name label
            AddLabel(go, def.DisplayName, def.IsPlayer);

            _grid.SetOccupant(def.StartCell, def.Id);

            _units[def.Id] = new BattleUnitHandle { View = unit, Go = go };

            _combatants.Add(new Combatant
            {
                Id = def.Id,
                DisplayName = def.DisplayName,
                IsPlayer = def.IsPlayer,
                MaxHP = def.HP,
                CurrentHP = def.HP,
                ATK = def.ATK,
                DEF = def.DEF,
                SPD = def.SPD,
                Range = def.Range,
                Position = def.StartCell,
                ActionInterval = Math.Max(1, baseActionInterval - def.SPD),
                ActionTimer = Math.Max(1, baseActionInterval - def.SPD),
            });
        }

        private static void AddLabel(GameObject parent, string text, bool isPlayer)
        {
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(parent.transform, false);
            labelGO.transform.localPosition = new Vector3(0, 0.55f, 0);
            var tm = labelGO.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 20;
            tm.characterSize = 0.06f;
            tm.anchor = TextAnchor.LowerCenter;
            tm.color = isPlayer ? Color.cyan : Color.red;
        }

        // ── Battle loop ────────────────────────────────────────────────────
        private IEnumerator BattleLoop()
        {
            Debug.Log("[Battle] START");
            _battleRunning = true;
            int ticks = 0;

            while (_battleRunning)
            {
                yield return new WaitForSeconds(tickDelay);
                ticks++;

                // Decrement timers; collect actors
                var actors = new List<Combatant>();
                foreach (Combatant c in _combatants)
                {
                    if (c.IsDefeated) continue;
                    c.ActionTimer--;
                    if (c.ActionTimer <= 0) actors.Add(c);
                }

                // Sort by SPD descending
                actors.Sort((a, b) => b.SPD.CompareTo(a.SPD));

                foreach (Combatant actor in actors)
                {
                    if (actor.IsDefeated) continue;
                    ProcessAction(actor);
                    actor.ActionTimer = actor.ActionInterval;

                    if (CheckWinCondition(out bool playerWon))
                    {
                        _battleRunning = false;
                        Debug.Log($"[Battle] END — {(playerWon ? "PLAYERS WIN" : "ENEMIES WIN")} in {ticks} ticks");
                        yield break;
                    }
                }

                if (ticks >= maxBattleTicks)
                {
                    int pAlive = _combatants.Count(c => c.IsPlayer && !c.IsDefeated);
                    int eAlive = _combatants.Count(c => !c.IsPlayer && !c.IsDefeated);
                    bool timedOutWin = pAlive > eAlive;
                    Debug.Log($"[Battle] TIMEOUT — {(timedOutWin ? "PLAYERS WIN" : "ENEMIES WIN")} by survivor count ({pAlive} vs {eAlive})");
                    _battleRunning = false;
                }
            }
        }

        private void ProcessAction(Combatant actor)
        {
            List<Combatant> opponents = _combatants
                .Where(c => c.IsPlayer != actor.IsPlayer && !c.IsDefeated)
                .ToList();
            if (opponents.Count == 0) return;

            // Check if any opponent is within attack range
            Combatant target = FindInRange(actor, opponents);

            if (target != null)
            {
                Attack(actor, target);
            }
            else
            {
                // Move one step toward nearest opponent
                MoveTowardNearest(actor, opponents);
            }
        }

        private Combatant FindInRange(Combatant actor, List<Combatant> opponents)
        {
            Combatant nearest = null;
            int nearestDist = int.MaxValue;
            foreach (Combatant opp in opponents)
            {
                int d = HexCoord.Distance(actor.Position, opp.Position);
                if (d <= actor.Range && d < nearestDist)
                {
                    nearestDist = d;
                    nearest = opp;
                }
            }
            return nearest;
        }

        private void Attack(Combatant actor, Combatant target)
        {
            int damage = Math.Max(1, (int)(actor.ATK * (100f / (100 + target.DEF))));
            target.CurrentHP -= damage;

            Debug.Log($"[Battle] {actor.DisplayName} attacks {target.DisplayName} for {damage} dmg " +
                      $"(HP: {target.CurrentHP}/{target.MaxHP})");

            // Visual: attack lunge + HP bar update + gizmo line
            if (_units.TryGetValue(actor.Id, out var actorView))
            {
                Vector3 targetPos = CoordToWorld(target.Position);
                actorView.View.PlayAttackAnim(targetPos);
                RecordAttack(CoordToWorld(actor.Position), targetPos);
            }

            if (_units.TryGetValue(target.Id, out var targetView))
                targetView.View.UpdateHP(target.CurrentHP, target.MaxHP);

            if (target.CurrentHP <= 0)
            {
                target.CurrentHP = 0;
                _grid.ClearOccupant(target.Position);
                Debug.Log($"[Battle] {target.DisplayName} DEFEATED");
                if (_units.TryGetValue(target.Id, out var deadView))
                    deadView.View.PlayDeathAnim();
            }
        }

        private void MoveTowardNearest(Combatant actor, List<Combatant> opponents)
        {
            var oppPositions = opponents.Select(o => o.Position).ToList();
            HexCoord? nearest = _grid.FindNearest(actor.Position, oppPositions);
            if (nearest == null) return;

            HexCoord? nextStep = _grid.GetNextStep(actor.Position, nearest.Value, actor.Id);
            if (nextStep == null) return;

            HexCoord from = actor.Position;
            HexCoord to = nextStep.Value;

            _grid.ClearOccupant(from);
            _grid.SetOccupant(to, actor.Id);
            actor.Position = to;

            Debug.Log($"[Battle] {actor.DisplayName} moves {from} → {to}");

            if (_units.TryGetValue(actor.Id, out var unitView))
                unitView.View.MoveTo(CoordToWorld(to), to);
        }

        private bool CheckWinCondition(out bool playerWon)
        {
            bool allEnemiesDead  = _combatants.All(c => c.IsPlayer || c.IsDefeated);
            bool allPlayersDead  = _combatants.All(c => !c.IsPlayer || c.IsDefeated);
            playerWon = allEnemiesDead;
            return allEnemiesDead || allPlayersDead;
        }

        // ── Coordinate conversion ──────────────────────────────────────────
        public Vector3 CoordToWorld(HexCoord coord)
        {
            float x = coord.Col * HexWidth + (coord.Row % 2 == 1 ? HexOffset : 0f);
            float y = coord.Row * HexHeight;
            return new Vector3(x, y, 0f);
        }

        // ── Debug gizmos ───────────────────────────────────────────────────
        private void RecordAttack(Vector3 from, Vector3 to)
        {
            _attackLines[_attackLineHead % _attackLines.Length] =
                new AttackLine { From = from, To = to, Time = UnityEngine.Time.time };
            _attackLineHead++;
        }

        private void OnDrawGizmos()
        {
            // 1. Hex grid outlines
            for (int row = 0; row < HexGrid.Rows; row++)
            {
                Gizmos.color = row < HexGrid.PlayerRowCount
                    ? new Color(0.3f, 0.8f, 1f, 0.5f)    // cyan — player side
                    : new Color(1f,   0.3f, 0.3f, 0.5f);  // red  — enemy side

                for (int col = 0; col < HexGrid.Cols; col++)
                {
                    Vector3 center = CoordToWorld(new HexCoord(col, row));
                    DrawHexOutline(center, GizmoHexRadius);
                }
            }

            // 2. Recent attack lines
            float now = UnityEngine.Time.time;
            Gizmos.color = Color.yellow;
            foreach (AttackLine line in _attackLines)
            {
                if (line.Time <= 0f || now - line.Time > AttackLineTTL) continue;
                Gizmos.DrawLine(line.From, line.To);
                Gizmos.DrawWireSphere(line.To, ArrowHeadRadius);
            }
        }

        private static void DrawHexOutline(Vector3 center, float radius)
        {
            for (int i = 0; i < 6; i++)
            {
                float a0 = Mathf.PI / 2f + i       * Mathf.PI / 3f;
                float a1 = Mathf.PI / 2f + (i + 1) * Mathf.PI / 3f;
                Vector3 v0 = center + new Vector3(Mathf.Cos(a0) * radius, Mathf.Sin(a0) * radius, 0f);
                Vector3 v1 = center + new Vector3(Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius, 0f);
                Gizmos.DrawLine(v0, v1);
            }
        }

        // ── Internal types ─────────────────────────────────────────────────
        private class Combatant
        {
            public string Id;
            public string DisplayName;
            public bool IsPlayer;
            public int MaxHP, CurrentHP;
            public int ATK, DEF, SPD, Range;
            public HexCoord Position;
            public int ActionInterval, ActionTimer;
            public bool IsDefeated => CurrentHP <= 0;
        }

        private class BattleUnitHandle
        {
            public BattleUnit View;
            public GameObject Go;
        }

        [Serializable]
        private class TestUnit
        {
            public string Id, DisplayName;
            public bool IsPlayer;
            public int HP, ATK, DEF, SPD, Range;
            public HexCoord StartCell;
            public Color Color;

            public TestUnit(string id, string displayName, bool isPlayer,
                            int hp, int atk, int def, int spd, int range,
                            HexCoord startCell, Color color)
            {
                Id = id; DisplayName = displayName; IsPlayer = isPlayer;
                HP = hp; ATK = atk; DEF = def; SPD = spd; Range = range;
                StartCell = startCell; Color = color;
            }
        }
    }
}
