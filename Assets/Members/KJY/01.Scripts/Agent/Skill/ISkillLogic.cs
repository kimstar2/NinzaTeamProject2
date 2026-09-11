using System;

namespace Members.KJY._01.Scripts.Agent.Skill
{
    public interface ISkillLogic
    {
        event Action OnSkillStart;
        event Action OnSkillEnd;
        
        AbstractSelector Attacker { get; }
            
        void ExecuteSkill();
    }
}