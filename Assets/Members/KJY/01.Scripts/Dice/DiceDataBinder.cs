using System.Linq;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Player;
using Members.KJY._01.Scripts.UI.Mono;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice
{
    public class DiceDataBinder : MonoModule
    {
        [SerializeField] private PlayerType playerType;
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private UIMonoTMP titleTMP;
        [SerializeField] private UIMonoTMP descTMP;
        [SerializeField] private UIMonoImage[] iconImage;

        public void OnEnable()
        {
            eventChannel.AddListener<OnDiceDataBind>(HandleDiceDataBind);
        }
        private void OnDisable()
        {
            eventChannel.RemoveListener<OnDiceDataBind>(HandleDiceDataBind);
        }

        private void HandleDiceDataBind(OnDiceDataBind evt)
        {
            if (evt.PlayerType != playerType) return;
            DiceDataSO diceData = evt.DiceData;
            titleTMP.SetText(diceData.SkillName);
            descTMP.SetText(diceData.Description);
            iconImage.ToList().ForEach(i=>i.SetImage(diceData.Icon));
        }
    }
}