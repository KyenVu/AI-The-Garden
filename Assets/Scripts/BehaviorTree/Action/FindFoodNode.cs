using System.Collections.Generic;
using UnityEngine;

public class FindFoodNode : Node
{
    private AgentBlackBoard bb;

    public FindFoodNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        List<Food> candidateFoods = new List<Food>();

        // 1. Check Local Radius
        Collider2D[] detectedFoods = Physics2D.OverlapCircleAll(bb.mover.transform.position, bb.foodSearchRadius, bb.foodLayer);
        foreach (Collider2D col in detectedFoods)
        {
            Food f = col.GetComponent<Food>();
            if (f != null && !candidateFoods.Contains(f)) candidateFoods.Add(f);
        }

        // 2. Check Global Base Knowledge
        bb.knownFoods.RemoveAll(f => f == null); // Clean up eaten foods
        foreach (Food f in bb.knownFoods)
        {
            if (f != null && !candidateFoods.Contains(f)) candidateFoods.Add(f);
        }
        if (candidateFoods.Count == 0)
        {
            bb.ui?.SetState("Hungry... No Food Known/Nearby");
            return _state = NodeState.Failure;
        }

        Transform closestFood = null;
        float minDistance = Mathf.Infinity;
        GridManager grid = bb.mover.grid;

        foreach (Food foodComponent in candidateFoods)
        {
            if (grid != null)
            {
                TileData tile = grid.GetTileAtWorldPosition(foodComponent.transform.position);
                if (tile == null || !tile.isRevealed) continue;
            }

            if (foodComponent.IsClaimed == false || foodComponent.claimedByAgent == bb.mover.gameObject)
            {
                float dist = Vector2.Distance(bb.mover.transform.position, foodComponent.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestFood = foodComponent.transform;
                }
            }
        }

        if (closestFood != null)
        {
            Food foodComponent = closestFood.GetComponent<Food>();
            if (foodComponent != null && foodComponent.TryClaim(bb.mover.gameObject))
            {
                TileData targetTile = grid.GetTileAtWorldPosition(closestFood.position);
                TileData destinationTile = targetTile;

                // Path to a walkable neighbor if the exact tile is blocked/unwalkable
                if (targetTile != null && !targetTile.Walkable)
                {
                    List<TileData> neighbors = grid.GetNeighbors(targetTile, true);
                    if (neighbors.Count > 0) destinationTile = grid.GetClosestTile(bb.mover.transform.position, neighbors);
                }

                bb.currentTarget = closestFood;
                bb.mover.SetDestinationTile(destinationTile, closestFood);

                // PATHFINDING FAIL-SAFE: Check if A* failed to build a route
                if (bb.mover.HasReachedDestination() && Vector2.Distance(bb.mover.transform.position, closestFood.position) > 2.0f)
                {
                    foodComponent.ReleaseClaim(bb.mover.gameObject);
                    bb.mover.ClearTarget();
                    bb.ui?.SetState("Target Food is unreachable!");
                    return _state = NodeState.Failure;
                }

                bb.ui?.SetState($"Moving to {closestFood.name}");
                return _state = NodeState.Success;
            }
        }

        bb.ui?.SetState("All known world food claimed");
        return _state = NodeState.Failure;
    }
}