using System.Collections.Generic;
using System.Linq;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.PSW.Code.Test;
using Members.PSW.Code.Unit_Logic.Runtime.Structs;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime.Skill
{
    public class SkillEndEvent : GameEvent
    {
        public static SkillEndEvent Instance { get; private set; } = new();
    }
    
    public class SkillExecutor : MonoModule
    {
        [SerializeField] public EventChannelSO eventChannel;
        
        private Dictionary<string, ISkillLogic> _skillDict;
        
        private bool _canExecute;
        
        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            
            _skillDict = GetComponentsInChildren<ISkillLogic>(true).ToDictionary(m => m.GetType().Name);
            
            InitSkill();
            DebugDictKey();
            _canExecute = true;
        }

        private void DebugDictKey()
        {
            foreach (string skillName in _skillDict.Keys)
            {
                Debug.Log(skillName);
            }
        }

        private void InitSkill()
        {
            foreach (ISkillLogic skill in _skillDict.Values)
            {
                skill.Init(this);
            }
        }

        private void OnEnable()
        {
            foreach (ISkillLogic skill in _skillDict.Values)
            {
                skill.OnCalculate += SkillCalc;
                skill.OnSkillFinished += HandleSkillFinish;
            }
        }

        private void OnDisable()
        {
            foreach (ISkillLogic skill in _skillDict.Values)
            {
                skill.OnCalculate -= SkillCalc;
                skill.OnSkillFinished -= HandleSkillFinish;
            }
        }

        private void SkillCalc(CalculateStat stat, SkillSO skill, GameObject target)
        {
            Debug.Log("SkillExecutor SkillCalc");
            Owner.GetModule<SkillCalculater>().Calculate(stat, skill, target);
        }

        private void HandleSkillFinish()
        {
            eventChannel.RaiseEvent(SkillEndEvent.Instance);
            _canExecute = true;
        }

        public void TryExecuteSkill(SkillSO skill, GameObject target)
        {
            if (!_canExecute) return;

            if (_skillDict.TryGetValue(skill.logicClassName, out ISkillLogic logic))
            {
                Debug.Log("스킬이 성공적으로 실행됨");
                logic.PlaySkill(skill, target);
            }

            _canExecute = false;
        }
    }
}