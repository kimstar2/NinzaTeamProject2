using System;
using System.Collections.Generic;
using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.Player.Dice
{
    [Serializable]
    public class PlayerDiceRollCheck
    {
        [field:SerializeField] public PlayerType PlayerType {get; private set;}
        [field:SerializeField] public bool DiceRollEnd {get; private set;}
        public UnityEvent<PlayerDiceRollData> onRollEnd;
        public UnityEvent<PlayerType> onRoll;
        
        public void OnRollEnd(DiceFaceType diceFaceType)
        {
            onRollEnd?.Invoke(new PlayerDiceRollData(PlayerType, diceFaceType));
            DiceRollEnd = true;
        }
        
        public void OnRoll()
        {
            onRoll?.Invoke(PlayerType);
            DiceRollEnd = false;
        }
    }
    
    public class PlayerDiceRollManager : AbstractDiceRollManager
    {
        [field:SerializeField] public List<PlayerDiceRollCheck> DiceRollCheckList {get; private set;}
        [SerializeField] private float maxRiskLevel;
        [SerializeField] private float riskLevel;
        [SerializeField] private float riskLevelIncrease; // 테스트용임
        [Min(1f),SerializeField] private float riskLevelIncreasePer; // 테스트용임
        private int _rollCount;
        
        public UnityEvent<float> onRiskLevelChanged;
        
        private void OnEnable()
        {
            eventChannel.AddListener<OnPlayerRollEnd>(HandleRollEnd);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnPlayerRollEnd>(HandleRollEnd);
        }
        

        private void HandleRollEnd(OnPlayerRollEnd obj)
        {
            PlayerDiceRollCheck check = DiceRollCheckList.Find(x => x.PlayerType == obj.PlayerType);
            check.OnRollEnd(obj.DiceFaceType);
            
            AllDiceRollEnd = DiceRollCheckList.TrueForAll(x => x.DiceRollEnd);

            if (AllDiceRollEnd)
            {
                onAllDiceRollEnd?.Invoke();
            }
        }

        public void Roll()
        {
            RollLogic();
            CalcRisk();
        }

        protected override void RollLogic()
        {
            if (!AllDiceRollEnd) return;
            foreach (PlayerDiceRollCheck check in DiceRollCheckList)
            {
                check.OnRoll();
                eventChannel.RaiseEvent(new OnPlayerRoll(check.PlayerType));
            }
            
            AllDiceRollEnd = false;
        }

        private void CalcRisk()
        {
            _rollCount++;
            riskLevel += riskLevelIncreasePer * _rollCount * riskLevelIncrease;
            onRiskLevelChanged?.Invoke(riskLevel / maxRiskLevel);
        }

        public void ResetRollCount() => _rollCount = 0;
    }
}