using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using UnityEngine;

namespace Members.KJY._01.Scripts.Module.Util
{
    public class GetEventChannel : MonoModule
    {
        [field:SerializeField] public EventChannelSO EventChannel {get; private set;}
    }
}