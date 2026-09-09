using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace _TevLib.Extension.DoT
{
    [CreateAssetMenu(fileName = "TweenStep data", menuName = "TevLib/Tween/TweenStep", order = 0)]
    public class TweenStepSO : ScriptableObject
    {
        [field: SerializeField] public SequenceActionType ActionType { get; private set; }
        [field: SerializeField] public SequenceInsertType InsertType { get; private set; }
        [field: SerializeField] public Ease EaseType { get; private set; }
        [field: SerializeField] public float Duration { get; private set; }
        
        [field:SerializeField] public bool IsRandomizeValue { get; private set; }
        [field:SerializeField] public bool UsingFastBeyond { get; private set; }
        [field:SerializeField] public Vector3 MinTransformValue { get; private set; }
        [field:SerializeField] public Vector3 MaxTransformValue { get; private set; }
        public Vector3 GetTransformValue()
        {
            Vector3 resultVector = MinTransformValue;
            if (IsRandomizeValue)
            {
                float x = Random.Range(MinTransformValue.x, MaxTransformValue.x);
                float y = Random.Range(MinTransformValue.y, MaxTransformValue.y);
                float z = Random.Range(MinTransformValue.z, MaxTransformValue.z);
                resultVector = new Vector3(x, y, z);
            }
            return resultVector;
        }
        [field:SerializeField] public float FadeValue { get; private set; }
        [field:SerializeField] public Color ColorValue { get; private set; }
        
        [field: SerializeField] public UnityEvent Callback { get; private set; }
    }
}