// File: Scripts/BehaviorTree/Action/FindFoodNode.cs (MODIFIED for Claiming)

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
        Collider2D[] detectedFoods = Physics2D.OverlapCircleAll(bb.mover.transform.position, bb.foodSearchRadius, bb.foodLayer);

        if (detectedFoods.Length == 0)
        {
            bb.ui?.SetState("Hungry... No Food In Range");
            _state = NodeState.Failure;
            return _state;
        }

        Transform closestFood = null;
        float minDistance = Mathf.Infinity;

        // Find the closest food that is not claimed by another agent
        foreach (Collider2D foodCollider in detectedFoods)
        {
            Food foodComponent = foodCollider.GetComponent<Food>();

            // Only consider food that is unclaimed OR already claimed by THIS agent
            if (foodComponent != null && (foodComponent.IsClaimed == false || foodComponent.claimedByAgent == bb.mover.gameObject))
            {
                float dist = Vector2.Distance(bb.mover.transform.position, foodCollider.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestFood = foodCollider.transform;
                }
            }
        }

        // If a valid (unclaimed/self-claimed) food was found
        if (closestFood != null)
        {
            Food foodComponent = closestFood.GetComponent<Food>();

            // Try to claim the resource before setting destination
            if (foodComponent != null && foodComponent.TryClaim(bb.mover.gameObject))
            {
                bb.currentTarget = closestFood;
                bb.mover.SetTarget(closestFood);
                bb.ui?.SetState($"Food Found - Moving to {closestFood.name}");
                _state = NodeState.Success;
                return _state;
            }
        }

        // If the loop finished without finding an available food source
        bb.ui?.SetState("All nearby food claimed");
        _state = NodeState.Failure;
        return _state;
    }
}