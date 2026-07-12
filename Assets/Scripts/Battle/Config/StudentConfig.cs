using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool.Battle
{
    [CreateAssetMenu(fileName = "StudentConfig", menuName = "MagicSchool/StudentConfig")]
    public class StudentConfig : ScriptableObject
    {
        public int RecruitCountPerSemester = 5;
        public int MaxSquadSize = 3;
        public Vector2Int BaseHPRange = new Vector2Int(40, 80);
        public Vector2Int BaseATKRange = new Vector2Int(5, 15);
        public Vector2Int BaseDEFRange = new Vector2Int(0, 10);
        public Vector2Int BaseMGRange = new Vector2Int(0, 10);
        public Vector2Int BaseMRRange = new Vector2Int(0, 10);
        public Vector2Int BaseSPDRange = new Vector2Int(3, 8);
        public Vector2Int BaseCRITRange = new Vector2Int(0, 10);
        public Vector2Int TraitsPerStudent = new Vector2Int(1, 2);
        
        public List<TraitType> AvailableTraits = new List<TraitType>
        {
            TraitType.Vanguard, TraitType.Striker, TraitType.Elementalist, TraitType.Ranger,
            TraitType.Kinetic, TraitType.Dreadknight, TraitType.Warden, TraitType.Trickster
        };
        
        public List<string> NamePool = new List<string>
        {
            "Alaric", "Bartholomew", "Cedric", "Dorian", "Elric", "Finneas", "Gideon", "Hadrian", "Ignatius", "Julian",
            "Aurelia", "Beatrice", "Cassandra", "Diana", "Evelyn", "Fiona", "Genevieve", "Helena", "Iris", "Juliet",
            "Kaelen", "Lysander", "Myron", "Nesta", "Orion", "Percival", "Quentin", "Rowan", "Silas", "Tristan",
            "Valerius", "Zephyr", "Maeve", "Ophelia", "Penelope", "Rowena", "Seraphina", "Theodora", "Ursula", "Vivian"
        };
    }
}
