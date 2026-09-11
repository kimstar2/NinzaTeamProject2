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

        public override void Execute(AbstractSelector attacker, AbstractSelector target)
        {
            tweenSteps[attackerStepIndex].SetTransformValue(attacker.TrmPivot.position);
            tweenSteps[targetStepIndex].SetTransformValue(target.TrmPivot.position);
            
            TweenSequencer.SetSteps(tweenSteps);
            TweenSequencer.SetTargetTrm(attacker.MyTransform);
            TweenSequencer.Sequence();
        }
    }
}