using System.Collections.Generic;

namespace MagicSchool.Battle
{
    /// <summary>
    /// Single source of truth for all trait breakpoint thresholds.
    /// Values must stay in sync with the GDD: .claude/docs/production/gdd/TraitSystem.md
    /// Referenced by TraitTracker (logic) and TraitHUDController (display).
    /// </summary>
    public static class TraitBreakpoints
    {
        public static readonly Dictionary<TraitType, int[]> All = new Dictionary<TraitType, int[]>
        {
            { TraitType.Vanguard,     new[] { 2, 4, 6, 8 } },
            { TraitType.Striker,      new[] { 2, 4, 6, 8 } },
            { TraitType.Elementalist, new[] { 2, 4, 6, 8 } },
            { TraitType.Ranger,       new[] { 2, 4, 6    } },
            { TraitType.Kinetic,      new[] { 2, 4       } },
            { TraitType.Dreadknight,  new[] { 2, 4       } },
            { TraitType.Warden,       new[] { 2, 3, 4    } },
            { TraitType.Trickster,    new[] { 2, 4       } },
            { TraitType.Astral,       new[] { 2, 4, 6    } },
            { TraitType.Wild,         new[] { 2, 4, 6    } },
            { TraitType.Shadow,       new[] { 2, 4, 6    } },
            { TraitType.Oracle,       new[] { 2, 3       } },
            { TraitType.Guardian,     new[] { 2, 4       } },
            { TraitType.Tech,         new[] { 2, 4       } },
            { TraitType.Void,         new[] { 2, 4       } },
        };
    }
}
