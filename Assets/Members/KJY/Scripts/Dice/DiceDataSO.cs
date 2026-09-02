using UnityEngine;

namespace Members.KJY.Scripts.Dice
{
    [CreateAssetMenu(fileName = "Dice data", menuName = "KJY/Game/Dice/Dice data", order = 0)]
    public class DiceDataSO : ScriptableObject
    {
        [field:Header("Basic Data"), SerializeField]
        public Sprite Icon { get; private set; }
        [field: SerializeField] public string SkillName { get; private set; }
        [field: TextArea, SerializeField] public string DescLine { get; private set; }
        
        [field:Header("Dice Data"), SerializeField]
        public float BaseDamage { get; private set; }
    }
}