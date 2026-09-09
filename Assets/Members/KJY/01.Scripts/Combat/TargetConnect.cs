using System;
using Members.KJY._01.Scripts.Player;
using UnityEngine;

namespace Members.KJY._01.Scripts.Combat
{
    public class TargetConnect : MonoBehaviour , IClickToGettable
    {
        [field:SerializeField] public Transform FromTransform { get; private set; }
        [SerializeField] private Transform startTrm;

        public void Get(Transform ownerTrm)
        {
            transform.SetParent(ownerTrm);
        }

        public void Drop()
        {
            transform.SetParent(FromTransform);
        }

        
        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(startTrm.position,transform.position);
        }
    }
}