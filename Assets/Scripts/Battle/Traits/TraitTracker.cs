using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool.Battle
{
    public class TraitTracker : MonoBehaviour
    {
        private static readonly Dictionary<TraitType, int[]> Breakpoints = new Dictionary<TraitType, int[]>
        {
            { TraitType.Vanguard,     new[] { 2, 4, 6, 8 } },
            { TraitType.Striker,      new[] { 2, 4, 6, 8 } },
            { TraitType.Elementalist, new[] { 2, 4, 6, 8 } },
            { TraitType.Ranger,       new[] { 2, 4, 6    } },
            { TraitType.Kinetic,      new[] { 2, 4       } },
            { TraitType.Dreadknight,  new[] { 2, 4       } },
            { TraitType.Warden,       new[] { 2, 3, 4    } },
            { TraitType.Trickster,    new[] { 2, 4       } },
        };

        private readonly Dictionary<string, ChampionData> _placed           = new Dictionary<string, ChampionData>();
        private readonly Dictionary<TraitType, int>       _counts            = new Dictionary<TraitType, int>();
        private readonly Dictionary<TraitType, int>       _activeBreakpoints = new Dictionary<TraitType, int>();

        public event Action<IReadOnlyDictionary<TraitType, int>, IReadOnlyDictionary<TraitType, int>> OnTraitCountsChanged;

        public void RegisterPlacement(string championId, HexCoord coord, ChampionData data)
        {
            _placed[championId] = data;
            Recalculate();
        }

        public void UnregisterPlacement(string championId)
        {
            _placed.Remove(championId);
            Recalculate();
        }

        public IReadOnlyDictionary<TraitType, int> GetTraitCounts()       => _counts;
        public IReadOnlyDictionary<TraitType, int> GetActiveBreakpoints() => _activeBreakpoints;

        private void Recalculate()
        {
            _counts.Clear();
            foreach (var data in _placed.Values)
            {
                if (data.VerticalTrait != VerticalTrait.None)
                    Increment(VerticalToTraitType(data.VerticalTrait));
                if (data.HorizontalTrait != HorizontalTrait.None)
                    Increment(HorizontalToTraitType(data.HorizontalTrait));
            }

            _activeBreakpoints.Clear();
            foreach (var kv in _counts)
            {
                int bp = HighestBreakpoint(kv.Key, kv.Value);
                if (bp > 0) _activeBreakpoints[kv.Key] = bp;
            }

            OnTraitCountsChanged?.Invoke(_counts, _activeBreakpoints);
        }

        private void Increment(TraitType t)
        {
            _counts.TryGetValue(t, out int cur);
            _counts[t] = cur + 1;
        }

        private static int HighestBreakpoint(TraitType t, int count)
        {
            if (!Breakpoints.TryGetValue(t, out var pts)) return 0;
            int best = 0;
            foreach (int pt in pts) if (count >= pt) best = pt;
            return best;
        }

        private static TraitType VerticalToTraitType(VerticalTrait v)
        {
            switch (v)
            {
                case VerticalTrait.Vanguard:     return TraitType.Vanguard;
                case VerticalTrait.Striker:      return TraitType.Striker;
                case VerticalTrait.Elementalist: return TraitType.Elementalist;
                case VerticalTrait.Ranger:       return TraitType.Ranger;
                default: throw new ArgumentOutOfRangeException(nameof(v));
            }
        }

        private static TraitType HorizontalToTraitType(HorizontalTrait h)
        {
            switch (h)
            {
                case HorizontalTrait.Kinetic:     return TraitType.Kinetic;
                case HorizontalTrait.Dreadknight: return TraitType.Dreadknight;
                case HorizontalTrait.Warden:      return TraitType.Warden;
                case HorizontalTrait.Trickster:   return TraitType.Trickster;
                default: throw new ArgumentOutOfRangeException(nameof(h));
            }
        }
    }
}
