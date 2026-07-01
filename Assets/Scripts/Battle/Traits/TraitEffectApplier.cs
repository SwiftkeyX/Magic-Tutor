using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool.Battle
{
    public static class TraitEffectApplier
    {
        public static void Apply(
            IReadOnlyDictionary<TraitType, int> activeBreakpoints,
            Dictionary<string, ChampionData>    championLookup,
            Dictionary<string, HexCoord>        placements,
            AutoBattleResolver                  resolver)
        {
            var mods     = new Dictionary<string, CombatantTraitModifiers>();
            var settings = new ResolverTraitSettings();

            foreach (var id in placements.Keys)
            {
                if (!championLookup.TryGetValue(id, out var champ)) continue;
                mods[id] = new CombatantTraitModifiers
                {
                    Role               = champ.Role,
                    IsFrontRow         = placements[id].Row >= 2,
                    AttackSpeedMult = 1.0f,
                };
            }

            foreach (var kv in activeBreakpoints)
            {
                switch (kv.Key)
                {
                    case TraitType.Vanguard:     ApplyVanguard(kv.Value,     championLookup, placements, mods); break;
                    case TraitType.Striker:      ApplyStriker(kv.Value,      championLookup, mods);             break;
                    case TraitType.Elementalist: ApplyElementalist(kv.Value, championLookup, mods);             break;
                    case TraitType.Ranger:       ApplyRanger(kv.Value,       championLookup, mods);             break;
                    case TraitType.Kinetic:      ApplyKinetic(kv.Value,      placements, mods, ref settings);   break;
                    case TraitType.Dreadknight:  ApplyDreadknight(kv.Value,  championLookup, mods);             break;
                    case TraitType.Warden:       ApplyWarden(kv.Value,       championLookup, placements, mods); break;
                    case TraitType.Trickster:    ApplyTrickster(kv.Value,    championLookup, mods);             break;
                }
            }

            resolver.ApplyPreBattleTraitModifiers(mods, settings);
        }

        private static void ApplyVanguard(int bp, Dictionary<string, ChampionData> lookup,
            Dictionary<string, HexCoord> placements, Dictionary<string, CombatantTraitModifiers> mods)
        {
            int bonus = bp == 2 ? 25 : bp == 4 ? 60 : bp == 6 ? 120 : 250;
            foreach (var id in placements.Keys)
            {
                if (!lookup.TryGetValue(id, out var c) || !mods.ContainsKey(id)) continue;
                var m = mods[id];
                if (bp >= 8) { m.BonusDEF += 60; m.BonusMR += 60; }
                if (c.VerticalTrait == VerticalTrait.Vanguard)
                {
                    m.BonusDEF += bonus; m.BonusMR += bonus;
                    if (bp >= 6 && c.Role == ChampionRole.Tank) { m.BonusDEF += 30; m.BonusMR += 30; }
                }
                mods[id] = m;
            }
        }

        private static void ApplyStriker(int bp, Dictionary<string, ChampionData> lookup,
            Dictionary<string, CombatantTraitModifiers> mods)
        {
            float pct  = bp == 2 ? 0.06f : bp == 4 ? 0.12f : bp == 6 ? 0.20f : 0.35f;
            var   keys = new List<string>(mods.Keys);
            foreach (var id in keys)
            {
                if (!lookup.TryGetValue(id, out var c) || c.VerticalTrait != VerticalTrait.Striker) continue;
                var m = mods[id];
                m.StrikerPctPerStack = pct;
                if (bp >= 8) m.StrikerBypassArmor        = true;
                if (bp >= 6 && c.Role == ChampionRole.Carry) m.StrikerMaxStackSpeedBonus = true;
                mods[id] = m;
            }
        }

        private static void ApplyElementalist(int bp, Dictionary<string, ChampionData> lookup,
            Dictionary<string, CombatantTraitModifiers> mods)
        {
            int  mgBonus = bp == 2 ? 25 : bp == 4 ? 55 : bp == 6 ? 95 : 150;
            var  keys    = new List<string>(mods.Keys);
            foreach (var id in keys)
            {
                if (!lookup.TryGetValue(id, out var c) || c.VerticalTrait != VerticalTrait.Elementalist) continue;
                var m = mods[id];
                m.BonusMG += mgBonus;
                if (bp >= 6) m.ElementalistExplosionPct = bp >= 8 ? 0.20f : 0.10f;
                if (bp >= 8) m.ElementalistTrueDamage   = true;
                mods[id] = m;
            }
        }

        private static void ApplyRanger(int bp, Dictionary<string, ChampionData> lookup,
            Dictionary<string, CombatantTraitModifiers> mods)
        {
            int rangeBonus = bp >= 6 ? 2 : 1;
            var keys       = new List<string>(mods.Keys);
            foreach (var id in keys)
            {
                if (!lookup.TryGetValue(id, out var c) || c.VerticalTrait != VerticalTrait.Ranger) continue;
                var m = mods[id];
                m.BonusRange      += rangeBonus;
                m.AttackSpeedMult *= 1.176f;    // equiv to old ×0.85 interval: +17.6% AS
                if (bp >= 4) m.RangerBonusDmgPerHex = bp >= 6 ? 0.12f : 0.05f;
                mods[id] = m;
            }
        }

        private static void ApplyKinetic(int bp,
            Dictionary<string, HexCoord> placements, Dictionary<string, CombatantTraitModifiers> mods,
            ref ResolverTraitSettings settings)
        {
            settings.KineticEnabled           = true;
            settings.KineticManaPerInterval   = bp >= 4 ? 15 : 5;
            settings.KineticSupportExtraBonus = true;
            if (bp >= 4)
            {
                foreach (var id in placements.Keys)
                {
                    if (!mods.ContainsKey(id)) continue;
                    var m = mods[id]; m.InitialMana += 30; mods[id] = m;
                }
            }
        }

        private static void ApplyDreadknight(int bp, Dictionary<string, ChampionData> lookup,
            Dictionary<string, CombatantTraitModifiers> mods)
        {
            float vamp = bp >= 4 ? 0.30f : 0.15f;
            var   keys = new List<string>(mods.Keys);
            foreach (var id in keys)
            {
                if (!lookup.TryGetValue(id, out var c) || c.HorizontalTrait != HorizontalTrait.Dreadknight) continue;
                var m = mods[id];
                m.OmnivampPct = vamp;
                if (bp >= 4) m.DreadknightShieldOnLowHP = true;
                mods[id] = m;
            }
        }

        private static void ApplyWarden(int bp, Dictionary<string, ChampionData> lookup,
            Dictionary<string, HexCoord> placements, Dictionary<string, CombatantTraitModifiers> mods)
        {
            int   shieldAmt = bp == 2 ? 250 : bp == 3 ? 450 : 750;
            int   mgBonus   = bp == 2 ? 20  : bp == 3 ? 35  : 60;
            float asMult    = bp == 3 ? 1.176f : bp >= 4 ? 1.429f : 1.0f;  // equiv: 0.85 → 1/0.85, 0.70 → 1/0.70
            var   keys      = new List<string>(mods.Keys);
            foreach (var id in keys)
            {
                if (!lookup.TryGetValue(id, out var c) || c.HorizontalTrait != HorizontalTrait.Warden) continue;
                var m = mods[id];
                if (m.IsFrontRow) m.InitialShield += shieldAmt;
                else              m.BonusMG       += mgBonus;
                if (bp >= 3) m.AttackSpeedMult *= asMult;
                mods[id] = m;
            }
        }

        private static void ApplyTrickster(int bp, Dictionary<string, ChampionData> lookup,
            Dictionary<string, CombatantTraitModifiers> mods)
        {
            var keys = new List<string>(mods.Keys);
            foreach (var id in keys)
            {
                if (!lookup.TryGetValue(id, out var c) || c.HorizontalTrait != HorizontalTrait.Trickster) continue;
                var m = mods[id];
                m.TricksterDashEnabled = true;
                if (bp >= 4) m.TricksterBleedEnabled = true;
                mods[id] = m;
            }
        }
    }
}
