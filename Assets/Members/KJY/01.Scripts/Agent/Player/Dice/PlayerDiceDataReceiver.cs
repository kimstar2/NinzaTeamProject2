using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.Player.Dice
{
    public class PlayerDiceDataReceiver : AbstractDiceDataReceiver
    {
        [SerializeField] private PlayerType playerType;


        private void OnEnable()
        {
            eventChannel.AddListener<OnPlayerDiceDataChanged>(HandleDiceDataChanged);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnPlayerDiceDataChanged>(HandleDiceDataChanged);
        }
        
        private void HandleDiceDataChanged(OnPlayerDiceDataChanged evt)
        {
            if (evt.PlayerType != playerType) return;
            ApplyDiceData(evt.DiceDataList, evt.AttackType);
        }
    }
}
