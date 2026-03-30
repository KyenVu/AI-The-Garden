using UnityEngine;
using System.Collections.Generic;

public class FindStationWaterNode : Node
{
    private AgentBlackBoard bb;

    public FindStationWaterNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        if (WaterStation.Instance != null && WaterStation.Instance.waterAvailable > 0)
        {
            if (WaterStation.Instance.TryClaim(bb.mover.gameObject))
            {
                bb.currentTarget = WaterStation.Instance.transform;

                Collider2D stationCollider = WaterStation.Instance.GetComponent<Collider2D>();
                List<TileData> stationTiles = bb.mover.grid.GetTilesUnderCollider(stationCollider);
                List<TileData> validNeighbors = new List<TileData>();

                // Perimeter search
                foreach (TileData tile in stationTiles)
                {
                    foreach (TileData neighbor in bb.mover.grid.GetNeighbors(tile, true))
                    {
                        if (neighbor.Walkable && !validNeighbors.Contains(neighbor))
                        {
                            validNeighbors.Add(neighbor);
                        }
                    }
                }

                TileData destinationTile = bb.mover.grid.GetClosestTile(bb.mover.transform.position, validNeighbors);

                if (destinationTile == null)
                {
                    WaterStation.Instance.ReleaseClaim(bb.mover.gameObject);
                    bb.ui?.SetState("Water Station is completely surrounded!");
                    return _state = NodeState.Failure;
                }

                bb.mover.SetDestinationTile(destinationTile, WaterStation.Instance.transform);

                bb.ui?.SetState("Found Water Station! Moving...");
                return _state = NodeState.Success;
            }
        }

        return _state = NodeState.Failure;
    }
}