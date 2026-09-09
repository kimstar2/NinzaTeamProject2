using UnityEngine;

namespace Members.KJY._01.Scripts.Player
{
    public interface IClickToGettable
    {
        Transform FromTransform { get; }
        void Get(Transform ownerTrm);
        void Drop();
    }
}