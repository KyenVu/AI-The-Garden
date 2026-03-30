using System.Collections.Generic;
using UnityEngine;

public class BuildCookingStationNode : Node
{
    private AgentBlackBoard bb;

    private const int BUILD_COST_FOOD = 10;
    private const int BUILD_COST_WOOD = 15;

    private float buildTime = 3f;
    private float timer = 0f;

    public BuildCookingStationNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        if (!bb.mover.HasReachedDestination())
        {
            bb.ui?.SetState("Moving to build site...");
            return _state = NodeState.Running;
        }

        bb.ui?.SetState($"Building Cooking Station... ({timer:0.0}/{buildTime:0.0})");
        timer += Time.deltaTime;

        if (timer < buildTime)
        {
            return _state = NodeState.Running;
        }

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

        Vector3 buildPos = bb.mover.transform.position;
        GameObject newStationGO = GameObject.Instantiate(
            bb.cookingStationPrefab,
            buildPos,
            Quaternion.identity
        );

        // ==========================================
        // MISSING FIX 1: Sync Transforms so the Safety Push works!
        // ==========================================
        Physics2D.SyncTransforms();

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
                bb.mover.transform.position = safeTile.transform.position;
            }
        }

        // ==========================================
        // MISSING FIX 2: Destroy the Build Marker! 
        // (If you don't do this, they get stuck in a loop forever)
        // ==========================================
        string targetName = $"BuildSite_{bb.mover.gameObject.GetInstanceID()}";
        GameObject marker = GameObject.Find(targetName);
        if (marker != null) GameObject.Destroy(marker);

        newStationGO.name = "Cooking Station";
        bb.ui?.SetState("Cooking Station Built!");
        bb.mover.ClearTarget();
        timer = 0;
        return _state = NodeState.Success;
    }
}