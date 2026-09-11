using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem
{
    [CreateAssetMenu(fileName = "Skill data", menuName = "KJY/Skill/Skill data", order = 0)]
    public class SkillDataSO : ScriptableObject
    {
        [field: SerializeField] public SkillLogicExecutor SkillLogicExecutor { get; private set; }
        [field: SerializeField] public string SkillName {get; private set;}
    }
}