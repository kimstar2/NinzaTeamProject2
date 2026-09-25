using _TevLib.Extension;
using _TevLib.Extension.DoT;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Util
{
    public class SeedSetter : MonoBehaviour
    {
        [SerializeField] private ShaderHashSO seedHash;
        [SerializeField] private SpriteRenderer targetRenderer;
        private MaterialPropertyBlock _mpb;
        
        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(_mpb);
        }
        public void SetSeed()
        {
            _mpb.Clear();
            int seed = Random.Range(int.MinValue, int.MaxValue);
            _mpb.SetFloat(seedHash.HashValue,seed);
            targetRenderer.SetPropertyBlock(_mpb);
        }
    }
}