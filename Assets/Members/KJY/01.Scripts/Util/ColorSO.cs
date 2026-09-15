using UnityEngine;

namespace Members.KJY._01.Scripts.Util
{
    [CreateAssetMenu(fileName = "ColorSOForUnityEvent", menuName = "KJY/Util/Color", order = 0)]
    public class ColorSO : ScriptableObject
    {
        [field:SerializeField] public Color DefaultColor {get; private set;}
        [field: SerializeField] public bool IsUsingRandomColor {get; private set;}
        [field:SerializeField] public Color[] Colors {get; private set;}
        

        public Color GetColor()
        {
            if (IsUsingRandomColor)
                return Colors[Random.Range(0, Colors.Length)];
            return DefaultColor;
        }
    }
}