using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicSchool.Battle
{
    public class AutoBattleResolver : MonoBehaviour
    {
        // ── Events ──────────────────────────────────────────────────────────
        public event Action<string, string, int, List<string>> OnCombatantActed;
        public event Action<string, HexCoord, HexCoord>        OnCombatantMoved;
        public event Action<string>                             OnCombatantDefeated;
        public event Action<BattleResult>                       OnBattleComplete;

        // ── Constants ────────────────────────────────────────────────────────
        private const float TickDelay          = 0.6f;
        private const int   BaseActionInterval = 10;
        private const int   MaxBattleTicks     = 200;

        // ── State ────────────────────────────────────────────────────────────
        private readonly List<Combatant>                 _combatants        = new();
        private readonly Dictionary<string, HexCoord>    _playerPlacements  = new();
        private          HexGrid                         _grid;
        private          bool                            _battleRunning;

        // ── Internal combatant ───────────────────────────────────────────────
        private class Combatant
        {
            public string   Id;
            public string   DisplayName;
            public bool     IsPlayer;
            public int      MaxHP;
            public int      CurrentHP;
            public int      ATK;
            public int      DEF;
            public int      SPD;
            public int      Range;
            public HexCoord Position;
            public int      ActionInterval;
            public int      ActionTimer;
            public bool     IsDefeated => CurrentHP <= 0;
        }

        // ── Setup API ────────────────────────────────────────────────────────
        public void SetCombatants(List<StudentCombatData> students, List<EnemyCombatData> enemies)
        {
            _combatants.Clear();
            _playerPlacements.Clear();

            foreach (var s in students)
                _combatants.Add(new Combatant
                {
                    Id = s.Id, DisplayName = s.DisplayName, IsPlayer = true,
                    MaxHP = s.MaxHP, CurrentHP = s.MaxHP,
                    ATK = s.ATK, DEF = s.DEF, SPD = s.SPD, Range = s.Range,
                    ActionInterval = Math.Max(1, BaseActionInterval - s.SPD),
                    ActionTimer    = Math.Max(1, BaseActionInterval - s.SPD),
                });

            foreach (var e in enemies)
                _combatants.Add(new Combatant
                {
                    Id = e.Id, DisplayName = e.DisplayName, IsPlayer = false,
                    MaxHP = e.MaxHP, CurrentHP = e.MaxHP,
                    ATK = e.ATK, DEF = e.DEF, SPD = e.SPD, Range = e.Range,
                    ActionInterval = Math.Max(1, BaseActionInterval - e.SPD),
                    ActionTimer    = Math.Max(1, BaseActionInterval - e.SPD),
                });
        }

        public void SetUnitPositions(Dictionary<string, HexCoord> placements)
        {
            if (_battleRunning) { Debug.LogError("[AutoBattleResolver] SetUnitPositions called after battle started."); return; }
            foreach (var kv in placements)
                _playerPlacements[kv.Key] = kv.Value;
        }

        // Returns auto-assigned enemy positions without starting the battle.
        public Dictionary<string, HexCoord> GetAutoEnemyPlacements()
        {
            var result  = new Dictionary<string, HexCoord>();
            int col     = 0;
            int row     = HexGrid.PlayerRowCount;   // row 4 = enemy front
            foreach (var c in _combatants.Where(c => !c.IsPlayer))
            {
                if (col >= HexGrid.Cols) { col = 0; row++; }
                result[c.Id] = new HexCoord(col++, row);
            }
            return result;
        }

        public List<CombatantSnapshot> GetCombatantSnapshots()
        {
            // Lazy-init: if called before SetCombatants (e.g. from BattleBoardManager.Start),
            // populate from stub components on the same GameObject.
            if (_combatants.Count == 0)
            {
                var roster   = GetComponent<StudentRosterStub>();
                var database = GetComponent<EnemyDatabaseStub>();
                if (roster != null && database != null)
                    SetCombatants(roster.GetStudents(), database.GetEnemies());
            }

            return _combatants.Select(c => new CombatantSnapshot
            {
                Id = c.Id, DisplayName = c.DisplayName, IsStudent = c.IsPlayer,
                MaxHP = c.MaxHP, CurrentHP = c.CurrentHP,
                Position = c.Position, Range = c.Range,
            }).ToList();
        }

        public int GetCurrentHP(string id) =>
            _combatants.FirstOrDefault(c => c.Id == id)?.CurrentHP ?? 0;

        public int GetMaxHP(string id) =>
            _combatants.FirstOrDefault(c => c.Id == id)?.MaxHP ?? 0;

        // ── Battle ───────────────────────────────────────────────────────────
        public void BeginBattle()
        {
            if (_battleRunning) { Debug.LogWarning("[AutoBattleResolver] Battle already running."); return; }

            _grid = GetComponent<HexGrid>();
            if (_grid == null) { Debug.LogError("[AutoBattleResolver] HexGrid component required on same GameObject."); return; }

            // Apply player placements
            foreach (var kv in _playerPlacements)
            {
                var c = _combatants.FirstOrDefault(x => x.Id == kv.Key);
                if (c == null) continue;
                c.Position = kv.Value;
                _grid.SetOccupant(kv.Value, c.Id);
            }

            // Auto-place enemies
            int col = 0, row = HexGrid.PlayerRowCount;
            foreach (var c in _combatants.Where(c => !c.IsPlayer))
            {
                if (col >= HexGrid.Cols) { col = 0; row++; }
                c.Position = new HexCoord(col++, row);
                _grid.SetOccupant(c.Position, c.Id);
            }

            StartCoroutine(BattleLoop());
        }

        private IEnumerator BattleLoop()
        {
            _battleRunning = true;
            Debug.Log("[AutoBattle] START");
            int ticks = 0;

            while (true)
            {
                // Decrement timers
                foreach (var c in _combatants.Where(c => !c.IsDefeated))
                    c.ActionTimer--;

                // Collect ready actors
                var ready = _combatants
                    .Where(c => !c.IsDefeated && c.ActionTimer <= 0)
                    .OrderByDescending(c => c.SPD)
                    .ToList();

                foreach (var actor in ready)
                {
                    if (actor.IsDefeated) continue;

                    var opponents = _combatants.Where(c => !c.IsDefeated && c.IsPlayer != actor.IsPlayer).ToList();
                    if (opponents.Count == 0) break;

                    var inRange = FindInRange(actor, opponents);
                    if (inRange != null)
                        Attack(actor, inRange);
                    else
                        MoveTowardNearest(actor, opponents);

                    actor.ActionTimer = actor.ActionInterval;

                    // Win check
                    bool playersAlive  = _combatants.Any(c => c.IsPlayer  && !c.IsDefeated);
                    bool enemiesAlive  = _combatants.Any(c => !c.IsPlayer && !c.IsDefeated);
                    if (!enemiesAlive || !playersAlive) goto BattleEnd;
                }

                ticks++;
                if (ticks >= MaxBattleTicks)
                {
                    int pCount = _combatants.Count(c =>  c.IsPlayer && !c.IsDefeated);
                    int eCount = _combatants.Count(c => !c.IsPlayer && !c.IsDefeated);
                    var result = new BattleResult { Won = pCount > eCount, TicksElapsed = ticks, TimedOut = true };
                    Debug.Log($"[AutoBattle] TIMEOUT — {(result.Won ? "PLAYERS WIN" : "PLAYERS LOSE")}");
                    OnBattleComplete?.Invoke(result);
                    _battleRunning = false;
                    yield break;
                }

                yield return new WaitForSeconds(TickDelay);
            }

            BattleEnd:
            {
                bool won = _combatants.Any(c => c.IsPlayer && !c.IsDefeated);
                int finalTicks = ticks;
                var res = new BattleResult { Won = won, TicksElapsed = finalTicks, TimedOut = false };
                Debug.Log($"[AutoBattle] END — {(won ? "PLAYERS WIN" : "PLAYERS LOSE")} in {finalTicks} ticks");
                OnBattleComplete?.Invoke(res);
                _battleRunning = false;
            }
        }

        // ── Combat helpers ───────────────────────────────────────────────────
        private Combatant FindInRange(Combatant actor, List<Combatant> opponents)
        {
            Combatant nearest  = null;
            int       minDist  = int.MaxValue;
            foreach (var o in opponents)
            {
                int d = HexCoord.Distance(actor.Position, o.Position);
                if (d <= actor.Range && d < minDist) { minDist = d; nearest = o; }
            }
            return nearest;
        }

        private void Attack(Combatant actor, Combatant target)
        {
            int damage = Math.Max(1, (int)(actor.ATK * (100f / (100 + target.DEF))));
            target.CurrentHP -= damage;
            Debug.Log($"[AutoBattle] {actor.DisplayName} attacks {target.DisplayName} for {damage} dmg (HP: {target.CurrentHP}/{target.MaxHP})");
            OnCombatantActed?.Invoke(actor.Id, target.Id, damage, new List<string>());

            if (target.IsDefeated)
            {
                _grid.ClearOccupant(target.Position);
                Debug.Log($"[AutoBattle] {target.DisplayName} DEFEATED");
                OnCombatantDefeated?.Invoke(target.Id);
            }
        }

        private void MoveTowardNearest(Combatant actor, List<Combatant> opponents)
        {
            var oppPositions = opponents.Select(o => o.Position).ToList();
            var nearest      = _grid.FindNearest(actor.Position, oppPositions);
            if (nearest == null) return;

            var next = _grid.GetNextStep(actor.Position, nearest.Value, actor.Id);
            if (next == null) return;

            _grid.ClearOccupant(actor.Position);
            var from      = actor.Position;
            actor.Position = next.Value;
            _grid.SetOccupant(actor.Position, actor.Id);

            Debug.Log($"[AutoBattle] {actor.DisplayName} moves {from} → {actor.Position}");
            OnCombatantMoved?.Invoke(actor.Id, from, actor.Position);
        }
    }
}
