using System;
using DevLib.ModuleSystem;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent
{
    public class HealthModule : MonoModule
    {
        [field: SerializeField] public float MaxHealth { get; private set; } = 10;
        private float _currentHealth;
        
        public delegate void HealthChanged(float health , float maxHealth); // 매개변수명 확인을 위함
        public event HealthChanged OnHealthChanged;
        public event Action OnDead;
        private bool _isDead;

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            InitHealth();
        }

        public void InitHealth()
        {
            _isDead = false;
            CurrentHealth = MaxHealth;
        }
        
        public float CurrentHealth
        {
            get => _currentHealth;
            private set
            {
                _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
                OnHealthChanged?.Invoke(_currentHealth , MaxHealth);
            }
        }
        
        
        public void TakeDamage(float damage)
        {
            if (_isDead) return;
            
            Debug.Log($"아야 {damage}");
            CurrentHealth -= damage;
            
            if (CurrentHealth <= 0)
            {
                _isDead = true;
                OnDead?.Invoke();
            }
        }
    }
}
