using System.Linq;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using Members.KJY._01.Scripts.Mono;
using Members.KJY._01.Scripts.UI.Mono;
using UnityEngine;
using ZLinq;

namespace Members.KJY._01.Scripts.Agent.Player.Dice
{
    public class PlayerDiceDataBinder : MonoModule
    {
        [SerializeField] private PlayerDataSO playerData;
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
            if (evt.PlayerType != playerData.PlayerType) return;
            DiceDataSO diceData = evt.DiceData;
            var d = diceData.SkillDataStructs
                .AsValueEnumerable()
                .Where(s => s.AgentAttackType == playerData.AttackType)
                .Select(s=>s)
                .First();
            titleTMP.SetText(d.SkillData.SkillName);
            descTMP.SetText(d.SkillData.SkillDescription);
            gradeOutline.SetColor(diceData.DiceGrade.GradeColor);
            iconImage.ToList().ForEach(i=>i.SetImage(diceData.Icon));
            rollParticle.SetParticleColor(diceData.DiceGrade.GradeColor);
            rollParticle.PlayParticle();
        }
        
        private void OnValidate()
        {
            if (playerData == null) return;
            gameObject.name = $"{nameof(PlayerDiceDataBinder)} ({playerData.PlayerType})";
        }
    }
}