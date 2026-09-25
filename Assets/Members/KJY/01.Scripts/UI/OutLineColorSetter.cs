using System;
using System.Linq;
using _TevLib.Extension.DoT;
using DG.Tweening;
using Members.KJY._01.Scripts.UI.Mono;
using Members.KJY._01.Scripts.Util;
using UnityEngine;
using ZLinq;

namespace Members.KJY._01.Scripts.UI
{
    public class OutLineColorSetter : MonoBehaviour
    {
        [SerializeField] private GradientSO gradient;
        [SerializeField] private UIMonoOutline[] outlines;
        [SerializeField] private TweenStep tweenStep;
        [SerializeField] private Transform trmId;
        private float _lastEvaluate = 0f;
        private int _id;

        private void Awake()
        {
            _id = trmId.GetInstanceID();
        }


        public void Set(float value)
        {
            DOTween.Kill(_id);

            DOTween.To(
                () => _lastEvaluate,
                x =>
                {
                    _lastEvaluate = x;
                    Color color = gradient.DefaultGradient.Evaluate(Mathf.Clamp01(x));

                    foreach (var outline in outlines)
                        outline.SetColor(color);
                },
                Mathf.Clamp01(value),
                tweenStep.Duration
            ).SetEase(tweenStep.EaseType).SetId(_id);
        }
    }
}