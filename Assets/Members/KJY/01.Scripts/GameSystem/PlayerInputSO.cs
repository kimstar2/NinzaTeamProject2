using System.Diagnostics.Tracing;
using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Events.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Members.KJY._01.Scripts.GameSystem
{
    [CreateAssetMenu(fileName = "Player Input", menuName = "KJY/System/Player Input", order = 0)]
    public class PlayerInputSO : ScriptableObject , Controls.IPlayerActions
    {
        [field:SerializeField] public EventChannelSO EventChannel { get; private set; }
        private Controls _controls;
        public Vector3 PointerPos {get; private set;}

        public void SetEnable()
        {
            if (_controls == null)
            {
                _controls = new Controls();
                _controls.Player.SetCallbacks(this);
            }
            _controls.Player.Enable();
        }


        public void SetDisable() => _controls?.Player.Disable();
        
        public void OnMove(InputAction.CallbackContext context) { }
        public void OnPointer(InputAction.CallbackContext context) => PointerPos = context.ReadValue<Vector2>();

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
                EventChannel.RaiseEvent(new OnPlayerClick(PointerPos));
        }

        public void OnInteract(InputAction.CallbackContext context) { }

        public void OnCrouch(InputAction.CallbackContext context) { }

        public void OnJump(InputAction.CallbackContext context) { }

        public void OnPrevious(InputAction.CallbackContext context) { }

        public void OnNext(InputAction.CallbackContext context) { }

        public void OnSprint(InputAction.CallbackContext context) { }
    }
}