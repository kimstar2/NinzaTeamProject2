using System.Collections.Generic;
using UnityEngine;

namespace Members.KJY._01.Scripts.GameSystem.EnemyParty
{
    public enum EnemyPartyType
    {
        Stage1,
        Stage2,
        Stage3
    }

    [CreateAssetMenu(fileName = "EnemyPartyList", menuName = "KJY/System/EnemyPartyList", order = 0)]
    public class EnemyPartyListSO : ScriptableObject
    {
        [field:SerializeField] public List<EnemyParty> EnemyPartyList {get; private set;}
    }
}