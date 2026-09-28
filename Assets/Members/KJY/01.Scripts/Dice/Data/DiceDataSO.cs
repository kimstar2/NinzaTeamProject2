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

        public bool CanUse(AgentAttackType attackType)
        {
            var skill = GetSkillDataStruct(attackType).SkillData;
            return skill != null && skill.IsSuitable(attackType);
        }

        public List<AgentAttackType> GetUsableTypes()
        {
            var types = new List<AgentAttackType>();
            if (SkillDataStructs == null) return types;
            foreach (var entry in SkillDataStructs)
                if (!types.Contains(entry.AgentAttackType) && CanUse(entry.AgentAttackType)) types.Add(entry.AgentAttackType);
            return types;
        }

        public int Strength
        {
            get
            {
                int strength = 0;
                if (SkillDataStructs == null) return strength;
                foreach (var entry in SkillDataStructs)
                    if (entry.SkillData != null && entry.SkillData.IsSuitable(entry.AgentAttackType))
                        strength = Mathf.Max(strength, entry.SkillData.Strength);
                return strength;
            }
        }

        public string GetDescription(float level)
        {
            var description = new StringBuilder(Description);
            if (SkillDataStructs == null) return description.ToString();

            var written = new List<SkillDataSO>();
            foreach (var entry in SkillDataStructs)
            {
                var skill = entry.SkillData;
                if (skill == null || written.Contains(skill)) continue;
                var roles = new List<string>();
                foreach (var other in SkillDataStructs)
                    if (other.SkillData == skill && skill.IsSuitable(other.AgentAttackType))
                        roles.Add(SkillDataSO.RoleName(other.AgentAttackType));
                if (roles.Count == 0) continue;
                written.Add(skill);
                if (description.Length > 0) description.AppendLine().AppendLine();
                if (skill.SkillName == MainName) description.Append("사용 직업: ").AppendLine(string.Join("·", roles));
                else description.Append('[').Append(string.Join("·", roles)).Append("] ").AppendLine(skill.SkillName);
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
