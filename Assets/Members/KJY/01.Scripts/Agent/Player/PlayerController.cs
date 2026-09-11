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
        private Camera _camera;

        protected override void InitializeModules()
        {
            base.InitializeModules();
            _camera = Camera.main;
        }

        private void OnEnable()
        {
            playerInput.SetEnable();
        }

        private void OnDisable()
        {
            playerInput.SetDisable();
        }


        public Vector3 PointerToWorldPos(Vector3 pointerPos)
        {
            Vector3 worldPos = _camera.ScreenToWorldPoint(pointerPos);
            return worldPos;
        }
    }
}
