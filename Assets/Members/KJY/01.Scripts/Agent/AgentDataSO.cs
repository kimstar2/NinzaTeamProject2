using UnityEngine;

namespace Members.KJY._01.Scripts.Agent
{
    public abstract class AgentDataSO : ScriptableObject
    {
        [field:SerializeField] public AgentAttackType AttackType {get; private set;}
    }
}