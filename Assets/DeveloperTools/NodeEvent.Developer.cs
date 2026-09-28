#if UNITY_EDITOR || DEVELOPMENT_BUILD
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Service;

namespace Members.CJY.Scripts
{
    public partial class NodeEvent
    {
        internal bool DeveloperHasPendingBattle => _pendingNode != null;

        internal bool DeveloperEnter(NodeConnect node)
        {
            if (_pendingNode != null || node == null || _nodeMaker == null || node.info == null
                || Members.KJY._01.Scripts.UI.SceneTransition.IsBusy
                || !System.Linq.Enumerable.Contains(_nodeMaker.DeveloperNodes, node))
                return false;
            if (node.IsBattle)
            {
                if (ServiceLocator.TryGet<IBattleDataStorage>(out var storage)) battleDataStorage = storage.Instance;
                if (battleDataStorage == null || !battleDataStorage.TrySetBattle(node.battleData)) return false;
                // Capture the position before teleporting, as normal battle entry does.
                battleDataStorage.SetReturnPoint(_nodeMaker.SaveKey);
            }
            if (!_nodeMaker.DeveloperMoveTo(node)) return false;
            // Preserve event encounter context and other normal entry observers.
            OnNodeSelected?.Invoke(node);
            if (node.IsBattle) battleDataStorage.Tp();
            else NodeSelected(node.info.type);
            return true;
        }
    }
}
#endif
