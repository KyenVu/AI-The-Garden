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
            // --- UNIVERSAL SETUP ---
            GameObject agentObj = bb.mlBrain != null ? bb.mlBrain.gameObject : bb.mover.gameObject;
            Transform agentTransform = bb.mlBrain != null ? bb.mlBrain.transform : bb.mover.transform;
            GridManager grid = bb.mlBrain != null ? bb.mlBrain.gridManager : bb.mover.grid;

            if (CookingStation.Instance.TryClaim(agentObj))
            {
                bb.currentTarget = CookingStation.Instance.transform;
                bb.destinationObject = CookingStation.Instance.transform; // CRITICAL FOR ML-BRAIN

                // --- SEMESTER 1 PATHFINDING ---
                if (bb.mover != null && grid != null)
                {
                    Collider2D stationCollider = CookingStation.Instance.GetComponent<Collider2D>();
                    List<TileData> stationTiles = grid.GetTilesUnderCollider(stationCollider);
                    List<TileData> validNeighbors = new List<TileData>();

                    foreach (TileData tile in stationTiles)
                    {
                        foreach (TileData neighbor in grid.GetNeighbors(tile, true))
                        {
                            if (neighbor.Walkable && !validNeighbors.Contains(neighbor))
                            {
                                validNeighbors.Add(neighbor);
                            }
                        }
                    }

                    TileData destinationTile = grid.GetClosestTile(agentTransform.position, validNeighbors);

                    if (destinationTile == null)
                    {
                        CookingStation.Instance.ReleaseClaim(agentObj);
                        bb.ui?.SetState("Food Station is completely surrounded!");
                        return _state = NodeState.Failure;
                    }

                    bb.mover.SetDestinationTile(destinationTile, CookingStation.Instance.transform);
                }

                bb.ui?.SetState("Found Food Station! Moving...");
                return _state = NodeState.Success;
            }
        }

        return _state = NodeState.Failure;
    }
}