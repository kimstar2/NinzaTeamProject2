using System;
using System.Diagnostics.Tracing;
using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Mono;
using Members.KJY._01.Scripts.UI.Mono;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice
{
    public class DiceDataReceiver : MonoBehaviour
    {
        [SerializeField] private PlayerType playerType;
        [SerializeField] private EventChannelSO eventChannel;
        [field:SerializeField] public MonoSprite FrontImage { get; private set; }
        [field:SerializeField] public MonoSprite BackImage { get; private set; }
        [field:SerializeField] public MonoSprite LeftImage { get; private set; }
        [field:SerializeField] public MonoSprite RightImage { get; private set; }
        [field:SerializeField] public MonoSprite TopImage { get; private set; }
        [field:SerializeField] public MonoSprite BottomImage { get; private set; }

        private void OnEnable()
        {
            eventChannel.AddListener<OnDiceDataChanged>(HandleDiceDataChanged);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnDiceDataChanged>(HandleDiceDataChanged);
        }
        
        private void HandleDiceDataChanged(OnDiceDataChanged evt)
        {
            if (evt.PlayerType != playerType) return;
            Debug.Log("Changed");
            DiceDataListSO list = evt.DiceDataList;
            FrontImage.SetSprite(list.Front.Icon);
            BackImage.SetSprite(list.Back.Icon);
            LeftImage.SetSprite(list.Left.Icon);
            RightImage.SetSprite(list.Right.Icon);
            TopImage.SetSprite(list.Top.Icon);
            BottomImage.SetSprite(list.Bottom.Icon);
        }

        public void SetFaceColor(Color color)
        {
            FrontImage.SetColor(color); 
            BackImage.SetColor(color); 
            LeftImage.SetColor(color); 
            RightImage.SetColor(color); 
            TopImage.SetColor(color); 
            BottomImage.SetColor(color);
            
        }
    }
}