using System;
using UnityEngine;

namespace Members.KJY.Scripts
{
    public class GroundChecker : MonoBehaviour
    {
        [SerializeField] private Rigidbody dice;
        private Rigidbody _rbCompo;

        private void Awake()
        {
            _rbCompo = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            Vector3 onlyY = Vector3.up * dice.linearVelocity.y;
            _rbCompo.linearVelocity = onlyY;

        }
    }
}