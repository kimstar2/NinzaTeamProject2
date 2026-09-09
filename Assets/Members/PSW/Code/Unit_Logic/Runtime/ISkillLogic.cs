using System;
using Members.PSW.Code.Test;

namespace Members.PSW.Code.Unit_Logic.Runtime
{
    public interface ISkillLogic
    {
        public event Action<CalculateStat> OnCalculate; 
        void PlaySkill(SkillSO skill);
    }
}