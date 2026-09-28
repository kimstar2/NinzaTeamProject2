using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.UI
{
    // Keep this Graphic on the topmost overlay Canvas so UI cannot cover the bars.
    public sealed class LetterboxBars : Graphic
    {
        private Rect ContentRect
        {
            get
            {
                Rect bounds = rectTransform.rect;
                Rect viewport = FixedAspectCamera.GetViewport(bounds.width, bounds.height);
                return new Rect(bounds.x + viewport.x * bounds.width,
                    bounds.y + viewport.y * bounds.height,
                    viewport.width * bounds.width, viewport.height * bounds.height);
            }
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect bounds = rectTransform.rect;
            Rect content = ContentRect;
            AddQuad(mesh, bounds.xMin, bounds.yMin, content.xMin, bounds.yMax);
            AddQuad(mesh, content.xMax, bounds.yMin, bounds.xMax, bounds.yMax);
            AddQuad(mesh, content.xMin, bounds.yMin, content.xMax, content.yMin);
            AddQuad(mesh, content.xMin, content.yMax, content.xMax, bounds.yMax);
        }

        private void AddQuad(VertexHelper mesh, float left, float bottom, float right, float top)
        {
            if (right <= left || top <= bottom)
                return;

            int start = mesh.currentVertCount;
            mesh.AddVert(new Vector3(left, bottom), color, Vector2.zero);
            mesh.AddVert(new Vector3(left, top), color, Vector2.zero);
            mesh.AddVert(new Vector3(right, top), color, Vector2.zero);
            mesh.AddVert(new Vector3(right, bottom), color, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2);
            mesh.AddTriangle(start + 2, start + 3, start);
        }

        public override bool Raycast(Vector2 screenPoint, Camera eventCamera)
        {
            // Block UI clicks on the bars, but leave the entire game area interactive.
            return base.Raycast(screenPoint, eventCamera)
                && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, screenPoint, eventCamera, out Vector2 localPoint)
                && !ContentRect.Contains(localPoint);
        }
    }
}
