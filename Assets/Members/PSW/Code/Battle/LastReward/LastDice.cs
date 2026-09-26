using System;
using DG.Tweening;
using Members.KJY._01.Scripts.Dice.Data;
using UnityEngine;

namespace Members.PSW.Code.Battle.LastReward
{
    // 면 표시 및 굴림 연출
    public class LastDice : MonoBehaviour
    {
        [Header("Face Renderers")]
        [SerializeField] private SpriteRenderer frontIcon;
        [SerializeField] private SpriteRenderer backIcon;
        [SerializeField] private SpriteRenderer leftIcon;
        [SerializeField] private SpriteRenderer rightIcon;
        [SerializeField] private SpriteRenderer topIcon;
        [SerializeField] private SpriteRenderer bottomIcon;
        [Header("Roll")]
        [SerializeField, Min(0.01f)] private float rollDuration = 1.2f;
        [SerializeField, Min(0f)] private float jumpHeight = 0.6f;
        [SerializeField, Min(1)] private int spinTurns = 3;
        [SerializeField, Min(0f)] private float resultDisplayDuration = 0.5f;

        public event Action RollComplete;
        public bool IsRolling { get; private set; }

        private Sequence _roll;
        private bool _hasStarted;

        public bool Throw(DiceDataListSO diceData, DiceFaceType resultFace)
        {
            if (_hasStarted || !isActiveAndEnabled)
                return false;
            if (!TryApplyEnemyFaces(diceData) || !Enum.IsDefined(typeof(DiceFaceType), resultFace))
                return false;

            // 기존 주사위 외형의 결과 각도
            Vector3 resultEuler = resultFace switch
            {
                DiceFaceType.Front => Vector3.zero,
                DiceFaceType.Back => new Vector3(0f, 180f, 0f),
                DiceFaceType.Left => new Vector3(0f, -90f, 0f),
                DiceFaceType.Right => new Vector3(0f, 90f, 0f),
                DiceFaceType.Top => new Vector3(-90f, 0f, 0f),
                DiceFaceType.Bottom => new Vector3(90f, 0f, 0f),
                _ => Vector3.zero
            };
            Vector3 startPosition = transform.localPosition;
            Vector3 spinEuler = resultEuler + new Vector3(360f * spinTurns, 360f * spinTurns, 0f);

            _hasStarted = true;
            IsRolling = true;
            _roll = DOTween.Sequence();
            _roll.Append(transform.DOLocalMoveY(startPosition.y + jumpHeight, rollDuration * 0.5f)
                .SetEase(Ease.OutQuad));
            _roll.Append(transform.DOLocalMoveY(startPosition.y, rollDuration * 0.5f)
                .SetEase(Ease.InQuad));
            _roll.Insert(0f, transform.DOLocalRotate(spinEuler, rollDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic));
            _roll.OnComplete(() =>
            {
                transform.localPosition = startPosition;
                transform.localRotation = Quaternion.Euler(resultEuler);
                IsRolling = false;
                _roll = null;
                Destroy(gameObject, resultDisplayDuration);
                RollComplete?.Invoke();
            });
            return true;
        }

        private bool TryApplyEnemyFaces(DiceDataListSO diceData)
        {
            if (diceData == null || rollDuration <= 0f || jumpHeight < 0f ||
                spinTurns < 1 || resultDisplayDuration < 0f)
            {
                Debug.Assert(false, "LastDice: 적 주사위 데이터와 연출 수치를 확인하세요.", this);
                return false;
            }

            SpriteRenderer[] icons = { frontIcon, backIcon, leftIcon, rightIcon, topIcon, bottomIcon };
            DiceDataSO[] data = { diceData.Front, diceData.Back, diceData.Left,
                diceData.Right, diceData.Top, diceData.Bottom };

            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i] == null || !icons[i].transform.IsChildOf(transform) || data[i] == null)
                {
                    Debug.Assert(false, "LastDice: 면 렌더러 또는 적의 면 데이터가 없습니다.", this);
                    return false;
                }
                for (int j = 0; j < i; j++)
                {
                    if (icons[i] == icons[j])
                    {
                        Debug.Assert(false, "LastDice: 면 렌더러가 중복 연결되었습니다.", this);
                        return false;
                    }
                }
            }

            // 죽은 적의 6면을 그대로 표시
            for (int i = 0; i < icons.Length; i++)
                icons[i].sprite = data[i].Icon;
            return true;
        }

        private void OnDisable()
        {
            // 취소 시 결과 미발행
            _roll?.Kill();
            _roll = null;
            IsRolling = false;
        }
    }
}
