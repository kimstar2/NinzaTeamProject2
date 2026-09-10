using System;
using _TevLib.Extension;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.UI.Mono
{
    public class UIMaterialEffect : MonoBehaviour
    {
        [SerializeField] private UIMonoImage targetImage;
        [SerializeField] private ShaderHashSO shaderHash;
        [SerializeField] private Ease blinkEase;
        [SerializeField] private Transform trmId;
        [SerializeField] private float defaultDur;
        private Material _materialIns;
        private int _id;
        private float _duration;
        public UnityEvent onBlinkComplete;

        protected void Awake()
        {
            _id = trmId.GetHashCode();
            _duration = defaultDur;
        }

        private void Start()
        {
            _materialIns = new Material(targetImage.Image.material);
            targetImage.SetMaterial(_materialIns);
        }
        
        public void SetDur(float duration) => _duration = duration;
        
        public void SetBlink(float blinkVal)
        {
            DOTween.Kill(_id);

            float crtBlink = _materialIns.GetFloat(shaderHash.HashValue);
            DOTween.To(() => crtBlink, x =>
            {
                crtBlink = x;
                SetVariable(crtBlink);
            },blinkVal, _duration)
            .SetId(_id)
            .SetEase(blinkEase)
            .OnComplete(() => onBlinkComplete?.Invoke());
        }

        private void SetVariable(float crtBlink)
        {
            _materialIns.SetFloat(shaderHash.HashValue, crtBlink);
            targetImage.Image.SetMaterialDirty();   
        }
    }
}