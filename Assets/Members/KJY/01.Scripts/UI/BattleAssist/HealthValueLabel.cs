using Members.KJY._01.Scripts.Agent.HealthSystem;
using TMPro;
using UnityEngine;

namespace Members.KJY._01.Scripts.UI
{
    public class HealthValueLabel : MonoBehaviour
    {
        [SerializeField] private HealthModule health;
        [SerializeField] private TMP_Text label;

        private void OnEnable()
        {
            health.OnHealthChanged += Refresh;
            Refresh(health.CurrentHealth, health.DefaultMaxHealth);
        }

        private void OnDisable() => health.OnHealthChanged -= Refresh;

        private void Refresh(float current, float maximum)
        {
            label.SetText("{0:0} / {1:0}", Mathf.Ceil(current), Mathf.Ceil(maximum));
            label.color = current <= maximum * 0.25f
                ? new Color(1f, 0.42f, 0.35f) : new Color(0.92f, 0.95f, 0.94f);
        }
    }
}
