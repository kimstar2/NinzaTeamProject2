using System;

namespace _TevLib.Extension.DoT
{
    [Serializable]
    public enum SequenceInsertType
    {
        Prepend, PrependCallback, PrependInterval,
        Append, AppendCallback, AppendInterval,
        Join, JoinCallback
    }
}