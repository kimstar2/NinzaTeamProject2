using _TevLib.Extension.DoT;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.HealthSystem
{
    public abstract class AbstractHealthBar : MonoBehaviour
    {
        [SerializeField] protected TweenStep tweenStep;
        
        protected virtual void HandleHealthChanged(float health, float maxHealth) => SetHealthBar(health/maxHealth);
        public abstract void SetHealthBar(float value);
    }
}