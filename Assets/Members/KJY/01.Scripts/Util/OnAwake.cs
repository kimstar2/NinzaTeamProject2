using System;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Util
{
    public class OnAwake : MonoBehaviour
    {
        [field: SerializeField] public UnityEvent OnAwakeRaise { get; private set; }

        private void Awake() => OnAwakeRaise?.Invoke();
    }
}