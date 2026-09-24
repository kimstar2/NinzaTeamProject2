using System;
using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using Members.KJY._01.Scripts.Mono;
using Members.KJY._01.Scripts.UI.Mono;
using UnityEngine;
using UnityEngine.Events;
using ZLinq;

namespace Members.KJY._01.Scripts.Agent.Enemy.Dice
{
    public class EnemyDiceDataBinder : MonoBehaviour
    {
        [SerializeField] private EnemyType enemyType;
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private UIMonoTMP titleTMP;
        [SerializeField] private UIMonoTMP descTMP;
        [SerializeField] private UIMonoOutline gradeOutline;
        [SerializeField] private UIMonoImage[] iconImage;
        [SerializeField] private MonoParticle rollParticle;
        private EnemyDataSO _enemyData;
        private float _level = 1f;
        public UnityEvent onDiceDataBind;
        public SkillDataStruct CurrentSkillData { get; private set; }
        public DiceDataSO CurrentDiceData { get; private set; }
        
        public void OnEnable()
        {
            eventChannel.AddListener<OnEnemyDiceDataBind>(HandleDiceDataBind);
            eventChannel.AddListener<OnEnemyDataChanged>(HandleEnemyDataChanged);
            eventChannel.AddListener<OnEnemyDead>(HandleEnemyDead);
        }

        private void HandleEnemyDataChanged(OnEnemyDataChanged obj)
        {
            if (obj.EnemyType != enemyType) return;
            _enemyData = obj.EnemyData;
            DataBind();
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnEnemyDiceDataBind>(HandleDiceDataBind);
            eventChannel.RemoveListener<OnEnemyDataChanged>(HandleEnemyDataChanged);
            eventChannel.RemoveListener<OnEnemyDead>(HandleEnemyDead);
        }

        private void HandleEnemyDead(OnEnemyDead evt)
        {
            if (evt.enemyType != enemyType || evt.isDead) return;
            CurrentDiceData = null;
            CurrentSkillData = default;
        }

        private void HandleDiceDataBind(OnEnemyDiceDataBind evt)
        {
            if (evt.EnemyType != enemyType) return;
            CurrentDiceData = evt.DiceData;
            _level = evt.Level;

            switch (evt.RollType)
            {
                case EnemyRollType.DeadRoll:
                    DeadRollDataBind();
                    break;
                case EnemyRollType.Roll:
                    DataBind();
                    break;
            }
        }

        private void DataBind()
        {
            if (_enemyData == null) return;
            if (CurrentDiceData == null) return;
            
            CurrentSkillData = CurrentDiceData.GetSkillDataStruct(_enemyData.AttackType);
            titleTMP.SetText(CurrentSkillData.SkillData.SkillName);
            descTMP.SetText(CurrentSkillData.SkillData.GetDescription(_level));
            gradeOutline.SetColor(CurrentDiceData.DiceGrade.GradeColor);
            iconImage.AsValueEnumerable().ToList().ForEach(i=>i.SetImage(CurrentDiceData.Icon));
            
            rollParticle.SetParticleColor(CurrentDiceData.DiceGrade.GradeColor);
            rollParticle.PlayParticle();
            
            onDiceDataBind?.Invoke();
        }
        private void DeadRollDataBind()
        {
            if (_enemyData == null) return;
            if (CurrentDiceData == null) return;

            CurrentSkillData = CurrentDiceData.GetSkillDataStruct(_enemyData.AttackType);
            titleTMP.SetText(CurrentSkillData.SkillData.SkillName);
            descTMP.SetText(CurrentSkillData.SkillData.GetDescription(_level));
            iconImage.AsValueEnumerable().ToList().ForEach(i=>i.SetImage(CurrentDiceData.Icon));
            rollParticle.SetParticleColor(Color.orangeRed);
            rollParticle.PlayParticle();
            onDiceDataBind?.Invoke();
        }

        private void OnValidate()
        {
            gameObject.name = $"{nameof(EnemyDiceDataBinder)} ({enemyType})";
        }
    }
}
