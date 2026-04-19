using System.Collections.Generic;
using UnityEngine;

public class BuildCookingStationNode : Node
{
    private AgentBlackBoard bb;
    private const int BUILD_COST_FOOD = 10;
    private const int BUILD_COST_WOOD = 15;
    private float buildTime = 3f;
    private float timer = 0f;

    public BuildCookingStationNode(AgentBlackBoard blackBoard) { this.bb = blackBoard; }

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

        bb.ui?.SetState($"Building Cooking Station... ({timer:0.0}/{buildTime:0.0})");
        timer += Time.deltaTime;

        if (timer < buildTime) return _state = NodeState.Running;

        if (CookingStation.Instance != null)
        {
            timer = 0;
            bb.ui?.SetState("Station already built.");
            return _state = NodeState.Success;
        }

        if (bb.baseRef.GetAmount(ResourceType.Food) < BUILD_COST_FOOD ||
            bb.baseRef.GetAmount(ResourceType.Wood) < BUILD_COST_WOOD)
        {
            bb.ui?.SetState("Build failed: Need Food & Wood!");
            timer = 0;
            return _state = NodeState.Failure;
        }

        bb.baseRef.RemoveResource(ResourceType.Food, BUILD_COST_FOOD);
        bb.baseRef.RemoveResource(ResourceType.Wood, BUILD_COST_WOOD);

        // Uses universal agent transform
        Vector3 buildPos = agentTransform.position;
        GameObject newStationGO = GameObject.Instantiate(bb.cookingStationPrefab, buildPos, Quaternion.identity);

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

        newStationGO.name = "Cooking Station";
        bb.ui?.SetState("Cooking Station Built!");

        if (bb.mover != null) bb.mover.ClearTarget();
        if (bb.mlBrain != null) bb.mlBrain.ClearTarget();
        timer = 0;

        return _state = NodeState.Success;
    }
}