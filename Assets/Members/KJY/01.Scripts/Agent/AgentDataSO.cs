using System;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Agent.Player;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent
{
    public abstract class AgentDataSO : ScriptableObject
    {
        [field: SerializeField] public float MaxHealth { get; private set; }
        [field:SerializeField] public AgentAttackType AttackType {get; private set;}
    }
}