using DevLib.CoreLib.Runtime;
using Members.KJY._01.Scripts.Dice.Data;

namespace Members.PSW.Code.Battle.LastReward
{
    // 굴림 완료 결과
    public class LastDiceEvent : GameEvent
    {
        public DiceFaceType FaceType { get; }
        public DiceDataSO DiceData { get; }

        public LastDiceEvent(DiceFaceType faceType, DiceDataSO diceData)
        {
            FaceType = faceType;
            DiceData = diceData;
        }
    }
}
