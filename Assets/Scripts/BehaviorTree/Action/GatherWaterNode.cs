using UnityEngine;

public class GatherWaterNode : Node
{
    private AgentBlackBoard bb;
    private float gatherDuration = 2f;
    private float timer = 0f;

    public GatherWaterNode(AgentBlackBoard bb)
    {
        this.bb = bb;
    }

    public override NodeState Evaluate()
    {
        WaterSource waterSource = bb.currentTarget?.GetComponent<WaterSource>();

        // --- UNIVERSAL SETUP ---
        GameObject agentObj = bb.mlBrain != null ? bb.mlBrain.gameObject : bb.mover.gameObject;
        bool isMoving = bb.mlBrain != null ? false : (bb.mover != null && !bb.mover.HasReachedDestination());

        if (waterSource == null || !waterSource.IsClaimed || waterSource.claimedByAgent != agentObj)
        {
            bb.ui?.SetState("Lost water target/claim");
            ClearAgentTarget();
            bb.currentTarget = null;
            return _state = NodeState.Failure;
        }

        if (isMoving)
        {
            bb.ui?.SetState("Moving to Water Source...");
            return _state = NodeState.Running;
        }

        if (bb.inventory.IsFull(ResourceType.Water))
        {
            bb.ui?.SetState("Water Inventory Full (Switch to Deposit)");
            waterSource.ReleaseClaim(agentObj);
            ClearAgentTarget();
            bb.currentTarget = null;
            timer = 0;
            return _state = NodeState.Success;
        }

        bb.ui?.SetState("Gathering Water...");
        timer += Time.deltaTime;

        if (timer < gatherDuration) return _state = NodeState.Running;

        timer = 0f;
        int amountGathered = waterSource.Drink(bb.stats.drinkRate);

        if (amountGathered <= 0)
        {
            bb.ui?.SetState("Water Source Empty");
            ClearAgentTarget();
            bb.currentTarget = null;
            return _state = NodeState.Failure;
        }

        int accepted = bb.inventory.AddResource(ResourceType.Water, amountGathered);

        if (accepted < amountGathered || waterSource.currentWaterCapacity <= 0 || bb.inventory.IsFull(ResourceType.Water))
        {
            bb.ui?.SetState($"Finished gathering Water: {accepted} accepted.");
            waterSource.ReleaseClaim(agentObj);
            ClearAgentTarget();
            bb.currentTarget = null;
            return _state = NodeState.Success;
        }

        return _state = NodeState.Running;
    }

    private void ClearAgentTarget()
    {
        if (bb.mover != null) bb.mover.ClearTarget();
        if (bb.mlBrain != null) bb.mlBrain.ClearTarget();
    }
}