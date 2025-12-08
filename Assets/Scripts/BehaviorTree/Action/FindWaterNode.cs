// File: Scripts/BehaviorTree/Action/FindWaterNode.cs (MODIFIED for Claiming)

using System.Collections.Generic;
using UnityEngine;

public class FindWaterNode : Node
{
    private AgentBlackBoard bb;

    public FindWaterNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        Collider2D[] waters = Physics2D.OverlapCircleAll(bb.mover.transform.position, bb.waterSearchRadius, bb.waterLayer);
        if (waters.Length == 0)
        {
            bb.ui?.SetState("Thirsty… No Water Nearby");
            return _state = NodeState.Failure;
        }

        // --- 1. Find the closest available water source ---
        Collider2D closestAvailableWater = null;
        float bestDist = Mathf.Infinity;

        foreach (var water in waters)
        {
            WaterSource waterSource = water.GetComponent<WaterSource>();

            // Only consider water that is unclaimed OR already claimed by THIS agent
            if (waterSource != null && (waterSource.IsClaimed == false || waterSource.claimedByAgent == bb.mover.gameObject))
            {
                float dist = Vector2.Distance(bb.mover.transform.position, water.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    closestAvailableWater = water;
                }
            }
        }

        if (closestAvailableWater == null)
        {
            bb.ui?.SetState("All nearby water claimed");
            return _state = NodeState.Failure;
        }

        // --- 2. Try to Claim and Set Destination ---
        WaterSource sourceToClaim = closestAvailableWater.GetComponent<WaterSource>();

        if (sourceToClaim.TryClaim(bb.mover.gameObject))
        {
            // Successfully claimed, now find the destination tile (Pathfinding logic preserved)
            List<TileData> tilesUnderWater = bb.mover.grid.GetTilesUnderCollider(closestAvailableWater);
            TileData destinationTile = null;

            if (tilesUnderWater.Count == 0)
            {
                destinationTile = bb.mover.grid.GetTileAtWorldPosition(closestAvailableWater.transform.position);
            }
            else
            {
                destinationTile = bb.mover.grid.GetClosestTile(bb.mover.transform.position, tilesUnderWater);
            }

            if (destinationTile == null)
            {
                sourceToClaim.ReleaseClaim(bb.mover.gameObject);
                bb.ui?.SetState("Water Found, but no reachable tile");
                return _state = NodeState.Failure;
            }

            // Set movement destination
            bb.currentTarget = closestAvailableWater.transform;
            bb.mover.SetDestinationTile(destinationTile, closestAvailableWater.transform);

            bb.ui?.SetState("Water Found — Moving");
            return _state = NodeState.Success;
        }
        else
        {
            // Claim failed (another agent beat us to it)
            bb.ui?.SetState("Water claimed just now");
            return _state = NodeState.Failure;
        }
    }
}