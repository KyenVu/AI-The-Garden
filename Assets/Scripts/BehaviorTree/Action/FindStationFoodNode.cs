using UnityEngine;
using System.Collections.Generic;

public class FindStationFoodNode : Node
{
    private AgentBlackBoard bb;

    public FindStationFoodNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        if (CookingStation.Instance != null && CookingStation.Instance.cookedFoodAvailable > 0)
        {
            if (CookingStation.Instance.TryClaim(bb.mover.gameObject))
            {
                bb.currentTarget = CookingStation.Instance.transform;

                Collider2D stationCollider = CookingStation.Instance.GetComponent<Collider2D>();
                List<TileData> stationTiles = bb.mover.grid.GetTilesUnderCollider(stationCollider);
                List<TileData> validNeighbors = new List<TileData>();

                // Get all valid neighbors around the ENTIRE perimeter of the big station
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
                    CookingStation.Instance.ReleaseClaim(bb.mover.gameObject);
                    bb.ui?.SetState("Food Station is completely surrounded!");
                    return _state = NodeState.Failure;
                }

                bb.mover.SetDestinationTile(destinationTile, CookingStation.Instance.transform);

                bb.ui?.SetState("Found Food Station! Moving...");
                return _state = NodeState.Success;
            }
        }

        return _state = NodeState.Failure;
    }
}