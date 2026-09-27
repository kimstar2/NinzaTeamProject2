using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Service;
using UnityEngine;

namespace Members.KJY._01.Scripts.UI
{
    public class StageBackground : MonoBehaviour
    {
        [SerializeField] private Transform backgroundRoot;

        private void Start()
        {
            if (!ServiceLocator.TryGet<IBattleDataStorage>(out var storage)) return;
            var stage = storage.Instance.CurrentStageData;
            if (stage != null && stage.backgroundPrefab != null)
                Instantiate(stage.backgroundPrefab, backgroundRoot);
        }
    }
}
