using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Dice
{
    public abstract class AbstractDiceRollManager : ModuleOwner
    {
        [SerializeField] protected EventChannelSO eventChannel;
        public bool AllDiceRollEnd { get; protected set; } = true;
        public UnityEvent onAllDiceRollEnd;   
    }
}