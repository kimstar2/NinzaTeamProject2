using System;
using System.Collections.Generic;
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
        
        private void OnEnable()
        {
            eventChannel.AddListener<OnPlayerRoll>(HandleRoll);
            eventChannel.AddListener<OnPlayerRollEnd>(HandleRollEnd);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnPlayerRoll>(HandleRoll);
            eventChannel.RemoveListener<OnPlayerRollEnd>(HandleRollEnd);
        }

        private void HandleRoll(OnPlayerRoll garbageEvent)
        {
            AllDiceRollEnd = false;
            foreach (PlayerDiceRollCheck check in DiceRollCheckList)
                check.OnRoll();
        }

        private void HandleRollEnd(OnPlayerRollEnd obj)
        {
            PlayerDiceRollCheck check = DiceRollCheckList.Find(x => x.PlayerType == obj.PlayerType);
            check.OnRollEnd(obj.DiceFaceType);
            
            AllDiceRollEnd = DiceRollCheckList.TrueForAll(x => x.DiceRollEnd);
            
            if (AllDiceRollEnd)
                onAllDiceRollEnd?.Invoke();
        }
    }
}