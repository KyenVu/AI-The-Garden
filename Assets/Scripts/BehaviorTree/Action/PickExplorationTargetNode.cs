using UnityEngine;
using System.Collections.Generic;

public class PickExplorationTargetNode : Node
{
    private AgentBlackBoard bb;

    public PickExplorationTargetNode(AgentBlackBoard blackBoard) { this.bb = blackBoard; }

    public override NodeState Evaluate()
    {
        // 1. If we already have a target, keep going!
        if (bb.currentTarget != null) return NodeState.Success;

        GridManager grid = bb.mover.grid;
        List<TileData> potentialTargets = new List<TileData>();

        // 2. Find ALL tiles that are currently in fog and walkable
        // We use a broader search to make it feel more random
        for (int i = 0; i < 100; i++)
        {
            TileData randomTile = grid.GetRandomWalkableTile();
            if (randomTile != null && !randomTile.isRevealed)
            {
                potentialTargets.Add(randomTile);
            }
        }

        // 3. Pick a random one from the fog pool
        if (potentialTargets.Count > 0)
        {
            TileData chosen = potentialTargets[Random.Range(0, potentialTargets.Count)];

            // Create a persistent target object
            // Find an existing target object, or create one if it doesn't exist
            GameObject tempTarget = GameObject.Find("ExploreTarget");
            if (tempTarget == null)
            {
                tempTarget = new GameObject("ExploreTarget");
            }

            // Move it to the new fog location
            tempTarget.transform.position = new Vector3(chosen.x, chosen.y, 0);

            bb.currentTarget = tempTarget.transform;
            bb.mover.SetTarget(bb.currentTarget);

            bb.ui?.SetState("Picking random fog target...");
            return NodeState.Success;
        }

        bb.ui?.SetState("Map fully explored!");
        return NodeState.Failure;
    }
}   