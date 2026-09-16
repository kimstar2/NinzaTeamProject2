using _TevLib.Extension.DoT;
using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Flags;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.SkillSystem
{
    public class SkillExecutor : MonoModule , IRequirePooling
    {
        [SerializeField] private Transform skillParent;
        [SerializeField] private EventChannelSO eventChannel;
        public bool CanExecuteSkill { get; private set; } = true;
        public ISkillLogicExecutor CurrentSkillLogic { get; private set; }

        public void TryExecuteSkill(AbstractSelector attacker, AbstractSelector target) // 스킬 사용 시도
        {
            if(!CanExecuteSkill) return; // 스킬 사용 못해 => 리턴
            
            CurrentSkillLogic = Instantiate(attacker.DiceInventory.GetDiceData()
                .SkillData.SkillLogicExecutor,skillParent.transform);
            
            if (CurrentSkillLogic == null)
            {
                HandleEndSkill();
                return;
            }
            
            CurrentSkillLogic.OnSkillFinished += HandleEndSkill;
            CurrentSkillLogic.SkillExecute(attacker, target);
        }

        public void HandleEndSkill()
        {
            if (CurrentSkillLogic != null)
            {
                CurrentSkillLogic.OnSkillFinished -= HandleEndSkill;
            }
            eventChannel.RaiseEvent(new OnExecuteNextCommand());
        }
    }
}