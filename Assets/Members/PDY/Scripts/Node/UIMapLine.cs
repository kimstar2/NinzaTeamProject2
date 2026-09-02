using UnityEngine;

namespace Members.PDY.Scripts.Node
{
    using UnityEngine;
    using UnityEngine.UI;

    public class UIMapLine : MaskableGraphic
    {
        // --- [에러가 났던 변수들 선언부] ---
        private Vector2 startPoint;
        private Vector2 endPoint;
        
        // 선의 두께 (이 변수가 없어서 두 번째 사진에서 에러가 났습니다)
        private float thickness = 5f; 
        
        // 곡선의 부드러움 정도
        private int segmentCount = 20; 

        // 노드의 반지름 크기만큼 선을 잘라낼 오프셋 값 (이 변수가 없어서 첫 번째 사진에서 에러가 났습니다)
        public float nodePadding = 40f; 

        // 맵 진행 방향 저장용 변수
        private MapDirection mapDirection = MapDirection.Horizontal; 
        // ------------------------------------

        public void DrawLine(Vector2 start, Vector2 end, MapDirection mapDir, float lineThickness = 5f)
        {
            mapDirection = mapDir;
            Vector2 dir = (end - start).normalized;
            
            // 노드의 반지름 크기만큼 바깥으로 밀어내어 선 긋기 시작
            startPoint = start + (dir * nodePadding);
            endPoint = end - (dir * nodePadding);
            
            thickness = lineThickness;
            SetVerticesDirty(); 
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); // 이전 메쉬 초기화

            if (startPoint == endPoint) return;

            Vector2 p0 = startPoint;
            Vector2 p3 = endPoint;
            Vector2 p1, p2;

            // 맵 방향에 따라 곡선이 뻗어나가는 축(장력)을 변경합니다.
            if (mapDirection == MapDirection.Horizontal)
            {
                float deltaX = (p3.x - p0.x) * 0.5f;
                p1 = p0 + new Vector2(deltaX, 0);
                p2 = p3 - new Vector2(deltaX, 0);
            }
            else // Vertical (세로 진행)
            {
                float deltaY = (p3.y - p0.y) * 0.5f;
                p1 = p0 + new Vector2(0, deltaY); 
                p2 = p3 - new Vector2(0, deltaY);
            }

            Vector2 prevPoint = p0;

            for (int i = 1; i <= segmentCount; i++)
            {
                float t = i / (float)segmentCount;
                Vector2 currPoint = CalculateCubicBezierPoint(t, p0, p1, p2, p3);

                DrawSegment(vh, prevPoint, currPoint);
                prevPoint = currPoint;
            }
        }

        private void DrawSegment(VertexHelper vh, Vector2 start, Vector2 end)
        {
            Vector2 dir = (end - start).normalized;
            
            // 선의 두께를 만들기 위한 수직 벡터 계산
            Vector2 normal = new Vector2(-dir.y, dir.x) * (thickness / 2f); 

            int startIndex = vh.currentVertCount;

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color; // Inspector에서 설정한 Color 적용

            // 사각형을 이루는 4개의 꼭짓점(Vertex) 세팅
            vertex.position = start - normal;
            vh.AddVert(vertex);

            vertex.position = start + normal;
            vh.AddVert(vertex);

            vertex.position = end + normal;
            vh.AddVert(vertex);

            vertex.position = end - normal;
            vh.AddVert(vertex);

            // 4개의 꼭짓점을 이어 2개의 삼각형(Triangle)으로 만듦
            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
        }

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