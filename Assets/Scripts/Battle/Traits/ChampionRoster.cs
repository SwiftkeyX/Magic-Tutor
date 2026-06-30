using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicSchool.Battle
{
    public class ChampionRoster : MonoBehaviour
    {
        private static readonly List<ChampionData> _all = new List<ChampionData>
        {
            // TFT-aligned stats. AttackSpeed = attacks/second (float accumulator, 0.1s tick).
            // MaxMana: Tanks 100-150, Supports 80, Spam Carries 40-60.
            new ChampionData { Id="ironclad",         DisplayName="Ironclad",          Cost=1, Role=ChampionRole.Tank,    VerticalTrait=VerticalTrait.Vanguard,     HorizontalTrait=HorizontalTrait.Warden,      MaxHP=650,  ATK=50, DEF=45, MG=0,  MR=45, AttackSpeed=0.60f, CRIT=2,  Range=1, MaxMana=100, StartingMana=0   },
            new ChampionData { Id="bloodhound",        DisplayName="Bloodhound",         Cost=1, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Striker,      HorizontalTrait=HorizontalTrait.Dreadknight, MaxHP=500,  ATK=55, DEF=20, MG=0,  MR=20, AttackSpeed=0.75f, CRIT=12, Range=1, MaxMana=50,  StartingMana=0   },
            new ChampionData { Id="pyromancer",        DisplayName="Pyromancer",         Cost=1, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Elementalist, HorizontalTrait=HorizontalTrait.Kinetic,     MaxHP=480,  ATK=40, DEF=20, MG=65, MR=20, AttackSpeed=0.70f, CRIT=8,  Range=2, MaxMana=50,  StartingMana=0   },
            new ChampionData { Id="windrunner",        DisplayName="Windrunner",         Cost=2, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Ranger,       HorizontalTrait=HorizontalTrait.Kinetic,     MaxHP=600,  ATK=65, DEF=20, MG=0,  MR=20, AttackSpeed=0.80f, CRIT=14, Range=3, MaxMana=50,  StartingMana=0   },
            new ChampionData { Id="grovekeeper",       DisplayName="Grove Keeper",       Cost=2, Role=ChampionRole.Support, VerticalTrait=VerticalTrait.Elementalist, HorizontalTrait=HorizontalTrait.Warden,      MaxHP=600,  ATK=40, DEF=25, MG=55, MR=25, AttackSpeed=0.65f, CRIT=4,  Range=2, MaxMana=80,  StartingMana=0   },
            new ChampionData { Id="shadowblade",       DisplayName="Shadowblade",        Cost=2, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Striker,      HorizontalTrait=HorizontalTrait.Trickster,   MaxHP=580,  ATK=65, DEF=25, MG=0,  MR=25, AttackSpeed=0.80f, CRIT=20, Range=1, MaxMana=40,  StartingMana=0   },
            new ChampionData { Id="phalanx",           DisplayName="Phalanx",            Cost=3, Role=ChampionRole.Tank,    VerticalTrait=VerticalTrait.Vanguard,     HorizontalTrait=HorizontalTrait.Dreadknight, MaxHP=800,  ATK=60, DEF=50, MG=0,  MR=50, AttackSpeed=0.60f, CRIT=3,  Range=1, MaxMana=120, StartingMana=30  },
            new ChampionData { Id="stormbringer",      DisplayName="Stormbringer",       Cost=3, Role=ChampionRole.Support, VerticalTrait=VerticalTrait.Ranger,       HorizontalTrait=HorizontalTrait.Warden,      MaxHP=700,  ATK=50, DEF=25, MG=65, MR=25, AttackSpeed=0.70f, CRIT=7,  Range=3, MaxMana=80,  StartingMana=0   },
            new ChampionData { Id="phantomassassin",   DisplayName="Phantom Assassin",   Cost=4, Role=ChampionRole.Carry,   VerticalTrait=VerticalTrait.Elementalist, HorizontalTrait=HorizontalTrait.Trickster,   MaxHP=750,  ATK=65, DEF=30, MG=90, MR=30, AttackSpeed=0.80f, CRIT=22, Range=2, MaxMana=60,  StartingMana=0   },
            new ChampionData { Id="dreadoverlord",     DisplayName="Dread Overlord",     Cost=5, Role=ChampionRole.Tank,    VerticalTrait=VerticalTrait.Vanguard,     HorizontalTrait=HorizontalTrait.Trickster,   MaxHP=1000, ATK=80, DEF=60, MG=0,  MR=60, AttackSpeed=0.65f, CRIT=4,  Range=1, MaxMana=150, StartingMana=50  },
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
