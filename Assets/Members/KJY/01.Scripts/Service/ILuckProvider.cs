namespace Members.KJY._01.Scripts.Service
{
    // 행운이 높을수록 강함도가 높은 스킬 면이 잘 나온다. 플레이어에게는 보이지 않는 확률 보정용.
    public interface ILuckProvider
    {
        float Luck { get; }
        void AddBonusLuck(float amount); // 아이템·유물용
    }
}
