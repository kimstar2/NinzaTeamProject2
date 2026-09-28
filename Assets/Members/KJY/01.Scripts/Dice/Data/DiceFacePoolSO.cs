using System.Collections.Generic;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Data
{
    [CreateAssetMenu(fileName = "Dice Face Pool", menuName = "KJY/Game/Dice/Dice Face Pool", order = 0)]
    public class DiceFacePoolSO : ScriptableObject
    {
        [SerializeField] private List<DiceDataSO> faces = new();
        public IReadOnlyList<DiceDataSO> Faces => faces;
    }
}
