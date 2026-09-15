using Members.KJY._01.Scripts.Agent.Player;

namespace Members.KJY._01.Scripts.Dice.Interface
{
    public interface IGetIsSelectService
    {
        (PlayerType type, bool isSelect) GetIsSelect();
    }
}