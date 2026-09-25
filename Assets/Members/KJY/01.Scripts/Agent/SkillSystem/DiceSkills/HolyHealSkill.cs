using System.Threading;
using _LumenLib.PoolingSystem.Runtime;
using Cysharp.Threading.Tasks;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.DiceSkills
{
    public class HolyHealSkill : AbstractParticleSkill
    {
        [field:SerializeField] public PoolItemSO HealParticle {get; private set;}
        private bool _isApplied;

        protected override bool CanHit => Executor != null && Executor.Attacker != null && !Executor.Attacker.IsDead;

        protected override UniTask AttackAsync(CancellationToken token)
        {
            PlayParticle(HealParticle, Executor.Attacker.MyAgent.transform.position + effectOffset);
            ApplyStat();
            return UniTask.CompletedTask;
        }

        public override void ApplyStat()
        {
            if (!CanApplyStat || _isApplied) return;
            _isApplied = true;
            Executor.Attacker.ApplyStat(ApplyStatType.Heal, GetStat(ApplyStatType.Heal)); // 지금 타겟 선택은 적 기준이라 일단 자기 회복
        }
    }
}
