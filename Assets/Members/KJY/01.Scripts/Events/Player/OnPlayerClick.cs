using DevLib.CoreLib.Runtime;
using UnityEngine;

namespace Members.KJY._01.Scripts.Events.Player
{
    public class OnPlayerClick : GameEvent
    {
        public Vector3 PointerPos {get; private set;}
        public OnPlayerClick(Vector3 pointerPos)
        {
            PointerPos = pointerPos;
        }
    }
}