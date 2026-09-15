using UnityEditor;
using UnityEngine;

namespace Members.PSW.Code.Eclipse.Editor
{
    [CustomEditor(typeof(EclipseEffect))]
    public sealed class EclipseEffectEditor : UnityEditor.Editor
    {
        private float _time;
        private bool _preview;
        private double _lastUpdate;

        private void OnEnable() => EditorApplication.update += UpdatePreview;

        private void OnDisable()
        {
            EditorApplication.update -= UpdatePreview;
            if (!Application.isPlaying && target) ((EclipseEffect)target).Stop();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("일식: 해·달 생성 → 합체 → 균열 → 조각 분리.\nPlay Mode: 자동 반복. 아래 버튼 또는 타임라인으로 미리보기.", MessageType.Info);
            EclipseEffect effect = (EclipseEffect)target;
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Replay Eclipse")) effect.Play();
                if (GUILayout.Button("Stop")) effect.Stop();
                return;
            }
            if (EditorUtility.IsPersistent(effect))
            {
                EditorGUILayout.HelpBox("씬 인스턴스 또는 Prefab Mode에서 미리볼 수 있습니다.", MessageType.None);
                return;
            }
            EditorGUI.BeginChangeCheck();
            _time = EditorGUILayout.Slider("Timeline (seconds)", _time, 0f, EclipseEffect.Duration);
            if (EditorGUI.EndChangeCheck())
            {
                _preview = false;
                effect.Sample(_time, true);
                SceneView.RepaintAll();
            }
            if (GUILayout.Button(_preview ? "Pause Preview" : "Preview Eclipse"))
            {
                _preview = !_preview;
                if (_preview) { _time = 0f; _lastUpdate = EditorApplication.timeSinceStartup; }
            }
            if (GUILayout.Button("Reset Preview"))
            {
                _preview = false;
                _time = 0f;
                effect.Stop();
                SceneView.RepaintAll();
            }
        }

        private void UpdatePreview()
        {
            if (!_preview || Application.isPlaying || !target) return;
            double now = EditorApplication.timeSinceStartup;
            _time += (float)(now - _lastUpdate);
            _lastUpdate = now;
            if (_time > EclipseEffect.Duration + 1f) _time = 0f;
            ((EclipseEffect)target).Sample(_time, true);
            SceneView.RepaintAll();
            Repaint();
        }
    }
}
