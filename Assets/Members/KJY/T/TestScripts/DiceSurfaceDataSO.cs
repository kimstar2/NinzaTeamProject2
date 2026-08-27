using UnityEngine;

namespace Members.KJY.T.TestScripts
{
    [CreateAssetMenu(fileName = "DiceDataSo", menuName = "Test/SO/DiceData", order = 0)]
    public class DiceSurfaceDataSO : ScriptableObject
    {
        public int diceNumber;
        public Sprite diceSprite;
    }
}