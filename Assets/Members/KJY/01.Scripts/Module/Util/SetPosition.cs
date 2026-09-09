using DevLib.ModuleSystem;
using UnityEngine;

namespace Members.KJY._01.Scripts.Module.Util
{
    public class SetPosition : MonoModule
    {
        [SerializeField] private Transform targetTrm;

        public void SetTrmPosition(Vector3 pos)
        {
            targetTrm.position = pos;
        }
    }
}