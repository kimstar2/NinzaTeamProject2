using System;
using System.Collections.Generic;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem
{
    public class SkillLogicExecutor : MonoBehaviour , ISkillLogicExecutor
    {
        [SerializeField] public List<AbstractSkillLogic> skills;
        public event Action OnSkillFinished;
        public event Action OnSkillExecute;

        public AbstractSelector Attacker { get; private set; }
        

        public void SkillFinished() => OnSkillFinished?.Invoke();
        public void SkillExecute(AbstractSelector attacker, AbstractSelector target)
        {
            foreach (AbstractSkillLogic skillLogic in skills)
                skillLogic.Execute(attacker, target);
            OnSkillExecute?.Invoke();
        }
    }
}