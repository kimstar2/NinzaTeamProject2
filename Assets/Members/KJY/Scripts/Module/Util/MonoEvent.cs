using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using UnityEngine;

namespace Members.KJY.Scripts.Module.Util
{
    public class MonoEvent : MonoBehaviour
    {
        public ModuleOwner Owner { get; private set; }
        public EventChannelSO EventChannel { get; private set; }
        public virtual void Initialize(ModuleOwner owner)
        {
            Owner = owner;
            EventChannel = Owner.GetModule<GetEventChannel>().EventChannel;
        }
    }
}