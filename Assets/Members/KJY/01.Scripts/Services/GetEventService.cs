using DevLib.CoreLib.Runtime;
using DevLib.ServiceLocator;
using UnityEngine;

namespace Members.KJY._01.Scripts.Services
{
    public class GetEventService : MonoBehaviour , IGetEventService
    {
        [field:SerializeField] public EventChannelSO EventChannel {get; private set;}

        private void Awake() => ServiceLocator.Register<IGetEventService>(this);
        private void OnDestroy() => ServiceLocator.UnRegister<IGetEventService>();
    }
}