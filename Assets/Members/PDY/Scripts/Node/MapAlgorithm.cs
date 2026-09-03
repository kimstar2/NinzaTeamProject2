using System;
using System.Collections.Generic;
using UnityEngine;

namespace Members.PDY.Scripts.Node
{
    public static class MapAlgorithm
    {
        private const int MaxStartingRowSelectionAttempts = 100;

        public static List<List<Node>> GenerateMapData(
            int columns,
            int rows,
            int startingPaths,
            float spacingX,
            float spacingY,
            MapDirection direction,
            float jitterX,
            float jitterY)
        {
            ValidateDimensions(columns, rows);

            List<List<Node>> map = CreateGrid(
                columns,
                rows,
                spacingX,
                spacingY,
                direction,
                jitterX,
                jitterY);

            List<int> currentRows = ConnectStartNode(map, rows, startingPaths);
            currentRows = ConnectIntermediateColumns(map, columns, rows, currentRows);
            ConnectBossNode(map, columns, rows, currentRows);

            return map;
        }

        private static void ValidateDimensions(int columns, int rows)
        {
            if (columns < 2)
                throw new ArgumentOutOfRangeException(nameof(columns), columns, "Map requires at least 2 columns.");

            if (rows < 1)
                throw new ArgumentOutOfRangeException(nameof(rows), rows, "Map requires at least 1 row.");
        }

        private static List<List<Node>> CreateGrid(
            int columns,
            int rows,
            float spacingX,
            float spacingY,
            MapDirection direction,
            float jitterX,
            float jitterY)
        {
            List<List<Node>> map = new(columns);

            float offsetX = direction == MapDirection.Horizontal
                ? (columns - 1) * spacingX / 2f
                : (rows - 1) * spacingX / 2f;
            float offsetY = direction == MapDirection.Horizontal
                ? (rows - 1) * spacingY / 2f
                : (columns - 1) * spacingY / 2f;

            for (int column = 0; column < columns; column++)
            {
                List<Node> columnNodes = new(rows);

                for (int row = 0; row < rows; row++)
                {
                    Node node = new(column, row)
                    {
                        type = AssignNodeType(column, columns),
                        position = CalculateNodePosition(
                            column,
                            row,
                            columns,
                            rows,
                            spacingX,
                            spacingY,
                            offsetX,
                            offsetY,
                            direction,
                            jitterX,
                            jitterY)
                    };

                    columnNodes.Add(node);
                }

                map.Add(columnNodes);
            }

            return map;
        }

        private static Vector2 CalculateNodePosition(
            int column,
            int row,
            int columns,
            int rows,
            float spacingX,
            float spacingY,
            float offsetX,
            float offsetY,
            MapDirection direction,
            float jitterX,
            float jitterY)
        {
            bool isEndpointColumn = column == 0 || column == columns - 1;
            float currentJitterX = isEndpointColumn ? 0f : UnityEngine.Random.Range(-jitterX, jitterX);
            float currentJitterY = isEndpointColumn ? 0f : UnityEngine.Random.Range(-jitterY, jitterY);

            if (direction == MapDirection.Horizontal)
            {
                float x = column * spacingX - offsetX + currentJitterX;
                float y = row * spacingY - offsetY + currentJitterY;

                if (isEndpointColumn)
                    y = rows / 2f * spacingY - offsetY;

                return new Vector2(x, y);
            }

            float verticalX = row * spacingX - offsetX + currentJitterX;
            float verticalY = column * spacingY - offsetY + currentJitterY;

            if (isEndpointColumn)
                verticalX = rows / 2f * spacingX - offsetX;

            return new Vector2(verticalX, verticalY);
        }

        private static List<int> ConnectStartNode(List<List<Node>> map, int rows, int startingPaths)
        {
            Node startNode = map[0][rows / 2];
            startNode.isUsed = true;
            startNode.type = NodeType.Start;

            List<int> currentRows = SelectStartingRows(rows, startingPaths);
            foreach (int row in currentRows)
            {
                Node nextNode = map[1][row];
                nextNode.isUsed = true;
                startNode.nextNodes.Add(nextNode);
            }

            return currentRows;
        }

        private static List<int> SelectStartingRows(int rows, int startingPaths)
        {
            int actualPaths = Mathf.Min(startingPaths, rows);
            int attempts = 0;
            List<int> selectedRows = new();

            while (selectedRows.Count < actualPaths && attempts < MaxStartingRowSelectionAttempts)
            {
                int randomRow = UnityEngine.Random.Range(0, rows);
                if (!selectedRows.Contains(randomRow))
                    selectedRows.Add(randomRow);

                attempts++;
            }

            selectedRows.Sort();
            return selectedRows;
        }

        private static List<int> ConnectIntermediateColumns(
            List<List<Node>> map,
            int columns,
            int rows,
            List<int> currentRows)
        {
            for (int column = 1; column < columns - 2; column++)
            {
                List<int> nextRows = new(currentRows.Count);

                for (int index = 0; index < currentRows.Count; index++)
                {
                    int currentRow = currentRows[index];
                    int minRow = index == 0 ? 0 : nextRows[index - 1];
                    int maxRow = index == currentRows.Count - 1 ? rows - 1 : currentRows[index + 1] + 1;

                    int clampMin = Mathf.Clamp(Mathf.Max(minRow, currentRow - 1), 0, rows - 1);
                    int clampMax = Mathf.Clamp(Mathf.Min(maxRow, currentRow + 1), 0, rows - 1);
                    if (clampMin > clampMax)
                        clampMin = clampMax;

                    int nextRow = UnityEngine.Random.Range(clampMin, clampMax + 1);
                    nextRows.Add(nextRow);

                    Node currentNode = map[column][currentRow];
                    Node nextNode = map[column + 1][nextRow];
                    currentNode.isUsed = true;
                    nextNode.isUsed = true;

                    if (!currentNode.nextNodes.Contains(nextNode))
                        currentNode.nextNodes.Add(nextNode);
                }

                currentRows = nextRows;
            }

            return currentRows;
        }

        private static void ConnectBossNode(
            List<List<Node>> map,
            int columns,
            int rows,
            IEnumerable<int> currentRows)
        {
            Node bossNode = map[columns - 1][rows / 2];
            bossNode.isUsed = true;
            bossNode.type = NodeType.Boss;

            foreach (int row in currentRows)
            {
                Node currentNode = map[columns - 2][row];
                if (!currentNode.nextNodes.Contains(bossNode))
                    currentNode.nextNodes.Add(bossNode);
            }
        }

        private static NodeType AssignNodeType(int column, int totalColumns)
        {
            if (column == 0)
                return NodeType.Start;
            if (column == totalColumns - 1)
                return NodeType.Boss;
            if (column == totalColumns - 2)
                return NodeType.Rest;

            int oneThirdPoint = totalColumns / 3;
            int twoThirdsPoint = totalColumns * 2 / 3;
            if (column == oneThirdPoint || column == twoThirdsPoint)
                return NodeType.Rest;

            if (column == 1)
                return NodeType.Enemy;

            float randomValue = UnityEngine.Random.value;
            if (randomValue < 0.20f)
                return NodeType.Merchant;
            if (randomValue < 0.40f)
                return NodeType.Elite;
            if (randomValue < 0.70f)
                return NodeType.Event;

            return NodeType.Enemy;
        }
    }
}
