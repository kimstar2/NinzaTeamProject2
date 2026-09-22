using DevLib.HashDataSystem;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.Skill
{
    public abstract class AbstractSkillLogic : MonoBehaviour
    {
        public SkillLogicExecutor Executor {get; private set;}
        public virtual void Init(SkillLogicExecutor executor) => Executor = executor;
        public abstract void ApplyStat();
        public abstract void Execute();
        public virtual void AnimEnd() {}
        public virtual void Attack() {}
    }
}