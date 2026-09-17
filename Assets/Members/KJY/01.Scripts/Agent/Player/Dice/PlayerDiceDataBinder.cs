using System.Linq;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using Members.KJY._01.Scripts.Mono;
using Members.KJY._01.Scripts.UI.Mono;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.Player.Dice
{
    public class PlayerDiceDataBinder : MonoModule
    {
        [SerializeField] private PlayerType playerType;
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private UIMonoTMP titleTMP;
        [SerializeField] private UIMonoTMP descTMP;
        [SerializeField] private UIMonoOutline gradeOutline;
        [SerializeField] private UIMonoImage[] iconImage;
        [SerializeField] private MonoParticle rollParticle;

        public void OnEnable()
        {
            eventChannel.AddListener<OnPlayerDiceDataBind>(HandleDiceDataBind);
        }
        private void OnDisable()
        {
            eventChannel.RemoveListener<OnPlayerDiceDataBind>(HandleDiceDataBind);
        }

        private void HandleDiceDataBind(OnPlayerDiceDataBind evt) // 후에 복잡 해지면 이벤트로 옮길수 도 있을듯[
        {
            if (evt.PlayerType != playerType) return;
            DiceDataSO diceData = evt.DiceData;
            titleTMP.SetText(diceData.SkillData.SkillName);
            descTMP.SetText(diceData.SkillData.SkillDescription);
            gradeOutline.SetColor(diceData.DiceGrade.GradeColor);
            iconImage.ToList().ForEach(i=>i.SetImage(diceData.Icon));
            rollParticle.SetParticleColor(diceData.DiceGrade.GradeColor);
            rollParticle.PlayParticle();
        }
        
        private void OnValidate()
        {
            gameObject.name = $"{nameof(PlayerDiceDataBinder)} ({playerType})";
        }
    }
}