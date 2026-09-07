using System;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Data
{
    [Serializable]
    public struct DiceFaceStruct
    {
        [field: SerializeField] public Vector3 Range { get; private set; }
        [field: SerializeField] public DiceFaceType Type { get; private set; }
    }
}