using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Dice;

namespace Members.KJY._01.Scripts.Events.Dice.Selector
{
    public class OnPlayerSelect : GameEvent
    {
        public DiceSelector DiceSelector {get; private set;}
        public OnPlayerSelect(DiceSelector diceSelector)
        {
            DiceSelector = diceSelector;
        }
    }
}