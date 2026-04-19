using UnityEngine;
using System.Collections.Generic;

public class FindBaseNode : Node
{
    private AgentBlackBoard bb;

    public FindBaseNode(AgentBlackBoard blackBoard) { this.bb = blackBoard; }

    public override NodeState Evaluate()
    {
        if (bb.baseRef == null) return NodeState.Failure;

        // --- UNIVERSAL SETUP ---
        Transform agentTransform = bb.mlBrain != null ? bb.mlBrain.transform : bb.mover.transform;
        GridManager grid = bb.mlBrain != null ? bb.mlBrain.gridManager : bb.mover.grid;

        Transform targetTransform = bb.baseRef.transform;
        string stateMessage = "Returning to Base...";

        if (PlankStation.Instance != null && bb.inventory.Wood > 0)
        {
            targetTransform = PlankStation.Instance.transform;
            stateMessage = "Moving to Plank Station...";
        }

        Collider2D targetCollider = targetTransform.GetComponent<Collider2D>();
        TileData destinationTile = null;

        if (targetCollider != null && grid != null)
        {
            List<TileData> tilesUnder = grid.GetTilesUnderCollider(targetCollider);
            List<TileData> validNeighbors = new List<TileData>();

            foreach (TileData tile in tilesUnder)
            {
                foreach (TileData neighbor in grid.GetNeighbors(tile, true))
                {
                    if (neighbor.Walkable && !tilesUnder.Contains(neighbor) && !validNeighbors.Contains(neighbor))
                    {
                        validNeighbors.Add(neighbor);
                    }
                }
            }
            destinationTile = grid.GetClosestTile(agentTransform.position, validNeighbors);
        }

        if (destinationTile == null)
        {
            bb.ui?.SetState("Target building is completely surrounded!");
            return NodeState.Failure;
        }

        // --- CRITICAL SEMESTER 2 HANDOFF ---
        bb.destinationObject = targetTransform;
        bb.currentTarget = targetTransform;

        // --- SEMESTER 1 PATHFINDING ---
        if (bb.mover != null)
        {
            bb.mover.SetDestinationTile(destinationTile, targetTransform);
        }

        bb.ui?.SetState(stateMessage);
        return NodeState.Success;
    }
}