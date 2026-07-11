using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicSchool.Battle
{
    // Right-docked hero inspector. Never referenced by BattleBoardManager and never
    // references it back — resolves its own data via ChampionRoster/TraitTracker and
    // listens for selections via the decoupled HeroSelection static event.
    [RequireComponent(typeof(UIDocument))]
    public class HeroInfoPanel : MonoBehaviour
    {
        [SerializeField] private ChampionRoster    _championRoster;
        [SerializeField] private EnemyDatabaseStub _enemyDatabase;

        private static readonly Dictionary<BattleBehaviorFlag, string> FlagDescriptions =
            new Dictionary<BattleBehaviorFlag, string>
            {
                { BattleBehaviorFlag.MagicAttack,        "Uses Magic power (MG/MR) instead of Attack/Defense" },
                { BattleBehaviorFlag.AOEAttack,          "Hits all enemies in range" },
                { BattleBehaviorFlag.FirstHitDouble,     "First attack against a new target deals double damage" },
                { BattleBehaviorFlag.TakesReducedDamage, "Takes reduced incoming damage" },
                { BattleBehaviorFlag.ShadowSurge,        "Gains a burst effect under specific conditions" },
            };

        private UIDocument    _document;
        private VisualElement _root;
        private Label _headerText;
        private Label _statsText;
        private Label _skillText;
        private Label _traitsText;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            _document.sortingOrder = BattleUISortOrder.HeroInfoPanel;
            _root = _document.rootVisualElement;

            // Cache element references — never call Q<> outside Awake
            _headerText = _root.Q<Label>("header");
            _statsText  = _root.Q<Label>("stats");
            _skillText  = _root.Q<Label>("skill");
            _traitsText = _root.Q<Label>("traits");

            ShowPlaceholder();
        }

        private void OnEnable()  { HeroSelection.OnHeroSelected += OnHeroSelected; }
        private void OnDisable() { HeroSelection.OnHeroSelected -= OnHeroSelected; }

        private void OnHeroSelected(string championId)
        {
            var data = _championRoster != null ? _championRoster.GetChampionById(championId) : null;
            if (data != null) { ShowHero(data); return; }

            var enemy = _enemyDatabase != null ? _enemyDatabase.GetEnemyById(championId) : null;
            if (enemy != null) { ShowEnemy(enemy); return; }

            // Neither a player champion nor a known enemy — a genuine miss, not a normal
            // empty state. Restore the placeholder and log loudly rather than leaving
            // stale content on screen (see BattleHUD.md, Hero Info Panel section).
            Debug.LogWarning($"[HeroInfoPanel] No champion or enemy found for id '{championId}'.");
            ShowPlaceholder();
        }

        private void ShowPlaceholder()
        {
            _headerText.text = "No hero selected";
            _statsText.text  = "";
            _skillText.text  = "";
            _traitsText.text = "Click a bench card or a placed hero to inspect it.";
        }

        private string GetSkillDescriptionText(SkillDefinition skill, int maxHp, int atk, int def, int mg)
        {
            if (skill == null || skill.Archetype == SkillArchetype.None)
                return null;

            string scalingStat = skill.UsesMagicOffense ? "MG" : "ATK";
            float baseStat = skill.UsesMagicOffense ? mg : atk;
            int dmg = (int)(baseStat * skill.OffenseMultiplier);
            string desc = "";

            switch (skill.Archetype)
            {
                case SkillArchetype.StandardProjectile:
                    if (skill.TargetTeam == TargetTeam.Ally)
                    {
                        int heal = (int)(mg * skill.HealMultiplier);
                        desc = $"Heals lowest HP% ally for {heal} ({(int)(skill.HealMultiplier * 100)}% MG)";
                        if (skill.FlatShieldAmount > 0)
                            desc += $" and shields them for {skill.FlatShieldAmount} HP";
                    }
                    else
                    {
                        desc = $"Fires a projectile dealing {dmg} ({(int)(skill.OffenseMultiplier * 100)}% {scalingStat}) damage.";
                    }
                    break;
                case SkillArchetype.ExplodingProjectile:
                    desc = $"Fires an exploding projectile, dealing {dmg} ({(int)(skill.OffenseMultiplier * 100)}% {scalingStat}) damage in a {skill.Radius}-hex radius.";
                    break;
                case SkillArchetype.LaserBeam:
                    desc = $"Fires a linear piercing beam, dealing {dmg} ({(int)(skill.OffenseMultiplier * 100)}% {scalingStat}) damage to all enemies hit.";
                    break;
                case SkillArchetype.GroundAoE:
                    desc = $"Drops a ground hazard on the largest enemy cluster, dealing {dmg} ({(int)(skill.OffenseMultiplier * 100)}% {scalingStat}) damage.";
                    if (skill.CrowdControl == CcType.Stun)
                        desc += $" Stuns hit targets for {skill.DurationTicks} ticks.";
                    else if (skill.CrowdControl == CcType.Root)
                        desc += $" Roots hit targets for {skill.DurationTicks} ticks.";
                    break;
                case SkillArchetype.BlinkStrike:
                    desc = $"Blinks to the lowest HP% enemy and strikes for {dmg} ({(int)(skill.OffenseMultiplier * 100)}% {scalingStat}) damage.";
                    break;
                case SkillArchetype.BouncingChain:
                    desc = $"Fires a chain bouncing up to {skill.BounceCount} times, dealing {dmg} ({(int)(skill.OffenseMultiplier * 100)}% {scalingStat}) damage.";
                    if (skill.CrowdControl == CcType.Silence)
                        desc += $" Silences hit targets for {skill.DurationTicks} ticks.";
                    break;
                case SkillArchetype.SelfBuff:
                    int shield = (int)(def * skill.ShieldDefMultiplier + skill.FlatShieldAmount + maxHp * skill.ShieldPctOfMaxHP);
                    desc = $"Gains {shield} shield for {skill.DurationTicks} ticks.";
                    if (skill.CrowdControl == CcType.Taunt)
                        desc += " Taunts adjacent enemies.";
                    if (skill.HealMultiplier > 0)
                        desc += $" Heals adjacent allies for {(int)(mg * skill.HealMultiplier)} HP.";
                    break;
            }

            if (desc.Length > 0)
                desc = char.ToUpper(desc[0]) + desc.Substring(1);

            return $"Skill: {skill.SkillName}\n{desc}";
        }

        private void ShowHero(ChampionData data)
        {
            _headerText.text = $"{data.DisplayName}  ({data.Role}, Cost {data.Cost})";

            _statsText.text =
                $"HP {data.MaxHP}   ATK {data.ATK}   DEF {data.DEF}\n" +
                $"MG {data.MG}   MR {data.MR}   CRIT {data.CRIT}%\n" +
                $"AS {data.AttackSpeed:0.00}   Range {data.Range}\n" +
                $"Mana {data.StartingMana}/{data.MaxMana}";

            string skillDesc = GetSkillDescriptionText(data.Skill, data.MaxHP, data.ATK, data.DEF, data.MG);
            if (skillDesc != null)
            {
                _skillText.text = skillDesc;
            }
            else
            {
                var flags = data.ToStudentCombatData().Flags;
                _skillText.text = "Skill: " + (flags.Count == 0
                    ? "No active skill"
                    : string.Join("\n", flags.Select(f =>
                        FlagDescriptions.TryGetValue(f, out var desc) ? $"{f} — {desc}" : f.ToString())));
            }

            _traitsText.text = "Traits: " + FormatTraits(data);
        }

        // Mirrors ShowHero — EnemyCombatData has no Role/Cost/VerticalTrait/
        // HorizontalTrait, so the header omits those and Traits is a fixed "None"
        // rather than calling FormatTraits (which is ChampionData-specific).
        private void ShowEnemy(EnemyCombatData data)
        {
            _headerText.text = $"{data.DisplayName}  (Enemy)";

            _statsText.text =
                $"HP {data.MaxHP}   ATK {data.ATK}   DEF {data.DEF}\n" +
                $"MG {data.MG}   MR {data.MR}   CRIT {data.CRIT}%\n" +
                $"AS {data.AttackSpeed:0.00}   Range {data.Range}\n" +
                $"Mana {data.StartingMana}/{data.MaxMana}";

            string skillDesc = GetSkillDescriptionText(data.Skill, data.MaxHP, data.ATK, data.DEF, data.MG);
            if (skillDesc != null)
            {
                _skillText.text = skillDesc;
            }
            else
            {
                var flags = data.Flags ?? new List<BattleBehaviorFlag>();
                _skillText.text = "Skill: " + (flags.Count == 0
                    ? "No active skill"
                    : string.Join("\n", flags.Select(f =>
                        FlagDescriptions.TryGetValue(f, out var desc) ? $"{f} — {desc}" : f.ToString())));
            }

            _traitsText.text = "Traits: None";
        }

        private string FormatTraits(ChampionData data)
        {
            var parts = new List<string>();
            AppendTrait(parts, data.VerticalTrait   != VerticalTrait.None   ? (TraitType?)ToTraitType(data.VerticalTrait)   : null);
            AppendTrait(parts, data.HorizontalTrait != HorizontalTrait.None ? (TraitType?)ToTraitType(data.HorizontalTrait) : null);

            return parts.Count == 0 ? "None" : string.Join(", ", parts);
        }

        private static void AppendTrait(List<string> parts, TraitType? trait)
        {
            if (trait == null) return;
            parts.Add(trait.Value.ToString());
        }

        private static TraitType ToTraitType(VerticalTrait v) => v switch
        {
            VerticalTrait.Vanguard     => TraitType.Vanguard,
            VerticalTrait.Striker      => TraitType.Striker,
            VerticalTrait.Elementalist => TraitType.Elementalist,
            VerticalTrait.Ranger       => TraitType.Ranger,
            VerticalTrait.Astral       => TraitType.Astral,
            VerticalTrait.Wild         => TraitType.Wild,
            VerticalTrait.Shadow       => TraitType.Shadow,
            _ => default,
        };

        private static TraitType ToTraitType(HorizontalTrait h) => h switch
        {
            HorizontalTrait.Kinetic     => TraitType.Kinetic,
            HorizontalTrait.Dreadknight => TraitType.Dreadknight,
            HorizontalTrait.Warden      => TraitType.Warden,
            HorizontalTrait.Trickster   => TraitType.Trickster,
            HorizontalTrait.Oracle      => TraitType.Oracle,
            HorizontalTrait.Guardian    => TraitType.Guardian,
            HorizontalTrait.Tech        => TraitType.Tech,
            HorizontalTrait.Void        => TraitType.Void,
            _ => default,
        };
    }
}
