using System;
using DG.Tweening;
using Members.PSW.Code.Test;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime.Skill.Logics
{
    public class TestSkillLogic : MonoBehaviour, ISkillLogic
    {
        public event Action<CalculateStat, SkillSO> OnCalculate;
        public event Action OnSkillFinished;

        private SkillExecutor _executor;
        private CalculateStat _currentStat;

        public void Init(SkillExecutor executor, CalculateStat currentStat)
        {
            _executor = executor;
            _currentStat = currentStat;
        }

        public void PlaySkill(SkillSO skill)
        {
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(1f);
            seq.AppendCallback(() => Debug.Log($"PlaySkill {skill.skillName}"));
            seq.AppendCallback(() => OnCalculate?.Invoke(_currentStat, skill));
            seq.AppendInterval(1f);
            seq.AppendCallback(() => OnSkillFinished?.Invoke());
            seq.AppendCallback(() => Debug.Log("I make Fucking Base System!!!!"));
        }
    }
}