using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Events.Player;
using Members.KJY._01.Scripts.GameSystem;
using Members.KJY._01.Scripts.Module.Util;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.Player
{
    public class PlayerController : ModuleOwner
    {
        [field: SerializeField] public PlayerInputSO playerInput;
        private SetPosition _setPosition;
        private Camera _camera;

        protected override void InitializeModules()
        {
            base.InitializeModules();
            _camera = Camera.main;
            _setPosition = GetModule<SetPosition>();
        }

        private void OnEnable()
        {
            playerInput.SetEnable();
            playerInput.EventChannel.AddListener<OnPlayerClick>(HandlePlayerClick);
        }

        private void OnDisable()
        {
            playerInput.EventChannel.RemoveListener<OnPlayerClick>(HandlePlayerClick);
            playerInput.SetDisable();
        }

        private void Update()
        {
            _setPosition.SetTrmPosition(PointerToWorldPos(playerInput.PointerPos));
        }

        private void HandlePlayerClick(OnPlayerClick evt) { }

        public Vector3 PointerToWorldPos(Vector3 pointerPos)
        {
            Vector3 worldPos = _camera.ScreenToWorldPoint(pointerPos);
            return worldPos;
        }
    }
}
