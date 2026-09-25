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
            => Executor.SkillData.GetScaledStat(statType, BaseLevel);

        protected void ApplyConfiguredStats(AbstractSelector target)
        {
            foreach (SkillApplyStat stat in Executor.SkillData.ApplyStats)
                target.ApplyStat(stat.ApplyStatType, stat.GetScaledValue(BaseLevel));
        }

        public abstract void ApplyStat();
        public abstract void Execute();
        public virtual void AnimEnd() {}
        public virtual void Attack() {}
    }
}
