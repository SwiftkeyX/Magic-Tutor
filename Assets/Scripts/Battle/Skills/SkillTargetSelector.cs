using System.Collections.Generic;
using System.Linq;

namespace MagicSchool.Battle
{
    // Stackable targeting: a Base Filter scopes the candidate hex list, then one or
    // more Priority Sorts order it. Mirrors TraitEffectApplier's static-class style —
    // this project uses no interfaces for enum-dispatched behavior.
    internal static class SkillTargetSelector
    {
        public static List<HexCoord> SelectTargetHexes(
            HexGrid grid,
            AutoBattleResolver.Combatant caster,
            IReadOnlyList<AutoBattleResolver.Combatant> all,
            TargetBaseFilter filter,
            TargetPrioritySort[] sorts,
            int range,
            int radius)
        {
            if (filter == TargetBaseFilter.LinearPath)
            {
                // Linear Path needs an aim point first (resolved against enemies within
                // range, ordered by the given sorts), then expands to the full beam path.
                var aimCandidates = EnemyHexesInRange(grid, caster, all, range);
                aimCandidates = ApplySorts(aimCandidates, grid, caster, all, sorts, radius);
                if (aimCandidates.Count == 0) return new List<HexCoord>();
                return grid.GetLinearPath(caster.Position, aimCandidates[0], range);
            }

            List<HexCoord> candidates = ResolveBaseFilter(grid, caster, all, filter, range);
            return ApplySorts(candidates, grid, caster, all, sorts, radius);
        }

        private static List<HexCoord> ResolveBaseFilter(
            HexGrid grid, AutoBattleResolver.Combatant caster, IReadOnlyList<AutoBattleResolver.Combatant> all,
            TargetBaseFilter filter, int range)
        {
            switch (filter)
            {
                case TargetBaseFilter.Adjacent:
                    return HexCoord.GetNeighbors(caster.Position, HexGrid.Cols, HexGrid.Rows)
                        .Where(h => IsEnemyOccupied(h, caster, all))
                        .ToList();

                case TargetBaseFilter.WithinRange:
                    return EnemyHexesInRange(grid, caster, all, range);

                case TargetBaseFilter.Global:
                    return all.Where(c => !c.IsDefeated && c.IsPlayer != caster.IsPlayer)
                        .Select(c => c.Position)
                        .ToList();

                default:
                    return new List<HexCoord>();
            }
        }

        private static List<HexCoord> EnemyHexesInRange(
            HexGrid grid, AutoBattleResolver.Combatant caster, IReadOnlyList<AutoBattleResolver.Combatant> all, int range)
        {
            var inRange = new HashSet<HexCoord>(grid.GetInRange(caster.Position, range));
            return all.Where(c => !c.IsDefeated && c.IsPlayer != caster.IsPlayer && inRange.Contains(c.Position))
                .Select(c => c.Position)
                .ToList();
        }

        private static List<HexCoord> ApplySorts(
            List<HexCoord> candidates, HexGrid grid, AutoBattleResolver.Combatant caster,
            IReadOnlyList<AutoBattleResolver.Combatant> all, TargetPrioritySort[] sorts, int radius)
        {
            if (sorts == null) return candidates;
            foreach (var sort in sorts)
                candidates = ApplyPrioritySort(candidates, caster, all, sort, radius);
            return candidates;
        }

        private static List<HexCoord> ApplyPrioritySort(
            List<HexCoord> candidates, AutoBattleResolver.Combatant caster,
            IReadOnlyList<AutoBattleResolver.Combatant> all, TargetPrioritySort sort, int radius)
        {
            switch (sort)
            {
                case TargetPrioritySort.CurrentTarget:
                {
                    var currentTarget = all.FirstOrDefault(c => c.Id == caster.CurrentTargetId && !c.IsDefeated);
                    if (currentTarget != null && candidates.Contains(currentTarget.Position))
                        return new List<HexCoord> { currentTarget.Position }
                            .Concat(candidates.Where(h => !h.Equals(currentTarget.Position)))
                            .ToList();
                    return candidates;
                }

                case TargetPrioritySort.Furthest:
                    return candidates.OrderByDescending(h => HexCoord.Distance(caster.Position, h)).ToList();

                case TargetPrioritySort.Closest:
                    return candidates.OrderBy(h => HexCoord.Distance(caster.Position, h)).ToList();

                case TargetPrioritySort.LowestHpPct:
                    return candidates
                        .OrderBy(h => HpPctAt(h, all))
                        .ToList();

                case TargetPrioritySort.LargestCluster:
                {
                    var enemyPositions = all.Where(c => !c.IsDefeated && c.IsPlayer != caster.IsPlayer).Select(c => c.Position);
                    // Re-rank candidates by cluster density rather than delegating fully to
                    // HexGrid, since we need the whole ordered list, not just the single best.
                    return candidates
                        .OrderByDescending(h => enemyPositions.Count(e => HexCoord.Distance(h, e) <= radius))
                        .ToList();
                }

                default:
                    return candidates;
            }
        }

        private static float HpPctAt(HexCoord hex, IReadOnlyList<AutoBattleResolver.Combatant> all)
        {
            var occupant = all.FirstOrDefault(c => !c.IsDefeated && c.Position.Equals(hex));
            if (occupant == null) return float.MaxValue;
            return (float)occupant.CurrentHP / occupant.MaxHP;
        }

        private static bool IsEnemyOccupied(HexCoord hex, AutoBattleResolver.Combatant caster, IReadOnlyList<AutoBattleResolver.Combatant> all) =>
            all.Any(c => !c.IsDefeated && c.IsPlayer != caster.IsPlayer && c.Position.Equals(hex));
    }
}
