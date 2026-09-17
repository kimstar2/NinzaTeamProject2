using System.Collections.Generic;
using _TevLib.Extension.DoT;
using DevLib.HashDataSystem;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.DiceSkills
{
    public class TestSkill : AbstractSkillLogic
    {
        [field:SerializeField] public ClassStepTweenSequencer TweenSequencer { get; private set; }
        [SerializeField] private int attackerStepIndex, targetStepIndex;
        [SerializeField] private List<TweenStepClass> tweenSteps;
        [SerializeField] private List<SkillApplyStat> skillApplyStats;

        public override void ApplyStat()
        {
            foreach (SkillApplyStat applyStat in skillApplyStats)
                Executor.Target.ApplyStat(applyStat.ApplyStatType,applyStat.Value);
        }

        public override void Execute()
        {
            Executor.PlayAnim();
            // tweenSteps[attackerStepIndex].SetTransformValue(Executor.Attacker.DefaultPosition.position);
            // tweenSteps[targetStepIndex].SetTransformValue(Executor.Target.DefaultPosition.position);
            //
            // TweenSequencer.SetSteps(tweenSteps);
            // TweenSequencer.SetTargetTrm(Executor.Attacker.MyAgent.transform);
            // TweenSequencer.Sequence();
        }

        public override void AnimEnd()
        {
            Executor.PlayIdleAnim();
            Executor.SkillFinished();
            Debug.Log("d");
        }
    }
}