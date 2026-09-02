using System;
using System.Diagnostics.Tracing;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using UnityEngine;

namespace Members.PSW.Code.Test
{
    public class TestDiceUIConnector : MonoModule
    {
        public event Action OnRerollClick;
        public event Action<int> OnDiceLock;

        public void RerollButton()
        {
            Debug.Log("리롤 시도");
            OnRerollClick?.Invoke();
        }

        public void LockButton(int index)
        {
            Debug.Log($"{index}번 주사위 잠금 시도");
            OnDiceLock?.Invoke(index);
        }
    }
}