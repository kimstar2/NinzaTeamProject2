using System;
using System.Collections.Generic;
using DevLib.HashDataSystem;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using Members.KJY._01.Scripts.Flags;
using Members.KJY._01.Scripts.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.SkillSystem
{
    [Serializable]
    public struct SkillApplyStat
    {
        [field:SerializeField] public  ApplyStatType ApplyStatType { get; private set; }
        [field:SerializeField] public  float Value { get; private set; }
    }
    
    public class SkillLogicExecutor : MonoBehaviour , ISkillLogicExecutor , IRequirePooling
    {
        [field:SerializeField] public HashDataSO IdleAnimHash {get; private set;}
        [field:SerializeField] public HashDataSO SkillAnimHash {get; private set;}
        [SerializeField] private List<AbstractSkillLogic> skills;
        public event Action OnSkillFinished;
        public event Action OnSkillExecute;
        public UnityEvent onAnimFinished;
        
        public AbstractSelector Attacker { get; private set; }
        public AbstractSelector Target { get; private set; }
        
        public void SkillFinished() => OnSkillFinished?.Invoke();
        public void SkillExecute(AbstractSelector attacker, AbstractSelector target)
        {
            Target = target;
            Attacker = attacker;
            
            Attacker.MyAgent.AnimTrigger.OnAnimFinished -= HandleAnimFinished;
            Attacker.MyAgent.AnimTrigger.OnAnimFinished += HandleAnimFinished;

            foreach (AbstractSkillLogic skillLogic in skills)
            {
                skillLogic.Init(this);
                skillLogic.Execute();
            }
            OnSkillExecute?.Invoke();
        }

        private void HandleAnimFinished()
        {
            Attacker.MyAgent.AnimTrigger.OnAnimFinished -= HandleAnimFinished;
            
            foreach (AbstractSkillLogic skillLogic in skills)
                skillLogic.AnimEnd();
        }

        public void Remove()
        {
            Destroy(gameObject);
        }

        public void PlayAnim()
        {
            if (SkillAnimHash != null && SkillAnimHash.HashValue != NoneHash.Value)
                Attacker.MyAgent.AnimCompo.RenderClipIfNotPlaying(SkillAnimHash.HashValue);
        }
        
        public void PlayIdleAnim()
        {
            if (IdleAnimHash != null && IdleAnimHash.HashValue != NoneHash.Value)
                Attacker.MyAgent.AnimCompo.RenderClipIfNotPlaying(IdleAnimHash.HashValue);
        }
    }
}