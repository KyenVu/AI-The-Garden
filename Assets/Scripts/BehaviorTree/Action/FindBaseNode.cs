using UnityEngine;
using System.Collections.Generic;

public class FindBaseNode : Node
{
    private AgentBlackBoard bb;

    public FindBaseNode(AgentBlackBoard blackBoard) { this.bb = blackBoard; }

    public override NodeState Evaluate()
    {
        if (bb.baseRef == null) return NodeState.Failure;

        Transform targetTransform = bb.baseRef.transform;
        string stateMessage = "Returning to Base...";

        // ==========================================
        // NEW: Route to Plank Station if carrying Wood!
        // ==========================================
        if (PlankStation.Instance != null && bb.inventory.Wood > 0)
        {
            targetTransform = PlankStation.Instance.transform;
            stateMessage = "Moving to Plank Station...";
        }

        // ==========================================
        // Pathfind to a safe edge around the chosen building
        // ==========================================
        Collider2D targetCollider = targetTransform.GetComponent<Collider2D>();
        TileData destinationTile = null;

        if (targetCollider != null && bb.mover.grid != null)
        {
            List<TileData> tilesUnder = bb.mover.grid.GetTilesUnderCollider(targetCollider);
            List<TileData> validNeighbors = new List<TileData>();

            foreach (TileData tile in tilesUnder)
            {
                foreach (TileData neighbor in bb.mover.grid.GetNeighbors(tile, true))
                {
                    if (neighbor.Walkable && !tilesUnder.Contains(neighbor) && !validNeighbors.Contains(neighbor))
                    {
                        validNeighbors.Add(neighbor);
                    }
                }
            }
            destinationTile = bb.mover.grid.GetClosestTile(bb.mover.transform.position, validNeighbors);
        }

        if (destinationTile == null)
        {
            bb.ui?.SetState("Target building is completely surrounded!");
            return NodeState.Failure;
        }

        bb.currentTarget = targetTransform;
        bb.mover.SetDestinationTile(destinationTile, targetTransform);

        bb.ui?.SetState(stateMessage);
        return NodeState.Success;
    }
}