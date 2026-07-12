using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicSchool.Battle
{
    public partial class AutoBattleResolver : MonoBehaviour
    {
        // ── Events ──────────────────────────────────────────────────────────
        // Setup event — fires at the end of SetCombatants(); subscribers call
        // GetCombatantSnapshots() themselves to get data (no payload per GDD).
        // May fire more than once before BeginBattle() if RunManager narrows the
        // roster (full roster first, then fielded squad after ConfirmSquadPlacement).
        public event Action                                     OnCombatantsSet;

        public event Action<string, string, int, List<string>> OnCombatantActed;
        public event Action<string, HexCoord, HexCoord>        OnCombatantMoved;
        public event Action<string>                             OnCombatantDefeated;
        public event Action<BattleResult>                       OnBattleComplete;
        public event Action<string, string>                     OnSkillCast;  // (casterId, skillName)
        public event Action<string, int, int>                   OnManaChanged;  // (id, current, max)
        public event Action<string, CastState>                  OnCastStateChanged;

        // Static forwarding event — AudioSystem subscribes here so it does not need
        // FindObjectOfType to reach a non-singleton AutoBattleResolver instance.
        public static event Action<BattleResult>                OnAnyBattleComplete;

        // ── Constants ────────────────────────────────────────────────────────
        private const float TickDelay      = 0.1f;   // 10 ticks/second — matches TFT resolution
        private const int   MaxBattleTicks = 1200;   // 120s max (was 200 × 0.6s)

        // Trait HP thresholds — values must match the GDD (TraitSystem.md)
        // Internal (not private): read by TraitAbilityExecutor.TickDreadknightShield.
        internal const float DreadknightShieldHpThresholdPct = 0.40f;  // Dreadknight BP4: shield granted below 40% HP
        private  const float TricksterDashHpThresholdPct     = 0.50f;  // Trickster BP2: dash triggered below 50% HP

        // ── State ────────────────────────────────────────────────────────────
        private readonly List<Combatant>              _combatants       = new List<Combatant>();
        private readonly Dictionary<string, HexCoord> _playerPlacements = new Dictionary<string, HexCoord>();
        private          HexGrid                      _grid;
        private          bool                         _battleRunning;

        [SerializeField] private ChampionRoster        _championRoster;

        // ── Kinetic trait state ──────────────────────────────────────────────
        private bool _kineticEnabled;
        private int  _kineticManaPerInterval;
        private bool _kineticSupportExtraBonus;
        private int  _kineticTickCounter;
        private readonly List<Combatant> _pendingBonusActions = new List<Combatant>();

        // Internal accessors so TraitAbilityExecutor.TickKineticMana can read/mutate this
        // resolver-owned state without duplicating it — private fields above are unchanged.
        internal bool KineticEnabled           => _kineticEnabled;
        internal int  KineticManaPerInterval    => _kineticManaPerInterval;
        internal bool KineticSupportExtraBonus  => _kineticSupportExtraBonus;
        internal int  KineticTickCounter
        {
            get => _kineticTickCounter;
            set => _kineticTickCounter = value;
        }
        internal List<Combatant> PendingBonusActions => _pendingBonusActions;

        // ── Skill state (Dread Overlord's persistent "Dread Zone" area debuff) ──
        private struct ActiveZoneEffect
        {
            public HexCoord Center;
            public int      Radius;
            public float    DefShredPct;
            public int      TicksRemaining;
        }
        private readonly List<ActiveZoneEffect> _activeZones = new List<ActiveZoneEffect>();

        // Combatant itself now lives in its own file (Combatant.cs), top-level in
        // this namespace — moved out so Skills/*.cs and Traits/*.cs can reference
        // it without going through AutoBattleResolver.

        // ── Lifecycle ────────────────────────────────────────────────────────
        private void Awake()
        {
            // H2: Register with RunManager so it can reach us without FindObjectOfType.
            RunManager.Instance?.RegisterAutoBattleResolver(this);
        }

        // Pre-battle setup API (SetCombatants, SetUnitPositions, GetAutoEnemyPlacements,
        // EnsureCombatantsInitialized, GetCombatantSnapshots, GetCurrentHP/MaxHP/
        // CurrentMana/MaxMana) and ApplyPreBattleTraitModifiers now live in
        // AutoBattleResolver.Setup.cs.

        // ── Battle ───────────────────────────────────────────────────────────
        public void BeginBattle()
        {
            if (_battleRunning) { Debug.LogWarning("[AutoBattleResolver] Battle already running."); return; }

            // GDD Core Rule 1: guard against being called outside the Battle phase.
            // RunManager.Instance == null is the legitimate standalone/test scenario
            // (BattleTestHarness / BattleTestMatchup — no RunManager in the scene).
            if (RunManager.Instance != null && RunManager.Instance.CurrentPhase != RunPhase.Battle)
            {
                Debug.LogError($"[AutoBattleResolver] BeginBattle() called in phase {RunManager.Instance.CurrentPhase}; expected Battle. Aborting.");
                return;
            }

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
                TickStatusCounters();
                TickDreadZones();
                ApplyBleedTicks();
                TickDreadknightShield();
                TickTricksterDashTrigger();
                TickTricksterUntargetable();
                TickKineticMana();
                ResolveKineticBonusActions();
                TickCastChannels();

                // Accumulate action progress (replaces integer timer decrement).
                // Casting/Channeling/Stunned units are locked out of basic attacks —
                // their action timers do not advance (GDD: "stops basic attacking" /
                // "frozen, cannot act, action timers paused").
                foreach (var c in _combatants.Where(c => !c.IsDefeated && !IsActionLocked(c)))
                {
                    float gain = c.AttackSpeed * TickDelay;
                    gain = TraitAbilityExecutor.ApplyStrikerSpeedBonus(c, gain);
                    c.ActionProgress += gain;
                }

                // Collect ready actors (progress ≥ 1.0 = one full attack cycle complete)
                var ready = _combatants
                    .Where(c => !c.IsDefeated && !IsActionLocked(c) && c.ActionProgress >= 1.0f)
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

                    // Subtract one full cycle; overflow carries into the next cycle naturally
                    actor.ActionProgress -= 1.0f;

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
                    OnAnyBattleComplete?.Invoke(result);
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
                OnAnyBattleComplete?.Invoke(res);
                _battleRunning = false;
            }
        }

        // The 9 per-tick BattleLoop phases now live in AutoBattleResolver.Phases.cs.
        // Combat helpers (targeting, damage/mana/state mutation choke points, internal
        // accessors for Skills/Traits) now live in AutoBattleResolver.CombatHelpers.cs.
        // Attack/skill-cast lifecycle (Attack, TriggerCast, ResolveCastPlaceholder,
        // HandleKill, MoveTowardNearest) now lives in AutoBattleResolver.Attack.cs.
    }
}
