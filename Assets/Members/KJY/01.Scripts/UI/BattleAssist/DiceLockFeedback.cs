using _TevLib.Extension.DoT;
using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using UnityEngine;

namespace Members.KJY._01.Scripts.UI
{
    public class DiceLockFeedback : MonoBehaviour
    {
        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private PlayerType playerType;
        [SerializeField] private DiceLockChecker diceLock;
        [SerializeField] private GameObject badge;
        [SerializeField] private TweenSequencer lockMotion;
        [SerializeField] private ParticleSystem sparks;
        private bool _locked;

        private void OnEnable()
        {
            eventChannel.AddListener<OnDiceLock>(HandleLock);
            eventChannel.AddListener<OnPlayerDead>(HandleDead);
            SetLocked(diceLock.IsLocked);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnDiceLock>(HandleLock);
            eventChannel.RemoveListener<OnPlayerDead>(HandleDead);
            lockMotion.Stop();
            sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void HandleLock(OnDiceLock evt)
        {
            if (evt.PlayerType == playerType) SetLocked(evt.IsLock);
        }

        private void HandleDead(OnPlayerDead evt)
        {
            if (evt.PlayerType == playerType && evt.IsDead) SetLocked(false);
        }

        private void SetLocked(bool value)
        {
            bool changed = _locked != value;
            _locked = value;
            badge.SetActive(value);
            if (value && changed)
            {
                lockMotion.Sequence();
                sparks.Play();
            }
            else if (!value)
            {
                lockMotion.Stop();
                sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
