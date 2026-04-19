using UnityEngine;

public class GatherFoodNode : Node
{
    private AgentBlackBoard bb;
    private float gatherTime = 2f;
    private float timer = 0f;

    public GatherFoodNode(AgentBlackBoard bb) { this.bb = bb; }

    public override NodeState Evaluate()
    {
        Food food = bb.currentTarget?.GetComponent<Food>();

        // --- UNIVERSAL ARRIVAL CHECK ---
        bool isMoving = bb.mlBrain != null ? false : (bb.mover != null && !bb.mover.HasReachedDestination());

        if (food == null || isMoving) return _state = NodeState.Running;

        if (bb.inventory.IsFull(ResourceType.Food))
        {
            bb.ui?.SetState("Food Inventory Full (Switch to Deposit)");
            ClearAgentTarget();
            bb.currentTarget = null;
            timer = 0;
            return _state = NodeState.Success;
        }

        bb.ui?.SetState("Gathering Food...");
        timer += Time.deltaTime;

        if (timer < gatherTime) return _state = NodeState.Running;

        timer = 0f;
        int amountToGather = 5;
        int accepted = bb.inventory.AddResource(ResourceType.Food, amountToGather);

        if (accepted > 0)
        {
            food.OnEaten();
            bb.ui?.SetState($"Gathered {accepted} Food!");
            ClearAgentTarget();
            bb.currentTarget = null;
            return _state = NodeState.Success;
        }

        bb.ui?.SetState("Gather failed (Inventory full)");
        ClearAgentTarget();
        bb.currentTarget = null;
        return _state = NodeState.Failure;
    }

    private void ClearAgentTarget()
    {
        if (bb.mover != null) bb.mover.ClearTarget();
        if (bb.mlBrain != null) bb.mlBrain.ClearTarget();
    }
}