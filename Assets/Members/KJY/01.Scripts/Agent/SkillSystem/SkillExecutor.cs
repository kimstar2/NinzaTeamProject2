using _TevLib.Extension.DoT;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Flags;
using UnityEngine;
using ZLinq;

namespace Members.KJY._01.Scripts.Agent.SkillSystem
{
    public class SkillExecutor : MonoModule , IRequirePooling
    {
        [SerializeField] private AgentType agentType;
        [SerializeField] private Transform skillParent;
        [SerializeField] private EventChannelSO eventChannel;
        public bool CanExecuteSkill { get; private set; } = true;
        public ISkillLogicExecutor CurrentSkillLogic { get; private set; }

        public void TryExecuteSkill(AbstractSelector attacker, AbstractSelector target) // 스킬 사용 시도
        {
            if(!CanExecuteSkill) return; // 스킬 사용 못해 => 리턴
            CanExecuteSkill = false;
            
            var face = attacker.DiceInventory.GetDiceData();
            var skillData = face != null ? face.GetSkillDataStruct(attacker.AgentData.AttackType).SkillData : null;
            
            if (skillData == null || skillData.SkillLogicExecutor == null)
            {
                Debug.LogWarning($"{name}: 주사위 스킬 연결 확인해줘", this);
                HandleEndSkill();
                return;
            }
            CurrentSkillLogic = Instantiate(skillData.SkillLogicExecutor, skillParent);
            CurrentSkillLogic.OnSkillFinished += HandleEndSkill;
            CurrentSkillLogic.SkillExecute(attacker, target, agentType, skillData);
        }

        public void HandleEndSkill()
        {
            if (CanExecuteSkill) return; // 끝 이벤트가 두 번 와도 다음 행동은 한 번만
            if (CurrentSkillLogic != null)
            {
                CurrentSkillLogic.OnSkillFinished -= HandleEndSkill;
            }
            CurrentSkillLogic = null;
            CanExecuteSkill = true;
            eventChannel.RaiseEvent(new OnExecuteNextCommand());
        }
    }
}
