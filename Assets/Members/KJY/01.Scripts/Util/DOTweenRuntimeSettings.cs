using DG.Tweening;
using UnityEngine;

namespace Members.KJY._01.Scripts.Util
{
    internal static class DOTweenRuntimeSettings
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // The catalog reveals all 119 entries together; reserve room for other UI too.
            DOTween.Init();
            DOTween.SetTweensCapacity(500, 200);
        }
    }
}
