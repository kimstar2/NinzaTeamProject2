using System;
using _TevLib.Extension.DoT;
using DevLib.ServiceLocator;
using DG.Tweening;
using Members.KJY._01.Scripts.Service;
using UnityEngine;

namespace Members.KJY._01.Scripts.Util
{
    public class CamLensSetter : MonoBehaviour
    {
        [SerializeField] private float value;
        [SerializeField] private bool useIgnoreTimeScale;
        [SerializeField] private TweenStep lensValueStep;
        private IGetCurrentCam _cineCam;
        
        private void Start()
        {
            _cineCam = ServiceLocator.Get<IGetCurrentCam>();
        }

        public void SetValue()
        {
            DOTween.Kill(_cineCam.ID);
            float crt;
            DOTween.To(() => _cineCam.CineCam.Lens.OrthographicSize,
                x =>
                {
                    crt = x;
                    _cineCam.SetValue(crt);
                }
                , value, lensValueStep.Duration).
                SetEase(lensValueStep.EaseType).
                SetUpdate(useIgnoreTimeScale).
                SetId(_cineCam.ID);
        }
    }
}