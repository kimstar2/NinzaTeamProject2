using Members.PSW.Code.Unit_Logic.Runtime.Structs;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime
{
    [CreateAssetMenu(fileName = "Unit Data", menuName = "Lumen/Unit/Data", order = 0)]
    public class UnitDataSO : ScriptableObject
    {
        public new string name;
        public int maxHealth;
        public CalculateStat baseStat;
    }
}