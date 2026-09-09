using System;
using Members.PSW.Code.Test;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime.Skill
{
    public interface ISkillLogic
    {
        public event Action<CalculateStat, SkillSO, GameObject> OnCalculate;
        public event Action OnSkillFinished;
        void Init(SkillExecutor executor);
        
        void PlaySkill(SkillSO skill, GameObject target);
    }
}