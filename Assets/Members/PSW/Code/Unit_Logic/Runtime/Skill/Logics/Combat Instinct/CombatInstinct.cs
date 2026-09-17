using System;
using System.Collections.Generic;
using DG.Tweening;
using Members.KJY._01.Scripts.Agent.SkillSystem;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Members.PSW.Code.Unit_Logic.Runtime.Skill.Logics.Combat_Instinct
{
    [Serializable]
    public struct DurationSetting
    {
        public float attackerMoveDuration;
    }

    [Serializable]
    public struct LineDistant
    {
        public float upSubLine;
        public float downSubLine;
    }
    
    public class CombatInstinct : AbstractSkillLogic
    {
        [Header("테스트용")] 
        [SerializeField] private GameObject attacker;
        [SerializeField] private GameObject target;
        
        [Header("필요 요소")] 
        [SerializeField] private DurationSetting timeSet;
        [SerializeField] private LineDistant lineDistant;
        [SerializeField] private LineRenderer mainLine;
        [SerializeField] private LineRenderer subLine1;
        [SerializeField] private LineRenderer subLine2;
        
        [SerializeField] private float endLength = 1.2f;
        
        private Vector3 _endPos;
        private bool _showLine;

        public override void Init(SkillLogicExecutor executor)
        {
            base.Init(executor);
            
            mainLine.SetPosition(0, executor.Attacker.DefaultPosition.position);
            mainLine.SetPosition(1, executor.Attacker.DefaultPosition.position);
            
            Vector2 dir = executor.Target.DefaultPosition.position - executor.Attacker.DefaultPosition.position;
            dir *= endLength;
            _endPos = executor.Attacker.DefaultPosition.position + (Vector3)dir;
        }

        private void Update()
        {
            if (_showLine)
            {
                mainLine.SetPosition(1, attacker.transform.position); //나중에 Executor.Attacker로 변경
                subLine1.SetPosition(1, attacker.transform.position + Vector3.up * lineDistant.upSubLine);
                subLine2.SetPosition(1, target.transform.position + Vector3.up * lineDistant.downSubLine);
            }
        }

        [ContextMenu("Test Init")]
        public void TestInit()
        {
            Vector3 atkPos = attacker.transform.position;
            
            mainLine.SetPosition(0, atkPos);
            mainLine.SetPosition(1, atkPos);

            
            
            Vector2 dir = target.transform.position - atkPos;
            dir *= endLength;
            _endPos = atkPos + (Vector3)dir;
        }
        
        [ContextMenu("Test Skill")]
        public void TestSkill()
        {
            _showLine = true;
            Sequence seq = DOTween.Sequence();

            seq.Append(attacker.transform.DOMove(_endPos, timeSet.attackerMoveDuration));
        }
        
        public override void Execute()
        {
            _showLine = true;
            Sequence seq = DOTween.Sequence();

            seq.Append(Executor.Attacker.DefaultPosition.DOMove(_endPos, timeSet.attackerMoveDuration));
        }
        
        public override void ApplyStat()
        {

        }
    }
}