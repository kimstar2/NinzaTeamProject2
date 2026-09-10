using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Module.Util
{
    [Serializable]
    public struct WaitOnPlayStruct
    {
        [field:SerializeField] public float waitTime;
        [field:SerializeField] public UnityEvent onPlay;
    }
    public class WaitOnPlay : MonoBehaviour
    {
        [SerializeField] List<WaitOnPlayStruct> waitOnPlayStructs;
        private CancellationTokenSource _cts;
        
        public void Play()
        {
            KillTask();
            _cts = new CancellationTokenSource();
            StartThis(_cts.Token).Forget();
        }

        private void KillTask()
        {
            if (_cts == null) return;
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        private async UniTask StartThis(CancellationToken token)
        {
            foreach (WaitOnPlayStruct waitOnP in waitOnPlayStructs)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(waitOnP.waitTime) , cancellationToken: token);
                waitOnP.onPlay.Invoke();
            }
        }
    }
}