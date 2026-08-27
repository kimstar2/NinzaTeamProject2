using UnityEngine;

namespace Members.PSW.Code.Test
{
    [CreateAssetMenu(fileName = "skill data", menuName = "Lumen/Test/Skill", order = 0)]
    public class SkillSO : ScriptableObject
    {
        public string skillName;
        public int damage;
    }
}