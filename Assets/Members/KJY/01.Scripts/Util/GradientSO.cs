using UnityEngine;

namespace Members.KJY._01.Scripts.Util
{
    [CreateAssetMenu(fileName = "GradientSO", menuName = "KJY/Util/GradientSO", order = 0)]
    public class GradientSO : ScriptableObject
    {
        [field:SerializeField] public Gradient DefaultGradient {get; private set;}
        [field: SerializeField] public bool IsUsingRandomGradient {get; private set;}
        [field:SerializeField] public Gradient[] Gradients {get; private set;}
        

        public Gradient GetGradient()
        {
            if (IsUsingRandomGradient)
                return Gradients[Random.Range(0, Gradients.Length)];
            return DefaultGradient;
        }
    }
}