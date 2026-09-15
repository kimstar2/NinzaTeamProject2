using Members.PSW.Code.Unit_Logic.Runtime.Structs;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime
{
    public enum UnitType
    {
        Player,
        Enemy
    }
    
    [CreateAssetMenu(fileName = "Unit Data", menuName = "Lumen/Unit/Unit Data", order = 0)]
    public class UnitDataSO : ScriptableObject
    {
        public new string name;
        public UnitType unitType;
        public int maxHealth;
        public CalculateStat baseStat;
    }
}