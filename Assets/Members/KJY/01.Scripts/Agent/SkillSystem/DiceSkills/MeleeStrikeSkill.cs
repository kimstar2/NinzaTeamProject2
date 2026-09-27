using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using _LumenLib.PoolingSystem.Runtime;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.DiceSkills
{
    public class MeleeStrikeSkill : AbstractParticleSkill
    {
        [SerializeField] private PoolItemSO impactParticle;
        [SerializeField, Min(1)] private int hitCount = 1;
        [SerializeField, Min(0f)] private float hitInterval = 0.18f;
        [SerializeField] private float[] hitWeights; // 타격별 피해 배율. 비어 있으면 모두 1
        private bool _isApplied;
        private float _hitWeight = 1f;

        protected override async UniTask AttackAsync(CancellationToken token)
        {
            for (int i = 0; i < Mathf.Max(1, hitCount) && CanHit; i++)
            {
                _isApplied = false;
                _hitWeight = hitWeights != null && i < hitWeights.Length ? hitWeights[i] : 1f;
                ApplyStat(); // 첫 타격은 OnAttack, 추가 베기는 설정한 간격에 맞춤.
                if (i + 1 < hitCount && CanHit)
                    await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0f, hitInterval)), cancellationToken: token);
            }
        }

        public override void ApplyStat()
        {
            if (!CanApplyStat || _isApplied) return;
            _isApplied = true;
            PlaySkillSound();
            PlayParticle(impactParticle, Executor.Target.MyAgent.transform.position + effectOffset);
            ApplyDamage(_hitWeight);
        }
    }
}
