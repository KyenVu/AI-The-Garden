using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    [Tooltip("Reference to the GridManager which holds tile data and walkability info.")]
    public GridManager grid;

    /// <summary>
    /// Calculates a path from startPos to targetPos using f = g + h logic.
    /// </summary>
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

        // OpenSet: Nodes discovered but not yet evaluated
        List<TileData> openSet = new List<TileData>();
        // ClosedSet: Nodes already evaluated (HashSet for O(1) lookup performance)
        HashSet<TileData> closedSet = new HashSet<TileData>();
        openSet.Add(startTile);

       
        Dictionary<TileData, float> gCost = new Dictionary<TileData, float>();
        Dictionary<TileData, float> hCost = new Dictionary<TileData, float>();
        Dictionary<TileData, TileData> cameFrom = new Dictionary<TileData, TileData>();

        gCost[startTile] = 0f;
        hCost[startTile] = GetDistance(startTile, targetTile);

        while (openSet.Count > 0)
        {
            // Pick node with lowest fCost (f = g + h)
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

            if (current == targetTile)
                return RetracePath(cameFrom, startTile, targetTile);

            foreach (TileData neighbor in grid.GetNeighbors(current))
            {

                if (!neighbor.Walkable || closedSet.Contains(neighbor))
                    continue;

                float tentativeG = gCost[current] + grid.MoveCost(current, neighbor);

                // If this new path is better than any previous path found for this neighbor
                if (!gCost.ContainsKey(neighbor) || tentativeG < gCost[neighbor])
                {
                    cameFrom[neighbor] = current; // Record the path step
                    gCost[neighbor] = tentativeG;
                    hCost[neighbor] = GetDistance(neighbor, targetTile);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; 
    }

    /// <summary>
    /// Reconstructs the final path by following the 'cameFrom' links back to the start.
    /// </summary>
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
        path.Reverse(); // Reverse to get Start -> End order
        return path;
    }

    /// <summary>
    /// Calculates the Manhattan Distance heuristic between two tiles.
    /// Manhattan is preferred for grid movement restricted to 4 directions.
    /// </summary>
    private float GetDistance(TileData a, TileData b)
    {
        int dstX = Mathf.Abs(a.x - b.x);
        int dstY = Mathf.Abs(a.y - b.y);
        return dstX + dstY;
    }
}