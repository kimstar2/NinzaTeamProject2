using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Agent.HealthSystem;
using Members.KJY._01.Scripts.Mono;
using Members.KJY._01.Scripts.UI.Mono;
using UnityEngine;


namespace Members.KJY._01.Scripts.Agent
{
    public class AbstractAgent : ModuleOwner
    {
        [field:SerializeField] public MonoSprite AgentRenderer {get; private set;}
        public IAnimatable AnimCompo {get; private set;}
        public IAnimatorTrigger AnimTrigger {get; private set;}
        public HealthModule HealthModule {get; private set;}
        
        protected override void InitializeModules()
        {
            base.InitializeModules();
            Debug.Log("get");
            HealthModule = GetModule<HealthModule>();
            AnimCompo = GetModule<IAnimatable>();
            AnimTrigger = GetModule<IAnimatorTrigger>();
        }   
    }
}