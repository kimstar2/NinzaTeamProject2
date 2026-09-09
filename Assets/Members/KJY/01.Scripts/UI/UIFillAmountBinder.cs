using _TevLib.Extension.DoT;
using DG.Tweening;
using Members.KJY._01.Scripts.UI.Mono;
using UnityEngine;

namespace Members.KJY._01.Scripts.UI
{
    public class UIFillAmountBinder : UIMonoImage
    {
        [SerializeField] private TweenStep step;
        [SerializeField] private Transform trmId;
        private int _id;
        
        protected override void Awake()
        {
            base.Awake();
            _id = trmId.GetHashCode();
        }

        public void FillAmount(float amount)
        {
            DOTween.Kill(_id);
            
            Image.DOFillAmount(amount,step.Duration)
                .SetEase(step.EaseType)
                .SetId(_id);
        }
    }
}
