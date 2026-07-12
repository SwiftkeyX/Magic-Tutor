using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicSchool.Battle
{
    // Pre-battle setup API (build combatants, inject placements, apply trait
    // modifiers) and the read-only snapshot/HP/mana accessors UI consumers use.
    public partial class AutoBattleResolver
    {
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

        // Standalone fallback (e.g. BattleTest.unity/Battle.unity opened directly, no
        // RunManager to call SetCombatants): populates from ChampionRoster/EnemyDatabaseStub
        // components on this GameObject. Callers must invoke this themselves before reading
        // snapshots if they can't guarantee SetCombatants already ran — see BattleBoardManager.Start().
        public void EnsureCombatantsInitialized()
        {
            if (_combatants.Count > 0) return;

            if (_championRoster != null)
                SetCombatants(_championRoster.GetStudents(), GetComponent<EnemyDatabaseStub>()?.GetEnemies() ?? new List<EnemyCombatData>());
            else
            {
                var stub     = GetComponent<StudentRosterStub>();
                var database = GetComponent<EnemyDatabaseStub>();
                if (stub != null && database != null)
                    SetCombatants(stub.GetStudents(), database.GetEnemies());
            }
        }

        public List<CombatantSnapshot> GetCombatantSnapshots()
        {
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

                c.OmnivampPct                     = m.OmnivampPct;
                c.Dreadknight.ShieldEnabled       = m.DreadknightShieldOnLowHP;
                c.Striker.PctPerStack             = m.StrikerPctPerStack;
                c.Striker.BypassArmor             = m.StrikerBypassArmor;
                c.Striker.MaxStackSpeedBonus      = m.StrikerMaxStackSpeedBonus;
                c.Trickster.DashEnabled           = m.TricksterDashEnabled;
                c.Trickster.BleedEnabled          = m.TricksterBleedEnabled;
                c.Elementalist.ExplosionPct       = m.ElementalistExplosionPct;
                c.Elementalist.TrueDamage         = m.ElementalistTrueDamage;
                c.Ranger.BonusDmgPerHex           = m.RangerBonusDmgPerHex;

                Debug.Log($"[Trait] {c.DisplayName}: DEF={c.DEF} MR={c.MR} MG={c.MG} Shield={c.Shield} Mana={c.Mana}");
            }

            _kineticEnabled           = globalSettings.KineticEnabled;
            _kineticManaPerInterval   = globalSettings.KineticManaPerInterval;
            _kineticSupportExtraBonus = globalSettings.KineticSupportExtraBonus;
            _kineticTickCounter       = 0;
        }
    }
}
