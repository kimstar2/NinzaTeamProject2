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
        public event Action<PlayerDataSO> OnDead;

        public void Dead() => OnDead?.Invoke(this);
    }
}
