using System.Threading;
using Cysharp.Threading.Tasks;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.DiceSkills
{
    public class MeleeStrikeSkill : AbstractParticleSkill
    {
        [SerializeField, Min(0f)] private float damage = 18f;
        private bool _isApplied;

        protected override UniTask AttackAsync(CancellationToken token)
        {
            ApplyStat(); // 휘두르는 모션의 OnAttack에 맞춰서 때림
            return UniTask.CompletedTask;
        }

        public override void ApplyStat()
        {
            if (!CanApplyStat || _isApplied) return;
            _isApplied = true;
            Executor.Target.ApplyStat(ApplyStatType.Damage, GetDamage(damage));
        }
    }
}
