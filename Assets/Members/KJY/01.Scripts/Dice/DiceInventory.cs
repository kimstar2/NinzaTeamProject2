using System;
using System.Collections;
using System.Collections.Generic;
using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Members.KJY._01.Scripts.Dice
{
    public class DiceInventory : MonoBehaviour
    {
        [SerializeField] private PlayerType playerType;
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private SetDiceFaceSO setDiceFace;
        [SerializeField] private DiceDataListSO defaultDiceDataList;
        [field:SerializeField] public DiceDataListSO RunTimeDiceDataList {get; private set;}

        private void Awake()
        {
            RunTimeDiceDataList = defaultDiceDataList.GetRuntimeList();
        }

        private void Start()
        {
            DiceDataChanged();
        }

        private void OnEnable()
        {
            eventChannel.AddListener<OnRollEnd>(HandleRollEnd);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnRollEnd>(HandleRollEnd);
        }

        [ContextMenu("Changed")]
        public void DiceDataChanged()
        {
            eventChannel.RaiseEvent(new OnDiceDataChanged(playerType, RunTimeDiceDataList));
        }

        private void HandleRollEnd(OnRollEnd evt)
        {
            if (evt.PlayerType != playerType) return;
            DiceDataSO diceData = RunTimeDiceDataList.GetDiceData(evt.DiceFaceType);
            eventChannel.RaiseEvent(new OnDiceDataBind(diceData ,playerType));
        }
        
    }
}