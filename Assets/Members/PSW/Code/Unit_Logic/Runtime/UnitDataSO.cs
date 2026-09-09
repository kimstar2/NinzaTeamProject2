using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime
{
    [CreateAssetMenu(fileName = "Unit Data", menuName = "Lumen/Unit/Data", order = 0)]
    public class UnitDataSO : ScriptableObject
    {
        public int maxHealth;
        public CalculateStat baseStat;
    }
}