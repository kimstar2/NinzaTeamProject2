using System;
using _TevLib.Extension;
using _TevLib.Extension.DoT;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Util
{
    public class MaterialEffect : MonoBehaviour
    {
        [SerializeField] private TweenStep tweenStep;
        [SerializeField] private SpriteRenderer[] targetRenderers;
        [SerializeField] private ShaderHashSO shaderHash;
        [SerializeField] private Transform trmID;
        private MaterialPropertyBlock _mpb;
        private int _id;
        
        public UnityEvent onBlinkComplete;
        
        
        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            foreach (SpriteRenderer sR in targetRenderers)
                sR.GetPropertyBlock(_mpb);
            _id = trmID.GetInstanceID();
        }

        public void SetValue(float blinkVal)
        {
            DOTween.Kill(_id);
            _mpb.Clear();
            
            float crtBlink = _mpb.GetFloat(shaderHash.HashValue);
            DOTween.To(() => crtBlink, x =>
                {
                    crtBlink = x;
                    SetVariable(crtBlink);
                },blinkVal, tweenStep.Duration)
                .SetId(_id)
                .SetEase(tweenStep.EaseType)
                .OnComplete(() => onBlinkComplete?.Invoke());
        }

        private void OnDisable() => DOTween.Kill(_id);

        public void SetImmediateValue(float value)
        {
            DOTween.Kill(_id);
            SetVariable(value);
        }

        private void SetVariable(float crtBlink)
        {
            _mpb.SetFloat(shaderHash.HashValue, crtBlink);
            foreach (SpriteRenderer sR in targetRenderers)
                sR.SetPropertyBlock(_mpb);
        }
    }
}
