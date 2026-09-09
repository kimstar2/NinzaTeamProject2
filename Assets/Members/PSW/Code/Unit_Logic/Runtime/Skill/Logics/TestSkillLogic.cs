using System;
using System.Net.Http.Headers;
using DG.Tweening;
using Members.PSW.Code.Test;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime.Skill.Logics
{
    public class TestSkillLogic : MonoBehaviour, ISkillLogic
    {
        public event Action<CalculateStat, SkillSO, GameObject> OnCalculate;
        public event Action OnSkillFinished;

        private SkillExecutor _executor;

        private CalculateStat CurrentStat => _executor.Owner.GetModule<StatModule>().CurrentStat;

        public void Init(SkillExecutor executor)
        {
            _executor = executor;
        }

        public void PlaySkill(SkillSO skill, GameObject target)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(_executor.Owner.transform.DORotate(new Vector3(0, 0, 1080f), 1.5f, RotateMode.FastBeyond360));
            seq.AppendCallback(() => OnCalculate?.Invoke(CurrentStat, skill, target));
            seq.AppendInterval(1f);
            seq.AppendCallback(() => OnSkillFinished?.Invoke());
        }
    }
}