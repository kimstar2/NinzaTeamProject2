using Members.KJY._01.Scripts;
using NaughtyAttributes;
using UnityEngine;

namespace Members.CJY.Scripts
{
    public class BattleNodeBT : NodeBT
    {
        [field: SerializeField, ReadOnly] public BattleData CurrentBattleData { get; private set; }

        public override void Init(NodeConnect nodeObj, NodeEvent nodeEv)
        {
            base.Init(nodeObj, nodeEv);
            CurrentBattleData = nodeObj.battleData;
        }

        public override void ResetItem()
        {
            base.ResetItem();
            CurrentBattleData = null;
        }
    }
}
