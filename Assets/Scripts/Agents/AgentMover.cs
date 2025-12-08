using System.Collections.Generic;
using UnityEngine;

public class AgentMover : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("References")]
    public GridManager grid;
    public Pathfinding pathfinding;

    private List<TileData> pathTiles = new List<TileData>();
    private int targetIndex = 0;

    public Transform currentTarget { get; private set; }
    private TileData destinationTile; // final cell we’re heading to

    // === API === //

    /// <summary>
    /// Sets a Transform target (e.g., direct position).
    /// </summary>
    public void SetTarget(Transform target)
    {
        if (pathfinding == null)
        {
            Debug.LogError($"[AgentMover] Missing Pathfinding reference on {name}!");
            return;
        }
        if (grid == null)
        {
            Debug.LogError($"[AgentMover] Missing Grid reference on {name}!");
            return;
        }

        currentTarget = target;
        destinationTile = null;

        if (currentTarget != null)
        {
            pathTiles = pathfinding.FindPath(transform.position, currentTarget.position);
            targetIndex = 0;
        }
    }


    /// <summary>
    /// Sets a destination tile and optionally the object that owns it (like water/food).
    /// </summary>
    public void SetDestinationTile(TileData tile, Transform targetObject = null)
    {
        destinationTile = tile;
        currentTarget = targetObject;

        if (destinationTile != null)
        {
            Vector3 worldTarget = new Vector3(destinationTile.x, destinationTile.y, 0);
            pathTiles = pathfinding.FindPath(transform.position, worldTarget);
            targetIndex = 0;
        }
    }

    void Update()
    {
        if (pathTiles != null)
            FollowPathTiles();
    }

    /// <summary>
    /// Moves agent along the computed TileData path.
    /// </summary>
    public void FollowPathTiles()
    {
        if (pathTiles == null || pathTiles.Count == 0)
            return;

        Vector3 targetPos = new Vector3(pathTiles[targetIndex].x, pathTiles[targetIndex].y, 0);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // reached this tile -> move to next
        if ((transform.position - targetPos).sqrMagnitude < 0.05f)
        {
            targetIndex++;
            if (targetIndex >= pathTiles.Count)
            {
                pathTiles = null; // done
            }
        }
    }

    /// <summary>
    /// Returns true if the agent has reached its destination.
    /// </summary>
    public bool HasReachedDestination()
    {
        // No more tiles left
        if (pathTiles == null || pathTiles.Count == 0)
            return true;

        return false;
    }

    /// <summary>
    /// Clears the current path and target.
    /// </summary>
    public void ClearTarget()
    {
        currentTarget = null;
        destinationTile = null;
        pathTiles = null;
    }

    private void OnDrawGizmos()
    {
        if (pathTiles != null && grid != null)
        {
            Gizmos.color = Color.green;
            foreach (TileData tile in pathTiles)
                Gizmos.DrawCube(new Vector3(tile.x, tile.y, 0), Vector3.one * 0.5f);
        }
    }
}
