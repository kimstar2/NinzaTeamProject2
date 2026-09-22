using System;
using _LumenLib.PoolingSystem.Runtime;
using UnityEngine;

namespace Members.KJY._01.Scripts.Pool
{
    public class PoolingParticle : MonoBehaviour , IPoolable
    {
        [field:SerializeField] public PoolItemSO Item { get; private set; }
        public GameObject GameObject => gameObject;

        public ParticleSystem Particle {get; private set;}
        public event Action<PoolingParticle> OnParticleEnd;
        

        private void Awake()
        {
            Particle = GetComponent<ParticleSystem>();
        }

        public void PlayParticle(bool withChildren = false) => Particle.Play(withChildren);
        public void PlayParticle(Transform trm,bool withChildren = false)
        {
            transform.position = trm.position;
            Particle.Play(withChildren);
        }
        
        public void PlayParticle(Vector3 pos,bool withChildren = false)
        {
            transform.position = pos;
            Particle.Play(withChildren);
        }


        public void StopParticle(bool withChildren = false) => Particle.Stop(withChildren);
        public void ClearParticle(bool withChildren = false) => Particle.Clear(withChildren);
        

        public void ResetItem()
        {
            StopParticle();
            ClearParticle();
        }

        private void OnParticleSystemStopped()
        {
            OnParticleEnd?.Invoke(this);
        }
    }
}