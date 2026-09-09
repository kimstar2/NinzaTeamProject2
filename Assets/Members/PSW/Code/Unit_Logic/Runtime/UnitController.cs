using DevLib.CoreLib.Runtime;
using DevLib.ModuleSystem;
using Members.PSW.Code.Test;
using Members.PSW.Code.Unit_Logic.Runtime.Skill;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Runtime
{
    public class UnitController : ModuleOwner
    {
        [Header("Test(나중에 삭제해야함)")] 
        [SerializeField] private SkillSO skill;
        [SerializeField] private GameObject target;

        [field: SerializeField , Header("유닛 데이터")] public UnitDataSO UnitData { get; private set; }
        
        [SerializeField, Header("이벤트 채널")] private EventChannelSO evt;
        
        private SkillExecutor _executor;
        
        protected override void InitializeModules()
        {
            base.InitializeModules();
            
            _executor = GetModule<SkillExecutor>();
        }

        private void OnEnable()
        {
            // evt.AddListener<>();  <- 스킬 전달해주는 이벤트 구독
        }

        private void OnDisable()
        {
            // evt.RemoveListener<>(); <- 스킬 전달해주는 이벤트 구취
        }

        [ContextMenu("Fuck You")]
        private void Test()
        {
            _executor.TryExecuteSkill(skill, target);
        }
        
        private void HandleSkillEvent(GameEvent evt)
        {
            _executor.TryExecuteSkill(skill, target);
        }
    }
}