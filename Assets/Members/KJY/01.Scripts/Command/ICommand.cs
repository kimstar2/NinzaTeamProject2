using System.Threading;
using Cysharp.Threading.Tasks;

namespace Members.KJY._01.Scripts.Command
{
    public interface ICommand
    {
        void Execute();
        UniTask ExecuteAction(CancellationToken token);
        
        
        void MoveNext();
    }
}