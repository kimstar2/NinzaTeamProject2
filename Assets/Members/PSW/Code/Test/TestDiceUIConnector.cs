using System;
using System.Diagnostics.Tracing;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.PSW.Code.DiceReroll_Lock.Events;
using UnityEngine;

namespace Members.PSW.Code.Test
{
    public class TestDiceUIConnector : MonoModule
    {
        [SerializeField] private EventChannelSO evt;
        
        public void RerollButton()
        {
            Debug.Log("리롤 시도");
            evt.RaiseEvent(DiceUIEvents.RerollEvent);
        }

        public void LockButton(int index)
        {
            Debug.Log($"{index}번 주사위 잠금 시도");
            evt.RaiseEvent(new LockEvent(index));
        }
    }
}