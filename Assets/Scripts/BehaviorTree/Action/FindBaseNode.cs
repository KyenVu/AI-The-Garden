// File: Scripts/BehaviorTree/Action/FindBaseNode.cs (MODIFIED)

using UnityEngine;

public class FindBaseNode : Node
{
    private AgentBlackBoard bb;

    public FindBaseNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        if (bb.baseRef == null || bb.baseRef.gameObject.transform == null)
        {
            bb.ui?.SetState("Base not set");
            return _state = NodeState.Failure;
        }

        GridManager gm = bb.mover.grid;
        Transform baseTransform = bb.baseRef.gameObject.transform;

        // 1. Get the TileData where the base object is located
        TileData baseTile = gm.GetTileAtWorldPosition(baseTransform.position);

        if (baseTile == null)
        {
            bb.ui?.SetState("No tile at base");
            return _state = NodeState.Failure;
        }

        // 2. Find Walkable Neighbor Tiles
        // We use GetNeighbors on the base tile (assuming the base tile is unwalkable)
        // or on the tiles around the base. If the base tile itself is walkable 
        // (which is usually true for the base center tile where interaction occurs), 
        // GetNeighbors will return adjacent tiles.
        System.Collections.Generic.List<TileData> neighborTiles = gm.GetNeighbors(baseTile);

        TileData finalDestinationTile = null;

        if (neighborTiles.Count > 0)
        {
            // NEW LOGIC: Select a random walkable neighbor tile to stand "near" the base.
            int randomIndex = UnityEngine.Random.Range(0, neighborTiles.Count);
            finalDestinationTile = neighborTiles[randomIndex];
            bb.ui?.SetState("Going to tile near base");
        }
        else
        {
            // Fallback: If no walkable neighbors, set the base tile itself as the target 
            // (useful for depositing resources directly, or if the tile type allows it).
            finalDestinationTile = baseTile;
            bb.ui?.SetState("Going to base tile");
        }

        // 3. Set the destination for the AgentMover
        Vector3 worldTarget = new Vector3(finalDestinationTile.x, finalDestinationTile.y, 0);

        bb.mover.SetDestinationTile(finalDestinationTile, baseTransform);
        // Note: SetDestinationTile handles pathfinding from current position to the target tile.

        return _state = NodeState.Success;
    }
}