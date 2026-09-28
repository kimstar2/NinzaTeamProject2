using System.Collections;
using UnityEngine;

namespace Members.PSW.Code.SettingSystem
{
    // 창 최대화 (Windows 빌드 전용)
    public sealed class WindowMaximizer : MonoBehaviour
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern System.IntPtr GetActiveWindow();
        [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);
        private const int SwMaximize = 3;
#endif
        private static WindowMaximizer _instance;

        public static void MaximizeNextFrame()
        {
            if (_instance == null)
            {
                var go = new GameObject(nameof(WindowMaximizer)) { hideFlags = HideFlags.HideInHierarchy };
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<WindowMaximizer>();
            }
            _instance.StopAllCoroutines();
            _instance.StartCoroutine(_instance.Maximize());
        }

        private IEnumerator Maximize()
        {
            yield return null;
            yield return null;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            ShowWindow(GetActiveWindow(), SwMaximize);
#endif
        }
    }
}
