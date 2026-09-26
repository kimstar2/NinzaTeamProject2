using Members.KJY._01.Scripts.Events.Dice.Selector;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using Members.KJY._01.Scripts.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.Player
{
    public class PlayerSelector : AbstractSelector
    {
        [field: SerializeField] public PlayerDataSO RuntimePlayerData { get; private set; }
        [field: SerializeField] public GradientSO LineColor { get; private set; }
        [field: SerializeField] public ColorSO PlayerColor {get; private set;}
        private bool _hasTarget;

        public UnityEvent onSetTarget;
        public UnityEvent onDead;
        public UnityEvent onInit;
        [Min(0f)] public float playerLevel = 1f;

        public void Init(PlayerDataSO data)
        {
            RuntimePlayerData = data;
            AgentData = data;
            IsDead = data.IsDead;
            eventChannel.RaiseEvent(new OnPlayerDead(data.PlayerType, data.IsDead));
            if (data.IsDead) return;
            EnterBattle();
            IsDead = false;
            OffSetTarget();
            ValidateData();
            MyAgent.HealthModule.InitHealth(data.MaxHealth,data.CurrentHealth);
            eventChannel.RaiseEvent(new OnDiceLock(false, RuntimePlayerData.PlayerType));
            eventChannel.RaiseEvent(new OnPlayerDataReceive(data));
            DiceInventory.DiceDataChanged();
            onInit?.Invoke();
            // eventChannel.RaiseEvent(new OnPlayerRoll(data.PlayerType));
        }

        // protected override void Start()
        // {
        //     base.Start();
        //     AgentData = RuntimePlayerData;
        //     ValidateData();
        // }

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
            if (IsDead) return;
            if (_hasTarget)
            {
                UnSelect(); // 연결된 애를 다시 누르면 연결 취소
                return;
            }
            IsSelect = true;
            eventChannel.RaiseEvent(new OnPlayerSelect(this)); // 상태부터 바꿔야 받는 쪽도 선택된 걸 앎
            onSelect?.Invoke();
        }

        protected override void UnSelect()
        {
            IsSelect = false;
            SetHasTarget(false);
            eventChannel.RaiseEvent(new OnPlayerUnSelect(RuntimePlayerData.PlayerType));
            onUnSelect?.Invoke();
        }

        private void HandleDiceSelect(OnPlayerSelect evt)
        {
            if (evt.PlayerSelector == this || !IsSelect) return;
            IsSelect = false; // 다른 애 선택하면 대기 표시만 끔. 이미 연결된 애는 그대로
            onUnSelect?.Invoke();
        }

        public void OnSetTarget()
        {
            SetHasTarget(true);
            IsSelect = false;
            onSetTarget?.Invoke();
        }
        
        public void OffSetTarget()
        {
            SetHasTarget(false);
            IsSelect = false;
            onUnSelect?.Invoke();
        }


        protected override void HandleDead()
        {
            if (IsDead) return;
            base.HandleDead();
            UnSelect();
            eventChannel.RaiseEvent(new OnPlayerDead(RuntimePlayerData.PlayerType, true));
            onDead?.Invoke();
        }

        private void SetHasTarget(bool hasTarget) => _hasTarget = hasTarget;
        
        public override void OnAttackCommand() => OffSetTarget();
        
        public override void ApplyDamage(float damage)
        {
            RuntimePlayerData.TakeDamage(damage);
            MyAgent.HealthModule.TakeDamage(damage);
        }

        public override void ApplyHeal(float heal)
        {
            RuntimePlayerData.Heal(heal);
            MyAgent.HealthModule.Heal(heal);
        }

        private void ValidateData()
        {
            IconImage.SetImage(RuntimePlayerData.PlayerImage);
            IconImage.SetColor(RuntimePlayerData.ImageColor);
            MyAgent.AgentRenderer.SetSprite(RuntimePlayerData.PlayerImage);
            MyAgent.AgentRenderer.SetColor(RuntimePlayerData.ImageColor);
            
            MyAgent.AnimCompo.SetController(RuntimePlayerData.AnimCon);
        }

        public override float GetLevel()
        {
            return playerLevel > 0f ? playerLevel : 1f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (RuntimePlayerData == null) return;
            gameObject.name = $"{nameof(PlayerSelector)} ({RuntimePlayerData.PlayerType})";
        }

#endif
    }
}
