using System;
using DevLib.ModuleSystem;
using DG.Tweening;
using Members.PSW.Code.Test;
using Members.PSW.Code.Unit_Logic.Runtime.Structs;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime.Skill.Logics
{
    public class TestSkillLogic : MonoBehaviour, ISkillLogic
    {
        public event Action<CalculateStat, SkillSO, GameObject> OnCalculate;
        public event Action OnSkillFinished;

        private SkillExecutor _executor;
        private ModuleOwner _owner;

        private CalculateStat CurrentStat => _executor.Owner.GetModule<StatModule>().CurrentStat;

        public void Init(SkillExecutor executor)
        {
            _executor = executor;
            _owner = executor.Owner;
        }

        public void PlaySkill(SkillSO skill, GameObject target)
        {
            Vector3 pastPos = _owner.transform.position;
            
            Sequence seq = DOTween.Sequence();
            seq.Append(_owner.transform.DOMove(target.transform.position, 1f).SetEase(Ease.Linear));
            
            seq.Append(_owner.transform.DORotate(new Vector3(0, 0, 45), 0.3f));
            seq.Append(_owner.transform.DORotate(new Vector3(0, 0, -45), 0.3f));
            seq.Append(_owner.transform.DORotate(new Vector3(0, 0, 0), 0.3f));
            
            seq.AppendCallback(() => OnCalculate?.Invoke(CurrentStat, skill, target));
            
            seq.Append(_owner.transform.DOMove(pastPos, 1f).SetEase(Ease.Linear));
            
            seq.AppendCallback(() => OnSkillFinished?.Invoke());
        }
    }
}