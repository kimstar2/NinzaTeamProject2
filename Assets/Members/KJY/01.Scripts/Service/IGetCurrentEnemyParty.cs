using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.GameSystem.EnemyParty;

namespace Members.KJY._01.Scripts.Service
{
    public interface IGetCurrentEnemyParty
    {
        EnemyPartyListSO EnemyPartyList { get; }
        EnemyDataSO GetData(EnemyPartyType crtType);
    }
}