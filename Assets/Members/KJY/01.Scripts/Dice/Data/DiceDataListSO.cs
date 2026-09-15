using System;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Data
{
    [CreateAssetMenu(fileName = "DiceDataList data", menuName = "KJY/Game/Dice/DiceDataList", order = 0)]
    public class DiceDataListSO : ScriptableObject
    {
        [field: SerializeField] public DiceDataSO Front { get; private set; }
        [field: SerializeField] public DiceDataSO Back { get; private set; }
        [field: SerializeField] public DiceDataSO Left { get; private set; }
        [field: SerializeField] public DiceDataSO Right { get; private set; }
        [field: SerializeField] public DiceDataSO Top { get; private set; }
        [field: SerializeField] public DiceDataSO Bottom { get; private set; }

        public DiceDataListSO GetRuntimeList()
        {
            return Instantiate(this);
        }
        
        public void SetDiceData(DiceDataSO diceData, DiceFaceType faceType)
        {
            _ = faceType switch
            {
                DiceFaceType.Front => Front = diceData,
                DiceFaceType.Back => Back = Back,
                DiceFaceType.Left => Left = Left,
                DiceFaceType.Right => Right = Right,
                DiceFaceType.Top => Top = Top,
                DiceFaceType.Bottom => Bottom = Bottom,
                _ => null
            };
        }
        
        public DiceDataSO GetDiceData(DiceFaceType faceType)
        {
            DiceDataSO diceData = faceType switch
            {
                DiceFaceType.Front => Front,
                DiceFaceType.Back => Back,
                DiceFaceType.Left => Left,
                DiceFaceType.Right => Right,
                DiceFaceType.Top => Top,
                DiceFaceType.Bottom => Bottom,
                _ => null
            };
            return diceData;
        }
    }
}