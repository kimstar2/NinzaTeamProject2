using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AnimatedButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    // =========================================================
    // 모든 AnimatedButton 관리
    // =========================================================

    private static readonly HashSet<AnimatedButton> AllButtons = new();


    // =========================================================
    // Hover
    // =========================================================

    [Header("Hover - 커질 오브젝트")]
    [SerializeField] private RectTransform scaleTarget;

    [SerializeField] private float hoverScale = 1.1f;


    [Header("Hover - X축 이동할 오브젝트")]
    [SerializeField] private RectTransform moveTarget;

    [Tooltip("양수 = 오른쪽 / 음수 = 왼쪽")]
    [SerializeField] private float moveX = 30f;


    [Header("Hover - 밀려날 오브젝트")]
    [SerializeField] private List<RectTransform> pushedTargets = new();

    [Tooltip("양수 = 오른쪽 / 음수 = 왼쪽")]
    [SerializeField] private float pushedX = 30f;


    [Header("Hover Animation")]
    [SerializeField] private float hoverDuration = 0.2f;

    [SerializeField] private Ease hoverEase = Ease.OutCubic;


    // =========================================================
    // Click
    // =========================================================

    [Header("Click - 연출 받을 오브젝트")]
    [Tooltip(
        "버튼을 누르는 순간 이 오브젝트들이 " +
        "Animation Root의 자식으로 이동합니다."
    )]
    [SerializeField] private List<RectTransform> clickTargets = new();


    [Header("Click - 연출용 부모")]
    [Tooltip(
        "Click Targets가 클릭 즉시 이 오브젝트의 자식이 되고 " +
        "이 오브젝트 기준 0,0으로 모입니다."
    )]
    [SerializeField] private RectTransform animationRoot;


    [Header("Click - 화면 가려진 후 비활성화")]
    [Tooltip(
        "연출 오브젝트들이 0,0까지 모인 순간 " +
        "이 오브젝트를 비활성화합니다."
    )]
    [SerializeField] private GameObject disableTarget;


    [Header("Click - 화면 가려진 후 활성화")]
    [Tooltip(
        "연출 오브젝트들이 0,0까지 모여 " +
        "화면을 완전히 가린 순간 활성화됩니다."
    )]
    [SerializeField] private GameObject activateWhenCovered;


    // =========================================================
    // Click Animation
    // =========================================================

    [Header("Click Animation")]

    [Tooltip("0,0으로 모이는 시간")]
    [SerializeField] private float gatherDuration = 0.4f;

    [Tooltip("모인 뒤 아래로 내려가는 거리")]
    [SerializeField] private float downDistance = 30f;

    [Tooltip("그 다음 위로 올라가는 거리")]
    [SerializeField] private float upDistance = 200f;

    [SerializeField] private float downDuration = 0.12f;

    [SerializeField] private float upDuration = 0.35f;

    [SerializeField] private Ease gatherEase = Ease.InOutCubic;


    // =========================================================
    // Option
    // =========================================================

    [Header("Option")]

    [Tooltip("연출이 끝난 후 Animation Root도 비활성화")]
    [SerializeField] private bool disableAnimationRootAfterAnimation = true;

    [Tooltip("Time.timeScale이 0이어도 UI 연출 실행")]
    [SerializeField] private bool useUnscaledTime = true;


    // =========================================================
    // 내부 변수
    // =========================================================

    private Button _button;

    private Vector3 _originalScale;

    /*
     * 부모가 바뀌어도
     * 원래 화면상의 크기로 돌아가기 위해 저장
     */
    private Vector3 _originalWorldScale;

    private Vector2 _moveTargetOriginalPosition;

    private readonly Dictionary<RectTransform, Vector2>
        _pushedOriginalPositions = new();

    private bool _clicked;

    private EventSystem _eventSystem;


    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        _button = GetComponent<Button>();
    }


    private void OnEnable()
    {
        AllButtons.Add(this);
    }


    private void Start()
    {
        SaveOriginalState();


        // 처음에는 결과 오브젝트를 꺼둠
        if (activateWhenCovered != null)
        {
            activateWhenCovered.SetActive(false);
        }
    }


    // =========================================================
    // 원래 상태 저장
    // =========================================================

    private void SaveOriginalState()
    {
        if (scaleTarget != null)
        {
            _originalScale =
                scaleTarget.localScale;

            _originalWorldScale =
                scaleTarget.lossyScale;
        }


        if (moveTarget != null)
        {
            _moveTargetOriginalPosition =
                moveTarget.anchoredPosition;
        }


        _pushedOriginalPositions.Clear();


        foreach (RectTransform target in pushedTargets)
        {
            if (target == null)
                continue;


            _pushedOriginalPositions[target] =
                target.anchoredPosition;
        }
    }


    // =========================================================
    // Hover Enter
    // =========================================================

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_clicked)
            return;


        if (!_button.interactable)
            return;


        PlayHover();
    }


    // =========================================================
    // Hover Exit
    // =========================================================

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_clicked)
            return;


        PlayHoverExit();
    }


    // =========================================================
    // Hover Animation
    // =========================================================

    private void PlayHover()
    {
        // -----------------------------------------------------
        // 확대
        // -----------------------------------------------------

        if (scaleTarget != null)
        {
            scaleTarget.DOKill();


            scaleTarget
                .DOScale(
                    _originalScale * hoverScale,
                    hoverDuration
                )
                .SetEase(hoverEase)
                .SetUpdate(useUnscaledTime);
        }


        // -----------------------------------------------------
        // X축 이동
        // -----------------------------------------------------

        if (moveTarget != null)
        {
            moveTarget.DOKill();


            moveTarget
                .DOAnchorPosX(
                    _moveTargetOriginalPosition.x + moveX,
                    hoverDuration
                )
                .SetEase(hoverEase)
                .SetUpdate(useUnscaledTime);
        }


        // -----------------------------------------------------
        // 주변 오브젝트 밀림
        // -----------------------------------------------------

        foreach (RectTransform target in pushedTargets)
        {
            if (target == null)
                continue;


            if (!_pushedOriginalPositions.TryGetValue(
                    target,
                    out Vector2 originalPosition))
            {
                continue;
            }


            target.DOKill();


            target
                .DOAnchorPosX(
                    originalPosition.x + pushedX,
                    hoverDuration
                )
                .SetEase(hoverEase)
                .SetUpdate(useUnscaledTime);
        }
    }


    // =========================================================
    // Hover Exit Animation
    // =========================================================

    private void PlayHoverExit()
    {
        // -----------------------------------------------------
        // 크기 복구
        // -----------------------------------------------------

        if (scaleTarget != null)
        {
            scaleTarget.DOKill();


            scaleTarget
                .DOScale(
                    _originalScale,
                    hoverDuration
                )
                .SetEase(hoverEase)
                .SetUpdate(useUnscaledTime);
        }


        // -----------------------------------------------------
        // 이동 복구
        // -----------------------------------------------------

        if (moveTarget != null)
        {
            moveTarget.DOKill();


            moveTarget
                .DOAnchorPos(
                    _moveTargetOriginalPosition,
                    hoverDuration
                )
                .SetEase(hoverEase)
                .SetUpdate(useUnscaledTime);
        }


        // -----------------------------------------------------
        // 밀린 오브젝트 복구
        // -----------------------------------------------------

        foreach (RectTransform target in pushedTargets)
        {
            if (target == null)
                continue;


            if (!_pushedOriginalPositions.TryGetValue(
                    target,
                    out Vector2 originalPosition))
            {
                continue;
            }


            target.DOKill();


            target
                .DOAnchorPos(
                    originalPosition,
                    hoverDuration
                )
                .SetEase(hoverEase)
                .SetUpdate(useUnscaledTime);
        }
    }


    // =========================================================
    // Click
    // =========================================================

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button !=
            PointerEventData.InputButton.Left)
        {
            return;
        }


        if (_clicked)
            return;


        if (!_button.interactable)
            return;


        PlayClickAnimation();
    }


    // =========================================================
    // Click Animation
    // =========================================================

    private void PlayClickAnimation()
    {
        // -----------------------------------------------------
        // 검사
        // -----------------------------------------------------

        if (animationRoot == null)
        {
            Debug.LogError(
                $"{name} : Animation Root를 지정해주세요."
            );

            return;
        }


        /*
         * AnimationRoot가 DisableTarget의 자식이면
         * DisableTarget을 끄는 순간 연출까지 꺼짐.
         */
        if (disableTarget != null)
        {
            if (animationRoot == disableTarget.transform ||
                animationRoot.IsChildOf(disableTarget.transform))
            {
                Debug.LogError(
                    "Animation Root는 Disable Target의 " +
                    "자식이면 안 됩니다."
                );

                return;
            }
        }


        _clicked = true;


        // =====================================================
        // ★ 모든 UI 상호작용 차단
        // =====================================================

        _button.interactable = false;


        _eventSystem =
            EventSystem.current;


        if (_eventSystem != null)
        {
            _eventSystem.enabled = false;
        }


        // =====================================================
        // 현재 Tween 제거
        // =====================================================

        KillAllTweens();


        // =====================================================
        // AnimationRoot 활성화
        // =====================================================

        animationRoot.gameObject.SetActive(true);

        /*
         * Canvas 바로 아래에 있다면
         * 가장 마지막 Sibling으로 보내서
         * 가장 위에 렌더링되게 함.
         */
        animationRoot.SetAsLastSibling();


        // =====================================================
        // ★ 클릭하자마자 즉시 부모 변경
        // =====================================================

        MoveTargetsToAnimationRootImmediately();


        // =====================================================
        // ★ Hover로 커져 있던 모든 오브젝트
        // 원래 크기로 DOTween 복구
        // =====================================================

        ResetAllHoverScales();


        // =====================================================
        // CanvasGroup 준비
        // =====================================================

        Dictionary<RectTransform, CanvasGroup>
            canvasGroups = new();


        foreach (RectTransform target in clickTargets)
        {
            if (target == null)
                continue;


            CanvasGroup group =
                target.GetComponent<CanvasGroup>();


            if (group == null)
            {
                group =
                    target.gameObject
                        .AddComponent<CanvasGroup>();
            }


            group.alpha = 1f;


            /*
             * 연출 중인 오브젝트 자체도
             * 클릭 / Hover / Raycast 차단
             */
            group.interactable = false;
            group.blocksRaycasts = false;


            canvasGroups[target] = group;
        }


        // =====================================================
        // 전체 Sequence
        // =====================================================

        Sequence masterSequence =
            DOTween.Sequence();


        // =====================================================
        // 1단계
        //
        // AnimationRoot 기준
        // 0,0으로 전부 모임
        // =====================================================

        Sequence gatherSequence =
            DOTween.Sequence();


        foreach (RectTransform target in clickTargets)
        {
            if (target == null)
                continue;


            target.DOKill();


            gatherSequence.Join(
                target
                    .DOAnchorPos(
                        Vector2.zero,
                        gatherDuration
                    )
                    .SetEase(gatherEase)
            );
        }


        masterSequence.Append(gatherSequence);


        // =====================================================
        // 2단계
        //
        // ★ 화면이 완전히 가려진 순간
        // =====================================================

        masterSequence.AppendCallback(() =>
        {
            // -------------------------------------------------
            // 기존 UI 비활성화
            // -------------------------------------------------

            if (disableTarget != null)
            {
                disableTarget.SetActive(false);
            }


            // -------------------------------------------------
            // ★ 원하는 새 오브젝트 활성화
            // -------------------------------------------------

            if (activateWhenCovered != null)
            {
                activateWhenCovered.SetActive(true);
            }
        });


        // =====================================================
        // 3단계
        //
        // 아래로 살짝 내려감
        // =====================================================

        Sequence downSequence =
            DOTween.Sequence();


        foreach (RectTransform target in clickTargets)
        {
            if (target == null)
                continue;


            downSequence.Join(
                target
                    .DOAnchorPosY(
                        -downDistance,
                        downDuration
                    )
                    .SetRelative()
                    .SetEase(Ease.OutQuad)
            );
        }


        masterSequence.Append(downSequence);


        // =====================================================
        // 4단계
        //
        // 위로 올라가면서 사라짐
        // =====================================================

        Sequence upSequence =
            DOTween.Sequence();


        foreach (RectTransform target in clickTargets)
        {
            if (target == null)
                continue;


            upSequence.Join(
                target
                    .DOAnchorPosY(
                        upDistance,
                        upDuration
                    )
                    .SetRelative()
                    .SetEase(Ease.InCubic)
            );


            // -------------------------------------------------
            // Fade Out
            // -------------------------------------------------

            if (canvasGroups.TryGetValue(
                    target,
                    out CanvasGroup group))
            {
                upSequence.Join(
                    group
                        .DOFade(
                            1f,
                            upDuration
                        )
                        .SetEase(Ease.InQuad)
                );
            }
        }


        masterSequence.Append(upSequence);


        // =====================================================
        // 연출 완료
        // =====================================================

        masterSequence
            .SetUpdate(useUnscaledTime)
            .OnComplete(FinishAnimation);
    }


    // =========================================================
    // 클릭 순간 AnimationRoot의 자식으로 이동
    // =========================================================

    private void MoveTargetsToAnimationRootImmediately()
    {
        foreach (RectTransform target in clickTargets)
        {
            if (target == null)
                continue;


            /*
             * true가 중요.
             *
             * 부모가 변경되더라도
             * 현재 화면상의:
             *
             * 위치
             * 크기
             * 회전
             *
             * 을 유지한다.
             */
            target.SetParent(
                animationRoot,
                true
            );


            // 가장 위에 렌더링
            target.SetAsLastSibling();


            target.gameObject.SetActive(true);
        }
    }


    // =========================================================
    // 모든 버튼의 Hover Scale 복구
    // =========================================================

    private static void ResetAllHoverScales()
    {
        AnimatedButton[] buttons =
            new AnimatedButton[AllButtons.Count];


        AllButtons.CopyTo(buttons);


        foreach (AnimatedButton button in buttons)
        {
            if (button == null)
                continue;


            button.ResetHoverScale();
        }
    }


    // =========================================================
    // 이 버튼의 Scale 복구
    // =========================================================

    private void ResetHoverScale()
    {
        if (scaleTarget == null)
            return;


        scaleTarget.DOKill();


        /*
         * 이미 AnimationRoot로 부모가
         * 변경됐을 수도 있기 때문에
         *
         * 단순 _originalScale이 아니라
         * 원래 World Scale을 기준으로 계산.
         */
        Vector3 targetScale =
            GetLocalScaleForWorldScale(
                scaleTarget,
                _originalWorldScale
            );


        scaleTarget
            .DOScale(
                targetScale,
                hoverDuration
            )
            .SetEase(Ease.OutCubic)
            .SetUpdate(useUnscaledTime);
    }


    // =========================================================
    // World Scale → Local Scale 계산
    // =========================================================

    private static Vector3 GetLocalScaleForWorldScale(
        Transform target,
        Vector3 desiredWorldScale)
    {
        if (target.parent == null)
        {
            return desiredWorldScale;
        }


        Vector3 parentScale =
            target.parent.lossyScale;


        return new Vector3(
            SafeDivide(
                desiredWorldScale.x,
                parentScale.x
            ),

            SafeDivide(
                desiredWorldScale.y,
                parentScale.y
            ),

            SafeDivide(
                desiredWorldScale.z,
                parentScale.z
            )
        );
    }


    private static float SafeDivide(
        float value,
        float divisor)
    {
        if (Mathf.Approximately(divisor, 0f))
        {
            return value;
        }


        return value / divisor;
    }


    // =========================================================
    // 현재 Tween 제거
    // =========================================================

    private void KillAllTweens()
    {
        if (scaleTarget != null)
        {
            scaleTarget.DOKill();
        }


        if (moveTarget != null)
        {
            moveTarget.DOKill();
        }


        foreach (RectTransform target in pushedTargets)
        {
            if (target == null)
                continue;


            target.DOKill();
        }


        foreach (RectTransform target in clickTargets)
        {
            if (target == null)
                continue;


            target.DOKill();


            CanvasGroup group =
                target.GetComponent<CanvasGroup>();


            if (group != null)
            {
                group.DOKill();
            }
        }
    }


    // =========================================================
    // 연출 종료
    // =========================================================

    private void FinishAnimation()
    {
        // -----------------------------------------------------
        // 연출 대상 전부 비활성화
        // -----------------------------------------------------

        foreach (RectTransform target in clickTargets)
        {
            if (target == null)
                continue;


            target.gameObject.SetActive(false);
        }


        // -----------------------------------------------------
        // AnimationRoot 비활성화
        // -----------------------------------------------------

        if (disableAnimationRootAfterAnimation &&
            animationRoot != null)
        {
            animationRoot.gameObject.SetActive(false);
        }


        // -----------------------------------------------------
        // 상호작용 다시 활성화
        // -----------------------------------------------------

        if (_eventSystem != null)
        {
            _eventSystem.enabled = true;
        }
    }


    // =========================================================
    // Disable
    // =========================================================

    private void OnDisable()
    {
        AllButtons.Remove(this);


        /*
         * disableTarget이 꺼지면서
         * 이 버튼 스크립트도 같이 꺼질 수 있음.
         *
         * 연출 중(_clicked == true)에는
         * EventSystem을 여기서 다시 켜면 안 됨.
         */
        if (!_clicked &&
            _eventSystem != null &&
            !_eventSystem.enabled)
        {
            _eventSystem.enabled = true;
        }
    }
}