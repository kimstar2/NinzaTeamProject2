using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.PSW.Code.Test;
using Members.PSW.Code.Unit_Logic.Runtime.Structs;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime.Skill
{
    public class CalcValueEvent : GameEvent
    {
        public SkillSetting SkillSet { get; private set; }
        public int Value { get; private set; }
        public GameObject Target { get; private set; }

        public CalcValueEvent(int value, SkillSetting skillSet, GameObject target)
        {
            Value = value;
            SkillSet = skillSet;
            Target = target;
        }
    }
    
    public class SkillCalculater : MonoModule
    {
        [SerializeField] private EventChannelSO eventChannel;
        
        public void Calculate(CalculateStat currentStat, SkillSO skill, GameObject target)
        {
            foreach (var set in skill.skillSet)
            {
                Calc(currentStat, set, target);
            }
        }

        private void Calc(CalculateStat currentStat, SkillSetting skill, GameObject target)
        {
            int value = 0;
            
            switch (skill.skillType)
            {
                case SkillType.Damage:
                    value = CrossValue(skill.damage, currentStat.damageValue);
                    if (skill.useSelf)
                    {
                        eventChannel.RaiseEvent(new CalcValueEvent(value, skill, Owner.gameObject));
                        return;
                    }
                    break;
                
                case SkillType.Heal:
                    value = CrossValue(skill.heal, currentStat.healValue);
                    if (skill.useSelf)
                    {
                        eventChannel.RaiseEvent(new CalcValueEvent(value, skill, Owner.gameObject));
                        return;
                    }
                    break;
            }
            
            eventChannel.RaiseEvent(new CalcValueEvent(value, skill, target));
        }

        private int CrossValue(int damage, float stat)
        {
            return (int)(damage * stat);
        }
    }
}