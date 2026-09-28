namespace Members.KJY._01.Scripts.Service
{
    public interface ILuckProvider
    {
        float Luck { get; }
        void AddBonusLuck(float amount); // 아이템·유물용
    }
}
