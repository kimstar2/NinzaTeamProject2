using System;
using DevLib.ModuleSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.HealthSystem
{
    public class HealthModule : MonoModule
    {
        [field: SerializeField] public float DefaultMaxHealth { get; private set; } = 10;
        private float _currentHealth;

        public UnityEvent<float> onTakeDamaged;
        public delegate void HealthChanged(float health , float maxHealth); // 매개변수명 확인을 위함
        public event HealthChanged OnHealthChanged;
        public event Action OnDead;
        private bool _isDead;

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            InitHealth(DefaultMaxHealth);
        }

        public void InitHealth(float maxHealth , bool initHealth = true)
        {
            _isDead = false;
            DefaultMaxHealth = maxHealth;
            if (initHealth)
                CurrentHealth = DefaultMaxHealth;
        }
        
        public float CurrentHealth
        {
            get => _currentHealth;
            private set
            {
                _currentHealth = Mathf.Clamp(value, 0, DefaultMaxHealth);
                OnHealthChanged?.Invoke(_currentHealth , DefaultMaxHealth);
                Debug.Log("d");
            }
        }
        
        
        public void TakeDamage(float damage)
        {
            if (_isDead) return;
            
            onTakeDamaged?.Invoke(damage);
            CurrentHealth -= damage;
            
            if (CurrentHealth <= 0)
            {
                Debug.Log("dead");
                _isDead = true;
                OnDead?.Invoke();
            }
        }

        public void Heal(float heal)
        {
            if (_isDead) return; // 회복이지 부활은 아님
            CurrentHealth += Mathf.Max(0f, heal);
        }
    }
}
