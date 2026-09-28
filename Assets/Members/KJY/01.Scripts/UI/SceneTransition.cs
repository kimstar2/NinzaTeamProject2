using _TevLib.Extension.DoT;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Members.KJY._01.Scripts.UI
{
    public class SceneTransition : MonoBehaviour
    {
        [SerializeField] private CanvasGroup curtain;
        [SerializeField] private TweenSequencer closeMotion, openMotion;
        [SerializeField, Min(0f)] private float bgmFadeOut = 0.5f;
        [SerializeField, Min(0f)] private float bgmFadeIn = 0.8f;
        private static SceneTransition _instance;
        public static float BgmFadeIn => _instance != null ? _instance.bgmFadeIn : 0.8f;
        public static bool IsBusy => _instance != null && _instance._busy;
        private string _nextScene;
        private bool _busy;

        public static event System.Action BeforeSceneLoad;

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
            if (_instance == null)
            {
                BeforeSceneLoad?.Invoke();
                SceneManager.LoadScene(scene);
                return;
            }
            if (_instance._busy) return;
            _instance._busy = true;
            _instance._nextScene = scene;
            _instance.curtain.blocksRaycasts = true;
            _instance.openMotion.Stop();
            _instance.closeMotion.Sequence();
            if (DevLib.ServiceLocator.ServiceLocator.TryGet<DevLib.ServiceLocator.IAudioService>(out var audio))
                audio.FadeOutBgm(_instance.bgmFadeOut);
        }

        // 닫기 시퀀스의 OnComplete에서 호출한다.
        public void LoadPendingScene()
        {
            if (!string.IsNullOrEmpty(_nextScene))
            {
                BeforeSceneLoad?.Invoke();
                SceneManager.LoadSceneAsync(_nextScene);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _nextScene = null;
            curtain.blocksRaycasts = true;
            StopAllCoroutines();
            StartCoroutine(OpenAfterLoad());
        }

        // 무거운 씬 첫 프레임 끊김 회피
        private System.Collections.IEnumerator OpenAfterLoad()
        {
            yield return null;
            yield return null;
            yield return new WaitForEndOfFrame();
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
