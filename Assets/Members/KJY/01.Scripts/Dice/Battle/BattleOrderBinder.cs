using System;
using System.Collections.Generic;
using _TevLib.Extension.DoT;
using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Flags;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Battle
{
    public class BattleOrderBinder : MonoBehaviour , IRequirePooling
    {
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private BattleOrder orderPrefab;
        [SerializeField] private TweenSequencer onChainSeq, offChainSeq;        
        private Dictionary<PlayerType, BattleOrder> _battleOrders = new();

        private void OnEnable()
        {
            eventChannel.AddListener<OnBattleChainChanged>(HandleBattleChainChanged);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnBattleChainChanged>(HandleBattleChainChanged);
        }

        private void HandleBattleChainChanged(OnBattleChainChanged evt)
        {
            if (evt.chainCount > 0)
                onChainSeq.Sequence();
            else
                offChainSeq.Sequence();
            
            if (evt.isAdd)
            {
                BattleOrder battleOrder = Instantiate(orderPrefab , transform);
                battleOrder.Image.SetImage(evt.playerData.PlayerImage);
                battleOrder.Image.SetColor(evt.playerData.ImageColor);
                _battleOrders.Add(evt.playerData.PlayerType, battleOrder);
                battleOrder.AddSeq.Sequence();
            }
            else
            {
                if (_battleOrders.Remove(evt.playerData.PlayerType, out BattleOrder battleOrder))
                {
                    battleOrder.RemoveSeq.Sequence();
                }
            }
        }
    }
}