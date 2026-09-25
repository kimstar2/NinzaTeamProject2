using System;
using Members.KJY._01.Scripts.Util;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.Enemy
{
    [CreateAssetMenu(fileName = "Enemy data", menuName = "KJY/Agent/Enemy data", order = 0)]
    public class EnemyDataSO : AgentDataSO
    {
        [field: SerializeField] public string EnemyName { get; private set; }
        [field: SerializeField] public AnimatorOverrideController EnemyAc { get; set; }
        [field: SerializeField] public Sprite EnemyImage { get; private set; }
        [field: SerializeField] public ColorSO ImageColor { get; private set; }
        [field: SerializeField] public int Cost {get; private set;}
        public event Action<EnemyDataSO> OnDead;

        public void Dead() => OnDead?.Invoke(this);
    }
}
