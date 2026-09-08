using System;
using Members.PSW.Code.Test;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime
{
    public abstract class AbstractSkillLogic : MonoBehaviour, ISkillLogic
    {
        public abstract event Action<CalculateStat> OnCalculate;
        public abstract void PlaySkill(SkillSO skill);

        protected abstract void CalculateEvent();
    }
}