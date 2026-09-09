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
        [SerializeField] private EventChannelSO evt;

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
            evt.AddListener<CalcValueEvent>(HandleValueType);
        }

        private void OnDisable()
        {
            evt.RemoveListener<CalcValueEvent>(HandleValueType);
        }

        private void HandleValueType(CalcValueEvent evt)
        {
            if (evt.Target != Owner.gameObject) return;
            
            switch (evt.SkillType)
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
            Debug.Log($"체력이 성공적으로 회복됨 {currentHealth}");
        }
    }
}