using _LumenLib.PoolingSystem.Runtime;
using Members.KJY._01.Scripts.Pool;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.Skill
{
    public interface IParticleSkill
    {
        PoolItemSO PoolParticle {get;}
        void PlayParticle();
    }
}