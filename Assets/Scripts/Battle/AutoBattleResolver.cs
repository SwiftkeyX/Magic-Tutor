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
        private const float DreadknightShieldHpThresholdPct = 0.40f;  // Dreadknight BP4: shield granted below 40% HP
        private const float TricksterDashHpThresholdPct     = 0.50f;  // Trickster BP2: dash triggered below 50% HP

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

        // ── Skill state (Dread Overlord's persistent "Dread Zone" area debuff) ──
        private struct ActiveZoneEffect
        {
            public HexCoord Center;
            public int      Radius;
            public float    DefShredPct;
            public int      TicksRemaining;
        }
        private readonly List<ActiveZoneEffect> _activeZones = new List<ActiveZoneEffect>();

        // ── Internal combatant ───────────────────────────────────────────────
        // internal (not private) so Assets/Scripts/Battle/Skills/*.cs can operate on it.
        internal class Combatant
        {
            public string   Id;
            public string   ChampionId;     // Players only — bridge to ChampionData.Id
            public string   DisplayName;
            public bool     IsPlayer;
            public int      MaxHP;
            public int      CurrentHP;
            public int      ATK;
            public int      DEF;
            public float    AttackSpeed;
            public int      Range;
            public HexCoord Position;
            public float    ActionProgress;  // accumulates AttackSpeed × TickDelay each tick; fires at ≥ 1.0
            public int      MaxMana;
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

            // ── Active Skill System fields ────────────────────────────────
            public CastState        State;                 // default Idle
            public int              CastTicksRemaining;
            public HexCoord         PendingTargetHex;       // locked in at cast-trigger time
            public bool             IsSilenced;
            public int              SilenceTicksRemaining;
            public bool             IsStunned;
            public int              StunTicksRemaining;
            public SkillDefinition  Skill;                  // Archetype.None = no skill
            public float            InterceptPct;           // Phalanx: % of ally damage redirected to self
            public int              InterceptTicksRemaining;
            public string           CurrentTargetId;        // last basic-attack target; used by the "Current Target" priority sort
        }

        // ── Lifecycle ────────────────────────────────────────────────────────
        private void Awake()
        {
            // H2: Register with RunManager so it can reach us without FindObjectOfType.
            RunManager.Instance?.RegisterAutoBattleResolver(this);
        }

        // ── Setup API ────────────────────────────────────────────────────────
        public void SetCombatants(List<StudentCombatData> students, List<EnemyCombatData> enemies)
        {
            _combatants.Clear();
            _playerPlacements.Clear();

            foreach (var s in students)
                _combatants.Add(new Combatant
                {
                    Id          = s.Id,
                    ChampionId  = s.ChampionId,
                    DisplayName = s.DisplayName,
                    IsPlayer    = true,
                    MaxHP       = s.MaxHP,
                    CurrentHP   = s.MaxHP,
                    ATK         = s.ATK,
                    DEF         = s.DEF,
                    AttackSpeed = s.AttackSpeed,
                    Range       = s.Range,
                    MG          = s.MG,
                    MR          = s.MR,
                    MaxMana     = s.MaxMana,
                    Mana        = s.StartingMana,
                    Flags       = s.Flags ?? new List<BattleBehaviorFlag>(),
                    Skill       = s.Skill ?? new SkillDefinition(),
                });

            foreach (var e in enemies)
                _combatants.Add(new Combatant
                {
                    Id          = e.Id,
                    DisplayName = e.DisplayName,
                    IsPlayer    = false,
                    MaxHP       = e.MaxHP,
                    CurrentHP   = e.MaxHP,
                    ATK         = e.ATK,
                    DEF         = e.DEF,
                    AttackSpeed = e.AttackSpeed,
                    Range       = e.Range,
                    MG          = e.MG,
                    MR          = e.MR,
                    MaxMana     = e.MaxMana,
                    Mana        = e.StartingMana,
                    Flags       = e.Flags ?? new List<BattleBehaviorFlag>(),
                    Skill       = e.Skill ?? new SkillDefinition(),
                });

            // Signal subscribers (e.g. BattleHUD) that GetCombatantSnapshots() now
            // reflects real data. No payload — callers pull snapshots themselves.
            Debug.Log($"[AutoBattleResolver] SetCombatants complete ({_combatants.Count(c => c.IsPlayer)} students, " +
                      $"{_combatants.Count(c => !c.IsPlayer)} enemies) — firing OnCombatantsSet.");
            OnCombatantsSet?.Invoke();
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
                ChampionId  = c.ChampionId,
                DisplayName = c.DisplayName,
                IsStudent   = c.IsPlayer,
                MaxHP       = c.MaxHP,
                CurrentHP   = c.CurrentHP,
                Position    = c.Position,
                Range       = c.Range,
                Mana        = c.Mana,
                MaxMana     = c.MaxMana,
                Flags       = c.Flags != null
                                  ? new List<BattleBehaviorFlag>(c.Flags)
                                  : new List<BattleBehaviorFlag>(),
            }).ToList();
        }

        public int GetCurrentHP(string id) =>
            _combatants.FirstOrDefault(c => c.Id == id)?.CurrentHP ?? 0;

        public int GetMaxHP(string id) =>
            _combatants.FirstOrDefault(c => c.Id == id)?.MaxHP ?? 0;

        public int GetCurrentMana(string id) =>
            _combatants.FirstOrDefault(c => c.Id == id)?.Mana ?? 0;

        public int GetMaxMana(string id) =>
            _combatants.FirstOrDefault(c => c.Id == id)?.MaxMana ?? 0;

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
                SetMana(c, c.Mana + m.InitialMana);

                if (Math.Abs(m.AttackSpeedMult - 1f) > 0.001f)
                    c.AttackSpeed *= m.AttackSpeedMult;
                // No interval recalc — accumulator derives speed from AttackSpeed directly each tick.

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
                // Phase 0 — Stun/Silence tick-down. Runs first so State is settled
                // before anything else reads it this tick (GDD: Stunned "action timers
                // paused"; silence just gates mana/casting, no state transition needed).
                foreach (var c in _combatants)
                {
                    if (c.IsDefeated) continue;
                    if (c.IsStunned)
                    {
                        c.StunTicksRemaining--;
                        if (c.StunTicksRemaining <= 0)
                        {
                            c.IsStunned = false;
                            SetState(c, CastState.Idle);
                        }
                    }
                    if (c.IsSilenced)
                    {
                        c.SilenceTicksRemaining--;
                        if (c.SilenceTicksRemaining <= 0) c.IsSilenced = false;
                    }
                    if (c.InterceptTicksRemaining > 0)
                    {
                        c.InterceptTicksRemaining--;
                        if (c.InterceptTicksRemaining <= 0) c.InterceptPct = 0f;
                    }
                }

                // Dread Zone tick-down — same iterate-and-expire pattern as Bleed DoT below.
                for (int zi = _activeZones.Count - 1; zi >= 0; zi--)
                {
                    var zone = _activeZones[zi];
                    zone.TicksRemaining--;
                    if (zone.TicksRemaining <= 0) _activeZones.RemoveAt(zi);
                    else _activeZones[zi] = zone;
                }

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
                    if (!c.IsDefeated && c.DreadknightShieldEnabled && !c.DreadknightShieldGranted && c.CurrentHP < c.MaxHP * DreadknightShieldHpThresholdPct)
                    {
                        c.Shield += (int)(c.MaxHP * 0.25f);
                        c.DreadknightShieldGranted = true;
                        Debug.Log($"[Trait] {c.DisplayName} Dreadknight Shield: +{(int)(c.MaxHP * 0.25f)}");
                    }

                // Phase 3 — Trickster dash trigger
                var dashCandidates = new List<Combatant>();
                foreach (var c in _combatants)
                    if (!c.IsDefeated && c.TricksterDashEnabled && !c.TricksterDashTriggered && c.CurrentHP < c.MaxHP * TricksterDashHpThresholdPct)
                        dashCandidates.Add(c);
                foreach (var c in dashCandidates) TraitAbilityExecutor.ExecuteTricksterDash(this, c);

                // Phase 4 — Trickster untargetable countdown
                foreach (var c in _combatants)
                    if (c.TricksterUntargetable)
                    {
                        c.TricksterUntargetableTicks--;
                        if (c.TricksterUntargetableTicks <= 0) c.TricksterUntargetable = false;
                    }

                // Phase 5 — Kinetic mana tick (every 3s = 30 ticks at 0.1s/tick)
                if (_kineticEnabled)
                {
                    _kineticTickCounter++;
                    if (_kineticTickCounter >= 30)
                    {
                        _kineticTickCounter = 0;
                        foreach (var c in _combatants)
                        {
                            if (c.IsDefeated || !c.IsPlayer) continue;
                            int gain = _kineticManaPerInterval;
                            if (_kineticSupportExtraBonus && c.Role == ChampionRole.Support) gain += 10;
                            SetMana(c, c.Mana + gain);
                            Debug.Log($"[Trait][Kinetic] {c.DisplayName} +{gain} mana → {c.Mana}/{c.MaxMana}");
                            if (c.Mana >= c.MaxMana) { SetMana(c, 0); _pendingBonusActions.Add(c); }
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

                // Phase 6.5 — Cast/Channel countdown + resolution
                var castingUnits = new List<Combatant>();
                foreach (var c in _combatants)
                    if (!c.IsDefeated && (c.State == CastState.Casting || c.State == CastState.Channeling))
                        castingUnits.Add(c);
                foreach (var c in castingUnits)
                {
                    if (c.IsDefeated) continue;

                    if (c.IsStunned)
                    {
                        // Interrupted by stun: mana was already consumed at trigger time
                        // (never refunded), so "loses the consumed mana" falls out for free.
                        SetState(c, CastState.Stunned);
                        continue;
                    }

                    c.CastTicksRemaining--;
                    if (c.CastTicksRemaining <= 0)
                    {
                        ResolveCastPlaceholder(c);
                        if (!c.IsDefeated) SetState(c, CastState.Idle);
                    }
                }

                // Accumulate action progress (replaces integer timer decrement).
                // Casting/Channeling/Stunned units are locked out of basic attacks —
                // their action timers do not advance (GDD: "stops basic attacking" /
                // "frozen, cannot act, action timers paused").
                foreach (var c in _combatants.Where(c => !c.IsDefeated && !IsActionLocked(c)))
                {
                    float gain = c.AttackSpeed * TickDelay;
                    if (c.StrikerMaxStackSpeedBonus && c.Role == ChampionRole.Carry && c.StrikerStacks >= 8)
                        gain /= 0.70f;  // +42.9% at max Striker stacks
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

        // ── Combat helpers ───────────────────────────────────────────────────
        // Casting/Channeling/Stunned units cannot act (basic-attack lockout / CC freeze).
        private static bool IsActionLocked(Combatant c) =>
            c.State == CastState.Casting || c.State == CastState.Channeling || c.State == CastState.Stunned;

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

        // Shared shield-absorb + HP-subtract + (optional) event + (optional) kill-check
        // choke point for all damage application. Callers that need to defer the kill
        // check (Attack does more work first; TriggerElementalistExplosion defers to
        // avoid mutating _combatants mid-iteration) pass autoHandleKill: false and
        // check target.IsDefeated themselves afterward. Returns the post-shield damage
        // actually applied to HP.
        internal int ApplyDamageAndCheckKill(Combatant actor, Combatant target, int damage, out int shieldAbsorbed,
            List<string> tags = null, bool bypassShield = false, bool autoHandleKill = true)
        {
            // Phalanx-style intercept: an adjacent ally with an active InterceptPct
            // (set by its own skill, ticked down like stun/silence) absorbs a % of
            // damage aimed at this target onto itself instead.
            var interceptor = _combatants.FirstOrDefault(c =>
                !c.IsDefeated && c != target && c.IsPlayer == target.IsPlayer &&
                c.InterceptTicksRemaining > 0 && c.InterceptPct > 0 &&
                HexCoord.Distance(c.Position, target.Position) <= 1);
            if (interceptor != null)
            {
                int redirected = (int)(damage * interceptor.InterceptPct);
                if (redirected > 0)
                {
                    damage -= redirected;
                    Debug.Log($"[Skill] {interceptor.DisplayName} intercepts {redirected} dmg meant for {target.DisplayName}");
                    ApplyDamageAndCheckKill(actor, interceptor, redirected, out _, tags: null, autoHandleKill: autoHandleKill);
                }
            }

            shieldAbsorbed = 0;
            if (!bypassShield && target.Shield > 0)
            {
                shieldAbsorbed = Math.Min(target.Shield, damage);
                target.Shield -= shieldAbsorbed;
                damage        -= shieldAbsorbed;
            }
            target.CurrentHP -= damage;

            if (tags != null)
                OnCombatantActed?.Invoke(actor?.Id, target.Id, damage, tags);

            if (autoHandleKill && target.IsDefeated)
                HandleKill(actor, target);

            return damage;
        }

        // Single choke point for all Mana mutations, so the UI always hears about
        // every change via one event (same philosophy as ApplyDamageAndCheckKill).
        private void SetMana(Combatant c, int newValue)
        {
            c.Mana = Math.Max(0, newValue);
            OnManaChanged?.Invoke(c.Id, c.Mana, c.MaxMana);
        }

        private void SetState(Combatant c, CastState newState)
        {
            c.State = newState;
            OnCastStateChanged?.Invoke(c.Id, newState);
        }

        // ── Skill execution support (internal — called by SkillArchetypeExecutor) ────
        internal HexGrid Grid => _grid;

        // Exposes all combatants (both teams) as a read-only list so SkillArchetypeExecutor
        // can call SkillTargetSelector for secondary-target resolution (e.g. Aegis's
        // SecondaryFilter ally shield) without needing a separate lookup path.
        internal IReadOnlyList<Combatant> AllCombatants => _combatants;

        internal List<Combatant> GetOpponentsOf(Combatant c) =>
            _combatants.Where(x => !x.IsDefeated && x.IsPlayer != c.IsPlayer).ToList();

        internal List<Combatant> GetAlliesOf(Combatant c) =>
            _combatants.Where(x => !x.IsDefeated && x.IsPlayer == c.IsPlayer && x != c).ToList();

        internal Combatant GetOccupantAt(HexCoord hex) =>
            _combatants.FirstOrDefault(x => !x.IsDefeated && x.Position.Equals(hex));

        // Applies the strongest active Dread-Zone def/MR shred (if any) covering
        // target's current hex. Used at damage-mitigation time by every damage
        // formula (basic attacks, casts, skills) so a zone affects all incoming
        // damage, not just its caster's own hits.
        internal int GetZoneShreddedDefense(Combatant target, int baseDefense)
        {
            float shredPct = 0f;
            foreach (var zone in _activeZones)
                if (HexCoord.Distance(zone.Center, target.Position) <= zone.Radius)
                    shredPct = Math.Max(shredPct, zone.DefShredPct);
            return shredPct > 0f ? (int)(baseDefense * (1f - shredPct)) : baseDefense;
        }

        internal void GrantMana(Combatant c, int amount)
        {
            if (c.IsSilenced) return;
            SetMana(c, c.Mana + amount);
        }

        internal void MoveUnit(Combatant c, HexCoord dest)
        {
            _grid.ClearOccupant(c.Position);
            var from = c.Position;
            c.Position = dest;
            _grid.SetOccupant(c.Position, c.Id);
            OnCombatantMoved?.Invoke(c.Id, from, c.Position);
        }

        internal void AddDreadZone(HexCoord center, int radius, float defShredPct, int ticks)
        {
            _activeZones.Add(new ActiveZoneEffect { Center = center, Radius = radius, DefShredPct = defShredPct, TicksRemaining = ticks });
        }

        private void Attack(Combatant actor, Combatant target)
        {
            actor.CurrentTargetId = target.Id;

            bool isMagic   = actor.Flags != null && actor.Flags.Contains(BattleBehaviorFlag.MagicAttack);
            int rawOffense = isMagic ? actor.MG  : actor.ATK;
            int rawDefense = GetZoneShreddedDefense(target, isMagic ? target.MR : target.DEF);
            int preMitDmg  = rawOffense;  // captured before mitigation for mana-on-hit

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
            {
                actor.StrikerStacks = Math.Min(8, actor.StrikerStacks + 1);
                Debug.Log($"[Trait][Striker] {actor.DisplayName} stack {actor.StrikerStacks}/8");
            }

            damage = ApplyDamageAndCheckKill(actor, target, damage, out int shieldAbsorbed, tags: null, autoHandleKill: false);

            int totalRaw = damage + shieldAbsorbed;
            if (actor.OmnivampPct > 0 && totalRaw > 0)
                actor.CurrentHP = Math.Min(actor.MaxHP, actor.CurrentHP + (int)(totalRaw * actor.OmnivampPct));

            if (actor.TricksterBleedNextAttack && actor.TricksterBleedEnabled)
            {
                target.BleedDamagePerTick  = Math.Max(1, (int)(target.MaxHP * 0.25f / 42));
                target.BleedTicksRemaining = 42;  // 4.2s at 0.1s/tick (was 7 at 0.6s/tick)
                actor.TricksterBleedNextAttack = false;
                Debug.Log($"[Trait] {target.DisplayName} afflicted with bleed by {actor.DisplayName}");
            }

            Debug.Log($"[AutoBattle] {actor.DisplayName} → {target.DisplayName}: {damage} dmg (HP:{target.CurrentHP}/{target.MaxHP})");
            OnCombatantActed?.Invoke(actor.Id, target.Id, damage, new List<string>());

            // Mana-on-attack (attacker, +10/hit) and mana-on-hit (defender, GDD formula:
            // max(1, (preMitDmg * 0.08) / MaxHP) * 100, capped at 40). Silenced units
            // generate no mana at all (GDD: "cannot cast skills and cannot generate mana").
            if (!actor.IsSilenced)
            {
                SetMana(actor, actor.Mana + 10);
                Debug.Log($"[Mana] {actor.DisplayName} +10 atk → {actor.Mana}/{actor.MaxMana}");
            }
            if (!target.IsSilenced)
            {
                int manaGain = Mathf.Min(40, Mathf.Max(1, Mathf.RoundToInt(preMitDmg * 0.08f / target.MaxHP * 100f)));
                SetMana(target, target.Mana + manaGain);
                Debug.Log($"[Mana] {target.DisplayName} +{manaGain} def (preMitDmg:{preMitDmg}, MaxHP:{target.MaxHP}) → {target.Mana}/{target.MaxMana}");
            }

            // Cast trigger — consume mana (reset to 0, plus any overflow past MaxMana per
            // GDD) before entering the Casting state, so a unit already mid-cast can't be
            // re-triggered by mana granted during its own attack resolution.
            if (actor.Mana >= actor.MaxMana && actor.State == CastState.Idle)
            {
                SetMana(actor, actor.Mana - actor.MaxMana);
                TriggerCast(actor);
            }
            if (!target.IsDefeated && target.Mana >= target.MaxMana && target.State == CastState.Idle)
            {
                SetMana(target, target.Mana - target.MaxMana);
                TriggerCast(target);
            }

            if (target.IsDefeated)
                HandleKill(actor, target);
        }

        // Mana capped — lock the target (or aim hex) in now, using the real
        // SkillTargetSelector when the caster has skill data, so casts/projectiles
        // resolve against whoever occupies that hex later even if the original
        // target moved or died. Falls back to the original placeholder (first-in-
        // range) for combatants with no Skill data yet (e.g. enemies).
        private void TriggerCast(Combatant caster)
        {
            var skill = caster.Skill;
            HexCoord? aimHex;

            if (skill != null && skill.Archetype == SkillArchetype.SelfBuff)
            {
                aimHex = caster.Position;  // GDD: Self-Buff templates target self, not an enemy hex
            }
            else if (skill != null && skill.Archetype != SkillArchetype.None)
            {
                // Pass skill.TargetTeam so ally-scoped skills (e.g. Novice Cleric) aim at
                // the correct team. TargetTeam defaults to Enemy, preserving all existing behavior.
                var hexes = SkillTargetSelector.SelectTargetHexes(_grid, caster, _combatants, skill.BaseFilter, skill.Sorts, skill.Range, skill.Radius, skill.TargetTeam);
                aimHex = hexes.Count > 0 ? hexes[0] : (HexCoord?)null;
            }
            else
            {
                var opponents = _combatants.Where(c => !c.IsDefeated && c.IsPlayer != caster.IsPlayer).ToList();
                aimHex = opponents.Count > 0 ? FindInRange(caster, opponents)?.Position : null;
            }

            if (aimHex == null) return;

            caster.PendingTargetHex   = aimHex.Value;
            caster.CastTicksRemaining = (skill != null && skill.LockoutTicks > 0) ? skill.LockoutTicks : 1;
            SetState(caster, (skill != null && skill.IsChannel) ? CastState.Channeling : CastState.Casting);
            Debug.Log($"[Skill] {caster.DisplayName} begins {caster.State} (target hex {caster.PendingTargetHex})");
            OnSkillCast?.Invoke(caster.Id, skill?.SkillName ?? "Basic Ability");
        }

        // Resolves the cast via SkillArchetypeExecutor when the caster has skill data;
        // otherwise falls back to the original placeholder single-target damage stub.
        private void ResolveCastPlaceholder(Combatant caster)
        {
            if (caster.Skill != null && caster.Skill.Archetype != SkillArchetype.None)
            {
                SkillArchetypeExecutor.Execute(this, caster);
                return;
            }

            Combatant target = null;
            foreach (var c in _combatants)
                if (!c.IsDefeated && c.Position.Equals(caster.PendingTargetHex) && c.IsPlayer != caster.IsPlayer)
                { target = c; break; }
            if (target == null) return;

            bool isMagic   = caster.Flags != null && caster.Flags.Contains(BattleBehaviorFlag.MagicAttack);
            int rawOffense = isMagic ? caster.MG : caster.ATK;
            int rawDefense = GetZoneShreddedDefense(target, isMagic ? target.MR : target.DEF);
            int damage     = Math.Max(1, (int)(rawOffense * (100f / (100 + rawDefense))));

            damage = ApplyDamageAndCheckKill(caster, target, damage, out _, tags: null, autoHandleKill: false);

            Debug.Log($"[Ability] {caster.DisplayName} casts on {target.DisplayName}: {damage} dmg (HP:{target.CurrentHP}/{target.MaxHP})");
            OnCombatantActed?.Invoke(caster.Id, target.Id, damage, new List<string> { "ABILITY" });
            if (target.IsDefeated) HandleKill(caster, target);
        }

        private void ApplyDamage(Combatant target, int damage)
        {
            ApplyDamageAndCheckKill(null, target, damage, out _, tags: null, autoHandleKill: false);
        }

        internal void HandleKill(Combatant actor, Combatant target)
        {
            _grid.ClearOccupant(target.Position);
            Debug.Log($"[AutoBattle] {target.DisplayName} DEFEATED");
            OnCombatantDefeated?.Invoke(target.Id);
            if (actor != null && actor.ElementalistExplosionPct > 0)
                TraitAbilityExecutor.TriggerElementalistExplosion(this, actor, target);
        }

        // ── Debug helpers (editor/QA only) ───────────────────────────────────
#if UNITY_EDITOR
        public void DebugSetAllPlayerHp(float pct)
        {
            foreach (var c in _combatants)
                if (c.IsPlayer) c.CurrentHP = Mathf.Max(1, Mathf.RoundToInt(c.MaxHP * pct));
        }

        // Force-triggers a cast for the named combatant regardless of current mana,
        // bypassing battle RNG/positioning. QA hook for exercising targeting/archetype
        // resolution directly (see ActiveSkillSystem plan's AC4 verification approach).
        public void DebugForceCast(string combatantId)
        {
            var c = _combatants.FirstOrDefault(x => x.Id == combatantId);
            if (c != null && !c.IsDefeated && c.State == CastState.Idle) TriggerCast(c);
        }
#endif

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
