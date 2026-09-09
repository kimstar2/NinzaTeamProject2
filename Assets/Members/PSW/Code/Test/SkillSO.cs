using System.Collections.Generic;
using Members.PSW.Code.Unit_Logic.Runtime;
using Members.PSW.Code.Unit_Logic.Runtime.Skill;
using UnityEngine;

namespace Members.PSW.Code.Test
{
    [CreateAssetMenu(fileName = "skill data", menuName = "Lumen/Test/Skill", order = 0)]
    public class SkillSO : ScriptableObject
    {
        public string skillName;
        public string logicClassName;
        public SkillSetting skillSet;
    }
}
