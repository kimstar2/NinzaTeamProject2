using System;
using Members.KJY._01.Scripts.Util;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.Player
{
    [CreateAssetMenu(fileName = "Player data", menuName = "KJY/Agent/Player data", order = 0)]
    public class PlayerDataSO : AgentDataSO
    {
        [field:SerializeField] public PlayerType PlayerType {get; private set;}
        [field:SerializeField] public AnimatorOverrideController AnimCon {get; private set;}
        [field:SerializeField] public Sprite PlayerImage {get; private set;}
        [field:SerializeField] public ColorSO ImageColor {get; private set;}
        
        [field:SerializeField] public int Cost {get; private set;}
        [field: SerializeField] public float CurrentHealth { get; private set; } // 런타임
        public bool IsDead => CurrentHealth <= 0;
        
        public void TakeDamage(float damage) // 런타임
        {
            CurrentHealth -= damage;
        }

        public void Heal(float heal)
        {
            if (IsDead || heal <= 0f || float.IsNaN(heal) || float.IsInfinity(heal)) return;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + heal);
        }

        public void Init()
        {
            CurrentHealth = MaxHealth;
        }
    }
}
