using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.Skill
{
    public abstract class AbstractSkillLogic : MonoBehaviour
    {
        public abstract void Execute(AbstractSelector attacker, AbstractSelector target);
    }
}