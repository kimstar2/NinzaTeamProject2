using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public class ButtonHoverEffect : MonoBehaviour,
IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Targets")]
    [SerializeField] private Graphic targetGraphic;
    [SerializeField] private RectTransform targetRect;
    [SerializeField] private MenuSelectionArrow selectionArrow;
    [SerializeField] private RectTransform arrowAnchor;

    [Header("Colors")]
    [SerializeField] private Color normalColor = new(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color hoverColor = Color.white;

    [Header("Scale")]
    [SerializeField, Range(1f, 1.2f)] private float hoverScale = 1.05f;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float hoverDuration = 0.12f;
    [SerializeField, Min(0f)] private float returnDuration = 0.18f;

    private Vector3 normalScale;
    private Coroutine transition;

    private void Reset()
    {
        targetGraphic = GetComponent<Image>();
        targetRect = GetComponent<RectTransform>();
    }

    private void Awake()
    {
        if (targetGraphic == null || targetRect == null)
        {
            Debug.LogError(
                $"{nameof(ButtonHoverEffect)} requires a target Graphic and RectTransform.",
                this);
            enabled = false;
            return;
        }

        normalScale = targetRect.localScale;
        SetNormalImmediately();
    }

    private void OnDisable()
    {
        StopTransition();

        if (targetGraphic != null && targetRect != null)
        {
            SetNormalImmediately();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetArrowVisible(true);
        ChangeState(
            hoverColor,
            normalScale * hoverScale,
            hoverDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetArrowVisible(false);
        ChangeState(
            normalColor,
            normalScale,
            returnDuration);
    }

    private void ChangeState(Color color, Vector3 scale, float duration)
    {
        StopTransition();

        if (duration <= 0f)
        {
            targetGraphic.color = color;
            targetRect.localScale = scale;
            return;
        }

        transition = StartCoroutine(AnimateTo(color, scale, duration));
    }

    private IEnumerator AnimateTo(
        Color destinationColor,
        Vector3 destinationScale,
        float duration)
    {
        Color startColor = targetGraphic.color;
        Vector3 startScale = targetRect.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = progress * progress * (3f - 2f * progress);

            targetGraphic.color = Color.LerpUnclamped(
                startColor,
                destinationColor,
                easedProgress);

            targetRect.localScale = Vector3.LerpUnclamped(
                startScale,
                destinationScale,
                easedProgress);

            yield return null;
        }

        targetGraphic.color = destinationColor;
        targetRect.localScale = destinationScale;
        transition = null;
    }

    private void StopTransition()
    {
        if (transition == null)
        {
            return;
        }

        StopCoroutine(transition);
        transition = null;
    }

    private void SetNormalImmediately()
    {
        targetGraphic.color = normalColor;
        targetRect.localScale = normalScale;
        SetArrowVisible(false);
    }

    private void SetArrowVisible(bool isVisible)
    {
        if (isVisible && selectionArrow != null && arrowAnchor != null)
        {
            selectionArrow.MoveTo(arrowAnchor);
        }
    }
}
