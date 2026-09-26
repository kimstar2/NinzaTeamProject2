using System;
using Members.KJY._01.Scripts.Agent.Enemy;
using UnityEngine;

namespace Members.KJY._01.Scripts.GameSystem.EnemyParty
{
    [Serializable]
    public struct EnemyParty
    {
        [field:SerializeField] public EnemyPartyType EnemyPartyType {get; private set;}
        [field:SerializeField] public EnemyDataSO[] EnemyDataList {get; private set;}
    }
}
