using Members.PSW.Code.Test;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Data
{
    [CreateAssetMenu(fileName = "Dice data", menuName = "KJY/Game/Dice/Dice data", order = 0)]
    public class DiceDataSO : ScriptableObject
    {
        [field: Header("Basic Data")]
        [field: SerializeField] public DiceGradeSO DiceGrade {get; private set;}
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public string SkillName { get; private set; }
        [field: TextArea, SerializeField] public string Description { get; private set; }

        [field: Header("Dice Data")]
        [field:SerializeField] public SkillSO SkillData { get; private set;}
        [field:SerializeField] public float BaseDamage { get; private set; }
    }
}