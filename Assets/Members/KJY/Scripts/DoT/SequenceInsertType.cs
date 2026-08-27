using System;

namespace Members.KJY.Scripts.DoT
{
    [Serializable]
    public enum SequenceInsertType
    {
        Prepend, PrependCallback, PrependInterval,
        Append, AppendCallback, AppendInterval,
        Join, JoinCallback
    }
}