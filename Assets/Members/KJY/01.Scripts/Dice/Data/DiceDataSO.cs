using System;
using System.Collections.Generic;
using System.Text;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Agent.SkillSystem;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Data
{
    [CreateAssetMenu(fileName = "Dice data", menuName = "KJY/Game/Dice/Dice data", order = 0)]
    public class DiceDataSO : ScriptableObject
    {
        [field: Header("Basic Data")]
        [field: SerializeField] public string MainName { get; private set; }
        [field: SerializeField, TextArea(2, 4)] public string Description { get; private set; }
        [field: SerializeField] public DiceGradeSO DiceGrade {get; private set;}
        [field: SerializeField] public Sprite Icon { get; private set; }

        [field: Header("Dice Data")]
        [field: SerializeField] public List<SkillDataStruct> SkillDataStructs { get; private set; } = new();

        public SkillDataStruct GetSkillDataStruct(AgentAttackType agentAttackType)
        {
            if (SkillDataStructs == null) return default;
            foreach (var entry in SkillDataStructs)
                if (entry.AgentAttackType == agentAttackType) return entry;
            return default;
        }

        public string GetDescription(float level)
        {
            var description = new StringBuilder(Description);
            if (SkillDataStructs == null) return description.ToString();

            foreach (var entry in SkillDataStructs)
            {
                var skill = entry.SkillData;
                if (skill == null) continue;
                if (description.Length > 0) description.AppendLine().AppendLine();

                string role = entry.AgentAttackType switch
                {
                    AgentAttackType.Archer => "궁수",
                    AgentAttackType.Melee => "전사",
                    AgentAttackType.Magic => "마법사",
                    AgentAttackType.Healer => "힐러",
                    _ => entry.AgentAttackType.ToString()
                };
                description.Append('[').Append(role).Append("] ").AppendLine(skill.SkillName);
                description.Append(skill.GetDescription(level));
            }
            return description.ToString();
        }

        public Sprite GetIcon(AgentAttackType attackType)
        {
            var skill = GetSkillDataStruct(attackType).SkillData;
            return skill != null && skill.Icon != null ? skill.Icon : Icon;
        }
    }

    [Serializable]
    public struct SkillDataStruct
    {
        [field: SerializeField] public AgentAttackType AgentAttackType {get; private set;}
        [field: SerializeField] public SkillDataSO SkillData {get; private set;}
        [field: SerializeField] public int SkillLevel {get; private set;}
        [field: SerializeField] public int BaseDamage {get; private set;}
    }
}
