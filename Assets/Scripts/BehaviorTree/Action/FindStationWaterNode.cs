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
            // --- UNIVERSAL SETUP ---
            GameObject agentObj = bb.mlBrain != null ? bb.mlBrain.gameObject : bb.mover.gameObject;
            Transform agentTransform = bb.mlBrain != null ? bb.mlBrain.transform : bb.mover.transform;
            GridManager grid = bb.mlBrain != null ? bb.mlBrain.gridManager : bb.mover.grid;

            if (WaterStation.Instance.TryClaim(agentObj))
            {
                bb.currentTarget = WaterStation.Instance.transform;
                bb.destinationObject = WaterStation.Instance.transform; // CRITICAL FOR ML-BRAIN

                // --- SEMESTER 1 PATHFINDING ---
                if (bb.mover != null && grid != null)
                {
                    Collider2D stationCollider = WaterStation.Instance.GetComponent<Collider2D>();
                    List<TileData> stationTiles = grid.GetTilesUnderCollider(stationCollider);
                    List<TileData> validNeighbors = new List<TileData>();

                    // Perimeter search
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
                        WaterStation.Instance.ReleaseClaim(agentObj);
                        bb.ui?.SetState("Water Station is completely surrounded!");
                        return _state = NodeState.Failure;
                    }

                    bb.mover.SetDestinationTile(destinationTile, WaterStation.Instance.transform);
                }

                bb.ui?.SetState("Found Water Station! Moving...");
                return _state = NodeState.Success;
            }
        }

        return _state = NodeState.Failure;
    }
}