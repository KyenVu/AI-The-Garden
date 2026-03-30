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
        List<WaterSource> candidateWaters = new List<WaterSource>();

        Collider2D[] waters = Physics2D.OverlapCircleAll(bb.mover.transform.position, bb.waterSearchRadius, bb.waterLayer);
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
        GridManager grid = bb.mover.grid;

        foreach (WaterSource waterSource in candidateWaters)
        {
            if (grid != null)
            {
                TileData tile = grid.GetTileAtWorldPosition(waterSource.transform.position);
                if (tile == null || !tile.isRevealed) continue;
            }

            if (waterSource.IsClaimed == false || waterSource.claimedByAgent == bb.mover.gameObject)
            {
                float dist = Vector2.Distance(bb.mover.transform.position, waterSource.transform.position);
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

        if (closestAvailableWater.TryClaim(bb.mover.gameObject))
        {
            List<TileData> tilesUnderWater = bb.mover.grid.GetTilesUnderCollider(closestAvailableWater.GetComponent<Collider2D>());
            TileData destinationTile = null;

            if (tilesUnderWater.Count == 0)
                destinationTile = bb.mover.grid.GetTileAtWorldPosition(closestAvailableWater.transform.position);
            else
                destinationTile = bb.mover.grid.GetClosestTile(bb.mover.transform.position, tilesUnderWater);

            // Path to a walkable neighbor if the exact tile is blocked
            if (destinationTile != null && !destinationTile.Walkable)
            {
                List<TileData> neighbors = grid.GetNeighbors(destinationTile, true);
                if (neighbors.Count > 0) destinationTile = grid.GetClosestTile(bb.mover.transform.position, neighbors);
            }

            if (destinationTile == null)
            {
                closestAvailableWater.ReleaseClaim(bb.mover.gameObject);
                bb.ui?.SetState("Water Found, but no reachable tile");
                return _state = NodeState.Failure;
            }

            bb.currentTarget = closestAvailableWater.transform;
            bb.mover.SetDestinationTile(destinationTile, closestAvailableWater.transform);

            // PATHFINDING FAIL-SAFE
            if (bb.mover.HasReachedDestination() && Vector2.Distance(bb.mover.transform.position, closestAvailableWater.transform.position) > 2.0f)
            {
                closestAvailableWater.ReleaseClaim(bb.mover.gameObject);
                bb.mover.ClearTarget();
                bb.ui?.SetState("Target Water is unreachable!");
                return _state = NodeState.Failure;
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