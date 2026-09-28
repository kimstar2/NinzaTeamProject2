using UnityEngine;

namespace Members.KJY._01.Scripts.UI
{
    [ExecuteAlways]
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class FixedAspectCamera : MonoBehaviour
    {
        public const float AspectRatio = 16f / 9f;

        private Camera _camera;
        private Rect _originalRect;

        private void OnEnable()
        {
            _camera = GetComponent<Camera>();
            _originalRect = _camera.rect;
            Update();
        }

        private void Update()
        {
            // Render textures have their own dimensions and must not be letterboxed.
            if (_camera.targetTexture != null || _camera.targetDisplay != 0)
                return;

            Rect viewport = GetViewport(Screen.width, Screen.height);
            if (_camera.rect != viewport)
                _camera.rect = viewport;
        }

        private void OnDisable()
        {
            if (_camera != null)
                _camera.rect = _originalRect;
        }

        public static Rect GetViewport(float width, float height)
        {
            if (width <= 0f || height <= 0f)
                return new Rect(0f, 0f, 1f, 1f);

            float screenAspect = width / height;
            if (screenAspect > AspectRatio)
            {
                float viewportWidth = AspectRatio / screenAspect;
                return new Rect((1f - viewportWidth) * 0.5f, 0f, viewportWidth, 1f);
            }

            float viewportHeight = screenAspect / AspectRatio;
            return new Rect(0f, (1f - viewportHeight) * 0.5f, 1f, viewportHeight);
        }
    }
}
