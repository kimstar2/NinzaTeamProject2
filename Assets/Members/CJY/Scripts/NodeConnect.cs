using System.Collections.Generic;

namespace Members.CJY.Scripts
{
    public class NodeConnect
    {
        public int column; // 몇번째 줄
        public int lane; // 몇번째 칸
        public NodeInfoSO info; // 타입
        public List<NodeConnect> nextNodes = new List<NodeConnect>();
        public NodeBT view;

        public NodeConnect(int column, int lane, NodeInfoSO info)
        {
            this.column = column;
            this.lane = lane;
            this.info = info;
        }
    }
}