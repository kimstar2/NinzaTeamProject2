using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _TevLib.Extension.DoT
{

    public class RandomSequencer : MonoBehaviour
    {
        [SerializeField] public bool debugMode = false;
        [field:SerializeField] public bool IsCanvas {get; private set;}
        [field: SerializeField] public Transform targetTrm;
        [SerializeField] private List<RandomTweenStep> sequenceStep;
        [Header("Tween Options")]
        [SerializeField] private UpdateType updateType;
        [SerializeField] private bool independentTime;
        
        [SerializeField] private bool targetLink;
        [SerializeField] private LinkBehaviour linkType;
        [SerializeField] private Transform id;
        public UnityEvent onSeqComplete;
        
        public bool HasTween =>
            _activeSequence != null &&
            _activeSequence.IsActive() &&
            _activeSequence.IsPlaying();

        private Transform _transform;
        private RectTransform _rectTrm;
        private CanvasGroup _canvasGroup;
        private Graphic _graphic;
        private Image _image;
        private SpriteRenderer _spriteRenderer;
        
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
                _graphic = targetTrm.GetComponent<Graphic>();
                _image = targetTrm.GetComponent<Image>();
            }
            else
            {
                _transform = targetTrm;
                _spriteRenderer = targetTrm.GetComponent<SpriteRenderer>();
            }
        }

        public bool SequenceAndResult()
        {
            KillTween();
            
            _activeSequence = DOTween.Sequence();
            _activeSequence.SetUpdate(updateType, independentTime);

            bool hasStep = false;

            foreach (RandomTweenStep step in sequenceStep)
                hasStep |= AddStep(_activeSequence, step.GetRandomStep());

            if (!hasStep)
            {
                KillTween();
                return false;
            }
            
            if (id != null)
                _activeSequence.SetId(id.GetHashCode());
            
            _activeSequence
                .SetLink(gameObject
                    , linkType)
                .OnComplete(HandleCompleteTween);

            return true;
        }
        
        [ContextMenu("Sequence")]
        public void Sequence()
        {
            KillTween();
            
            _activeSequence = DOTween.Sequence();
            _activeSequence.SetUpdate(updateType, independentTime);

            bool hasStep = false;

            foreach (RandomTweenStep step in sequenceStep)
                hasStep |= AddStep(_activeSequence, step.GetRandomStep());

            if (!hasStep)
            {
                KillTween();
                return;
            }

            if (id != null)
                _activeSequence.SetId(id.GetHashCode());
            
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
        
        private bool AddStep(Sequence sequence, TweenStepSO step)
        {
            switch (step.InsertType)
            {
                case SequenceInsertType.PrependInterval:
                    sequence.PrependInterval(Mathf.Max(0f, step.Duration));
                    return true;

                case SequenceInsertType.AppendInterval:
                    sequence.AppendInterval(Mathf.Max(0f, step.Duration));
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

        
        private Tween MakeTween(TweenStepSO step)
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
                case SequenceActionType.DoColor:
                    return CreateColorTween(step);
                case SequenceActionType.DoFade:
                    return CreateFadeTween(step);
                case SequenceActionType.DoLocalMove:
                    return CreateLocalMoveTween(step);
                case SequenceActionType.DoRotate:
                    return CreateRotationTween(step);
                case SequenceActionType.DoSizeDelta:
                    return CreateSizeDeltaTween(step);
                case SequenceActionType.DoFillAmount:
                    return CreateFillAmountTween(step);
            }

            return null;
        }
        
        private Tween CreateMoveTween(TweenStepSO step)
            => (IsCanvas ? _rectTrm : _transform).DOMove(step.GetTransformValue(), step.Duration);

        private Tween CreateLocalScaleTween(TweenStepSO step)
            => (IsCanvas ? _rectTrm : _transform).DOScale(step.GetTransformValue(), step.Duration);

        private Tween CreateCanvasAlpha(TweenStepSO step)
        {
            if (_canvasGroup == null)
            {
                Debug.Log("CanvasGroup not found");
                return null;
            }
            return _canvasGroup.DOFade(step.FadeValue, step.Duration);
        }

        private Tween CreateAnchoredPositionTween(TweenStepSO step)
        {
            if (!IsCanvas)
            {
                Debug.Log("Canvas not found");
                return null;
            }
            return _rectTrm.DOAnchorPos(step.GetTransformValue(), step.Duration);
        }

        private Tween CreateLocalRotationTween(TweenStepSO step)
            => (IsCanvas ? _rectTrm : _transform).DOLocalRotate(step.GetTransformValue(), step.Duration,
                step.UsingFastBeyond ?
                    RotateMode.FastBeyond360 :
                    RotateMode.Fast);

        private Tween CreateLocalMoveTween(TweenStepSO step)
            => (IsCanvas ? _rectTrm : _transform).DOLocalMove(step.GetTransformValue(), step.Duration);

        private Tween CreateRotationTween(TweenStepSO step)
            => (IsCanvas ? _rectTrm : _transform).DORotate(step.GetTransformValue(), step.Duration,
                step.UsingFastBeyond ?
                    RotateMode.FastBeyond360 :
                    RotateMode.Fast);

        private Tween CreateColorTween(TweenStepSO step)
        {
            if (IsCanvas)
            {
                if (_graphic == null)
                    return LogMissingComponent<Graphic>(step.ActionType);

                return _graphic.DOColor(step.ColorValue, step.Duration);
            }

            if (_spriteRenderer == null)
                return LogMissingComponent<SpriteRenderer>(step.ActionType);

            return _spriteRenderer.DOColor(step.ColorValue, step.Duration);
        }

        private Tween CreateFadeTween(TweenStepSO step)
        {
            float alpha = Mathf.Clamp01(step.FadeValue);

            if (IsCanvas)
            {
                if (_graphic == null)
                    return LogMissingComponent<Graphic>(step.ActionType);

                return _graphic.DOFade(alpha, step.Duration);
            }

            if (_spriteRenderer == null)
                return LogMissingComponent<SpriteRenderer>(step.ActionType);

            return _spriteRenderer.DOFade(alpha, step.Duration);
        }

        private Tween CreateSizeDeltaTween(TweenStepSO step)
        {
            if (_rectTrm == null)
                return LogMissingComponent<RectTransform>(step.ActionType);

            return _rectTrm.DOSizeDelta(step.GetTransformValue(), step.Duration);
        }

        private Tween CreateFillAmountTween(TweenStepSO step)
        {
            if (_image == null)
                return LogMissingComponent<Image>(step.ActionType);

            return _image.DOFillAmount(Mathf.Clamp01(step.FadeValue), step.Duration);
        }

        private Tween LogMissingComponent<T>(SequenceActionType actionType) where T : Component
        {
            Debug.LogWarning($"{name}: {actionType} requires {typeof(T).Name} on {targetTrm.name}.", this);
            return null;
        }
        
        private void KillTween()
        {
            if (id != null)
                DOTween.Kill(id.GetHashCode());
            _activeSequence.Kill();
            _activeSequence = null;
        }
        #endregion
    }
}
