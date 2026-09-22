using System;
using System.Collections.Generic;
using DG.Tweening;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Agent.SkillSystem;
using Members.KJY._01.Scripts.Agent.SkillSystem.Skill;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

namespace Members.PSW.Code.Unit_Logic.Runtime.Skill.Logics.Combat_Instinct
{
    [Serializable]
    public struct DurationSetting
    {
        public float attackerMoveDuration;
        public float attackWaitDuration;
        public float waitFadeLineTime;
        public float lineFadeDuration;

        public float effectDuration;
    }

    [Serializable]
    public struct LineDistant
    {
        public float sub1Start;
        public float sub2Start;
        
        public float upSubLine;
        public float downSubLine;
    }

    [Serializable]
    public struct EffectEvent
    {
        public UnityEvent onLineStart;
        public UnityEvent onLineEnd;

        public UnityEvent onEffectStart;
        public UnityEvent onEffectImpact;
        public UnityEvent onEffectEnd;
    }
    
    public class CombatInstinct : AbstractSkillLogic
    {
        [Header("테스트용")] 
        [SerializeField] private GameObject attacker;
        [SerializeField] private GameObject target;

        [Header("카메라")] 
        [SerializeField] private CinemachineCamera targetCam;
        
        [Header("값 세팅")] 
        [SerializeField] private DurationSetting timeSet;
        [SerializeField] private LineDistant lineDistant;
        
        [Header("검로 관련 요소")]
        [SerializeField] private Transform lineParent;
        [SerializeField] private LineRenderer mainLine;
        [SerializeField] private LineRenderer subLine1;
        [SerializeField] private LineRenderer subLine2;

        [Header("이펙트 관련")] 
        [SerializeField] private ParticleSystem effect;
        [SerializeField] private ParticleSystem shinyEffect;
        [SerializeField] private EffectEvent effectEvent;

        [Header("연출 관련")] 
        [SerializeField] private CanvasGroup vignette;
        
        [SerializeField] private float endLength = 1.2f;
        
        public List<SkillApplyStat> applyStats;
        public UnityEvent onSkillFinished;

        private Vector3 _startPos;
        private Vector3 _endPos;
        private bool _showLine;
        private AbstractSelector _target;

        public override void Init(SkillLogicExecutor executor)
        {
            base.Init(executor);

            _startPos = executor.Attacker.DefaultPosition.position;
            
            vignette.alpha = 0;
            
            mainLine.SetPosition(0, Vector3.zero);
            mainLine.SetPosition(1, Vector3.zero);
            subLine1.SetPosition(0, Vector3.right * lineDistant.sub1Start + Vector3.up * lineDistant.upSubLine);
            subLine1.SetPosition(1, Vector3.right * lineDistant.sub1Start + Vector3.up * lineDistant.upSubLine);
            subLine2.SetPosition(0, Vector3.right * lineDistant.sub2Start + Vector3.up * lineDistant.downSubLine);
            subLine2.SetPosition(1, Vector3.right * lineDistant.sub2Start + Vector3.up * lineDistant.downSubLine);
            
            Vector2 dir = executor.Target.DefaultPosition.position - executor.Attacker.DefaultPosition.position;
            dir *= endLength;
            _endPos = executor.Attacker.DefaultPosition.position + (Vector3)dir;
        }

        private void Update()
        {
            if (_showLine)
            {
                // Vector3 dir = _startPos - Executor.Attacker.DefaultPosition.position;
                Vector3 dir = _startPos - attacker.transform.position;
                
                mainLine.SetPosition(0, dir); //나중에 Executor.Attacker로 변경
                subLine1.SetPosition(0, dir + Vector3.right * lineDistant.sub1Start + Vector3.up * lineDistant.upSubLine);
                subLine2.SetPosition(0, dir + Vector3.right * lineDistant.sub2Start + Vector3.up * lineDistant.downSubLine);
            }
        }

        [ContextMenu("Test Init")]
        public void TestInit()
        {
            _startPos = attacker.transform.position;

            vignette.alpha = 0;
            
            mainLine.SetPosition(0, Vector3.zero);
            mainLine.SetPosition(1, Vector3.zero);
            subLine1.SetPosition(0, Vector3.right * lineDistant.sub1Start + Vector3.up * lineDistant.upSubLine);
            subLine1.SetPosition(1, Vector3.right * lineDistant.sub1Start + Vector3.up * lineDistant.upSubLine);
            subLine2.SetPosition(0, Vector3.right * lineDistant.sub2Start + Vector3.up * lineDistant.downSubLine);
            subLine2.SetPosition(1, Vector3.right * lineDistant.sub2Start + Vector3.up * lineDistant.downSubLine);
            
            Vector2 dir = target.transform.position - _startPos;
            dir *= endLength;
            _endPos = _startPos + (Vector3)dir;
        }
        
        [ContextMenu("Test Skill")]
        public void TestSkill()
        {
            _showLine = true;
            Sequence seq = DOTween.Sequence();

            seq.AppendInterval(timeSet.attackWaitDuration);
            seq.AppendCallback(() => effectEvent.onLineStart?.Invoke());
            seq.Append(attacker.transform.DOMove(_endPos, timeSet.attackerMoveDuration));
            seq.AppendInterval(timeSet.waitFadeLineTime);
            
            seq.AppendCallback(() =>
            {
                targetCam.Target.TrackingTarget = target.transform;
                targetCam.Priority = 15;
            });
            LineFadeOut(seq);

            seq.AppendCallback(() => effect.transform.position = target.transform.position);
            seq.AppendCallback(() => shinyEffect.transform.position = target.transform.position);
            seq.Append(vignette.DOFade(1, 1f));
            seq.AppendCallback(() => effectEvent.onEffectStart?.Invoke());
            
            seq.AppendInterval(timeSet.effectDuration/2);
            seq.AppendCallback(() =>
            {
                effectEvent.onEffectImpact?.Invoke();
                targetCam.Lens.OrthographicSize = 5;
            });
            seq.AppendCallback(() => ApplyStat());
            seq.AppendInterval(timeSet.effectDuration/2);
            
            seq.AppendCallback(() => effectEvent.onEffectEnd?.Invoke());
            seq.AppendCallback(() =>
            {
                targetCam.Priority = 0;
            });
            seq.Append(vignette.DOFade(0, 1f));
            seq.Append(attacker.transform.DOMove(_startPos, 1f));
            seq.AppendCallback(() => onSkillFinished?.Invoke());
        }
        
        public override void Execute()
        {
            _showLine = true;
            Sequence seq = DOTween.Sequence();

            seq.AppendInterval(timeSet.attackWaitDuration);
            seq.AppendCallback(() => effectEvent.onLineStart?.Invoke());
            seq.Append(Executor.Attacker.DefaultPosition.DOMove(_endPos, timeSet.attackerMoveDuration));
            seq.AppendInterval(timeSet.waitFadeLineTime);
            
            seq.AppendCallback(() =>
            {
                targetCam.Target.TrackingTarget = Executor.Target.DefaultPosition;
                targetCam.Priority = 15;
            });
            LineFadeOut(seq);

            seq.AppendCallback(() => effect.transform.position = Executor.Target.DefaultPosition.position);
            seq.AppendCallback(() => shinyEffect.transform.position = Executor.Target.DefaultPosition.position);
            seq.Append(vignette.DOFade(1, 1f));
            seq.AppendCallback(() => effectEvent.onEffectStart?.Invoke());
            
            seq.AppendInterval(timeSet.effectDuration/2);
            seq.AppendCallback(() =>
            {
                effectEvent.onEffectImpact?.Invoke();
                targetCam.Lens.OrthographicSize = 5;
            });
            seq.AppendCallback(() => ApplyStat());
            seq.AppendInterval(timeSet.effectDuration/2);
            
            seq.AppendCallback(() => effectEvent.onEffectEnd?.Invoke());
            seq.AppendCallback(() =>
            {
                targetCam.Priority = 0;
            });
            seq.Append(vignette.DOFade(0, 1f));
            seq.Append(Executor.Attacker.DefaultPosition.DOMove(_startPos, 1f));
            seq.AppendCallback(() => onSkillFinished?.Invoke());
        }

        private void LineFadeOut(Sequence seq)
        {
            seq.AppendCallback(() =>
                mainLine.DOColor(new Color2( new Color(1, 1, 1, 1), new Color(1, 1, 1, 1)),
                    new Color2(new Color(1, 1, 1, 0), new Color(1, 1, 1, 0)), timeSet.lineFadeDuration)
            );
            seq.AppendCallback(() =>
                subLine1.DOColor(new Color2( new Color(1, 1, 1, 1), new Color(1, 1, 1, 1)),
                    new Color2(new Color(1, 1, 1, 0), new Color(1, 1, 1, 0)), timeSet.lineFadeDuration)
            );
            seq.AppendCallback(() =>
                subLine2.DOColor(new Color2( new Color(1, 1, 1, 1), new Color(1, 1, 1, 1)),
                    new Color2(new Color(1, 1, 1, 0), new Color(1, 1, 1, 0)), timeSet.lineFadeDuration)
            );
            seq.AppendCallback(() => effectEvent.onLineEnd?.Invoke());
            seq.Append(lineParent.DOScaleY(0, timeSet.lineFadeDuration));
        }
        
        public override void ApplyStat()    
        {
            foreach (var applyStat in applyStats)
            {
                _target.ApplyStat(applyStat.ApplyStatType, applyStat.Value);
            }
        }
    }
}