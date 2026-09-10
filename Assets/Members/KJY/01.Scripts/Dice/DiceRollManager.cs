using System;
using System.Collections.Generic;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Dice
{
    [Serializable]
    public class DiceRollCheck
    {
        [field:SerializeField] public PlayerType PlayerType {get; private set;}
        [field:SerializeField] public bool DiceRollEnd {get; private set;}
        public UnityEvent<DiceRollData> onRollEnd;
        public UnityEvent<PlayerType> onRoll;
        
        public void OnRollEnd(DiceFaceType diceFaceType)
        {
            onRollEnd?.Invoke(new DiceRollData(PlayerType, diceFaceType));
            DiceRollEnd = true;
        }
        
        public void OnRoll()
        {
            onRoll?.Invoke(PlayerType);
            DiceRollEnd = false;
        }
    }
    
    public class DiceRollManager : ModuleOwner
    {
        [field:SerializeField] public List<DiceRollCheck> DiceRollCheckList {get; private set;}
        [field:SerializeField] public bool AllDiceRollEnd {get; private set;}
        [SerializeField] private EventChannelSO eventChannel;
        public UnityEvent onAllDiceRollEnd;
        
        private void OnEnable()
        {
            eventChannel.AddListener<OnRoll>(HandleRoll);
            eventChannel.AddListener<OnRollEnd>(HandleRollEnd);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnRoll>(HandleRoll);
            eventChannel.RemoveListener<OnRollEnd>(HandleRollEnd);
        }

        private void HandleRoll(OnRoll garbageEvent)
        {
            AllDiceRollEnd = false;
            foreach (DiceRollCheck check in DiceRollCheckList)
                check.OnRoll();
        }

        private void HandleRollEnd(OnRollEnd obj)
        {
            DiceRollCheck check = DiceRollCheckList.Find(x => x.PlayerType == obj.PlayerType);
            check.OnRollEnd(obj.DiceFaceType);
            
            AllDiceRollEnd = DiceRollCheckList.TrueForAll(x => x.DiceRollEnd);
            
            if (AllDiceRollEnd)
                onAllDiceRollEnd?.Invoke();
        }
    }
}