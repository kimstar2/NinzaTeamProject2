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
            if (statType == ApplyStatType.Damage) value *= Executor.Attacker.Effects.OutgoingMultiplier; // 둔화·공격력 증가
            return value;
        }

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

        protected void Reflect(AbstractSelector target, float dealt)
        {
            var attacker = Executor.Attacker;
            float ratio = target.Effects.ReflectRatio;
            if (ratio <= 0f || dealt <= 0f || attacker == null || attacker == target || attacker.IsDead) return;
            target.Effects.ShowPopup("반사!", new Color(0.85f, 0.9f, 1f));
            attacker.ApplyDamage(dealt * ratio);
        }

        public abstract void ApplyStat();
        public abstract void Execute();
        public virtual void AnimEnd() {}
        public virtual void Attack() {}
    }
}
