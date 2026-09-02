using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Members.KJY.T.TestScripts
{
    public class Dice : MonoBehaviour // 테스트 용 코드임
    {
        [SerializeField] private float rollForce;
        [SerializeField] private float torqueForce;
        [SerializeField] private Vector3 range;
        [SerializeField] private bool lockX, lockY, lockZ;
        [SerializeField] private float extraGravity = 200f;
        [SerializeField] private float gravityDelay = 0.15f;
        [SerializeField] private Transform groundChecker;
        [SerializeField] private float checkerDistance;
        [SerializeField] private LayerMask layerMask;
        private float _timeAir;
        private Rigidbody _rbCompo;
        
        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            _rbCompo = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
                Roll();

            IsGrounded = CheckGround();
        }

        private bool CheckGround()
        {
            Debug.DrawRay(groundChecker.position, groundChecker.up * checkerDistance, Color.green);
            return Physics.Raycast(groundChecker.transform.position, Vector3.down, checkerDistance, layerMask);
        }

        private void FixedUpdate()
        {
            ApplyVelocity();
            Vector3 newV = new Vector3(0, _rbCompo.linearVelocity.y, 0);
            _rbCompo.linearVelocity = newV;
            if (!IsGrounded)
            {
                CalcAirTime();
            }
            else
                _timeAir = 0f;
        }

        private void CalcAirTime()
        {
            _timeAir += Time.deltaTime;
        }

        private void ApplyVelocity()
        {
            if (_timeAir > gravityDelay)
                _rbCompo.AddForce(Vector3.up * (-extraGravity * Time.fixedDeltaTime), ForceMode.Acceleration);
        }

        [ContextMenu("Roll")]
        public void Roll()
        {
            _rbCompo.WakeUp();
            Vector3 randomRange = GetRandom();
            
            _rbCompo.AddForce(Vector3.up * rollForce, ForceMode.Impulse);
            _rbCompo.AddTorque(randomRange * torqueForce, ForceMode.Impulse);
        }

        private Vector3 GetRandom()
        {
            Vector3 newV = new Vector3(
                lockX ? range.x :
                Random.Range(-range.x, range.x),
                lockY ? range.y :
                Random.Range(-range.y, range.y),
                lockZ ? range.z :
                Random.Range(-range.z, range.z));
            return newV;
        }
    }
}