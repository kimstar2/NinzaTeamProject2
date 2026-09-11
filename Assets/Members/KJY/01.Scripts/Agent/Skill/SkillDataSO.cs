using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.Skill
{
    [CreateAssetMenu(fileName = "Skill data", menuName = "KJY/Skill/Skill data", order = 0)]
    public class SkillDataSO : ScriptableObject
    {
        [field: SerializeField] public AbstractSkillLogic SkillLogic { get; private set; }
        [field: SerializeField] public string skillName;
    }
}