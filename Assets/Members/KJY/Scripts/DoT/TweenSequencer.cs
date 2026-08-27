using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY.Scripts.DoT
{

    public class TweenSequencer : MonoBehaviour
    {
        [field:SerializeField] public bool IsCanvas {get; private set;}
        [SerializeField] private List<TweenStep> sequenceStep;
        [Header("Tween Options")]
        [SerializeField] private UpdateType updateType;
        [SerializeField] private bool independentTime;
        
        [SerializeField] private bool targetLink;
        [SerializeField] private LinkBehaviour linkType;
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
            if (IsCanvas)
            {
                _rectTrm = GetComponent<RectTransform>();
                _canvasGroup = GetComponent<CanvasGroup>();
            }
            else
                _transform = GetComponent<Transform>();
        }

        public bool Sequence()
        {
            KillTween();
            
            _activeSequence = DOTween.Sequence();
            _activeSequence.SetUpdate(updateType, independentTime);

            bool hasTween = false;
            
            foreach (TweenStep step in sequenceStep)
            {
                Tween myTween = MakeTween(step);
                if (myTween == null)
                    continue;
                InsertTween(_activeSequence , myTween , step);
                myTween.SetEase(step.EaseType);
                hasTween = true;
            }

            if (!hasTween)
            {
                KillTween();
                return false;
            }

            _activeSequence
                .SetLink(gameObject
                    , linkType)
                .OnComplete(HandleCompleteTween);
            
            return true;
        }

        private void HandleCompleteTween()
        {
            _activeSequence = null;
            onSeqComplete?.Invoke();
        }

        #region TweenHelper
        
        private void InsertTween(Sequence seq ,Tween tween ,TweenStep step)
        {
            switch (step.InsertType)
            {
                case SequenceInsertType.Prepend:
                    seq.Prepend(tween);
                    break;
                case SequenceInsertType.PrependCallback:
                    seq.PrependCallback(() => step.Callback?.Invoke());
                    break;
                case SequenceInsertType.PrependInterval:
                    seq.PrependInterval(step.Interval);
                    break;
                case SequenceInsertType.Append:
                    seq.Append(tween);
                    break;
                case SequenceInsertType.AppendCallback:
                    seq.AppendCallback(() => step.Callback?.Invoke());
                    break;
                case SequenceInsertType.AppendInterval:
                    seq.AppendInterval(step.Interval);
                    break;
                case SequenceInsertType.Join:
                    seq.Join(tween);
                    break;
                case SequenceInsertType.JoinCallback:
                    seq.JoinCallback(() => step.Callback?.Invoke());
                    break;
            }
        }

        
        private Tween MakeTween(TweenStep step)
        {
            switch (step.ActionType)
            {
                case SequenceActionType.DOAnchoredPosition:
                    return CreateAnchoredPositionTween(step); 
                case SequenceActionType.DOCanvasAlpha:
                    return CreateCanvasAlpha(step);
                case SequenceActionType.DOLocalScale:
                    return CreateLocalScaleTween(step);
                case SequenceActionType.DOMove:
                    return CreateMoveTween(step);
                case SequenceActionType.DOLocalRotation:
                    return CreateLocalRotationTween(step);
            }

            return null;
        }

        private Tween CreateMoveTween(TweenStep step)
            => (IsCanvas ? _rectTrm : transform).DOMove(step.TransformValue, step.Duration);

        private Tween CreateLocalScaleTween(TweenStep step)
            => (IsCanvas ? _rectTrm : transform).DOScale(step.TransformValue, step.Duration);

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
            => (IsCanvas ? _rectTrm : transform).DOLocalRotate(step.TransformValue, step.Duration);
        
        private void KillTween()
        {
            transform.DOKill();
            _activeSequence = null;
        }
        #endregion
    }
}