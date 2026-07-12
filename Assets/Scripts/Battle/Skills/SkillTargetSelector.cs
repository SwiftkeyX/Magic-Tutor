using System.Collections.Generic;
using System.Linq;

namespace MagicSchool.Battle
{
    // Stackable targeting: a Base Filter scopes the candidate hex list, then one or
    // more Priority Sorts order it. Mirrors TraitEffectApplier's static-class style —
    // this project uses no interfaces for enum-dispatched behavior.
    //
    // targetTeam = Enemy (default) → original behavior, all filters scope to opponents.
    // targetTeam = Ally            → same filters/sorts scope to the caster's own team.
    // All 10 launch champions default to TargetTeam.Enemy, so no existing behavior changes.
    internal static class SkillTargetSelector
    {
        public static List<HexCoord> SelectTargetHexes(
            HexGrid grid,
            Combatant caster,
            IReadOnlyList<Combatant> all,
            TargetBaseFilter filter,
            TargetPrioritySort[] sorts,
            int range,
            int radius,
            TargetTeam targetTeam = TargetTeam.Enemy)
        {
            if (filter == TargetBaseFilter.LinearPath)
            {
                // Linear Path needs an aim point first (resolved against the correct team
                // within range, ordered by the given sorts), then expands to the full beam path.
                var aimCandidates = targetTeam == TargetTeam.Ally
                    ? AllyHexesInRange(grid, caster, all, range)
                    : EnemyHexesInRange(grid, caster, all, range);
                aimCandidates = ApplySorts(aimCandidates, grid, caster, all, sorts, radius, targetTeam);
                if (aimCandidates.Count == 0) return new List<HexCoord>();
                return grid.GetLinearPath(caster.Position, aimCandidates[0], range);
            }

            List<HexCoord> candidates = ResolveBaseFilter(grid, caster, all, filter, range, targetTeam);
            return ApplySorts(candidates, grid, caster, all, sorts, radius, targetTeam);
        }

        private static List<HexCoord> ResolveBaseFilter(
            HexGrid grid, Combatant caster, IReadOnlyList<Combatant> all,
            TargetBaseFilter filter, int range, TargetTeam targetTeam)
        {
            switch (filter)
            {
                case TargetBaseFilter.Adjacent:
                    return HexCoord.GetNeighbors(caster.Position, HexGrid.Cols, HexGrid.Rows)
                        .Where(h => targetTeam == TargetTeam.Ally
                            ? IsAllyOccupied(h, caster, all)
                            : IsEnemyOccupied(h, caster, all))
                        .ToList();

                case TargetBaseFilter.WithinRange:
                    return targetTeam == TargetTeam.Ally
                        ? AllyHexesInRange(grid, caster, all, range)
                        : EnemyHexesInRange(grid, caster, all, range);

                case TargetBaseFilter.Global:
                    return all
                        .Where(c => !c.IsDefeated && (targetTeam == TargetTeam.Ally
                            ? c.IsPlayer == caster.IsPlayer    // own team (including self, per GDD)
                            : c.IsPlayer != caster.IsPlayer))  // opponent team
                        .Select(c => c.Position)
                        .ToList();

                default:
                    return new List<HexCoord>();
            }
        }

        private static List<HexCoord> EnemyHexesInRange(
            HexGrid grid, Combatant caster, IReadOnlyList<Combatant> all, int range)
        {
            var inRange = new HashSet<HexCoord>(grid.GetInRange(caster.Position, range));
            return all.Where(c => !c.IsDefeated && c.IsPlayer != caster.IsPlayer && inRange.Contains(c.Position))
                .Select(c => c.Position)
                .ToList();
        }

        private static List<HexCoord> AllyHexesInRange(
            HexGrid grid, Combatant caster, IReadOnlyList<Combatant> all, int range)
        {
            var inRange = new HashSet<HexCoord>(grid.GetInRange(caster.Position, range));
            return all.Where(c => !c.IsDefeated && c.IsPlayer == caster.IsPlayer && inRange.Contains(c.Position))
                .Select(c => c.Position)
                .ToList();
        }

        private static List<HexCoord> ApplySorts(
            List<HexCoord> candidates, HexGrid grid, Combatant caster,
            IReadOnlyList<Combatant> all, TargetPrioritySort[] sorts, int radius,
            TargetTeam targetTeam)
        {
            if (sorts == null) return candidates;
            foreach (var sort in sorts)
                candidates = ApplyPrioritySort(candidates, caster, all, sort, radius, targetTeam);
            return candidates;
        }

        private static List<HexCoord> ApplyPrioritySort(
            List<HexCoord> candidates, Combatant caster,
            IReadOnlyList<Combatant> all, TargetPrioritySort sort, int radius,
            TargetTeam targetTeam)
        {
            switch (sort)
            {
                case TargetPrioritySort.CurrentTarget:
                {
                    // GDD: "CurrentTarget — Enemy team only; has no ally equivalent."
                    if (targetTeam == TargetTeam.Ally) return candidates;
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
                    // Re-rank candidates by cluster density scoped to the correct team.
                    var teamPositions = all
                        .Where(c => !c.IsDefeated && (targetTeam == TargetTeam.Ally
                            ? c.IsPlayer == caster.IsPlayer
                            : c.IsPlayer != caster.IsPlayer))
                        .Select(c => c.Position);
                    return candidates
                        .OrderByDescending(h => teamPositions.Count(e => HexCoord.Distance(h, e) <= radius))
                        .ToList();
                }

                default:
                    return candidates;
            }
        }

        private static float HpPctAt(HexCoord hex, IReadOnlyList<Combatant> all)
        {
            var occupant = all.FirstOrDefault(c => !c.IsDefeated && c.Position.Equals(hex));
            if (occupant == null) return float.MaxValue;
            return (float)occupant.CurrentHP / occupant.MaxHP;
        }

        private static bool IsEnemyOccupied(HexCoord hex, Combatant caster, IReadOnlyList<Combatant> all) =>
            all.Any(c => !c.IsDefeated && c.IsPlayer != caster.IsPlayer && c.Position.Equals(hex));

        private static bool IsAllyOccupied(HexCoord hex, Combatant caster, IReadOnlyList<Combatant> all) =>
            all.Any(c => !c.IsDefeated && c.IsPlayer == caster.IsPlayer && c.Position.Equals(hex));
    }
}
