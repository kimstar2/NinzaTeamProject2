using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Dice
{
    public abstract class AbstractDiceRollManager : ModuleOwner
    {
        [field:SerializeField] public bool AllDiceRollEnd { get; protected set; }
        [SerializeField] protected EventChannelSO eventChannel;
        public UnityEvent onAllDiceRollEnd;   
    }
}