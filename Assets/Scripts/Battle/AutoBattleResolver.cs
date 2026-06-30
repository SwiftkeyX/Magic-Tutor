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
        private readonly List<Combatant>              _combatants       = new List<Combatant>();
        private readonly Dictionary<string, HexCoord> _playerPlacements = new Dictionary<string, HexCoord>();
        private          HexGrid                      _grid;
        private          bool                         _battleRunning;

        // ── Kinetic trait state ──────────────────────────────────────────────
        private bool _kineticEnabled;
        private int  _kineticManaPerInterval;
        private bool _kineticSupportExtraBonus;
        private int  _kineticTickCounter;
        private readonly List<Combatant> _pendingBonusActions = new List<Combatant>();

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
            public float    AttackSpeed;
            public int      Range;
            public HexCoord Position;
            public int      ActionInterval;
            public int      ActionTimer;
            public bool     IsDefeated => CurrentHP <= 0;

            // ── Trait fields ─────────────────────────────────────────────
            public int  MG;
            public int  MR;
            public List<BattleBehaviorFlag> Flags;
            public ChampionRole Role;
            public bool  IsFrontRow;
            public int   Shield;
            public float OmnivampPct;
            public bool  DreadknightShieldEnabled;
            public bool  DreadknightShieldGranted;
            public int   StrikerStacks;
            public float StrikerPctPerStack;
            public bool  StrikerBypassArmor;
            public bool  StrikerMaxStackSpeedBonus;
            public int   Mana;
            public bool  TricksterDashEnabled;
            public bool  TricksterBleedEnabled;
            public bool  TricksterDashTriggered;
            public bool  TricksterUntargetable;
            public int   TricksterUntargetableTicks;
            public bool  TricksterBleedNextAttack;
            public int   BleedDamagePerTick;
            public int   BleedTicksRemaining;
            public float ElementalistExplosionPct;
            public bool  ElementalistTrueDamage;
            public float RangerBonusDmgPerHex;
        }

        // ── Setup API ────────────────────────────────────────────────────────
        public void SetCombatants(List<StudentCombatData> students, List<EnemyCombatData> enemies)
        {
            _combatants.Clear();
            _playerPlacements.Clear();

            foreach (var s in students)
                _combatants.Add(new Combatant
                {
                    Id             = s.Id,
                    DisplayName    = s.DisplayName,
                    IsPlayer       = true,
                    MaxHP          = s.MaxHP,
                    CurrentHP      = s.MaxHP,
                    ATK            = s.ATK,
                    DEF            = s.DEF,
                    AttackSpeed    = s.AttackSpeed,
                    Range          = s.Range,
                    MG             = s.MG,
                    MR             = s.MR,
                    Flags          = s.Flags ?? new List<BattleBehaviorFlag>(),
                    ActionInterval = Mathf.Max(2, Mathf.RoundToInt(1f / (s.AttackSpeed * TickDelay))),
                    ActionTimer    = Mathf.Max(2, Mathf.RoundToInt(1f / (s.AttackSpeed * TickDelay))),
                });

            foreach (var e in enemies)
                _combatants.Add(new Combatant
                {
                    Id             = e.Id,
                    DisplayName    = e.DisplayName,
                    IsPlayer       = false,
                    MaxHP          = e.MaxHP,
                    CurrentHP      = e.MaxHP,
                    ATK            = e.ATK,
                    DEF            = e.DEF,
                    AttackSpeed    = e.AttackSpeed,
                    Range          = e.Range,
                    MG             = e.MG,
                    MR             = e.MR,
                    Flags          = e.Flags ?? new List<BattleBehaviorFlag>(),
                    ActionInterval = Mathf.Max(2, Mathf.RoundToInt(1f / (e.AttackSpeed * TickDelay))),
                    ActionTimer    = Mathf.Max(2, Mathf.RoundToInt(1f / (e.AttackSpeed * TickDelay))),
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
            var result = new Dictionary<string, HexCoord>();
            int col    = 0;
            int row    = HexGrid.PlayerRowCount;   // row 4 = enemy front
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
            // populate from components on the same GameObject.
            if (_combatants.Count == 0)
            {
                var roster = GetComponent<ChampionRoster>();
                if (roster != null)
                    SetCombatants(roster.GetStudents(), GetComponent<EnemyDatabaseStub>()?.GetEnemies() ?? new List<EnemyCombatData>());
                else
                {
                    var stub     = GetComponent<StudentRosterStub>();
                    var database = GetComponent<EnemyDatabaseStub>();
                    if (stub != null && database != null)
                        SetCombatants(stub.GetStudents(), database.GetEnemies());
                }
            }

            return _combatants.Select(c => new CombatantSnapshot
            {
                Id          = c.Id,
                DisplayName = c.DisplayName,
                IsStudent   = c.IsPlayer,
                MaxHP       = c.MaxHP,
                CurrentHP   = c.CurrentHP,
                Position    = c.Position,
                Range       = c.Range,
            }).ToList();
        }

        public int GetCurrentHP(string id) =>
            _combatants.FirstOrDefault(c => c.Id == id)?.CurrentHP ?? 0;

        public int GetMaxHP(string id) =>
            _combatants.FirstOrDefault(c => c.Id == id)?.MaxHP ?? 0;

        // ── Trait pre-battle application ─────────────────────────────────────
        public void ApplyPreBattleTraitModifiers(
            Dictionary<string, CombatantTraitModifiers> perUnitMods,
            ResolverTraitSettings globalSettings)
        {
            if (_battleRunning)
            {
                Debug.LogError("[AutoBattleResolver] ApplyPreBattleTraitModifiers called after battle started.");
                return;
            }

            foreach (var kv in perUnitMods)
            {
                Combatant c = null;
                foreach (var x in _combatants) { if (x.Id == kv.Key) { c = x; break; } }
                if (c == null) continue;

                var m = kv.Value;
                c.Role       = m.Role;
                c.IsFrontRow = m.IsFrontRow;
                c.DEF       += m.BonusDEF;
                c.MR        += m.BonusMR;
                c.MG        += m.BonusMG;
                c.ATK       += m.BonusATK;
                c.Range     += m.BonusRange;
                c.Shield     = m.InitialShield;
                c.Mana       = m.InitialMana;

                if (Math.Abs(m.AttackSpeedMult - 1f) > 0.001f)
                {
                    c.AttackSpeed    *= m.AttackSpeedMult;
                    c.ActionInterval  = Mathf.Max(2, Mathf.RoundToInt(1f / (c.AttackSpeed * TickDelay)));
                    c.ActionTimer     = c.ActionInterval;
                }

                c.OmnivampPct              = m.OmnivampPct;
                c.DreadknightShieldEnabled  = m.DreadknightShieldOnLowHP;
                c.StrikerPctPerStack        = m.StrikerPctPerStack;
                c.StrikerBypassArmor        = m.StrikerBypassArmor;
                c.StrikerMaxStackSpeedBonus = m.StrikerMaxStackSpeedBonus;
                c.TricksterDashEnabled      = m.TricksterDashEnabled;
                c.TricksterBleedEnabled     = m.TricksterBleedEnabled;
                c.ElementalistExplosionPct  = m.ElementalistExplosionPct;
                c.ElementalistTrueDamage    = m.ElementalistTrueDamage;
                c.RangerBonusDmgPerHex      = m.RangerBonusDmgPerHex;

                Debug.Log($"[Trait] {c.DisplayName}: DEF={c.DEF} MR={c.MR} MG={c.MG} Shield={c.Shield} Mana={c.Mana}");
            }

            _kineticEnabled           = globalSettings.KineticEnabled;
            _kineticManaPerInterval   = globalSettings.KineticManaPerInterval;
            _kineticSupportExtraBonus = globalSettings.KineticSupportExtraBonus;
            _kineticTickCounter       = 0;
        }

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
                // Phase 1 — Bleed DoT
                var bleedTargets = new List<Combatant>();
                foreach (var c in _combatants)
                    if (!c.IsDefeated && c.BleedTicksRemaining > 0) bleedTargets.Add(c);
                foreach (var c in bleedTargets)
                {
                    ApplyDamage(c, c.BleedDamagePerTick);
                    c.BleedTicksRemaining--;
                    if (c.IsDefeated) HandleKill(null, c);
                }

                // Phase 2 — Dreadknight low-HP shield
                foreach (var c in _combatants)
                    if (!c.IsDefeated && c.DreadknightShieldEnabled && !c.DreadknightShieldGranted && c.CurrentHP < c.MaxHP * 0.40f)
                    {
                        c.Shield += (int)(c.MaxHP * 0.25f);
                        c.DreadknightShieldGranted = true;
                        Debug.Log($"[Trait] {c.DisplayName} Dreadknight Shield: +{(int)(c.MaxHP * 0.25f)}");
                    }

                // Phase 3 — Trickster dash trigger
                var dashCandidates = new List<Combatant>();
                foreach (var c in _combatants)
                    if (!c.IsDefeated && c.TricksterDashEnabled && !c.TricksterDashTriggered && c.CurrentHP < c.MaxHP * 0.50f)
                        dashCandidates.Add(c);
                foreach (var c in dashCandidates) ExecuteTricksterDash(c);

                // Phase 4 — Trickster untargetable countdown
                foreach (var c in _combatants)
                    if (c.TricksterUntargetable)
                    {
                        c.TricksterUntargetableTicks--;
                        if (c.TricksterUntargetableTicks <= 0) c.TricksterUntargetable = false;
                    }

                // Phase 5 — Kinetic mana tick
                if (_kineticEnabled)
                {
                    _kineticTickCounter++;
                    if (_kineticTickCounter >= 5)
                    {
                        _kineticTickCounter = 0;
                        foreach (var c in _combatants)
                        {
                            if (c.IsDefeated || !c.IsPlayer) continue;
                            int gain = _kineticManaPerInterval;
                            if (_kineticSupportExtraBonus && c.Role == ChampionRole.Support) gain += 10;
                            c.Mana += gain;
                            if (c.Mana >= 100) { c.Mana -= 100; _pendingBonusActions.Add(c); }
                        }
                    }
                }

                // Phase 6 — Kinetic bonus actions
                foreach (var actor in _pendingBonusActions)
                {
                    if (actor.IsDefeated) continue;
                    var opp2 = new List<Combatant>();
                    foreach (var x in _combatants)
                        if (!x.IsDefeated && x.IsPlayer != actor.IsPlayer) opp2.Add(x);
                    var inRange2 = FindInRange(actor, opp2);
                    if (inRange2 != null)
                    {
                        Attack(actor, inRange2);
                        Debug.Log($"[Trait] {actor.DisplayName} Mana Bonus Action!");
                    }
                }
                _pendingBonusActions.Clear();

                // Decrement timers
                foreach (var c in _combatants.Where(c => !c.IsDefeated))
                    c.ActionTimer--;

                // Collect ready actors
                var ready = _combatants
                    .Where(c => !c.IsDefeated && c.ActionTimer <= 0)
                    .OrderByDescending(c => c.AttackSpeed)
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

                    // Striker max-stack speed bonus
                    int resetInterval = actor.ActionInterval;
                    if (actor.StrikerMaxStackSpeedBonus && actor.Role == ChampionRole.Carry && actor.StrikerStacks >= 8)
                        resetInterval = Math.Max(2, (int)(resetInterval * 0.70f));
                    actor.ActionTimer = resetInterval;

                    // Win check
                    bool playersAlive = _combatants.Any(c =>  c.IsPlayer && !c.IsDefeated);
                    bool enemiesAlive = _combatants.Any(c => !c.IsPlayer && !c.IsDefeated);
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
                bool won       = _combatants.Any(c => c.IsPlayer && !c.IsDefeated);
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
            Combatant nearest = null;
            int minDist = int.MaxValue;
            foreach (var o in opponents)
            {
                if (o.TricksterUntargetable) continue;
                int d = HexCoord.Distance(actor.Position, o.Position);
                if (d <= actor.Range && d < minDist) { minDist = d; nearest = o; }
            }
            return nearest;
        }

        private void Attack(Combatant actor, Combatant target)
        {
            bool isMagic   = actor.Flags != null && actor.Flags.Contains(BattleBehaviorFlag.MagicAttack);
            int rawOffense = isMagic ? actor.MG  : actor.ATK;
            int rawDefense = isMagic ? target.MR : target.DEF;

            if (!isMagic && actor.StrikerPctPerStack > 0)
            {
                float stackMult = 1f + actor.StrikerStacks * actor.StrikerPctPerStack;
                rawOffense = (int)(rawOffense * stackMult);
                if (actor.StrikerBypassArmor) rawDefense = (int)(rawDefense * 0.6f);
            }

            int damage = Math.Max(1, (int)(rawOffense * (100f / (100 + rawDefense))));

            if (!isMagic && actor.RangerBonusDmgPerHex > 0)
            {
                int dist = HexCoord.Distance(actor.Position, target.Position);
                damage = (int)(damage * (1f + actor.RangerBonusDmgPerHex * dist));
            }

            if (!isMagic && actor.StrikerPctPerStack > 0)
                actor.StrikerStacks = Math.Min(8, actor.StrikerStacks + 1);

            int shieldAbsorbed = 0;
            if (target.Shield > 0)
            {
                shieldAbsorbed = Math.Min(target.Shield, damage);
                target.Shield -= shieldAbsorbed;
                damage        -= shieldAbsorbed;
            }
            target.CurrentHP -= damage;

            int totalRaw = damage + shieldAbsorbed;
            if (actor.OmnivampPct > 0 && totalRaw > 0)
                actor.CurrentHP = Math.Min(actor.MaxHP, actor.CurrentHP + (int)(totalRaw * actor.OmnivampPct));

            if (actor.TricksterBleedNextAttack && actor.TricksterBleedEnabled)
            {
                target.BleedDamagePerTick  = Math.Max(1, (int)(target.MaxHP * 0.25f / 7));
                target.BleedTicksRemaining = 7;
                actor.TricksterBleedNextAttack = false;
                Debug.Log($"[Trait] {target.DisplayName} afflicted with bleed by {actor.DisplayName}");
            }

            Debug.Log($"[AutoBattle] {actor.DisplayName} → {target.DisplayName}: {damage} dmg (HP:{target.CurrentHP}/{target.MaxHP})");
            OnCombatantActed?.Invoke(actor.Id, target.Id, damage, new List<string>());

            if (target.IsDefeated)
                HandleKill(actor, target);
        }

        private void ApplyDamage(Combatant target, int damage)
        {
            if (target.Shield > 0)
            {
                int absorbed   = Math.Min(target.Shield, damage);
                target.Shield -= absorbed;
                damage        -= absorbed;
            }
            target.CurrentHP -= damage;
        }

        private void HandleKill(Combatant actor, Combatant target)
        {
            _grid.ClearOccupant(target.Position);
            Debug.Log($"[AutoBattle] {target.DisplayName} DEFEATED");
            OnCombatantDefeated?.Invoke(target.Id);
            if (actor != null && actor.ElementalistExplosionPct > 0)
                TriggerElementalistExplosion(actor, target);
        }

        private void TriggerElementalistExplosion(Combatant actor, Combatant killed)
        {
            var adjacent  = _grid.GetInRange(killed.Position, 1);
            var secondary = new List<(Combatant a, Combatant t)>();

            foreach (var coord in adjacent)
            {
                if (coord.Equals(killed.Position)) continue;

                Combatant hit = null;
                foreach (var c in _combatants)
                    if (!c.IsDefeated && c.Position.Equals(coord) && c.IsPlayer != actor.IsPlayer)
                    { hit = c; break; }
                if (hit == null) continue;

                int dmg = (int)(killed.MaxHP * actor.ElementalistExplosionPct);
                if (!actor.ElementalistTrueDamage)
                    dmg = Math.Max(1, (int)(dmg * (100f / (100 + hit.MR))));
                hit.CurrentHP -= dmg;

                Debug.Log($"[Trait] Elementalist explosion: {hit.DisplayName} -{dmg}");
                OnCombatantActed?.Invoke(actor.Id, hit.Id, dmg, new List<string> { "EXPLOSION" });

                if (hit.IsDefeated) secondary.Add((actor, hit));
            }

            foreach (var (a, t) in secondary) HandleKill(a, t);
        }

        private void ExecuteTricksterDash(Combatant c)
        {
            c.TricksterDashTriggered     = true;
            c.TricksterUntargetable      = true;
            c.TricksterUntargetableTicks = 2;
            c.TricksterBleedNextAttack   = c.TricksterBleedEnabled;

            // Find deepest backline enemy target
            Combatant backlineTarget = null;
            int best = c.IsPlayer ? int.MinValue : int.MaxValue;
            foreach (var x in _combatants)
            {
                if (x.IsDefeated || x.IsPlayer == c.IsPlayer) continue;
                if ( c.IsPlayer && x.Position.Row > best) { best = x.Position.Row; backlineTarget = x; }
                if (!c.IsPlayer && x.Position.Row < best) { best = x.Position.Row; backlineTarget = x; }
            }
            if (backlineTarget == null) return;

            // Find open neighbor adjacent to backline target
            HexCoord? dashDest = null;
            foreach (var nb in HexCoord.GetNeighbors(backlineTarget.Position, HexGrid.Cols, HexGrid.Rows))
                if (!_grid.IsOccupied(nb)) { dashDest = nb; break; }

            if (!dashDest.HasValue)
            {
                c.TricksterUntargetableTicks = 0;
                c.TricksterUntargetable      = false;
                return;
            }

            _grid.ClearOccupant(c.Position);
            var from   = c.Position;
            c.Position = dashDest.Value;
            _grid.SetOccupant(c.Position, c.Id);

            Debug.Log($"[Trait] {c.DisplayName} Trickster Dash: {from} → {c.Position}");
            OnCombatantMoved?.Invoke(c.Id, from, c.Position);
        }

        private void MoveTowardNearest(Combatant actor, List<Combatant> opponents)
        {
            var oppPositions = opponents.Select(o => o.Position).ToList();
            var nearest      = _grid.FindNearest(actor.Position, oppPositions);
            if (nearest == null) return;

            var next = _grid.GetNextStep(actor.Position, nearest.Value, actor.Id);
            if (next == null) return;

            _grid.ClearOccupant(actor.Position);
            var from       = actor.Position;
            actor.Position = next.Value;
            _grid.SetOccupant(actor.Position, actor.Id);

            Debug.Log($"[AutoBattle] {actor.DisplayName} moves {from} → {actor.Position}");
            OnCombatantMoved?.Invoke(actor.Id, from, actor.Position);
        }
    }
}
