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
            ClearResult();
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
            ClearResult();
        }

        private void ClearResult()
        {
            // 적 데이터 준비와 굴림 결과 공개는 별개다. 새 적은 빈 면에서 시작한다.
            CurrentDiceData = null;
            CurrentSkillData = default;
            _level = 1f;
            titleTMP.SetText(string.Empty);
            descTMP.SetText(string.Empty);
            // foreach (var icon in iconImage) icon.SetColor(new Color(46f,46f,46f));
            // gradeOutline.SetColor(Color.white);
            rollParticle.ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
            if (CurrentSkillData.SkillData == null) return;
            titleTMP.SetText(CurrentSkillData.SkillData.SkillName);
            descTMP.SetText(CurrentSkillData.SkillData.GetDescription(_level));
            gradeOutline.SetColor(CurrentDiceData.DiceGrade.GradeColor);
            iconImage.AsValueEnumerable().ToList().ForEach(i=>
            {
                i.SetImage(CurrentDiceData.GetIcon(_enemyData.AttackType));
                i.SetColor(Color.white);
            });
            
            rollParticle.SetParticleColor(CurrentDiceData.DiceGrade.GradeColor);
            rollParticle.PlayParticle();
            
            onDiceDataBind?.Invoke();
        }
        private void DeadRollDataBind()
        {
            if (_enemyData == null) return;
            if (CurrentDiceData == null) return;

            CurrentSkillData = CurrentDiceData.GetSkillDataStruct(_enemyData.AttackType);
            if (CurrentSkillData.SkillData == null) return;
            titleTMP.SetText(CurrentSkillData.SkillData.SkillName);
            descTMP.SetText(CurrentSkillData.SkillData.GetDescription(_level));
            iconImage.AsValueEnumerable().ToList().ForEach(i=>i.SetImage(CurrentDiceData.GetIcon(_enemyData.AttackType)));
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
