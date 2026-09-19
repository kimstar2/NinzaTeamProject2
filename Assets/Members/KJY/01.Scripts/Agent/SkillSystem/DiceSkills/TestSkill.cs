using System;
using System.Collections.Generic;
using _TevLib.Extension.DoT;
using DevLib.HashDataSystem;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.DiceSkills
{
    public class TestSkill : AbstractSkillLogic
    {
        [field:SerializeField] public TransformTweenSequencer ActionSeq { get; private set; }
        [field:SerializeField] public TransformTweenSequencer ReturnSeq { get; private set; }
        [SerializeField] private List<TransformTweenStep> playerTweenSteps;
        [SerializeField] private List<TransformTweenStep> enemyTweenSteps;
        [SerializeField] private List<SkillApplyStat> skillApplyStats;
        public UnityEvent onAnimEnd;

        public override void ApplyStat()
        {
            foreach (SkillApplyStat applyStat in skillApplyStats)
                Executor.Target.ApplyStat(applyStat.ApplyStatType,applyStat.Value);
        }

        public override void Execute()
        {
            ActionSeq.SetTargetTrm(Executor.Attacker.MyAgent.transform);
            ReturnSeq.SetTargetTrm(Executor.Attacker.MyAgent.transform);
            switch (Executor.AgentType)
            {
                case AgentType.Player:
                    ActionSeq.SetSteps(playerTweenSteps);
                    break;
             
                case AgentType.Enemy:
                    ActionSeq.SetSteps(enemyTweenSteps);
                    break;
            }
            ActionSeq.Sequence();
        }

        public override void AnimEnd()
        {
            Executor.PlayIdleAnim();
            onAnimEnd?.Invoke();
        }
    }
}