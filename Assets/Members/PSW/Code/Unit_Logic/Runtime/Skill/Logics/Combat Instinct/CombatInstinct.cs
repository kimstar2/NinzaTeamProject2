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
        private Transform _body;
        private Transform _victim;
        private Sequence _sequence;
        private bool _showLine;
        private bool _impactApplied;
        private int _cameraPriority;
        private float _cameraSize;
        private Transform _cameraTarget;

        public override void Init(SkillLogicExecutor executor, float baseLevel)
        {
            base.Init(executor, baseLevel);
            Prepare(executor.Attacker.MyAgent.transform, executor.Target.MyAgent.transform);
        }

        private void Prepare(Transform body, Transform victim)
        {
            _sequence?.Kill();
            _body = body;
            _victim = victim;
            _startPos = body.position;
            Vector3 direction = victim.position - _startPos;
            _endPos = _startPos + direction * endLength;
            _impactApplied = false;
            vignette.alpha = 0f;
            lineParent.localScale = Vector3.one;
            lineParent.position = _startPos + Vector3.up * 0.45f;
            foreach (var line in new[] { mainLine, subLine1, subLine2 })
            {
                line.useWorldSpace = true;
                line.startColor = line.endColor = Color.white;
                line.SetPosition(0, lineParent.position);
                line.SetPosition(1, lineParent.position);
            }
            if (targetCam != null)
            {
                _cameraPriority = targetCam.Priority;
                _cameraSize = targetCam.Lens.OrthographicSize;
                _cameraTarget = targetCam.Target.TrackingTarget;
            }
        }

        private void LateUpdate()
        {
            if (!_showLine || _body == null) return;
            Vector3 from = _startPos + Vector3.up * 0.45f;
            Vector3 to = _body.position + Vector3.up * 0.45f;
            mainLine.SetPosition(0, from);
            mainLine.SetPosition(1, to);
            subLine1.SetPosition(0, from + Vector3.up * lineDistant.upSubLine);
            subLine1.SetPosition(1, to + Vector3.up * lineDistant.upSubLine);
            subLine2.SetPosition(0, from + Vector3.up * lineDistant.downSubLine);
            subLine2.SetPosition(1, to + Vector3.up * lineDistant.downSubLine);
        }

        [ContextMenu("Test Init")]
        public void TestInit()
        {
            if (attacker != null && target != null) Prepare(attacker.transform, target.transform);
        }

        [ContextMenu("Test Skill")]
        public void TestSkill()
        {
            TestInit();
            if (_body != null && _victim != null) Execute();
        }

        public override void Execute()
        {
            _sequence?.Kill();
            _showLine = true;
            if (Executor != null) Executor.PlayAnim();
            _sequence = DOTween.Sequence();
            _sequence.Append(vignette.DOFade(0.22f, timeSet.attackWaitDuration));
            _sequence.AppendCallback(() =>
            {
                effectEvent.onLineStart?.Invoke();
            });
            // 이동 기준점은 고정하고 실제 캐릭터만 움직인다.
            _sequence.Append(_body.DOMove(_endPos, timeSet.attackerMoveDuration).SetEase(Ease.OutCubic));
            _sequence.AppendInterval(timeSet.waitFadeLineTime);
            _sequence.AppendCallback(() =>
            {
                if (targetCam != null)
                {
                    targetCam.Target.TrackingTarget = _victim;
                    targetCam.Priority = 15;
                }
                Vector3 center = _victim.position + Vector3.up * 0.5f;
                float span = 3f;
                if (Executor != null && Executor.SkillData.IsArea)
                {
                    var targets = Executor.GetTargets();
                    Bounds bounds = new Bounds(center, Vector3.zero);
                    foreach (var candidate in targets)
                        bounds.Encapsulate(candidate.MyAgent.transform.position + Vector3.up * 0.5f);
                    center = bounds.center;
                    span = Mathf.Clamp(bounds.size.y + 2.5f, 3f, 8f);
                }
                effect.transform.position = center;
                shinyEffect.transform.position = center;
                // 저장된 파티클의 크기 비율을 유지하면서 적 진형 전체를 덮는다.
                effect.transform.localScale = Vector3.one * (span / 5f);
                shinyEffect.transform.localScale = Vector3.one * (span / 5f);
                effectEvent.onEffectStart?.Invoke();
            });
            LineFadeOut(_sequence);
            _sequence.AppendInterval(timeSet.effectDuration * 0.3f);
            _sequence.AppendCallback(() =>
            {
                effectEvent.onEffectImpact?.Invoke();
                ApplyStat();
            });
            _sequence.AppendInterval(timeSet.effectDuration * 0.7f);
            _sequence.AppendCallback(() =>
            {
                effectEvent.onEffectEnd?.Invoke();
                RestoreCamera();
                if (Executor != null) Executor.PlayIdleAnim();
            });
            _sequence.Append(vignette.DOFade(0f, 0.18f));
            _sequence.Join(_body.DOMove(_startPos, 0.25f).SetEase(Ease.InOutSine));
            _sequence.AppendCallback(() =>
            {
                _showLine = false;
                onSkillFinished?.Invoke();
                if (Executor != null) Executor.Remove();
            });
        }

        private void LineFadeOut(Sequence sequence)
        {
            var visible = new Color2(Color.white, Color.white);
            var hidden = new Color2(new Color(1, 1, 1, 0), new Color(1, 1, 1, 0));
            sequence.Append(mainLine.DOColor(visible, hidden, timeSet.lineFadeDuration));
            sequence.Join(subLine1.DOColor(visible, hidden, timeSet.lineFadeDuration));
            sequence.Join(subLine2.DOColor(visible, hidden, timeSet.lineFadeDuration));
            sequence.AppendCallback(() => effectEvent.onLineEnd?.Invoke());
        }

        public override void ApplyStat()
        {
            if (_impactApplied) return;
            _impactApplied = true;
            PlaySkillSound();
            if (Executor != null) ApplyConfiguredStats(Executor.Target);
        }

        private void RestoreCamera()
        {
            if (targetCam == null) return;
            targetCam.Priority = _cameraPriority;
            targetCam.Lens.OrthographicSize = _cameraSize;
            targetCam.Target.TrackingTarget = _cameraTarget;
        }

        private void OnDestroy()
        {
            _sequence?.Kill();
            _showLine = false;
            if (_body != null) _body.position = _startPos;
            RestoreCamera();
        }
    }
}
