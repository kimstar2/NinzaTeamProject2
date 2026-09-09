using System;
using Members.PSW.Code.Test;

namespace Members.PSW.Code.Unit_Logic.Runtime.Skill
{
    public interface ISkillLogic
    {
        public event Action<CalculateStat, SkillSO> OnCalculate;
        public event Action OnSkillFinished;
        void Init(SkillExecutor executor, CalculateStat currentStat);
        
        void PlaySkill(SkillSO skill);
    }
}