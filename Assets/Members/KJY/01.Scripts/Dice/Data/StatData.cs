using System;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Data
{
    [Serializable]
    public struct StatData
    {
        [field:SerializeField] public float BaseDamage { get; private set; }
    }
}