using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.Skill
{
    using DevLib.ServiceLocator;
    using DevLib.SoundSystem.Runtime;

    public abstract class AbstractSkillLogic : MonoBehaviour
    {
        [Header("Sound")]
        [SerializeField] private SoundClipSO skillSound;

        public SkillLogicExecutor Executor {get; private set;}
        public float BaseLevel {get; private set;}
        public virtual void Init(SkillLogicExecutor executor , float baseLevel)
        {
            Executor = executor;
            BaseLevel = baseLevel;
        }

        // Call at the skill's cast or impact timing, or connect to a UnityEvent.
        public void PlaySkillSound()
        {
            if (skillSound == null) return;
            ServiceLocator.Get<IAudioService>().Play(skillSound);
        }

        protected float GetStat(ApplyStatType statType)
        {
            float value = Executor.SkillData.GetScaledStat(statType, BaseLevel) * Executor.PowerMultiplier;
            if (statType == ApplyStatType.Damage && ServiceLocator.TryGet<IDamageModifiers>(out var modifiers))
                value *= modifiers.GetOutgoingMultiplier(Executor.Attacker.AgentData);
            if (statType == ApplyStatType.Damage) value *= Executor.Attacker.Effects.OutgoingMultiplier; // 둔화
            return value;
        }

        // 광역이면 대상 진영 전체, 아니면 받은 대상 한 명에게 적용
        protected void ApplyConfiguredStats(AbstractSelector target)
        {
            if (Executor.IsMissed)
            {
                Executor.ShowMissOnce(target);
                return;
            }
            if (!Executor.SkillData.IsArea)
            {
                ApplyConfiguredStatsTo(target);
                return;
            }
            foreach (var areaTarget in Executor.GetTargets())
                if (areaTarget != null && !areaTarget.IsDead) ApplyConfiguredStatsTo(areaTarget);
        }

        private void ApplyConfiguredStatsTo(AbstractSelector target)
        {
            foreach (SkillApplyStat stat in Executor.SkillData.ApplyStats)
            {
                float value = GetStat(stat.ApplyStatType);
                if (value <= 0f) continue;
                if (stat.ApplyStatType == ApplyStatType.Damage)
                    value *= target.Effects.UseMark(Executor.SkillData.Synergy == SkillDataSO.SynergyType.ExploitMark);
                float healthBefore = target.MyAgent.HealthModule.CurrentHealth;
                target.ApplyStat(stat.ApplyStatType, value);
                if (stat.ApplyStatType == ApplyStatType.Damage)
                    Reflect(target, healthBefore - target.MyAgent.HealthModule.CurrentHealth);
            }
            Executor.ApplySynergy(target);
        }

        // 반사 상태인 대상을 때리면 실제로 깎인 체력의 일부가 공격자에게 되돌아온다 (보호로 줄지 않음)
        protected void Reflect(AbstractSelector target, float dealt)
        {
            var attacker = Executor.Attacker;
            float ratio = target.Effects.ReflectRatio;
            if (ratio <= 0f || dealt <= 0f || attacker == null || attacker == target || attacker.IsDead) return;
            attacker.ApplyDamage(dealt * ratio);
        }

        public abstract void ApplyStat();
        public abstract void Execute();
        public virtual void AnimEnd() {}
        public virtual void Attack() {}
    }
}
