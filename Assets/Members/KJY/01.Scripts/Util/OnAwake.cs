using System.Collections;
using Members.KJY._01.Scripts.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Util
{
    public class OnAwake : MonoBehaviour
    {
        [field: SerializeField] public UnityEvent OnAwakeRaise { get; private set; }
        [SerializeField] private bool waitSceneTransition; // 커튼 열린 뒤 실행

        private void Awake()
        {
            if (waitSceneTransition && SceneTransition.IsBusy) StartCoroutine(RaiseAfterTransition());
            else OnAwakeRaise?.Invoke();
        }

        private IEnumerator RaiseAfterTransition()
        {
            yield return new WaitUntil(() => !SceneTransition.IsBusy);
            OnAwakeRaise?.Invoke();
        }
    }
}
