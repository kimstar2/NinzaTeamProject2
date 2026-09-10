using System;
using DevLib.CoreLib.Runtime;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Dice.Interface;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Events.Dice.Selector;
using Members.KJY._01.Scripts.UI.Mono;
using Members.KJY._01.Scripts.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Dice
{
    public class DiceSelector : AbstractSelector
    {
        [field: SerializeField] public PlayerType PlayerType { get; private set; }
        [field: SerializeField] public GradientSO LineColor { get; private set; }
        [field: SerializeField] public ColorSO PlayerColor {get; private set;}
        [SerializeField] private UIMonoImage targetImage;
        [SerializeField] private UIMonoOutline targetOutline;
        [SerializeField] private Color selectColor;
        [SerializeField] private Color unSelectColor;
        private bool _hasTarget;

        public UnityEvent onSetTarget;


        private void Start() => targetOutline.SetColor(PlayerColor);

        private void OnEnable()
        {
            eventChannel.AddListener<OnPlayerSelect>(HandleDiceSelect);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<OnPlayerSelect>(HandleDiceSelect);
        }

        protected override void Select()
        {
            if (CheckIsSelect())
                return;
            
            eventChannel.RaiseEvent(new OnPlayerSelect(this));
            
            if (_hasTarget)
            {
                SetHasTarget(false);
                eventChannel.RaiseEvent(new OnPlayerUnSelect(PlayerType));
                return;
            }
            
            targetImage.SetColor(selectColor);
            IsSelect = true;
            onSelect?.Invoke();
        }

        protected override void UnSelect()
        {
            targetImage.SetColor(unSelectColor);
            IsSelect = false;
            if (!_hasTarget)
                onUnSelect?.Invoke();
        }

        private void HandleDiceSelect(OnPlayerSelect evt)
        {
            if (evt.DiceSelector.PlayerType == PlayerType)
            {
                eventChannel.RaiseEvent(new OnPlayerUnSelect(PlayerType));
                onUnSelect?.Invoke();
            }
            else
                UnSelect(); // 내가 아니라면? UnSelect
        }

        public void OnSetTarget()
        {
            SetHasTarget(true);
            UnSelect();
            onSetTarget?.Invoke();
        }

        private bool CheckIsSelect()
        {
            (PlayerType playerType, bool isSelect) tuple = ServiceLocator.Get<IGetIsSelectService>().GetIsSelect();
            if (PlayerType == PlayerType.None) return false;
            
            return PlayerType != tuple.playerType && tuple.isSelect;
        }

        private void SetHasTarget(bool hasTarget) => _hasTarget = hasTarget;
        

#if UNITY_EDITOR
        private void OnValidate()
        {
            gameObject.name = $"{nameof(DiceSelector)} ({PlayerType})";
        }

#endif
    }
}