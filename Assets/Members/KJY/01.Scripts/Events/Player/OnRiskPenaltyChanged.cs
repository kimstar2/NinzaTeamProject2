using DevLib.CoreLib.Runtime;

namespace Members.KJY._01.Scripts.Events.Player
{
    public class OnRiskPenaltyChanged : GameEvent
    {
        public float PenaltyValue { get; private set; }

        public OnRiskPenaltyChanged(float penaltyValue)
        {
            PenaltyValue = penaltyValue;
        }
    }
}