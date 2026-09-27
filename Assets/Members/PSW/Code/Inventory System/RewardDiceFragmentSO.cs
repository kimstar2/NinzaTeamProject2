using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Agent.SkillSystem;
using UnityEngine;

namespace Members.PSW.Code.InventorySystem
{
    // A runtime reward fits the existing LYW list while retaining the full combat data.
    public sealed class RewardDiceFragmentSO : ScriptableObject
    {
        public DiceDataSO DiceData { get; private set; }
        public float Level { get; private set; }

        public void Initialize(DiceDataSO diceData, float level)
        {
            DiceData = diceData;
            Level = level;
            name = diceData.name;
        }
    }
}
