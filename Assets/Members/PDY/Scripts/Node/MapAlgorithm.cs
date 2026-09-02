namespace Members.PDY.Scripts.Node
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class MapAlgorithm
    {
        // [수정됨] jitterX, jitterY 매개변수 추가
        public static List<List<Node>> GenerateMapData(int columns, int rows, int startingPaths, float spacingX, float spacingY, MapDirection direction, float jitterX, float jitterY)
        {
            List<List<Node>> map = new List<List<Node>>();

            float offsetX = (direction == MapDirection.Horizontal) ? ((columns - 1) * spacingX) / 2f : ((rows - 1) * spacingX) / 2f;
            float offsetY = (direction == MapDirection.Horizontal) ? ((rows - 1) * spacingY) / 2f : ((columns - 1) * spacingY) / 2f;

            for (int c = 0; c < columns; c++)
            {
                var columnList = new List<Node>();
                for (int r = 0; r < rows; r++)
                {
                    Node node = new Node(c, r);
                    node.type = AssignNodeType(c, columns);

                    // [추가됨] 시작(0열)과 보스(마지막 열)는 흔들리지 않게 0으로 고정
                    float currentJitterX = (c == 0 || c == columns - 1) ? 0f : Random.Range(-jitterX, jitterX);
                    float currentJitterY = (c == 0 || c == columns - 1) ? 0f : Random.Range(-jitterY, jitterY);

                    if (direction == MapDirection.Horizontal)
                    {
                        // 기본 좌표에 지터값을 더해줍니다.
                        float x = (c * spacingX) - offsetX + currentJitterX;
                        float y = (r * spacingY) - offsetY + currentJitterY;
                        
                        if (c == 0 || c == columns - 1) 
                            y = ((rows / 2f) * spacingY) - offsetY;
                            
                        node.position = new Vector2(x, y);
                    }
                    else 
                    {
                        float x = (r * spacingX) - offsetX + currentJitterX;
                        float y = (c * spacingY) - offsetY + currentJitterY;
                        
                        if (c == 0 || c == columns - 1) 
                            x = ((rows / 2f) * spacingX) - offsetX;
                            
                        node.position = new Vector2(x, y);
                    }
                    
                    columnList.Add(node);
                }
                map.Add(columnList);
            }

            Node startNode = map[0][rows / 2];
            startNode.isUsed = true;
            startNode.type = NodeType.Start;

            List<int> currentRows = new List<int>();
            int actualPaths = Mathf.Min(startingPaths, rows);
            int safeCount = 0;

            while (currentRows.Count < actualPaths && safeCount < 100)
            {
                int randomRow = Random.Range(0, rows);
                if (!currentRows.Contains(randomRow)) currentRows.Add(randomRow);
                safeCount++;
            }
            currentRows.Sort(); 

            foreach (int row in currentRows)
            {
                Node nextNode = map[1][row];
                nextNode.isUsed = true;
                startNode.nextNodes.Add(nextNode);
            }

            for (int c = 1; c < columns - 2; c++) 
            {
                List<int> nextRows = new List<int>();
                for (int i = 0; i < currentRows.Count; i++)
                {
                    int currentRow = currentRows[i];
                    int minRow = (i == 0) ? 0 : nextRows[i - 1]; 
                    int maxRow = (i == currentRows.Count - 1) ? rows - 1 : currentRows[i + 1] + 1;

                    int clampMin = Mathf.Clamp(Mathf.Max(minRow, currentRow - 1), 0, rows - 1);
                    int clampMax = Mathf.Clamp(Mathf.Min(maxRow, currentRow + 1), 0, rows - 1);
                    if (clampMin > clampMax) clampMin = clampMax; 

                    int nextRow = Random.Range(clampMin, clampMax + 1);
                    nextRows.Add(nextRow);

                    Node currentNode = map[c][currentRow];
                    Node nextNode = map[c + 1][nextRow];
                    
                    currentNode.isUsed = true;
                    nextNode.isUsed = true;

                    if (!currentNode.nextNodes.Contains(nextNode))
                        currentNode.nextNodes.Add(nextNode);
                }
                currentRows = nextRows;
            }

            Node bossNode = map[columns - 1][rows / 2];
            bossNode.isUsed = true;
            bossNode.type = NodeType.Boss;

            foreach (int row in currentRows)
            {
                Node currentNode = map[columns - 2][row];
                if (!currentNode.nextNodes.Contains(bossNode))
                    currentNode.nextNodes.Add(bossNode);
            }

            return map;
        }

        private static NodeType AssignNodeType(int column, int totalColumns)
        {
            if (column == 0) return NodeType.Start;
            if (column == totalColumns - 1) return NodeType.Boss;
            if (column == totalColumns - 2) return NodeType.Rest;
            // [핵심 수정] 맵의 1/3, 2/3 지점 계산
            int oneThirdPoint = totalColumns / 3;
            int twoThirdsPoint = (totalColumns * 2) / 3;

            // 해당 지점은 무조건 휴식(Rest) 노드로 고정
            if (column == oneThirdPoint || column == twoThirdsPoint) 
                return NodeType.Rest;
            
            if (column == 1) return NodeType.Enemy; 
        
            // 나머지 열들은 확률에 따라 랜덤 배정 (휴식 노드 제외 후 확률 재조정)
            float r = Random.value;
            if (r < 0.20f) return NodeType.Merchant; // 20%
            if (r < 0.40f) return NodeType.Elite;    // 20%
            if (r < 0.70f) return NodeType.Event;    // 30%
            return NodeType.Enemy;                   // 30%
        }
    }
}