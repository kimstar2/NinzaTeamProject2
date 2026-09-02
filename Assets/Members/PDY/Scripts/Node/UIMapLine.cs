using UnityEngine;

namespace Members.PDY.Scripts.Node
{
    using UnityEngine;
    using UnityEngine.UI;

    public class UIMapLine : MonoBehaviour
    {
        // 선을 몇 개의 조각으로 쪼개서 부드럽게 만들지 결정 (숫자가 클수록 부드러움)
        private int segmentCount = 20; 

        public void DrawLine(Vector2 startPos, Vector2 endPos, float thickness = 5f)
        {
            // 1. 기존 프리팹에 달려있던 직선 Image 비활성화 (곡선 조각들로 대체하기 위함)
            Image baseImage = GetComponent<Image>();
            if (baseImage != null) baseImage.enabled = false;

            // 2. 곡선의 제어점(Control Points) 설정
            // X축 방향으로 거리가 멀수록 선이 가로로 먼저 뻗어나가도록 '장력'을 줍니다.
            float distanceX = Mathf.Abs(endPos.x - startPos.x);
            Vector2 p0 = startPos;
            Vector2 p3 = endPos;
            Vector2 p1 = p0 + new Vector2(distanceX * 0.5f, 0); // 시작점에서 가로로 뻗는 점
            Vector2 p2 = p3 - new Vector2(distanceX * 0.5f, 0); // 끝점에서 가로로 뻗는 점

            Vector2 previousPoint = p0;

            // 3. 곡선을 여러 개의 짧은 직선 조각(Segment)으로 쪼개서 동적으로 생성합니다.
            for (int i = 1; i <= segmentCount; i++)
            {
                float t = i / (float)segmentCount;
                Vector2 currentPoint = CalculateCubicBezierPoint(t, p0, p1, p2, p3);

                CreateStraightSegment(previousPoint, currentPoint, thickness);
                previousPoint = currentPoint;
            }
        }

        // 짧은 선 조각을 생성하는 함수
        private void CreateStraightSegment(Vector2 start, Vector2 end, float thickness)
        {
            GameObject segment = new GameObject("CurveSegment");
            segment.transform.SetParent(transform, false);
            
            Image img = segment.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.6f); // 곡선의 투명도를 살짝 낮춰 자연스럽게

            RectTransform rect = segment.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            Vector2 dir = end - start;
            float distance = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            rect.sizeDelta = new Vector2(distance, thickness);
            rect.anchoredPosition = start + (dir / 2f);
            rect.localRotation = Quaternion.Euler(0, 0, angle);
        }

        // 3차 베지어 곡선 수학 공식
        private Vector2 CalculateCubicBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;

            return p;
        }
    }
}