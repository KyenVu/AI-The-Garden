// File: Scripts/BehaviorTree/Action/FindBuildSiteNode.cs (New File)

using UnityEngine;
using System.Collections.Generic;

public class FindBuildSiteNode : Node
{
    private AgentBlackBoard bb;
    private const int BUILD_DISTANCE = 5; // The desired distance from the base

    public FindBuildSiteNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        GridManager gm = bb.mover.grid;
        if (bb.baseRef == null || gm == null)
        {
            bb.ui?.SetState("Build failed: Base or Grid missing.");
            return _state = NodeState.Failure;
        }

        Vector3 basePos = bb.baseRef.gameObject.transform.position;

        // 1. Determine a random direction vector for the offset
        // This ensures the station isn't always built in the same direction.
        Vector2 randomDir = new Vector2(
        Random.Range(-1, 2),
        Random.Range(-1, 2)
        ).normalized * BUILD_DISTANCE;

        // Ensure we don't pick (0,0) (pure vertical/horizontal is fine)
        if (randomDir == Vector2.zero)
            randomDir = Vector2.right * BUILD_DISTANCE;

        // 2. Calculate the distant target world position
        Vector3 targetWorldPos = basePos + new Vector3(randomDir.x, randomDir.y, 0);

        // 3. Find the closest walkable tile near that distant position.
        // We use GetTileAtWorldPosition, which snaps to the grid tile center.
        TileData finalDestinationTile = gm.GetTileAtWorldPosition(targetWorldPos);

        if (finalDestinationTile == null)
        {
            bb.ui?.SetState("Build site out of bounds.");
            return _state = NodeState.Failure;
        }

        // Final sanity check: Ensure we are building on a default/empty tile.
        // If the tile is obstructed (e.g., by another resource), we cannot build.
        // Assuming your 'defaultTile' is the empty, walkable space suitable for building.
        if (finalDestinationTile.tileType != gm.gridConfig.defaultTile)
        {
            bb.ui?.SetState("Build site obstructed.");
            return _state = NodeState.Failure;
        }


        // 4. Set the destination on the BlackBoard and Mover
        // The currentTarget is set to the base object so the BuildNode can access it easily,
        // but the pathfinding target is the distant tile.
        bb.currentTarget = bb.baseRef.gameObject.transform;
        bb.mover.SetDestinationTile(finalDestinationTile, bb.baseRef.gameObject.transform);

        bb.ui?.SetState($"Moving to distant build site ({finalDestinationTile.x},{finalDestinationTile.y})");
        return _state = NodeState.Success;
    }
}