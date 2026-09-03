using UnityEngine;
using UnityEngine.UI;

namespace Members.PDY.Scripts.Node
{
    public class UIMapLine : MaskableGraphic
    {
        private const int SegmentCount = 20;

        [SerializeField] private float nodePadding = 40f;

        private Vector2 _startPoint;
        private Vector2 _endPoint;
        private float _thickness = 5f;
        private MapDirection _mapDirection = MapDirection.Horizontal;

        public void DrawLine(Vector2 start, Vector2 end, MapDirection mapDirection, float lineThickness = 5f)
        {
            _mapDirection = mapDirection;

            Vector2 direction = (end - start).normalized;
            _startPoint = start + direction * nodePadding;
            _endPoint = end - direction * nodePadding;
            _thickness = lineThickness;

            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            if (_startPoint == _endPoint)
                return;

            Vector2 firstControlPoint;
            Vector2 secondControlPoint;
            CalculateControlPoints(_startPoint, _endPoint, out firstControlPoint, out secondControlPoint);

            Vector2 previousPoint = _startPoint;
            for (int segment = 1; segment <= SegmentCount; segment++)
            {
                float progress = segment / (float)SegmentCount;
                Vector2 currentPoint = CalculateCubicBezierPoint(
                    progress,
                    _startPoint,
                    firstControlPoint,
                    secondControlPoint,
                    _endPoint);

                DrawSegment(vertexHelper, previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
        }

        private void CalculateControlPoints(
            Vector2 start,
            Vector2 end,
            out Vector2 firstControlPoint,
            out Vector2 secondControlPoint)
        {
            if (_mapDirection == MapDirection.Horizontal)
            {
                float deltaX = (end.x - start.x) * 0.5f;
                firstControlPoint = start + new Vector2(deltaX, 0f);
                secondControlPoint = end - new Vector2(deltaX, 0f);
                return;
            }

            float deltaY = (end.y - start.y) * 0.5f;
            firstControlPoint = start + new Vector2(0f, deltaY);
            secondControlPoint = end - new Vector2(0f, deltaY);
        }

        private void DrawSegment(VertexHelper vertexHelper, Vector2 start, Vector2 end)
        {
            Vector2 direction = (end - start).normalized;
            Vector2 normal = new(-direction.y, direction.x);
            normal *= _thickness * 0.5f;

            int startIndex = vertexHelper.currentVertCount;
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            vertex.position = start - normal;
            vertexHelper.AddVert(vertex);

            vertex.position = start + normal;
            vertexHelper.AddVert(vertex);

            vertex.position = end + normal;
            vertexHelper.AddVert(vertex);

            vertex.position = end - normal;
            vertexHelper.AddVert(vertex);

            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
        }

        private static Vector2 CalculateCubicBezierPoint(
            float progress,
            Vector2 start,
            Vector2 firstControlPoint,
            Vector2 secondControlPoint,
            Vector2 end)
        {
            float inverseProgress = 1f - progress;
            float progressSquared = progress * progress;
            float inverseSquared = inverseProgress * inverseProgress;

            return inverseSquared * inverseProgress * start
                   + 3f * inverseSquared * progress * firstControlPoint
                   + 3f * inverseProgress * progressSquared * secondControlPoint
                   + progressSquared * progress * end;
        }
    }
}
