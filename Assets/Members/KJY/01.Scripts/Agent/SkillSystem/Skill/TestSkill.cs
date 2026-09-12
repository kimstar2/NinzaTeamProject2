using System;
using System.Collections.Generic;
using _TevLib.Extension.DoT;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.Skill
{
    public class TestSkill : AbstractSkillLogic
    {
        [field:SerializeField] public ClassStepTweenSequencer TweenSequencer { get; private set; }
        [SerializeField] private int attackerStepIndex, targetStepIndex;
        [SerializeField] private List<TweenStepClass> tweenSteps;
        [SerializeField] private List<SkillApplyStat> skillApplyStats;
        private AbstractSelector _target;

        public override void InitAndExecute(AbstractSelector attacker, AbstractSelector target)
        {
            _target = target;
            
            tweenSteps[attackerStepIndex].SetTransformValue(attacker.DefaultPosition.position);
            tweenSteps[targetStepIndex].SetTransformValue(target.DefaultPosition.position);
            
            TweenSequencer.SetSteps(tweenSteps);
            TweenSequencer.SetTargetTrm(attacker.MyTransform);
            TweenSequencer.Sequence();
        }

        public override void ApplyStat()
        {
            foreach (SkillApplyStat applyStat in skillApplyStats)
                _target.ApplyStat(applyStat.ApplyStatType,applyStat.Value);
        }
    }
}