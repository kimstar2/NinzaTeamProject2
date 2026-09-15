using System.Collections.Generic;
using _TevLib.Extension.DoT;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem.DiceSkills
{
    public class SolarEclipseSkill : AbstractSkillLogic
    {
        [field:SerializeField] public ClassStepTweenSequencer LunarTweenSequencer { get; private set; }
        [field:SerializeField] public ClassStepTweenSequencer SolarTweenSequencer { get; private set; }
        [SerializeField] private List<TweenStepClass> lunarTweenStep;
        [SerializeField] private List<TweenStepClass> solarTweenStep;
        [SerializeField] private int attackerTrmIndex;
        
        public override void InitAndExecute(AbstractSelector attacker, AbstractSelector target)
        {
            lunarTweenStep[attackerTrmIndex].SetTransformValue(attacker.DefaultPosition.position);
            solarTweenStep[attackerTrmIndex].SetTransformValue(attacker.DefaultPosition.position);
            LunarTweenSequencer.SetSteps(lunarTweenStep);
            SolarTweenSequencer.SetSteps(solarTweenStep);
            LunarTweenSequencer.Sequence();
            SolarTweenSequencer.Sequence();
        }

        public override void ApplyStat()
        {
        }
    }
}