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
        // --- UNIVERSAL SETUP ---
        Transform agentTransform = bb.mlBrain != null ? bb.mlBrain.transform : bb.mover.transform;
        GridManager grid = bb.mlBrain != null ? bb.mlBrain.gridManager : bb.mover.grid;
        GameObject agentObj = agentTransform.gameObject;

        List<WaterSource> candidateWaters = new List<WaterSource>();

        Collider2D[] waters = Physics2D.OverlapCircleAll(agentTransform.position, bb.waterSearchRadius, bb.waterLayer);
        foreach (Collider2D col in waters)
        {
            WaterSource w = col.GetComponent<WaterSource>();
            if (w != null && !candidateWaters.Contains(w)) candidateWaters.Add(w);
        }

        bb.knownWaters.RemoveAll(w => w == null);
        foreach (WaterSource w in bb.knownWaters)
        {
            if (w != null && !candidateWaters.Contains(w)) candidateWaters.Add(w);
        }

        if (candidateWaters.Count == 0)
        {
            bb.ui?.SetState("Thirsty… No Water Known or In Range");
            return _state = NodeState.Failure;
        }

        WaterSource closestAvailableWater = null;
        float bestDist = Mathf.Infinity;

        foreach (WaterSource waterSource in candidateWaters)
        {
            if (grid != null)
            {
                TileData tile = grid.GetTileAtWorldPosition(waterSource.transform.position);
                // --- FOG REMOVED HERE ---
                if (tile == null || !tile.isRevealed) continue;
            }

            if (waterSource.IsClaimed == false || waterSource.claimedByAgent == agentObj)
            {
                float dist = Vector2.Distance(agentTransform.position, waterSource.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    closestAvailableWater = waterSource;
                }
            }
        }

        if (closestAvailableWater == null)
        {
            bb.ui?.SetState("All known water claimed");
            return _state = NodeState.Failure;
        }

        if (closestAvailableWater.TryClaim(agentObj))
        {
            bb.destinationObject = closestAvailableWater.transform;
            bb.currentTarget = closestAvailableWater.transform;

            if (bb.mover != null)
            {
                List<TileData> tilesUnderWater = grid.GetTilesUnderCollider(closestAvailableWater.GetComponent<Collider2D>());
                TileData destinationTile = null;

                if (tilesUnderWater.Count == 0)
                    destinationTile = grid.GetTileAtWorldPosition(closestAvailableWater.transform.position);
                else
                    destinationTile = grid.GetClosestTile(agentTransform.position, tilesUnderWater);

                if (destinationTile != null && !destinationTile.Walkable)
                {
                    List<TileData> neighbors = grid.GetNeighbors(destinationTile, true);
                    if (neighbors.Count > 0) destinationTile = grid.GetClosestTile(agentTransform.position, neighbors);
                }

                if (destinationTile == null)
                {
                    closestAvailableWater.ReleaseClaim(agentObj);
                    bb.ui?.SetState("Water Found, but no reachable tile");
                    return _state = NodeState.Failure;
                }

                bb.mover.SetDestinationTile(destinationTile, closestAvailableWater.transform);

                if (bb.mover.HasReachedDestination() && Vector2.Distance(agentTransform.position, closestAvailableWater.transform.position) > 2.0f)
                {
                    closestAvailableWater.ReleaseClaim(agentObj);
                    bb.mover.ClearTarget();
                    bb.ui?.SetState("Target Water is unreachable!");
                    return _state = NodeState.Failure;
                }
            }

            bb.ui?.SetState("Water Found — Moving");
            return _state = NodeState.Success;
        }
        else
        {
            bb.ui?.SetState("Water claimed just now");
            return _state = NodeState.Failure;
        }
    }
}