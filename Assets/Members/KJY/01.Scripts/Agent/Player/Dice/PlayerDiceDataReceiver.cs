using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Dice.Data;
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
            DiceDataListSO list = evt.DiceDataList;
            FrontImage.SetSprite(list.Front.Icon);
            BackImage.SetSprite(list.Back.Icon);
            LeftImage.SetSprite(list.Left.Icon);
            RightImage.SetSprite(list.Right.Icon);
            TopImage.SetSprite(list.Top.Icon);
            BottomImage.SetSprite(list.Bottom.Icon);
        }
    }
}