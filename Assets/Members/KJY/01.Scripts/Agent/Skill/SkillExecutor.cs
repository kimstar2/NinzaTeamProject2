using System.Collections.Generic;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Events.Dice;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.Skill
{
    public class SkillExecutor : MonoModule
    {
        [SerializeField] private EventChannelSO eventChannel;
        public bool CanExecuteSkill { get; private set; }
        public ISkillLogic CurrentSkillLogic { get; private set; }

        public void TryExecuteSkill(SkillDataSO skillData) // 스킬 사용 시도
        {
            if(!CanExecuteSkill) return; // 스킬 사용 못해 => 리턴
            
            CurrentSkillLogic = skillData.SkillLogic; 
            if (CurrentSkillLogic == null) return; // 로직이 없음 ;; 리턴

            CurrentSkillLogic.OnSkillEnd -= HandleSkillEnd; // 안전 검사
            CurrentSkillLogic.OnSkillEnd += HandleSkillEnd;
            
            CurrentSkillLogic.ExecuteSkill(); // 스킬 실행
        }

        private void HandleSkillEnd() // 스킬 끝나면 배틀 옵저버한테 다음 커맨드로 넘어가라고 명령
        {
            if (CurrentSkillLogic == null) return;
            
            CurrentSkillLogic.OnSkillEnd -= HandleSkillEnd;
            eventChannel.RaiseEvent(new OnExecuteNextCommand());
        }
    }
}