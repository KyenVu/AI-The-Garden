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
        // --- UNIVERSAL SETUP ---
        Transform agentTransform = bb.mlBrain != null ? bb.mlBrain.transform : bb.mover.transform;
        GridManager gm = bb.mlBrain != null ? bb.mlBrain.gridManager : bb.mover.grid;

        if (bb.baseRef == null || gm == null)
        {
            bb.ui?.SetState("Build failed: Base or Grid missing.");
            return _state = NodeState.Failure;
        }

        Vector3 basePos = bb.baseRef.gameObject.transform.position;

        Vector2 randomDir = new Vector2(
            Random.Range(-1, 2),
            Random.Range(-1, 2)
        ).normalized * BUILD_DISTANCE;

        if (randomDir == Vector2.zero)
            randomDir = Vector2.right * BUILD_DISTANCE;

        Vector3 targetWorldPos = basePos + new Vector3(randomDir.x, randomDir.y, 0);
        TileData finalDestinationTile = gm.GetTileAtWorldPosition(targetWorldPos);

        if (finalDestinationTile == null || finalDestinationTile.tileType != gm.gridConfig.defaultTile)
        {
            bb.ui?.SetState("Build site obstructed.");
            return _state = NodeState.Failure;
        }

        // --- CRITICAL SEMESTER 2 HANDOFF ---
        // We create a temporary invisible marker for the ML Agent to target
        string targetName = $"BuildSite_{agentTransform.gameObject.GetInstanceID()}";
        GameObject existingMarker = GameObject.Find(targetName);
        if (existingMarker != null) GameObject.Destroy(existingMarker);

        GameObject tempTarget = new GameObject(targetName);
        tempTarget.transform.position = new Vector3(finalDestinationTile.x, finalDestinationTile.y, 0);

        bb.destinationObject = tempTarget.transform;
        bb.currentTarget = tempTarget.transform;

        // --- SEMESTER 1 PATHFINDING ---
        if (bb.mover != null)
        {
            bb.mover.SetDestinationTile(finalDestinationTile, tempTarget.transform);
        }

        bb.ui?.SetState($"Moving to build site ({finalDestinationTile.x},{finalDestinationTile.y})");
        return _state = NodeState.Success;
    }
}