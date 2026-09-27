using System;
using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
using UnityEngine;

namespace Members.PSW.Code.Test
{
    public class SoundTester : MonoBehaviour
    {
        [SerializeField] private SoundClipSO soundClip;
        
        private void Start()
        {
            ServiceLocator.Get<IAudioService>().Play(soundClip);
        }
    }
}
