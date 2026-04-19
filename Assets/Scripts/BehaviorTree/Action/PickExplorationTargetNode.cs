using UnityEngine;
using System.Collections.Generic;

public class PickExplorationTargetNode : Node
{
    private AgentBlackBoard bb;

    public PickExplorationTargetNode(AgentBlackBoard blackBoard) { this.bb = blackBoard; }

    public override NodeState Evaluate()
    {
        // --- NEW: THE INTERRUPT CHECK ---
        // If we have already hit the reveal limit, this node must FAIL.
        // This forces the 'Explore Sequence' to fail so the 'Return Knowledge Sequence' can run.
        if (bb.fogTilesRevealed >= 3)
        {
            return NodeState.Failure;
        }

        // 1. If we already have a target, keep going!
        if (bb.currentTarget != null) return NodeState.Success;

        // --- UNIVERSAL SETUP ---
        GridManager grid = bb.mlBrain != null ? bb.mlBrain.gridManager : bb.mover.grid;
        GameObject agentObj = bb.mlBrain != null ? bb.mlBrain.gameObject : bb.mover.gameObject;

        if (grid == null) return NodeState.Failure;

        List<TileData> potentialTargets = new List<TileData>();

        // 2. Find ALL tiles that are currently in fog and walkable
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
            TileData chosen = potentialTargets[UnityEngine.Random.Range(0, potentialTargets.Count)];

            string targetName = $"ExploreTarget_{agentObj.GetInstanceID()}";
            GameObject tempTarget = GameObject.Find(targetName);
            if (tempTarget == null)
            {
                tempTarget = new GameObject(targetName);
            }

            tempTarget.transform.position = new Vector3(chosen.x, chosen.y, 0);

            bb.currentTarget = tempTarget.transform;
            bb.destinationObject = tempTarget.transform;

            if (bb.mover != null)
            {
                bb.mover.SetTarget(bb.currentTarget);
            }

            bb.ui?.SetState("Picking random fog target...");
            return NodeState.Success;
        }

        bb.ui?.SetState("Map fully explored!");
        return NodeState.Failure;
    }
}