using System;
using System.Collections.Generic;
using System.Linq;
using DevLib.ModuleSystem;
using Members.PSW.Code.Test;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime
{
    public class SkillExecutor : MonoModule, IAfterInitModule
    {
        private Dictionary<Type, ISkillLogic> _skillDict;
        
        private bool _canExecute;

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
        }
        
        public void AfterInit()
        {
            _skillDict = GetComponentsInChildren<ISkillLogic>(true).ToDictionary(m => m.GetType());
        }

        public void TryExecuteSkill(SkillSO skill)
        {
            if (!_canExecute) return;

            if (_skillDict.TryGetValue(skill.logicType, out ISkillLogic logic))
            {
                Debug.Log("스킬이 성공적으로 실행됨");
                logic.PlaySkill(skill);
            }
        }
    }
}