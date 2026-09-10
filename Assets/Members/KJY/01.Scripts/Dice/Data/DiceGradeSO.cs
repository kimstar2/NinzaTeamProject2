using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Data
{
    [CreateAssetMenu(fileName = "Dice Grade", menuName = "KJY/Game/Dice/Dice Grade data", order = 0)]
    public class DiceGradeSO : ScriptableObject
    {
        [field: SerializeField] public DiceGrade Grade { get; private set; }
        [field: SerializeField] public Color GradeColor { get; private set; }
    }

    public enum DiceGrade
    {
        Common,
        Uncommon,
        Rare
    }
}