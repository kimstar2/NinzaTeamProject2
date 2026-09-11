using _TevLib.Extension.DoT;
using Members.KJY._01.Scripts.Events.Dice;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.Skill.T
{
    public class UsingTweenSkillLogic : AbstractSkillLogic
    {
        [SerializeField] private TweenSequencer tweenSeq;
        
        public UsingTweenSkillLogic(AbstractSelector attacker) : base(attacker) { }

        protected override void ExecuteLogic()
        {
            tweenSeq.targetTrm = Attacker.MyTransform;
        }
    }
}