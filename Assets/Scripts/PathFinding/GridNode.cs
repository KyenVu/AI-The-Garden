using UnityEngine;

/// <summary>
/// Represents a single point (or tile) in the grid used for pathfinding.
/// Each node knows:
/// - If it's walkable or blocked
/// - Its world position in the scene
/// - Its position in the grid (X, Y index)
/// - Its cost values for A* pathfinding (gCost, hCost, fCost)
/// </summary>
public class GridNode
{
    public bool isWalkable;
    public Vector3 worldPosition;
    public int gridX, gridY;

    // Cost from the starting node to this node
    public int gCost;

    // Heuristic cost: estimated cost from this node to the target node
    public int hCost;
    public GridNode parent;

    public int fCost => gCost + hCost;

    // Constructor to initialize the node
    public GridNode(bool _isWalkable, Vector3 _worldPos, int _x, int _y)
    {
        isWalkable = _isWalkable;
        worldPosition = _worldPos;
        gridX = _x;
        gridY = _y;
    }
}
