using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using UnityEngine;

namespace Members.KJY.Scripts.Module.Util
{
    public class GetEventChannel : MonoModule
    {
        [field:SerializeField] public EventChannelSO EventChannel {get; private set;}
    }
}