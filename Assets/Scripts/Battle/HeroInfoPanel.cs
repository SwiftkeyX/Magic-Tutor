using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MagicSchool.Battle
{
    // Right-docked hero inspector. Never referenced by BattleBoardManager and never
    // references it back — resolves its own data via ChampionRoster/TraitTracker and
    // listens for selections via the decoupled HeroSelection static event.
    public class HeroInfoPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform  _root;
        [SerializeField] private ChampionRoster _championRoster;
        [SerializeField] private TraitTracker   _traitTracker;

        private static readonly Dictionary<BattleBehaviorFlag, string> FlagDescriptions =
            new Dictionary<BattleBehaviorFlag, string>
            {
                { BattleBehaviorFlag.MagicAttack,        "Uses Magic power (MG/MR) instead of Attack/Defense" },
                { BattleBehaviorFlag.AOEAttack,          "Hits all enemies in range" },
                { BattleBehaviorFlag.FirstHitDouble,     "First attack against a new target deals double damage" },
                { BattleBehaviorFlag.TakesReducedDamage, "Takes reduced incoming damage" },
                { BattleBehaviorFlag.ShadowSurge,        "Gains a burst effect under specific conditions" },
            };

        private Text _headerText;
        private Text _statsText;
        private Text _skillText;
        private Text _traitsText;

        private void Awake()
        {
            BuildLayout();
            ShowPlaceholder();
        }

        private void OnEnable()  { HeroSelection.OnHeroSelected += OnHeroSelected; }
        private void OnDisable() { HeroSelection.OnHeroSelected -= OnHeroSelected; }

        private void OnHeroSelected(string championId)
        {
            if (_championRoster == null) return;
            var data = _championRoster.GetChampionById(championId);
            if (data == null) return; // not a player champion (e.g. enemy click) — leave panel as-is
            ShowHero(data);
        }

        private void BuildLayout()
        {
            if (_root == null) return;

            var lg = _root.GetComponent<VerticalLayoutGroup>();
            if (lg == null)
            {
                lg = _root.gameObject.AddComponent<VerticalLayoutGroup>();
                lg.spacing                = 6f;
                lg.childAlignment         = TextAnchor.UpperLeft;
                lg.childForceExpandWidth  = true;
                lg.childForceExpandHeight = false;
                lg.padding                = new RectOffset(8, 8, 8, 8);
            }

            _headerText = CreateText("Header", 16, FontStyle.Bold);
            _statsText  = CreateText("Stats", 13, FontStyle.Normal);
            _skillText  = CreateText("Skill", 13, FontStyle.Normal);
            _traitsText = CreateText("Traits", 13, FontStyle.Normal);
        }

        private Text CreateText(string name, int fontSize, FontStyle style)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root, false);
            var txt       = go.AddComponent<Text>();
            txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize  = fontSize;
            txt.fontStyle = style;
            txt.color     = Color.white;
            var csf = go.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return txt;
        }

        private void ShowPlaceholder()
        {
            _headerText.text = "No hero selected";
            _statsText.text  = "";
            _skillText.text  = "";
            _traitsText.text = "Click a bench card or a placed hero to inspect it.";
        }

        private void ShowHero(ChampionData data)
        {
            _headerText.text = $"{data.DisplayName}  ({data.Role}, Cost {data.Cost})";

            _statsText.text =
                $"HP {data.MaxHP}   ATK {data.ATK}   DEF {data.DEF}\n" +
                $"MG {data.MG}   MR {data.MR}   CRIT {data.CRIT}%\n" +
                $"AS {data.AttackSpeed:0.00}   Range {data.Range}\n" +
                $"Mana {data.StartingMana}/{data.MaxMana}";

            var flags = data.ToStudentCombatData().Flags;
            _skillText.text = "Skill: " + (flags.Count == 0
                ? "No active skill"
                : string.Join("\n", flags.Select(f =>
                    FlagDescriptions.TryGetValue(f, out var desc) ? $"{f} — {desc}" : f.ToString())));

            _traitsText.text = "Traits: " + FormatTraits(data);
        }

        private string FormatTraits(ChampionData data)
        {
            var counts     = _traitTracker != null ? _traitTracker.GetTraitCounts()       : null;
            var breakpoints = _traitTracker != null ? _traitTracker.GetActiveBreakpoints() : null;

            var parts = new List<string>();
            AppendTrait(parts, data.VerticalTrait   != VerticalTrait.None   ? (TraitType?)ToTraitType(data.VerticalTrait)   : null, counts, breakpoints);
            AppendTrait(parts, data.HorizontalTrait != HorizontalTrait.None ? (TraitType?)ToTraitType(data.HorizontalTrait) : null, counts, breakpoints);

            return parts.Count == 0 ? "None" : string.Join(", ", parts);
        }

        private static void AppendTrait(
            List<string> parts, TraitType? trait,
            IReadOnlyDictionary<TraitType, int> counts,
            IReadOnlyDictionary<TraitType, int> breakpoints)
        {
            if (trait == null) return;
            int count = counts != null && counts.TryGetValue(trait.Value, out var c) ? c : 0;
            string active = breakpoints != null && breakpoints.TryGetValue(trait.Value, out var bp) ? bp.ToString() : "-";
            parts.Add($"{trait} {count}/{active}");
        }

        private static TraitType ToTraitType(VerticalTrait v) => v switch
        {
            VerticalTrait.Vanguard     => TraitType.Vanguard,
            VerticalTrait.Striker      => TraitType.Striker,
            VerticalTrait.Elementalist => TraitType.Elementalist,
            VerticalTrait.Ranger       => TraitType.Ranger,
            _ => default,
        };

        private static TraitType ToTraitType(HorizontalTrait h) => h switch
        {
            HorizontalTrait.Kinetic     => TraitType.Kinetic,
            HorizontalTrait.Dreadknight => TraitType.Dreadknight,
            HorizontalTrait.Warden      => TraitType.Warden,
            HorizontalTrait.Trickster   => TraitType.Trickster,
            _ => default,
        };
    }
}
