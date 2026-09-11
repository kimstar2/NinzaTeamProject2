using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Members.KJY._01.Scripts.Command
{
    public class OnActionCommand : ICommand
    {
        public Action StartAction { get; private set; }
        public Action EndAction { get; private set; }
        
        private UniTaskCompletionSource _nextSignal;
        
        
        public OnActionCommand(Action startAction ,float delay , Action endAction)
        {
            StartAction = startAction;
            EndAction = endAction;
        }

        public void Execute() => StartAction?.Invoke();

        public async UniTask ExecuteAction(CancellationToken token)
        {
            _nextSignal = new UniTaskCompletionSource();
            _nextSignal.Task.ToCancellationToken(token);
            
            await _nextSignal.Task;
            EndAction?.Invoke();
        }

        public void MoveNext()
        {
            _nextSignal?.TrySetResult();
            _nextSignal = null;
        }
    }
}