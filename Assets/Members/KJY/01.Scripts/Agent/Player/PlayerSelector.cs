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

        public UnityEvent onSetTarget;
        public UnityEvent onDead;
        public UnityEvent onInit;
        [Min(0f)] public float playerLevel = 1f;

        public void Init(PlayerDataSO data)
        {
            RuntimePlayerData = data;
            AgentData = data;
            if (data.DiceList != null) DiceInventory.SetDiceList(data.DiceList);
            global::Members.KJY._01.Scripts.Dice.Data.DiceCatalogProgress.Discover(data.DiceList);
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

        public override void SelectToggle() => Select();

        protected override void Select()
        {
            if (IsDead) return;
            eventChannel.RaiseEvent(new OnPlayerSelect(this));
        }

        protected override void UnSelect()
        {
            IsSelect = false;
            eventChannel.RaiseEvent(new OnPlayerUnSelect(RuntimePlayerData.PlayerType));
            onUnSelect?.Invoke();
        }

        public void BeginSelection()
        {
            IsSelect = true;
            onSelect?.Invoke();
        }

        public void OnSetTarget()
        {
            IsSelect = false;
            onSetTarget?.Invoke();
        }
        
        public void OffSetTarget()
        {
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
            float faceLevel = DiceInventory is Dice.PlayerDiceInventory inventory ? inventory.FaceLevel : 1f;
            return Mathf.Max(1f, playerLevel) * faceLevel;
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
