using System.Collections.Generic;
using UnityEngine;

public class BuildWaterStationNode : Node
{
    private AgentBlackBoard bb;

    // Define both costs here
    private const int BUILD_COST_WATER = 10;
    private const int BUILD_COST_WOOD = 15;

    private float buildTime = 3f;
    private float timer = 0f;

    public BuildWaterStationNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        // 1. Ensure the agent is at the chosen build location
        if (!bb.mover.HasReachedDestination())
        {
            bb.ui?.SetState("Moving to build site...");
            return _state = NodeState.Running;
        }

        // 2. Simulate build time
        bb.ui?.SetState($"Building Water Station... ({timer:0.0}/{buildTime:0.0})");
        timer += Time.deltaTime;

        if (timer < buildTime)
        {
            return _state = NodeState.Running;
        }

        // 3. Check for race condition
        // (Make sure you have a WaterStation.Instance setup similar to CookingStation!)
        if (WaterStation.Instance != null)
        {
            timer = 0;
            bb.ui?.SetState("Station already built.");
            return _state = NodeState.Success;
        }

        // 4. SAFETY CHECK: Do we have enough of BOTH resources?
        if (bb.baseRef.GetAmount(ResourceType.Water) < BUILD_COST_WATER ||
            bb.baseRef.GetAmount(ResourceType.Wood) < BUILD_COST_WOOD)
        {
            bb.ui?.SetState("Build failed: Need Water & Wood!");
            timer = 0;
            return _state = NodeState.Failure;
        }

        // 5. Deduct both resources safely
        bb.baseRef.RemoveResource(ResourceType.Water, BUILD_COST_WATER);
        bb.baseRef.RemoveResource(ResourceType.Wood, BUILD_COST_WOOD);

        // Instantiate the Water Station
        Vector3 buildPos = bb.mover.transform.position;
        GameObject newStationGO = GameObject.Instantiate(
            bb.waterStationPrefab,
            buildPos,
            Quaternion.identity
        );

        // Set Sorting Layer
        SpriteRenderer sr = newStationGO.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Second Tile Layer";
        }
        Collider2D stationCollider = newStationGO.GetComponent<Collider2D>();
        if (stationCollider != null && bb.mover.grid != null)
        {
            List<TileData> stationTiles = bb.mover.grid.GetTilesUnderCollider(stationCollider);
            List<TileData> validNeighbors = new List<TileData>();

            foreach (TileData tile in stationTiles)
            {
                foreach (TileData neighbor in bb.mover.grid.GetNeighbors(tile, true))
                {
                    if (neighbor.Walkable && !stationTiles.Contains(neighbor) && !validNeighbors.Contains(neighbor))
                    {
                        validNeighbors.Add(neighbor);
                    }
                }
            }

            TileData safeTile = bb.mover.grid.GetClosestTile(bb.mover.transform.position, validNeighbors);
            if (safeTile != null)
            {
                // Snap agent position safely outside the building
                bb.mover.transform.position = safeTile.transform.position;
            }
        }
        newStationGO.name = "Water Station";
        bb.ui?.SetState("Water Station Built!");
        bb.mover.ClearTarget();
        timer = 0;
        return _state = NodeState.Success;
    }
}