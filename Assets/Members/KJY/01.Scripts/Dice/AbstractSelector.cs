using DevLib.CoreLib.Runtime;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Dice
{
    public abstract class AbstractSelector : MonoBehaviour
    {
        [field:SerializeField] public bool IsSelect {get; protected set;} 
        [field: SerializeField] public Transform LineConnectTrm { get; private set; }
        [SerializeField] protected EventChannelSO eventChannel;
        public UnityEvent onSelect;
        public UnityEvent onUnSelect;
        
        public void SelectToggle()
        {
            if (IsSelect)
                UnSelect();
            else
                Select();
        }

        protected abstract void Select();
        protected abstract void UnSelect();
    }
}