using System;

namespace Members.KJY._01.Scripts.Agent.SkillSystem
{ 
    public interface ISkillLogicExecutor
    {
        event Action OnSkillFinished;
        event Action OnSkillExecute;
        
        AbstractSelector Attacker { get; }
            
        void SkillFinished();
        void SkillExecute(AbstractSelector attacker, AbstractSelector target);
    }
}