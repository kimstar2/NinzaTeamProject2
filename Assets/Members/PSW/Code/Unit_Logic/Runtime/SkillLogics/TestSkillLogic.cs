using System;
using DG.Tweening;
using Members.PSW.Code.Test;

namespace Members.PSW.Code.Unit_Logic.Runtime.SkillLogics
{
    public class TestSkillLogic : AbstractSkillLogic
    {
        public override event Action<CalculateStat> OnCalculate;

        public override void PlaySkill(SkillSO skill)
        {
            Sequence tween = DOTween.Sequence();
            tween.AppendInterval(2f);
            
            CalculateEvent();
        }

        protected override void CalculateEvent()
        {
            // OnCalculate?.Invoke();
        }
    }
}