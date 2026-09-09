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

        public CalcValueEvent(int value)
        {
            Value = value;
        }
    }
    
    public class SkillCalculater : MonoModule
    {
        [SerializeField] private EventChannelSO eventChannel;
        
        public void Calculate(CalculateStat currentStat, SkillSO skill)
        {
            int value = 0;
            
            switch (skill.skillSet.skillType)
            {
                case SkillType.Damage:
                    value = AttackType(skill.skillSet.damage, currentStat.damageValue);
                    break;
            }
            
            eventChannel.RaiseEvent(new CalcValueEvent(value));
        }

        private int AttackType(int damage, float stat)
        {
            return (int)(damage * stat);
        }
    }
}