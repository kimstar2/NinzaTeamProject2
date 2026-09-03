using DevLib.CoreLib.Runtime;

namespace Members.PSW.Code.DiceReroll_Lock.Events
{
    public static class DiceUIEvents
    {
        public static readonly RerollEvent RerollEvent = new RerollEvent();
    }
    
    public class RerollEvent : GameEvent
    {
        
    }

    public class LockEvent : GameEvent
    {
        public int DiceNum { get; private set; }

        public LockEvent(int value)
        {
            DiceNum = value;
        }
    }

    public class PEvent : GameEvent
    {
        public string YourName { get; private set; }

        public PEvent(string s)
        {
            YourName = s;
        }
    }
}