using UnityEngine;

namespace Members.KJY._TevLib_Dot_.HashDataSystem
{
    [CreateAssetMenu(fileName = "AnimHash data", menuName = "TevLib/System/HashData/AnimHash", order = 0)]
    public class AnimHashSO : ScriptableObject
    {
        [field: SerializeField] public string HashName { get; private set; }
        [field: SerializeField] public int HashValue { get; private set; }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(HashName))
            {
                HashValue = 0;
                return;
            }
            
            HashValue = Animator.StringToHash(HashName);
        }
    }
}