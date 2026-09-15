using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using Members.KJY._01.Scripts.UI.Mono;
using UnityEngine;
using ZLinq;

namespace Members.KJY._01.Scripts.Agent.Enemy.Dice
{
    public class EnemyDiceDataBinder : MonoBehaviour
    {
        [SerializeField] private EnemyNumber enemyType;
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private UIMonoTMP titleTMP;
        [SerializeField] private UIMonoTMP descTMP;
        [SerializeField] private UIMonoOutline gradeOutline;
        [SerializeField] private UIMonoImage[] iconImage;

        public void OnEnable()
        {
            eventChannel.AddListener<OnEnemyDiceDataBind>(HandleDiceDataBind);
        }
        private void OnDisable()
        {
            eventChannel.RemoveListener<OnEnemyDiceDataBind>(HandleDiceDataBind);
        }

        private void HandleDiceDataBind(OnEnemyDiceDataBind evt)
        {
            if (evt.EnemyType != enemyType) return;
            
            DiceDataSO diceData = evt.DiceData;
            titleTMP.SetText(diceData.SkillData.SkillName);
            descTMP.SetText(diceData.SkillData.SkillDescription);
            gradeOutline.SetColor(diceData.DiceGrade.GradeColor);
            iconImage.AsValueEnumerable().ToList().ForEach(i=>i.SetImage(diceData.Icon));
        }
        
        private void OnValidate()
        {
            gameObject.name = $"{nameof(EnemyDiceDataBinder)} ({enemyType})";
        }
    }
}