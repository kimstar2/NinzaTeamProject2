using DevLib.ServiceLocator;
using DG.Tweening;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Service;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.Player.Dice
{
    public class PlayerDice : AbstractDice
    {
        [SerializeField] private PlayerType playerType;
        [Tooltip("행운 1일 때 강함도 100 면이 0 면보다 e^값 배 잘 나온다")]
        [SerializeField, Min(0f)] private float luckSharpness = 3f;
        private PlayerDataSO _playerData;
        private bool _isDead;
        public UnityEvent onDead;
        
        private void OnEnable()
        {
            eventChannel.AddListener<OnPlayerRoll>(HandleRoll);
            eventChannel.AddListener<OnDiceLock>(HandleDiceLock);
            eventChannel.AddListener<OnPlayerDead>(HandleDead);
            eventChannel.AddListener<OnPlayerDataReceive>(HandleDataReceive);
        }
        private void OnDisable()
        {
            roll?.Kill();
            roll = null;
            eventChannel.RemoveListener<OnPlayerRoll>(HandleRoll);
            eventChannel.RemoveListener<OnDiceLock>(HandleDiceLock);
            eventChannel.RemoveListener<OnPlayerDead>(HandleDead);
            eventChannel.RemoveListener<OnPlayerDataReceive>(HandleDataReceive);
        }

        private void HandleDataReceive(OnPlayerDataReceive evt)
        {
            if (evt.PlayerDataData.PlayerType != playerType) return;
            _playerData = evt.PlayerDataData;
            _isDead = false;
            isLock = false;
        }
        
        private void HandleRoll(OnPlayerRoll obj)
        {
            if (_isDead) return;
            if (obj.playerType == playerType || obj.playerType == PlayerType.All)
                Roll(destTrm.localPosition, GetRandom());
        }

        private void HandleDead(OnPlayerDead evt)
        {
            if (evt.PlayerType != playerType || _isDead) return;
            _isDead = true;
            roll?.Kill();
            roll = null;
            onDead?.Invoke();
        }

        private void HandleDiceLock(OnDiceLock obj)
        {
            if (obj.PlayerType != playerType) return;
            isLock = obj.IsLock;
        }
        
        protected override float GetFaceWeight(DiceFaceType faceType)
        {
            if (_playerData == null || _playerData.DiceList == null ||
                !ServiceLocator.TryGet<ILuckProvider>(out var luckProvider) || luckProvider.Luck <= 0f) return 1f;
            var skill = _playerData.DiceList.GetDiceData(faceType)?.GetSkillDataStruct(_playerData.AttackType).SkillData;
            float strength = skill != null ? skill.Strength : 0f;
            return Mathf.Exp(luckProvider.Luck * luckSharpness * strength / 100f);
        }

        protected override void CompleteRoll()
        {
            crtFaceType = isLock ? crtFaceType : diceFaces[currenRan].Type;
            eventChannel.RaiseEvent(new OnPlayerRollEnd(crtFaceType, playerType));
        }
    }
}
