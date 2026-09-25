using Members.KJY._01.Scripts.Command;

namespace Members.KJY._01.Scripts.Dice.Battle
{
    public interface IBattleObserverService
    {
        public void AddCommand(ICommand command);
        public void RemoveCommand(ICommand command);
    }
}