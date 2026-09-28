using System.Collections.Generic;

namespace Members.CJY.Scripts
{
    public partial class NodeMaker
    {
        internal IEnumerable<NodeConnect> DeveloperNodes
        {
            get
            {
                foreach (var column in nodeConnects)
                    foreach (var node in column) yield return node;
            }
        }

        internal bool DeveloperMoveTo(NodeConnect node)
        {
            if (currentNode == null || node == null || nodeEvent == null || nodeEvent.DeveloperHasPendingBattle
                || Members.KJY._01.Scripts.UI.SceneTransition.IsBusy
                || !nodeConnects.Exists(column => column.Contains(node)))
                return false;
            StopAllCoroutines();
            currentNode = node;
            NodeVisualSetting();
            PlayPreviewAnim();
            SaveMap();
            foreach (var hud in UnityEngine.Object.FindObjectsByType<Members.KJY._01.Scripts.UI.MapHud>(UnityEngine.FindObjectsSortMode.None))
                hud.DeveloperRefresh(this);
            return true;
        }
    }
}
