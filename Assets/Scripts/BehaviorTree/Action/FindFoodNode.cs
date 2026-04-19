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
        // --- UNIVERSAL SETUP ---
        Transform agentTransform = bb.mlBrain != null ? bb.mlBrain.transform : bb.mover.transform;
        GridManager grid = bb.mlBrain != null ? bb.mlBrain.gridManager : bb.mover.grid;
        GameObject agentObj = agentTransform.gameObject;

        List<Food> candidateFoods = new List<Food>();

        Collider2D[] detectedFoods = Physics2D.OverlapCircleAll(agentTransform.position, bb.foodSearchRadius, bb.foodLayer);
        foreach (Collider2D col in detectedFoods)
        {
            Food f = col.GetComponent<Food>();
            if (f != null && !candidateFoods.Contains(f)) candidateFoods.Add(f);
        }

        bb.knownFoods.RemoveAll(f => f == null);
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

        foreach (Food foodComponent in candidateFoods)
        {
            if (grid != null)
            {
                TileData tile = grid.GetTileAtWorldPosition(foodComponent.transform.position);
                // --- FOG REMOVED HERE ---
                if (tile == null) continue;
            }

            if (foodComponent.IsClaimed == false || foodComponent.claimedByAgent == agentObj)
            {
                float dist = Vector2.Distance(agentTransform.position, foodComponent.transform.position);
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
            if (foodComponent != null && foodComponent.TryClaim(agentObj))
            {
                bb.destinationObject = closestFood;
                bb.currentTarget = closestFood;

                if (bb.mover != null)
                {
                    TileData targetTile = grid.GetTileAtWorldPosition(closestFood.position);
                    TileData destinationTile = targetTile;

                    if (targetTile != null && !targetTile.Walkable)
                    {
                        List<TileData> neighbors = grid.GetNeighbors(targetTile, true);
                        if (neighbors.Count > 0) destinationTile = grid.GetClosestTile(agentTransform.position, neighbors);
                    }

                    bb.mover.SetDestinationTile(destinationTile, closestFood);

                    if (bb.mover.HasReachedDestination() && Vector2.Distance(agentTransform.position, closestFood.position) > 2.0f)
                    {
                        foodComponent.ReleaseClaim(agentObj);
                        bb.mover.ClearTarget();
                        bb.ui?.SetState("Target Food is unreachable!");
                        return _state = NodeState.Failure;
                    }
                }

                bb.ui?.SetState($"Moving to {closestFood.name}");
                return _state = NodeState.Success;
            }
        }

        bb.ui?.SetState("All known world food claimed");
        return _state = NodeState.Failure;
    }
}