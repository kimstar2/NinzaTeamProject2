using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class MenuSelectionArrow : MonoBehaviour
{
    [SerializeField, Min(0.01f)]
    private float moveTime = 0.15f;

    private RectTransform arrowRect;
    private RectTransform destination;
    private Vector3 velocity;

    private void Awake()
    {
        arrowRect = GetComponent<RectTransform>();
    }

    public void MoveTo(RectTransform anchor)
    {
        if (anchor == null)
            return;

        destination = anchor;
    }

    private void LateUpdate()
    {
        if (destination == null)
            return;

        // 각 버튼의 표시 위치를 화살표 부모 좌표로 변환.
        Vector3 targetPosition = arrowRect.parent != null
            ? arrowRect.parent.InverseTransformPoint(destination.position)
            : destination.position;

        arrowRect.localPosition = Vector3.SmoothDamp(
            arrowRect.localPosition,
            targetPosition,
            ref velocity,
            moveTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime);
    }

    private void OnDisable()
    {
        destination = null;
        velocity = Vector3.zero;
    }
}