using System;
using System.Collections.Generic;
using Members.PSW.Code.Unit_Logic;
using Members.PSW.Code.Unit_Logic.Runtime;
using UnityEngine;

namespace Members.PSW.Code.Test
{
    [CreateAssetMenu(fileName = "skill data", menuName = "Lumen/Test/Skill", order = 0)]
    public class SkillSO : ScriptableObject
    {
        public string skillName;
        [SerializeReference] public Type logicType;
        public List<SkillSetting> skillSet;
    }
}
