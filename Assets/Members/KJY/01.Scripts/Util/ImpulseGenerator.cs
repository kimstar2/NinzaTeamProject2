using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Members.KJY._01.Scripts.Util
{
    [Serializable]
    public struct ImpulseSetting
    {
        [field: SerializeField] public Vector3 MinImpulseVel {get; private set;}
        [field: SerializeField] public Vector3 MaxImpulseVel {get; private set;}
    }
    public class ImpulseGenerator : MonoBehaviour
    {
        [SerializeField] private float allGain = 1f;
        [SerializeField] private List<ImpulseSetting> impulseSettings;
        public CinemachineImpulseSource Cis { get; private set; }

        private void Awake()
        {
            Cis = GetComponent<CinemachineImpulseSource>();
        }

        public void Generate()
        {
            int r = Random.Range(0, impulseSettings.Count);
            var setting = impulseSettings[r];
            float x = Random.Range(setting.MinImpulseVel.x, setting.MaxImpulseVel.x);
            float y = Random.Range(setting.MinImpulseVel.y, setting.MaxImpulseVel.y);
            float z = Random.Range(setting.MinImpulseVel.z, setting.MaxImpulseVel.z);
            
            Vector3 impulseVal = new Vector3(x, y, z);
            
            Cis.GenerateImpulseWithVelocity(impulseVal * allGain);
        }   
        public void GenerateWithForce(float force)
        {
            int r = Random.Range(0, impulseSettings.Count);
            var setting = impulseSettings[r];
            float x = Random.Range(setting.MinImpulseVel.x, setting.MaxImpulseVel.x);
            float y = Random.Range(setting.MinImpulseVel.y, setting.MaxImpulseVel.y);
            float z = Random.Range(setting.MinImpulseVel.z, setting.MaxImpulseVel.z);
            
            Vector3 impulseVal = new Vector3(x, y, z);
            
            Cis.DefaultVelocity = impulseVal;
            Cis.GenerateImpulseWithForce(force * allGain);
        }
    }
}