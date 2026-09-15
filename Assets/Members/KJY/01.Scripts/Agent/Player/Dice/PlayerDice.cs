using DG.Tweening;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.Player.Dice
{
    public class PlayerDice : AbstractDice
    {
        [SerializeField] private PlayerType playerType;
        
        private void OnEnable()
        {
            eventChannel.AddListener<OnPlayerRoll>(HandleRoll);
            eventChannel.AddListener<OnDiceLock>(HandleDiceLock);
        }
        private void OnDisable()
        {
            roll?.Kill();
            roll = null;
            eventChannel.RemoveListener<OnPlayerRoll>(HandleRoll);
        }
        
        private void HandleRoll(OnPlayerRoll obj) => Roll(destTrm.localPosition,GetRandom());
        private void HandleDiceLock(OnDiceLock obj)
        {
            if (obj.PlayerType != playerType) return;
            isLock = obj.IsLock;
        }
        
        protected override void CompleteRoll()
        {
            crtFaceType = isLock ? crtFaceType : diceFaces[currenRan].Type;
            eventChannel.RaiseEvent(new OnPlayerRollEnd(crtFaceType, playerType));
        }
    }
}