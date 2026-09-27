using System;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Data
{
    // 주사위 도감에 나올 전체 면 목록. Resources/DiceCatalog 에 두면 도감이 이 목록을 우선 사용한다.
    [CreateAssetMenu(fileName = "DiceCatalog", menuName = "KJY/Game/Dice/DiceCatalog", order = 1)]
    public class DiceCatalogSO : ScriptableObject
    {
        public const string ResourcePath = "DiceCatalog";
        [field: SerializeField] public DiceDataSO[] Faces { get; private set; }
        [field: SerializeField] public DiceDataSO[] StarterFaces { get; private set; } // 처음부터 해금된 플레이어 기본 스킬

        public bool IsStarter(DiceDataSO face) =>
            face != null && StarterFaces != null && Array.IndexOf(StarterFaces, face) >= 0;

        public bool IsUnlocked(DiceDataSO face) => IsStarter(face) || DiceCatalogProgress.IsDiscovered(face);
    }
}
