using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public GridManager grid;

    public List<TileData> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        // Convert world positions into grid coordinates
        TileData startTile = grid.GetTileAtWorldPosition(startPos);
        TileData targetTile = grid.GetTileAtWorldPosition(targetPos);

        if (startTile == null || targetTile == null)
        {
            Debug.LogWarning("Pathfinding failed: invalid start or target tile.");
            return null;
        }

        List<TileData> openSet = new List<TileData>();
        HashSet<TileData> closedSet = new HashSet<TileData>();
        openSet.Add(startTile);

        Dictionary<TileData, float> gCost = new Dictionary<TileData, float>();
        Dictionary<TileData, float> hCost = new Dictionary<TileData, float>();
        Dictionary<TileData, TileData> cameFrom = new Dictionary<TileData, TileData>();

        gCost[startTile] = 0f;
        hCost[startTile] = GetDistance(startTile, targetTile);

        while (openSet.Count > 0)
        {
            // Pick node with lowest fCost
            TileData current = openSet[0];
            float currentFCost = gCost[current] + hCost[current];
            foreach (var t in openSet)
            {
                float fCost = gCost[t] + hCost[t];
                if (fCost < currentFCost)
                {
                    current = t;
                    currentFCost = fCost;
                }
            }

            openSet.Remove(current);
            closedSet.Add(current);

            // Goal reached
            if (current == targetTile)
                return RetracePath(cameFrom, startTile, targetTile);

            // Explore neighbors
            foreach (TileData neighbor in grid.GetNeighbors(current))
            {
                if (!neighbor.Walkable || closedSet.Contains(neighbor))
                    continue;

                float tentativeG = gCost[current] + grid.MoveCost(current, neighbor);
                if (!gCost.ContainsKey(neighbor) || tentativeG < gCost[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gCost[neighbor] = tentativeG;
                    hCost[neighbor] = GetDistance(neighbor, targetTile);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; // no path found
    }

    private List<TileData> RetracePath(Dictionary<TileData, TileData> cameFrom, TileData start, TileData end)
    {
        List<TileData> path = new List<TileData>();
        TileData current = end;
        while (current != start)
        {
            path.Add(current);
            if (!cameFrom.ContainsKey(current)) break;
            current = cameFrom[current];
        }
        path.Reverse();
        return path;
    }

    private float GetDistance(TileData a, TileData b)
    {
        int dstX = Mathf.Abs(a.x - b.x);
        int dstY = Mathf.Abs(a.y - b.y);
        return dstX + dstY; // Manhattan distance
    }
}
