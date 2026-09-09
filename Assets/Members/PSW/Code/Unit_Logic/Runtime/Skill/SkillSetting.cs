using System;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime.Skill
{
    public enum SkillType
    {
        Damage,
        Heal,
        Buff,
        Debuff,
        Shield
    }
    
    [Serializable]
    public struct SkillSetting
    {
        public SkillType skillType;
        public int damage;
        public int heal;
        public int buff;
        public int debuff;
        public int shield;
    }
}