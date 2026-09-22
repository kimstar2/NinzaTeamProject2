using System;
using System.Collections.Generic;
using _LumenLib.PoolingSystem.Runtime;
using _TevLib.Extension.DoT;
using DevLib.HashDataSystem;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using Members.KJY._01.Scripts.Pool;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.DiceSkills
{
    public class MagicSkill : AbstractSkillLogic
    {
        [field: SerializeField] public PoolItemSO PoolParticle { get; private set; }

        [field: SerializeField] public TransformTweenSequencer ActionSeq { get; private set; }
        [field: SerializeField] public TransformTweenSequencer ReturnSeq { get; private set; }
        [SerializeField] private List<TransformTweenStep> playerTweenSteps;
        [SerializeField] private List<TransformTweenStep> enemyTweenSteps;
        [SerializeField] private List<SkillApplyStat> skillApplyStats;
        private ObjectPool _pool;
        public UnityEvent onAnimEnd;

        private void Awake()
        {
            _pool = ServiceLocator.Get<ObjectPool>();
        }

        public override void ApplyStat()
        {
            foreach (SkillApplyStat applyStat in skillApplyStats)
                Executor.Target.ApplyStat(applyStat.ApplyStatType, applyStat.Value);
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

        private void ReturnToPool(PoolingParticle particle)
        {
            if (particle ==null) return;
            particle.OnParticleEnd -= ReturnToPool;
            _pool.Push(particle);
            Debug.Log("return");
        }

        public override void Attack() => PlayParticle();
        private void PlayParticle()
        {
            PoolingParticle particle =_pool.Pop(PoolParticle.ItemName) as PoolingParticle;
            if (particle == null) return;
            
            particle.GameObject.SetActive(true);
            particle.OnParticleEnd += ReturnToPool;
            particle.PlayParticle(Executor.Target.MyAgent.transform);
        }
    }
}