using System.Collections.Generic;
using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using UnityEngine;

namespace Members.KJY._01.Scripts
{
    public class GetDiceList : MonoBehaviour
    {
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private List<DiceDataSO> GetDiceFaceList = new();

        private void OnEnable()
        {
            eventChannel.AddListener<OnEnemyDiceDataBind>(HandleDiceDataBind);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnEnemyDiceDataBind>(HandleDiceDataBind);
        }

        private void HandleDiceDataBind(OnEnemyDiceDataBind evt)
        {
            if (evt.RollType != EnemyRollType.DeadRoll) return;
            AddDice(evt.DiceData);
        }

        public void AddDice(DiceDataSO dice)
        {
            if (dice == null) return;
            GetDiceFaceList.Add(dice);
        }
    }
}
