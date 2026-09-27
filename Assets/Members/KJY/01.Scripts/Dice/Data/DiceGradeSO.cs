using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Data
{
    [CreateAssetMenu(fileName = "Dice Grade", menuName = "KJY/Game/Dice/Dice Grade data", order = 0)]
    public class DiceGradeSO : ScriptableObject
    {
        [field: SerializeField] public DiceGrade Grade { get; private set; }
        [field: SerializeField] public Color GradeColor { get; private set; }
        public string DisplayName => GetName(Grade);

        // 일반 = 일반 몬스터, 희귀 = 정예, 전설 = 플레이어·보스 고유
        public static string GetName(DiceGrade grade) => grade switch
        {
            DiceGrade.Uncommon => "희귀",
            DiceGrade.Rare => "전설",
            _ => "일반"
        };
    }

    public enum DiceGrade
    {
        Common,
        Uncommon,
        Rare
    }
}