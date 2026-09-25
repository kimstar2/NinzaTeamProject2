using System;
using System.Collections.Generic;
using System.Linq;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Agent.SkillSystem;
using Members.PSW.Code.Test;
using UnityEngine;
using ZLinq;

namespace Members.KJY._01.Scripts.Dice.Data
{
    [CreateAssetMenu(fileName = "Dice data", menuName = "KJY/Game/Dice/Dice data", order = 0)]
    public class DiceDataSO : ScriptableObject
    {
        [field: Header("Basic Data")]
        [field: SerializeField] public DiceGradeSO DiceGrade {get; private set;}
        [field: SerializeField] public Sprite Icon { get; private set; }

        [field: Header("Dice Data")]
        [field:SerializeField] public List<SkillDataStruct> SkillDataStructs { get; private set;}

        public SkillDataStruct GetSkillDataStruct(AgentAttackType agentAttackType)
        {
            var skillDataStruct = SkillDataStructs
                .AsValueEnumerable()
                .Where(s=>s.AgentAttackType == agentAttackType)
                .Select(s=>s)
                .FirstOrDefault();
            return skillDataStruct;
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
