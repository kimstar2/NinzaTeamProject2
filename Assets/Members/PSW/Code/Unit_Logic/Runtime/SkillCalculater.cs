using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.PSW.Code.Test;
using Members.PSW.Code.Unit_Logic.Runtime.Skill;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime
{
    public class CalcValueEvent : GameEvent
    {
        public SkillType SkillType { get; private set; }
        public int Value { get; private set; }
        public GameObject Target { get; private set; }

        public CalcValueEvent(int value, SkillType skillType, GameObject target)
        {
            Value = value;
            SkillType = skillType;
            Target = target;
        }
    }
    
    public class SkillCalculater : MonoModule
    {
        [SerializeField] private EventChannelSO eventChannel;
        
        public void Calculate(CalculateStat currentStat, SkillSO skill, GameObject target)
        {
            int value = 0;
            
            switch (skill.skillSet.skillType)
            {
                case SkillType.Damage:
                    value = AttackType(skill.skillSet.damage, currentStat.damageValue);
                    break;
                case SkillType.Heal:
                    value = AttackType(skill.skillSet.heal, currentStat.healthValue);
                    break;
            }
            
            eventChannel.RaiseEvent(new CalcValueEvent(value, skill.skillSet.skillType, target));
        }

        private int AttackType(int damage, float stat)
        {
            return (int)(damage * stat);
        }
    }
}