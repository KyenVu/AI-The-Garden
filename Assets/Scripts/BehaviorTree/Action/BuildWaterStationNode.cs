using System.Collections.Generic;
using UnityEngine;

public class BuildWaterStationNode : Node
{
    private AgentBlackBoard bb;

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
        // --- UNIVERSAL SETUP ---
        Transform agentTransform = bb.mlBrain != null ? bb.mlBrain.transform : bb.mover.transform;
        GridManager grid = bb.mlBrain != null ? bb.mlBrain.gridManager : bb.mover.grid;
        bool isMoving = bb.mlBrain != null ? false : (bb.mover != null && !bb.mover.HasReachedDestination());

        if (isMoving)
        {
            bb.ui?.SetState("Moving to build site...");
            return _state = NodeState.Running;
        }

        bb.ui?.SetState($"Building Water Station... ({timer:0.0}/{buildTime:0.0})");
        timer += Time.deltaTime;

        if (timer < buildTime) return _state = NodeState.Running;

        if (WaterStation.Instance != null)
        {
            timer = 0;
            bb.ui?.SetState("Station already built.");
            return _state = NodeState.Success;
        }

        if (bb.baseRef.GetAmount(ResourceType.Water) < BUILD_COST_WATER ||
            bb.baseRef.GetAmount(ResourceType.Wood) < BUILD_COST_WOOD)
        {
            bb.ui?.SetState("Build failed: Need Water & Wood!");
            timer = 0;
            return _state = NodeState.Failure;
        }

        bb.baseRef.RemoveResource(ResourceType.Water, BUILD_COST_WATER);
        bb.baseRef.RemoveResource(ResourceType.Wood, BUILD_COST_WOOD);

        Vector3 buildPos = agentTransform.position;
        GameObject newStationGO = GameObject.Instantiate(bb.waterStationPrefab, buildPos, Quaternion.identity);

        Physics2D.SyncTransforms();

        SpriteRenderer sr = newStationGO.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.sortingLayerName = "Second Tile Layer";

        Collider2D stationCollider = newStationGO.GetComponent<Collider2D>();
        if (stationCollider != null && grid != null)
        {
            List<TileData> stationTiles = grid.GetTilesUnderCollider(stationCollider);
            List<TileData> validNeighbors = new List<TileData>();

            foreach (TileData tile in stationTiles)
            {
                foreach (TileData neighbor in grid.GetNeighbors(tile, true))
                {
                    if (neighbor.Walkable && !stationTiles.Contains(neighbor) && !validNeighbors.Contains(neighbor))
                    {
                        validNeighbors.Add(neighbor);
                    }
                }
            }

            TileData safeTile = grid.GetClosestTile(agentTransform.position, validNeighbors);
            if (safeTile != null) agentTransform.position = safeTile.transform.position; // Snap agent outside
        }

        string targetName = $"BuildSite_{agentTransform.gameObject.GetInstanceID()}";
        GameObject marker = GameObject.Find(targetName);
        if (marker != null) GameObject.Destroy(marker);

        newStationGO.name = "Water Station";
        bb.ui?.SetState("Water Station Built!");

        ClearAgentTarget();
        timer = 0;
        return _state = NodeState.Success;
    }

    private void ClearAgentTarget()
    {
        if (bb.mover != null) bb.mover.ClearTarget();
        if (bb.mlBrain != null) bb.mlBrain.ClearTarget();
    }
}