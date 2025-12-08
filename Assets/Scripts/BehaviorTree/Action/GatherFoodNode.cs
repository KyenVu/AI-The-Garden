// File: Scripts/BehaviorTree/Action/GatherFoodNode.cs (New File)

using UnityEngine;

public class GatherFoodNode : Node
{
    private AgentBlackBoard bb;
    private float gatherTime = 2f; // Time delay for gathering
    private float timer = 0f;

    public GatherFoodNode(AgentBlackBoard bb)
    {
        this.bb = bb;
    }

    public override NodeState Evaluate()
    {
        Food food = bb.currentTarget?.GetComponent<Food>();

        // 1. Check if we are still moving, or if the target is invalid
        if (food == null || !bb.mover.HasReachedDestination())
        {
            bb.ui?.SetState("Moving to BLEEEE...");
            return _state = NodeState.Running;
        }

        // 2. Pre-Check: Stop if inventory is full (Should be caught by IsInventoryNotFullNode, but serves as a mid-task fail-safe)
        if (bb.inventory.IsFull(ResourceType.Food))
        {
            bb.ui?.SetState("Food Inventory Full (Switch to Deposit)");
            bb.mover.ClearTarget();
            bb.currentTarget = null;
            timer = 0;
            return _state = NodeState.Success; // Success means move to the next step: deposit
        }

        // 3. Action: Simulate gathering delay
        bb.ui?.SetState("Gathering Food...");
        timer += Time.deltaTime;

        if (timer < gatherTime)
            return _state = NodeState.Running; // Still gathering

        // 4. Gathering Completed: Execute Harvest
        timer = 0f;

        // Use the Food's nutrition value as the amount to gather (e.g., 30 units)
        int amountToGather = bb.stats.foodHarvestRate;

        // Add resource to the Agent's Inventory
        int accepted = bb.inventory.AddResource(ResourceType.Food, amountToGather);

        if (accepted > 0)
        {
            // Remove the resource from the world (re-using the existing OnEaten logic)
            food.OnEaten(); // Destroys the Food GameObject and replaces the tile

            bb.ui?.SetState($"Gathered {accepted} Food!");

            // Success: clear target and let the Sequence proceed to FindBaseNode
            bb.mover.ClearTarget();
            bb.currentTarget = null;
            return _state = NodeState.Success;
        }

        // If accepted is 0, the node fails to proceed to deposit
        bb.ui?.SetState("Gather failed (Inventory full)");
        bb.mover.ClearTarget();
        bb.currentTarget = null;
        return _state = NodeState.Failure;
    }
}