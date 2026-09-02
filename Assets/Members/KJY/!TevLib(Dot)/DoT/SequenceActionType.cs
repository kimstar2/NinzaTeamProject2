using System;
using UnityEngine;

namespace Members.KJY._TevLib_Dot_.DoT
{
    [Serializable]
    public enum SequenceActionType
    {
        [InspectorName("DONone")]
        DoNone,
        [InspectorName("DOAnchoredPosition")]
        DoAnchoredPosition,
        [InspectorName("DOCanvasAlpha")]
        DoCanvasAlpha,
        [InspectorName("DOLocalScale")]
        DoLocalScale, 
        [InspectorName("DOMove")]
        DoMove,
        [InspectorName("DOLocalRotation")]
        DoLocalRotation
    }
}