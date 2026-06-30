using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicSchool.Battle
{
    public class ChampionRoster : MonoBehaviour
    {
        private static readonly List<ChampionData> _all = new List<ChampionData>
        {
            // AttackSpeed = attacks/second. ActionInterval = round(1 / (AS × 0.6s/tick))
            new ChampionData { Id="ironclad",         DisplayName="Ironclad",          Cost=1, Role=ChampionRole.Tank,    VerticalTrait=VerticalTrait.Vanguard,     HorizontalTrait=HorizontalTrait.Warden,      MaxHP=100, ATK=13, DEF=18, MG=4,  MR=12, AttackSpeed=0.21f, CRIT=2,  Range=1 },
            new ChampionData { Id="bloodhound",        DisplayName="Bloodhound",         Cost=1, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Striker,      HorizontalTrait=HorizontalTrait.Dreadknight, MaxHP=70,  ATK=18, DEF=5,  MG=0,  MR=5,  AttackSpeed=0.33f, CRIT=12, Range=1 },
            new ChampionData { Id="pyromancer",        DisplayName="Pyromancer",         Cost=1, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Elementalist, HorizontalTrait=HorizontalTrait.Kinetic,     MaxHP=60,  ATK=8,  DEF=3,  MG=20, MR=8,  AttackSpeed=0.28f, CRIT=8,  Range=2 },
            new ChampionData { Id="windrunner",        DisplayName="Windrunner",         Cost=2, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Ranger,       HorizontalTrait=HorizontalTrait.Kinetic,     MaxHP=75,  ATK=20, DEF=5,  MG=0,  MR=6,  AttackSpeed=0.42f, CRIT=14, Range=3 },
            new ChampionData { Id="grovekeeper",       DisplayName="Grove Keeper",       Cost=2, Role=ChampionRole.Support, VerticalTrait=VerticalTrait.Elementalist, HorizontalTrait=HorizontalTrait.Warden,      MaxHP=85,  ATK=7,  DEF=8,  MG=26, MR=14, AttackSpeed=0.24f, CRIT=4,  Range=2 },
            new ChampionData { Id="shadowblade",       DisplayName="Shadowblade",        Cost=2, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Striker,      HorizontalTrait=HorizontalTrait.Trickster,   MaxHP=65,  ATK=24, DEF=4,  MG=0,  MR=6,  AttackSpeed=0.56f, CRIT=20, Range=1 },
            new ChampionData { Id="phalanx",           DisplayName="Phalanx",            Cost=3, Role=ChampionRole.Tank,    VerticalTrait=VerticalTrait.Vanguard,     HorizontalTrait=HorizontalTrait.Dreadknight, MaxHP=145, ATK=16, DEF=26, MG=5,  MR=18, AttackSpeed=0.21f, CRIT=3,  Range=1 },
            new ChampionData { Id="stormbringer",      DisplayName="Stormbringer",       Cost=3, Role=ChampionRole.Support, VerticalTrait=VerticalTrait.Ranger,       HorizontalTrait=HorizontalTrait.Warden,      MaxHP=95,  ATK=18, DEF=8,  MG=30, MR=12, AttackSpeed=0.33f, CRIT=7,  Range=3 },
            new ChampionData { Id="phantomassassin",   DisplayName="Phantom Assassin",   Cost=4, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Elementalist, HorizontalTrait=HorizontalTrait.Trickster,   MaxHP=85,  ATK=15, DEF=6,  MG=42, MR=10, AttackSpeed=0.42f, CRIT=22, Range=2 },
            new ChampionData { Id="dreadoverlord",     DisplayName="Dread Overlord",     Cost=5, Role=ChampionRole.Tank,    VerticalTrait=VerticalTrait.Vanguard,     HorizontalTrait=HorizontalTrait.Trickster,   MaxHP=195, ATK=26, DEF=36, MG=8,  MR=26, AttackSpeed=0.24f, CRIT=4,  Range=1 },
        };

        public List<StudentCombatData> GetStudents()
            => _all.Select(c => c.ToStudentCombatData()).ToList();

        public ChampionData GetChampionById(string id)
            => _all.Find(c => c.Id == id);

        public Dictionary<string, ChampionData> GetChampionLookup()
        {
            var dict = new Dictionary<string, ChampionData>();
            foreach (var c in _all) dict[c.Id] = c;
            return dict;
        }
    }
}
