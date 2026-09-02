using System.Collections.Generic;
using DG.Tweening;
using Members.KJY._TevLib_Dot_.HashDataSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._TevLib_Dot_.DoT
{

    public class TweenSequencer : MonoBehaviour
    {
        [SerializeField] public bool debugMode = false;
        [field:SerializeField] public bool IsCanvas {get; private set;}
        [field: SerializeField] public Transform targetTrm;
        [SerializeField] private List<TweenStep> sequenceStep;
        [Header("Tween Options")]
        [SerializeField] private UpdateType updateType;
        [SerializeField] private bool independentTime;
        
        [SerializeField] private bool targetLink;
        [SerializeField] private LinkBehaviour linkType;
        [SerializeField] private AnimHashSO id;
        public UnityEvent onSeqComplete;
        
        public bool HasTween =>
            _activeSequence != null &&
            _activeSequence.IsActive() &&
            _activeSequence.IsPlaying();

        private Transform _transform;
        private RectTransform _rectTrm;
        private CanvasGroup _canvasGroup;
        
        private Sequence _activeSequence;
        
        private void Awake()
        {
            CacheComponents();
        }

        private void CacheComponents()
        {
            if (IsCanvas)
            {
                _rectTrm = targetTrm.GetComponent<RectTransform>();
                _canvasGroup = targetTrm.GetComponent<CanvasGroup>();
            }
            else
                _transform = targetTrm;
        }

        public bool SequenceAndResult()
        {
            KillTween();
            
            _activeSequence = DOTween.Sequence();
            _activeSequence.SetUpdate(updateType, independentTime);

            bool hasStep = false;

            foreach (TweenStep step in sequenceStep)
                hasStep |= AddStep(_activeSequence, step);

            if (!hasStep)
            {
                KillTween();
                return false;
            }
            
            if (id != null)
                _activeSequence.SetId(id.HashValue);
            
            _activeSequence
                .SetLink(gameObject
                    , linkType)
                .OnComplete(HandleCompleteTween);

            return true;
        }
        
        public void Sequence()
        {
            KillTween();
            
            _activeSequence = DOTween.Sequence();
            _activeSequence.SetUpdate(updateType, independentTime);

            bool hasStep = false;

            foreach (TweenStep step in sequenceStep)
                hasStep |= AddStep(_activeSequence, step);

            if (!hasStep)
            {
                KillTween();
                return;
            }

            if (id != null)
                _activeSequence.SetId(id.HashValue);
            
            _activeSequence
                .SetLink(gameObject
                    , linkType)
                .OnComplete(HandleCompleteTween);
        }

        private void HandleCompleteTween()
        {
            _activeSequence = null;
            onSeqComplete?.Invoke();
        }

        #region TweenHelper
        
        private bool AddStep(Sequence sequence, TweenStep step)
        {
            switch (step.InsertType)
            {
                case SequenceInsertType.PrependInterval:
                    sequence.PrependInterval(Mathf.Max(0f, step.Interval));
                    return true;

                case SequenceInsertType.AppendInterval:
                    sequence.AppendInterval(Mathf.Max(0f, step.Interval));
                    return true;

                case SequenceInsertType.PrependCallback:
                    sequence.PrependCallback(() => step.Callback?.Invoke());
                    return true;

                case SequenceInsertType.AppendCallback:
                    sequence.AppendCallback(() => step.Callback?.Invoke());
                    return true;

                case SequenceInsertType.JoinCallback:
                    sequence.JoinCallback(() => step.Callback?.Invoke());
                    return true;
            }

            Tween tween = MakeTween(step);
            if (tween == null)
                return false;

            tween.SetEase(step.EaseType);

            switch (step.InsertType)
            {
                case SequenceInsertType.Prepend:
                    sequence.Prepend(tween);
                    break;

                case SequenceInsertType.Append:
                    sequence.Append(tween);
                    break;

                case SequenceInsertType.Join:
                    sequence.Join(tween);
                    break;

                default:
                    return false;
            }

            return true;
        }

        
        private Tween MakeTween(TweenStep step)
        {
            switch (step.ActionType)
            {
                case SequenceActionType.DoAnchoredPosition:
                    return CreateAnchoredPositionTween(step); 
                case SequenceActionType.DoCanvasAlpha:
                    return CreateCanvasAlpha(step);
                case SequenceActionType.DoLocalScale:
                    return CreateLocalScaleTween(step);
                case SequenceActionType.DoMove:
                    return CreateMoveTween(step);
                case SequenceActionType.DoLocalRotation:
                    return CreateLocalRotationTween(step);
            }

            return null;
        }
        
        private Tween CreateMoveTween(TweenStep step)
            => (IsCanvas ? _rectTrm : _transform).DOMove(step.TransformValue, step.Duration);

        private Tween CreateLocalScaleTween(TweenStep step)
            => (IsCanvas ? _rectTrm : _transform).DOScale(step.TransformValue, step.Duration);

        private Tween CreateCanvasAlpha(TweenStep step)
        {
            if (_canvasGroup == null)
            {
                Debug.Log("CanvasGroup not found");
                return null;
            }
            return _canvasGroup.DOFade(step.FadeValue, step.Duration);
        }

        private Tween CreateAnchoredPositionTween(TweenStep step)
        {
            if (!IsCanvas)
            {
                Debug.Log("Canvas not found");
                return null;
            }
            return _rectTrm.DOAnchorPos(step.TransformValue, step.Duration);
        }

        private Tween CreateLocalRotationTween(TweenStep step)
            => (IsCanvas ? _rectTrm : _transform).DOLocalRotate(step.TransformValue, step.Duration);
        
        private void KillTween()
        {
            if (id != null)
                DOTween.Kill(id.HashValue);
            _activeSequence.Kill();
            _activeSequence = null;
        }
        #endregion
    }
}