using _TevLib.Extension.DoT;
using DG.Tweening;
using Members.KJY._01.Scripts.UI.Mono;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.HealthSystem
{
    public class AgentUIHealthBar : AbstractHealthBar
    {
        [SerializeField] private MonoSlider healthBar;
        
        
        
        public override void SetHealthBar(float value)
        {
            healthBar.Slider.DOValue(value, tweenStep.Duration).SetEase(tweenStep.EaseType);
        }
    }
}