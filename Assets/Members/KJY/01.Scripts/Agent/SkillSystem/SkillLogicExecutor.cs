using System;
using System.Collections.Generic;
using DevLib.HashDataSystem;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using Members.KJY._01.Scripts.Flags;
using Members.KJY._01.Scripts.Util;
using UnityEngine;

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
        [field:SerializeField] public HashDataSO SkillAnimHash {get; private set;}
        [SerializeField] private List<AbstractSkillLogic> skills;
        public event Action OnSkillFinished;
        public event Action OnSkillExecute;

        public AbstractSelector Attacker { get; private set; }
        

        public void SkillFinished() => OnSkillFinished?.Invoke();
        public void SkillExecute(AbstractSelector attacker, AbstractSelector target)
        {
            if (SkillAnimHash != null && SkillAnimHash.HashValue != NoneHash.Value)
                attacker.MyAgent.AnimCompo.RenderClipIfNotPlaying(SkillAnimHash.HashValue);
            foreach (AbstractSkillLogic skillLogic in skills)
                skillLogic.InitAndExecute(attacker, target);
            OnSkillExecute?.Invoke();
        }

        public void Remove()
        {
            Destroy(gameObject);
        }
    }
}