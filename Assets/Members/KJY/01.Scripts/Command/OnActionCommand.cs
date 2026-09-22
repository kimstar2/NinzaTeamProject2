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
        
        
        public OnActionCommand(Action startAction , Action endAction)
        {
            StartAction = startAction;
            EndAction = endAction;
        }


        public async UniTask ExecuteAction()
        {
            StartAction?.Invoke();
            await _nextSignal.Task;
            EndAction?.Invoke();
        }

        public void SetNextSignal(CancellationToken token)
        {
            _nextSignal = new UniTaskCompletionSource();
            _nextSignal.Task.ToCancellationToken(token);
        }

        
        public void MoveNext()
        {
            _nextSignal?.TrySetResult();
            _nextSignal = null;
        }
    }
}