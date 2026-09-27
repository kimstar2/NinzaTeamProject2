using _TevLib.Extension.DoT;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Members.KJY._01.Scripts.UI
{
    public class SceneTransition : MonoBehaviour
    {
        [SerializeField] private CanvasGroup curtain;
        [SerializeField] private TweenSequencer closeMotion, openMotion;
        private static SceneTransition _instance;
        private string _nextScene;
        private bool _busy;

        private void Awake()
        {
            if (_instance != null) { Destroy(gameObject); return; }
            _instance = this;
            curtain.alpha = 1f;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start() => openMotion.Sequence();

        public static void Load(int index) => Load(SceneUtility.GetScenePathByBuildIndex(index));

        public static void Load(string scene)
        {
            if (_instance == null) { SceneManager.LoadScene(scene); return; }
            if (_instance._busy) return;
            _instance._busy = true;
            _instance._nextScene = scene;
            _instance.curtain.blocksRaycasts = true;
            _instance.openMotion.Stop();
            _instance.closeMotion.Sequence();
        }

        // 닫기 시퀀스의 OnComplete에서 호출한다.
        public void LoadPendingScene()
        {
            if (!string.IsNullOrEmpty(_nextScene)) SceneManager.LoadSceneAsync(_nextScene);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _nextScene = null;
            curtain.blocksRaycasts = true;
            openMotion.Sequence();
        }

        public void FinishOpen()
        {
            curtain.blocksRaycasts = false;
            _busy = false;
        }

        private void OnDestroy()
        {
            if (_instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _instance = null;
        }
    }
}
