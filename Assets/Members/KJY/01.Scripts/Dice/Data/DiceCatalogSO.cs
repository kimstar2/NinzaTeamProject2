using System;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Data
{
    [CreateAssetMenu(fileName = "DiceCatalog", menuName = "KJY/Game/Dice/DiceCatalog", order = 1)]
    public class DiceCatalogSO : ScriptableObject
    {
        public const string ResourcePath = "DiceCatalog";
        [field: SerializeField] public DiceDataSO[] Faces { get; private set; }
        [field: SerializeField] public DiceDataSO[] StarterFaces { get; private set; }

        public bool IsStarter(DiceDataSO face) =>
            face != null && StarterFaces != null && Array.IndexOf(StarterFaces, face) >= 0;

        public bool IsUnlocked(DiceDataSO face) => IsStarter(face) || DiceCatalogProgress.IsDiscovered(face);
    }
}
