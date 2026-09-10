using System;

namespace Members.PSW.Code.Unit_Logic.Runtime.Structs
{
    public enum SkillType
    {
        Damage,
        Heal,
        Buff,
        Shield
    }
    
    [Serializable]
    public struct SkillSetting
    {
        public SkillType skillType;
        public bool useSelf;
        
        //딜
        public int damage;
        //힐
        public int heal;
        //버프 (디버프 포함)
        public int buff;
        //보호막 부여
        public int shield;
    }
}