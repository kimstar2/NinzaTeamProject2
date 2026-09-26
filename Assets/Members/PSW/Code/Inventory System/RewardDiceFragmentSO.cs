using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Agent.SkillSystem;

namespace Members.PSW.Code.InventorySystem
{
    // A runtime reward fits the existing LYW list while retaining the full combat data.
    public sealed class RewardDiceFragmentSO : DiceFragmentSO
    {
        public DiceDataSO DiceData { get; private set; }
        public float Level { get; private set; }
        public SkillDataSO SkillData { get; private set; }

        public void Initialize(DiceDataSO diceData, float level, AgentAttackType? attackType = null)
        {
            DiceData = diceData;
            Level = level;
            if (attackType.HasValue)
                SkillData = diceData.GetSkillDataStruct(attackType.Value).SkillData;
            else if (diceData.SkillDataStructs != null && diceData.SkillDataStructs.Count == 1)
                SkillData = diceData.SkillDataStructs[0].SkillData;
            diceFragmentSprite = SkillData != null && SkillData.Icon != null ? SkillData.Icon : diceData.Icon;
            name = diceData.name;
        }
    }
}
