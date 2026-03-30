using System.Collections.Generic;
using UnityEngine;

public class FindTreeNode : Node
{
    private AgentBlackBoard bb;

    public FindTreeNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        List<Tree> candidateTrees = new List<Tree>();

        Collider2D[] detectedTrees = Physics2D.OverlapCircleAll(bb.mover.transform.position, bb.treeSearchRadius, bb.treeLayer);
        foreach (Collider2D col in detectedTrees)
        {
            Tree t = col.GetComponent<Tree>();
            if (t != null && !candidateTrees.Contains(t)) candidateTrees.Add(t);
        }

        bb.knownTrees.RemoveAll(t => t == null);
        foreach (Tree t in bb.knownTrees)
        {
            if (t != null && !candidateTrees.Contains(t)) candidateTrees.Add(t);
        }

        if (candidateTrees.Count == 0)
        {
            bb.ui?.SetState("No Trees Known or In Range");
            return _state = NodeState.Failure;
        }

        Transform closestTree = null;
        float minDistance = Mathf.Infinity;
        GridManager grid = bb.mover.grid;

        foreach (Tree treeComponent in candidateTrees)
        {
            if (grid != null)
            {
                TileData tile = grid.GetTileAtWorldPosition(treeComponent.transform.position);
                if (tile == null || !tile.isRevealed) continue;
            }

            if (treeComponent.IsClaimed == false || treeComponent.claimedByAgent == bb.mover.gameObject)
            {
                float dist = Vector2.Distance(bb.mover.transform.position, treeComponent.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestTree = treeComponent.transform;
                }
            }
        }

        if (closestTree != null)
        {
            Tree treeComponent = closestTree.GetComponent<Tree>();
            if (treeComponent != null && treeComponent.TryClaim(bb.mover.gameObject))
            {
                TileData targetTile = grid.GetTileAtWorldPosition(closestTree.position);
                TileData destinationTile = targetTile;

                // Path to a walkable neighbor if the exact tile is blocked
                if (targetTile != null && !targetTile.Walkable)
                {
                    List<TileData> neighbors = grid.GetNeighbors(targetTile, true);
                    if (neighbors.Count > 0) destinationTile = grid.GetClosestTile(bb.mover.transform.position, neighbors);
                }

                bb.currentTarget = closestTree;
                bb.mover.SetDestinationTile(destinationTile, closestTree);

                // PATHFINDING FAIL-SAFE
                if (bb.mover.HasReachedDestination() && Vector2.Distance(bb.mover.transform.position, closestTree.position) > 2.0f)
                {
                    treeComponent.ReleaseClaim(bb.mover.gameObject);
                    bb.mover.ClearTarget();
                    bb.ui?.SetState("Target Tree is unreachable!");
                    return _state = NodeState.Failure;
                }

                bb.ui?.SetState($"Moving to {closestTree.name}");
                return _state = NodeState.Success;
            }
        }

        bb.ui?.SetState("All known trees claimed");
        return _state = NodeState.Failure;
    }
}