using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY.Scripts.DoT
{
    [Serializable]
    public struct TweenStep
    {
        [field: SerializeField] public SequenceActionType ActionType { get; private set; }
        [field: SerializeField] public SequenceInsertType InsertType { get; private set; }
        [field: SerializeField] public Ease EaseType { get; private set; }
        
        [field: SerializeField, Header("Transform")]
        public Vector3 TransformValue { get; private set; }
        [field: SerializeField, Header("Canvas Only")]
        public float FadeValue { get; private set; }
        public float Duration { get; private set; }
        
        [field: SerializeField, Header("CallbackSet")]
        public UnityEvent Callback { get; private set; }
        public float Interval { get; private set; }
        
    }
}