using DG.Tweening;
using Members.KJY._01.Scripts.Dice;
using Members.KJY._01.Scripts.Events.Dice.Agent.Enemy;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Agent.Enemy.Dice
{
    public class EnemyDice : AbstractDice
    {
        [SerializeField] private EnemyType enemyType;
        [Header("Last Dice Face Color")]
        [SerializeField] private EnemyDiceDataReceiver faceReceiver;
        [SerializeField] private Color deadChargeColor = new(0.65f, 0.12f, 0.04f, 1f);
        [SerializeField] private Color deadRollColor = new(1f, 0.55f, 0.08f, 1f);
        [SerializeField] private Color deadRevealColor = new(1f, 0.95f, 0.65f, 1f);
        [Header("Last Dice Motion")]
        [SerializeField, Min(0.01f)] private float deadChargeTime = 0.14f;
        [SerializeField, Min(0.01f)] private float deadRiseTime = 0.30f;
        [SerializeField, Min(0f)] private float deadHangTime = 0.18f;
        [SerializeField, Min(0.01f)] private float deadFallTime = 0.16f;
        [SerializeField, Min(0.01f)] private float deadSettleTime = 0.12f;
        [SerializeField, Min(0f)] private float deadRevealTime = 0.30f;
        [SerializeField, Min(0.1f)] private float deadRiseHeight = 1.65f;
        [SerializeField] private Vector3Int deadSpinTurns = new(1, 2, 0);
        public UnityEvent onDeadRoll;
        public UnityEvent onDeadRollLanded;
        public UnityEvent onDeadRollComplete;
        private Vector3 _restScale;
        private bool _isDeadRolling;
        private bool _deadRollCompleted;

        private void Awake()
        {
            _restScale = transform.localScale;
        }

        private void OnEnable()
        {
            eventChannel.AddListener<OnEnemyRoll>(HandleEnemyRoll);
            eventChannel.AddListener<OnEnemyDead>(HandleEnemyDead);
        }

        private void OnDisable()
        {
            roll?.Kill();
            roll = null;
            if (_isDeadRolling) transform.localScale = _restScale;
            _isDeadRolling = false;
            
            eventChannel.RemoveListener<OnEnemyRoll>(HandleEnemyRoll);
            eventChannel.RemoveListener<OnEnemyDead>(HandleEnemyDead);
        }

        private void HandleEnemyDead(OnEnemyDead evt)
        {
            if (evt.enemyType != enemyType || evt.isDead) return;
            roll?.Kill();
            roll = null;
            _isDeadRolling = false;
            _deadRollCompleted = false;
            isLock = false;
            transform.localScale = _restScale;
            faceReceiver.SetFaceColor(Color.white);
        }
        private void HandleEnemyRoll(OnEnemyRoll enemyRoll)
        {
            if (enemyRoll.enemyType != enemyType) return;
            if (_isDeadRolling || _deadRollCompleted) return;

            switch (enemyRoll.rollType)
            {
                case EnemyRollType.DeadRoll:
                    DeadRoll(destTrm.localPosition,GetRandom());
                    break;
                case EnemyRollType.Roll:
                    if (enemyRoll.isDead) break;
                    Roll(destTrm.localPosition,GetRandom());
                    break;
            }
        }
        
        private void DeadRoll(Vector3 localEndPosition, Vector3 resultEuler)
        {
            roll?.Kill();
            _isDeadRolling = true;
            onDeadRoll?.Invoke();

            Vector3 apex = localEndPosition + Vector3.up * deadRiseHeight;
            // Whole turns preserve the selected face; arbitrary spin angles do not.
            Vector3 spinEnd = resultEuler + (Vector3)deadSpinTurns * 360f;
            float flightTime = deadRiseTime + deadHangTime + deadFallTime;

            roll = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable);
            roll.Append(transform.DOScale(Vector3.Scale(_restScale, new Vector3(1.08f, 0.88f, 1.08f)), deadChargeTime)
                .SetEase(Ease.InQuad));
            roll.Join(transform.DOLocalMove(localEndPosition - Vector3.up * 0.08f, deadChargeTime)
                .SetEase(Ease.InQuad));
            roll.Join(TweenFaceColor(deadChargeColor, deadChargeTime));

            roll.Append(transform.DOLocalMove(apex, deadRiseTime).SetEase(Ease.OutCubic));
            roll.Join(transform.DOScale(_restScale * 1.25f, deadRiseTime).SetEase(Ease.OutQuad));
            roll.Join(TweenFaceColor(deadRollColor, deadRiseTime));
            // Insert rotation after laying out movement so it does not extend the rise step.
            roll.AppendInterval(deadHangTime);
            roll.Append(transform.DOLocalMove(localEndPosition, deadFallTime).SetEase(Ease.InCubic));
            roll.Join(transform.DOScale(_restScale, deadFallTime).SetEase(Ease.InQuad));
            roll.AppendCallback(() =>
            {
                transform.localRotation = Quaternion.Euler(resultEuler);
                faceReceiver.SetFaceColor(deadRevealColor);
                onDeadRollLanded?.Invoke();
            });
            roll.Append(transform.DOScale(Vector3.Scale(_restScale, new Vector3(1.10f, 0.90f, 1.10f)), deadSettleTime * 0.35f)
                .SetEase(Ease.OutQuad));
            roll.Append(transform.DOScale(_restScale, deadSettleTime * 0.65f).SetEase(Ease.OutQuad));
            roll.AppendInterval(deadRevealTime);
            roll.Insert(deadChargeTime + flightTime, TweenFaceColor(deadRollColor, deadSettleTime + deadRevealTime));
            roll.Insert(deadChargeTime,
                transform.DOLocalRotate(spinEnd, flightTime, RotateMode.FastBeyond360).SetEase(Ease.OutCubic));
            roll.OnComplete(CompleteDeadRoll);
        }

        private Tween TweenFaceColor(Color color, float duration)
            => DOTween.To(() => faceReceiver.FrontImage.SpriteRenderer.color,
                faceReceiver.SetFaceColor, color, duration).SetEase(Ease.OutQuad);

        private void CompleteDeadRoll()
        {
            roll = null;
            _isDeadRolling = false;
            _deadRollCompleted = true;
            crtFaceType = diceFaces[currenRan].Type;
            eventChannel.RaiseEvent(new OnEnemyDeadRollEnd(crtFaceType, enemyType));
            onDeadRollComplete?.Invoke();
        }

        protected override void CompleteRoll()
        {
            crtFaceType = diceFaces[currenRan].Type;
            eventChannel.RaiseEvent(new OnEnemyRollEnd(crtFaceType, enemyType));
        }
    }
}
