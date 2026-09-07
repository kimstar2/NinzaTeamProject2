using System;
using Members.PSW.Code.Test;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime
{
    public abstract class AbstractSkillLogic : MonoBehaviour, ISkillLogic
    {
        public event Action OnCalculate;

        public abstract void PlaySkill(SkillSO skill);

        protected void CalculateEvent()
        {
            OnCalculate?.Invoke();
        }
    }
}