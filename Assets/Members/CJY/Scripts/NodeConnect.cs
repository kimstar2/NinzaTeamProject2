using System.Collections.Generic;
using Members.KJY._01.Scripts;

namespace Members.CJY.Scripts
{
    public class NodeConnect
    {
        public int column; // 몇번째 줄
        public int lane; // 몇번째 칸
        public NodeInfoSO info; // 타입
        public List<NodeConnect> nextNodes = new List<NodeConnect>();
        public NodeBT view;
        public BattleData battleData;
        public bool IsBattle => info.type is NodeType.Battle or NodeType.Elite or NodeType.Boss;

        public NodeConnect(int column, int lane, NodeInfoSO info)
        {
            this.column = column;
            this.lane = lane;
            this.info = info;
        }
    }
}