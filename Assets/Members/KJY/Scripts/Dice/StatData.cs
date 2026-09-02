using System;
using UnityEngine;

namespace Members.KJY.Scripts.Dice
{
    [Serializable]
    public struct StatData
    {
        [field:SerializeField] public float BaseDamage { get; private set; }
    }
}