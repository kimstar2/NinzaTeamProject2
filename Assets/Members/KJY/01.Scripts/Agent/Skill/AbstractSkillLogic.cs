using System;
using DevLib.CoreLib.Runtime;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.Skill
{
    public abstract class AbstractSkillLogic : MonoBehaviour, ISkillLogic
    {
        public event Action OnSkillStart;
        public event Action OnSkillEnd;
        [field: SerializeField] public SkillDataSO SkillData { get; private set; }
        public AbstractSelector Attacker { get; private set; }

        public AbstractSkillLogic(AbstractSelector attacker)
        {
            Attacker = attacker;
        }

        public void ExecuteSkill() => ExecuteLogic();
        protected abstract void ExecuteLogic();
        protected virtual void SkillStart() => OnSkillStart?.Invoke();
        protected virtual void SkillEnd() => OnSkillEnd?.Invoke();
    }
}