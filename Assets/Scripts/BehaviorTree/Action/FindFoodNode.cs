// File: Scripts/BehaviorTree/Action/FindFoodNode.cs (MODIFIED: World Food ONLY)

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
        // --- REMOVED: Cooking Station Check (Node is now specialized for world resources) ---

        Collider2D[] detectedFoods = Physics2D.OverlapCircleAll(bb.mover.transform.position, bb.foodSearchRadius, bb.foodLayer);

        if (detectedFoods.Length == 0)
        {
            bb.ui?.SetState("Hungry... No World Food In Range");
            _state = NodeState.Failure;
            return _state;
        }

        Transform closestFood = null;
        float minDistance = Mathf.Infinity;

        // Find the closest world food that is not claimed
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

        // If a valid (unclaimed/self-claimed) world food was found
        if (closestFood != null)
        {
            Food foodComponent = closestFood.GetComponent<Food>();

            if (foodComponent != null && foodComponent.TryClaim(bb.mover.gameObject))
            {
                bb.currentTarget = closestFood;
                bb.mover.SetTarget(closestFood);
                bb.ui?.SetState($"World Food Found - Moving to {closestFood.name}");
                _state = NodeState.Success;
                return _state;
            }
        }

        // If the loop finished without finding an available food source
        bb.ui?.SetState("All nearby world food claimed");
        _state = NodeState.Failure;
        return _state;
    }
}