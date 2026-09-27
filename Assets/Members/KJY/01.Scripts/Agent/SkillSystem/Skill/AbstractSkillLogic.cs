using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.Skill
{
    public abstract class AbstractSkillLogic : MonoBehaviour
    {
        public SkillLogicExecutor Executor {get; private set;}
        public float BaseLevel {get; private set;}
        public virtual void Init(SkillLogicExecutor executor , float baseLevel)
        {
            Executor = executor;
            BaseLevel = baseLevel;
        }

        protected float GetStat(ApplyStatType statType)
            => Executor.SkillData.GetScaledStat(statType, BaseLevel) * Executor.PowerMultiplier;

        protected void ApplyConfiguredStats(AbstractSelector target)
        {
            foreach (SkillApplyStat stat in Executor.SkillData.ApplyStats)
            {
                float value = GetStat(stat.ApplyStatType);
                if (value <= 0f) continue;
                if (stat.ApplyStatType == ApplyStatType.Damage)
                    value *= target.Effects.UseMark(Executor.SkillData.Synergy == SkillDataSO.SynergyType.ExploitMark);
                target.ApplyStat(stat.ApplyStatType, value);
            }
            Executor.ApplySynergy();
        }

        public abstract void ApplyStat();
        public abstract void Execute();
        public virtual void AnimEnd() {}
        public virtual void Attack() {}
    }
}
