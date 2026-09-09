namespace Members.PSW.Code.DiceReroll_Lock
{
    public interface IReroll
    {
        public bool IsLocked { get; }
        void Reroll();
        void Lock();
    }
}