using System;
using System.Collections.Generic;
using Members.KJY._01.Scripts.Agent.Enemy;
using UnityEngine;

namespace Members.KJY._01.Scripts.GameSystem.EnemyParty
{
    [Serializable]
    public struct EnemyParty
    {
        [field:SerializeField] public EnemyPartyType EnemyPartyType {get; private set;}
        [field:SerializeField] public EnemyDataSO[] EnemyDataList {get; private set;}
        [field:SerializeField] public string StageName {get; private set;}
        [field:SerializeField] public EnemyDataSO Boss {get; private set;}
        [field:SerializeField, Range(1f, 2f)] public float LateHealthMultiplier {get; private set;}
        [field:SerializeField, Range(1f, 1.5f)] public float LateDicePower {get; private set;}
        [field:SerializeField, Min(0)] public int BaseGoldReward {get; private set;}
    }
}
