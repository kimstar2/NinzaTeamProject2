using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Command
{
    public interface ICommand
    {
        void Execute();
        UniTask ExecuteAction(CancellationToken token);
    }
}