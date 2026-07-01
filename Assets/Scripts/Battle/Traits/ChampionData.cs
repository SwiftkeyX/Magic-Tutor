using System.Collections.Generic;

namespace MagicSchool.Battle
{
    [System.Serializable]
    public class ChampionData
    {
        public string          Id;
        public string          DisplayName;
        public int             Cost;
        public ChampionRole    Role;
        public VerticalTrait   VerticalTrait;
        public HorizontalTrait HorizontalTrait;
        public int   MaxHP, ATK, DEF, MG, MR, CRIT, Range;
        public float AttackSpeed;    // attacks per second (TFT-style AS)
        public int   MaxMana     = 100;
        public int   StartingMana = 0;
        public SkillDefinition Skill = new SkillDefinition();

        public StudentCombatData ToStudentCombatData()
        {
            var flags = new List<BattleBehaviorFlag>();
            bool isMagicUser = VerticalTrait == VerticalTrait.Elementalist
                            || (Role == ChampionRole.Support && MG > ATK);
            if (isMagicUser) flags.Add(BattleBehaviorFlag.MagicAttack);

            return new StudentCombatData
            {
                Id = Id, DisplayName = DisplayName,
                MaxHP = MaxHP, ATK = ATK, DEF = DEF,
                MG = MG, MR = MR, AttackSpeed = AttackSpeed, CRIT = CRIT,
                Range = Range, MaxMana = MaxMana, StartingMana = StartingMana,
                Flags = flags, Skill = Skill
            };
        }
    }
}
