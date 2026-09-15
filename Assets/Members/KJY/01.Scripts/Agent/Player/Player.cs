using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent.Player
{
    public class Player : AbstractAgent
    {
        public PlayerDataSO CurrentPlayerData {get; private set;}
    }
}