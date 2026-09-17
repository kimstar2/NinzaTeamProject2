using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Dice.Interface;
using Members.KJY._01.Scripts.Events.Dice.Selector;
using Members.KJY._01.Scripts.UI.Mono;
using Members.KJY._01.Scripts.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.Player
{
    public class PlayerSelector : AbstractSelector
    {
        [field: SerializeField] public PlayerDataSO PlayerData { get; private set; }
        [field: SerializeField] public GradientSO LineColor { get; private set; }
        [field: SerializeField] public ColorSO PlayerColor {get; private set;}
        [SerializeField] private UIMonoOutline targetOutline;
        private bool _hasTarget;

        public UnityEvent onSetTarget;

        private void Start()
        {
            targetOutline.SetColor(PlayerColor);
            ValidateData();
        }

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
                eventChannel.RaiseEvent(new OnPlayerUnSelect(PlayerData.PlayerType));
                onUnSelect?.Invoke();
                return;
            }
            Debug.Log("뱅");
            IsSelect = true;
            onSelect?.Invoke();
        }

        protected override void UnSelect()
        {
            IsSelect = false;
            if (!_hasTarget)
                onUnSelect?.Invoke();
        }

        private void HandleDiceSelect(OnPlayerSelect evt)
        {
            if (evt.PlayerSelector.PlayerData.PlayerType == PlayerData.PlayerType)
            {
                eventChannel.RaiseEvent(new OnPlayerUnSelect(PlayerData.PlayerType));
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
        
        public void OffSetTarget()
        {
            SetHasTarget(false);
            UnSelect();
        }


        private bool CheckIsSelect()
        {
            (PlayerType playerType, bool isSelect) tuple = ServiceLocator.Get<IGetIsSelectService>().GetIsSelect();
            if (PlayerData.PlayerType == PlayerType.None) return false;
            
            return PlayerData.PlayerType != tuple.playerType && tuple.isSelect;
        }

        private void SetHasTarget(bool hasTarget) => _hasTarget = hasTarget;
        
        public override void OnAttackCommand() => UnSelect();
        
        public override void ApplyDamage(float damage)
        {
            HealthModule.TakeDamage(damage);
        }

        public override void ApplyHeal(float heal) { }
        
        private void ValidateData()
        {
            IconImage.SetImage(PlayerData.PlayerImage);
            IconImage.SetColor(PlayerData.ImageColor);
            MyAgent.AgentRenderer.SetSprite(PlayerData.PlayerImage);
            MyAgent.AgentRenderer.SetColor(PlayerData.ImageColor);
            
            MyAgent.AnimCompo.SetController(PlayerData.AnimCon);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (PlayerData == null) return;
            gameObject.name = $"{nameof(PlayerSelector)} ({PlayerData.PlayerType})";
        }

#endif
    }
}