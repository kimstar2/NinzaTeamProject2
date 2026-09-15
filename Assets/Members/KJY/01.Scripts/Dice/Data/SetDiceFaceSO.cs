using System;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Data
{
    [CreateAssetMenu(fileName = "SetDiceFace", menuName = "KJY/Game/Dice/SetDiceFace", order = 0)]
    public class SetDiceFaceSO : ScriptableObject
    {
        [field: SerializeField] public SetDiceFaceStruct Front { get; private set; }
        [field: SerializeField] public SetDiceFaceStruct Back { get; private set; }
        [field: SerializeField] public SetDiceFaceStruct Left { get; private set; }
        [field: SerializeField] public SetDiceFaceStruct Right { get; private set; }
        [field: SerializeField] public SetDiceFaceStruct Top { get; private set; }
        [field: SerializeField] public SetDiceFaceStruct Bottom { get; private set; }

        public int GetFaceToNumber(DiceFaceType faceType)
        {
            int returnNum = faceType switch
            {
                DiceFaceType.Front => Front.Num,
                DiceFaceType.Back => Back.Num,
                DiceFaceType.Left => Left.Num,
                DiceFaceType.Right => Right.Num,
                DiceFaceType.Top => Top.Num,
                DiceFaceType.Bottom => Bottom.Num,
                _ => -1
            };
            return returnNum;
        }
    }
    
    [Serializable]
    public struct SetDiceFaceStruct
    {
        [field: SerializeField] public DiceFaceType FaceType { get; private set; }
        [field: SerializeField] public int Num { get; private set; }
    }
}