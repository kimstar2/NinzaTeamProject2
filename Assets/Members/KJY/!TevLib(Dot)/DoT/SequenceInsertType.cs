using System;

namespace Members.KJY._TevLib_Dot_.DoT
{
    [Serializable]
    public enum SequenceInsertType
    {
        Prepend, PrependCallback, PrependInterval,
        Append, AppendCallback, AppendInterval,
        Join, JoinCallback
    }
}