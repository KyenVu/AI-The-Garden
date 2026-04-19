using UnityEngine;
using System.Collections.Generic;

public class FindBaseNode : Node
{
    private AgentBlackBoard bb;

    public FindBaseNode(AgentBlackBoard blackBoard) { this.bb = blackBoard; }

    public override NodeState Evaluate()
    {
        // 1. Failsafe to ensure we know where the base is
        if (bb.baseRef == null)
        {
            bb.baseRef = GameObject.FindAnyObjectByType<FireBase>();
            if (bb.baseRef == null) return NodeState.Failure;
        }

        Transform targetTransform = bb.baseRef.transform;
        string stateMessage = "Returning to Base...";

        // Workers deposit wood at the Plank Station instead of the Fireplace
        if (PlankStation.Instance != null && bb.inventory.Wood > 0)
        {
            targetTransform = PlankStation.Instance.transform;
            stateMessage = "Moving to Plank Station...";
        }

        // --- CRITICAL SEMESTER 2 HANDOFF ---
        // Just give the ML Agent the target Transform. It handles its own steering!
        bb.destinationObject = targetTransform;
        bb.currentTarget = targetTransform;

        // --- SEMESTER 1 PATHFINDING (Only runs for A* Agents) ---
        if (bb.mover != null)
        {
            GridManager grid = bb.mover.grid;
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
                destinationTile = grid.GetClosestTile(bb.mover.transform.position, validNeighbors);
            }
            else if (grid != null)
            {
                // Fallback if the building has no collider
                destinationTile = grid.GetTileAtWorldPosition(targetTransform.position);
            }

            if (destinationTile == null)
            {
                bb.ui?.SetState("Target building is completely surrounded!");
                return NodeState.Failure;
            }

            bb.mover.SetDestinationTile(destinationTile, targetTransform);
        }

        bb.ui?.SetState(stateMessage);
        return NodeState.Success;
    }
}