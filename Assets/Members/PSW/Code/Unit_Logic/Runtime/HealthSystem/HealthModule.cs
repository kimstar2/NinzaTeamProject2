using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.PSW.Code.Unit_Logic.Runtime.Skill;
using Members.PSW.Code.Unit_Logic.Runtime.Structs;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime.HealthSystem
{
    public class HealthModule : MonoModule, IDamageable
    {
        [SerializeField] private int currentHealth;
        [SerializeField] private EventChannelSO eventChannel;

        private UnitController _unit;
        
        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            
            _unit = owner as UnitController;
            Debug.Assert(_unit != null, "UnitController is null");
            
            currentHealth = _unit.UnitData.maxHealth;
        }

        private void OnEnable()
        {
            eventChannel.AddListener<CalcValueEvent>(HandleValueType);
        }

        private void OnDisable()
        {
            eventChannel.RemoveListener<CalcValueEvent>(HandleValueType);
        }

        private void HandleValueType(CalcValueEvent evt)
        {
            if (evt.SkillSet.enemyTargetAll && _unit.UnitData.unitType == UnitType.Enemy)
                GetSkillSwitch(evt);

            else if (evt.SkillSet.teamTargetAll && _unit.UnitData.unitType == UnitType.Player)
                GetSkillSwitch(evt);

            else if (evt.Target == Owner.gameObject)
                GetSkillSwitch(evt);
        }

        private void GetSkillSwitch(CalcValueEvent evt)
        {
            switch (evt.SkillSet.skillType)
            {
                case SkillType.Damage:
                    GetDamage(evt.Value);
                    break;
                case SkillType.Heal:
                    GetHealth(evt.Value);
                    break;
            }
        }

        public void GetDamage(int damage)
        {
            currentHealth -= damage;
            Debug.Log($"데미지가 성공적으로 감소함 {currentHealth}");
        }

        public void GetHealth(int health)
        {
            currentHealth += health;
            Debug.Log($"체력이 성공적으로 회복됨 {Owner.gameObject.name}");
        }
    }
}