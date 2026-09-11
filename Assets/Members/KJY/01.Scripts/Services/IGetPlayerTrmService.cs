using Members.KJY._01.Scripts.Agent.Player;
using UnityEngine;

namespace Members.KJY._01.Scripts.Services
{
    public interface IGetPlayerTrmService
    {
        Transform GetPlayerTrm(PlayerType playerType);
    }
}