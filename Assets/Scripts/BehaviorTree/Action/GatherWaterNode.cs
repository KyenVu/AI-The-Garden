// File: Scripts/BehaviorTree/Action/GatherWaterNode.cs (Complete Implementation)

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

        // 1. FAIL if target is invalid or claim is lost/stolen
        if (waterSource == null || !waterSource.IsClaimed || waterSource.claimedByAgent != bb.mover.gameObject)
        {
            bb.ui?.SetState("Lost water target/claim");
            bb.mover.ClearTarget();
            bb.currentTarget = null;
            return _state = NodeState.Failure;
        }

        // 2. Movement Check & Arrival Force (Prevents getting stuck)
        if (!bb.mover.HasReachedDestination())
        {
            bb.ui?.SetState("Moving to Water Source...");
            return _state = NodeState.Running;
        }

        // 3. Pre-Check: Inventory Full (Allows agent to succeed and proceed to DepositNode)
        if (bb.inventory.IsFull(ResourceType.Water))
        {
            bb.ui?.SetState("Water Inventory Full (Switch to Deposit)");
            waterSource.ReleaseClaim(bb.mover.gameObject); // RELEASE CLAIM
            bb.mover.ClearTarget();
            bb.currentTarget = null;
            timer = 0;
            return _state = NodeState.Success;
        }

        // 4. Action: Simulate gathering delay
        bb.ui?.SetState("Gathering Water...");
        timer += Time.deltaTime;

        if (timer < gatherDuration)
            return _state = NodeState.Running;

        // 5. Gathering Completed: Execute Harvest
        timer = 0f;

        // Amount gathered is determined by the agent's drink rate, which depletes the source.
        int amountGathered = waterSource.Drink(bb.stats.drinkRate);

        if (amountGathered <= 0) // Source depleted/empty
        {
            bb.ui?.SetState("Water Source Empty");
            bb.mover.ClearTarget();
            bb.currentTarget = null;
            return _state = NodeState.Failure;
        }

        // Add the resource unit to the agent's inventory
        int accepted = bb.inventory.AddResource(ResourceType.Water, amountGathered);

        // If not all gathered water was accepted, inventory is now full.
        if (accepted < amountGathered || waterSource.currentWaterCapacity <= 0 || bb.inventory.IsFull(ResourceType.Water))
        {
            bb.ui?.SetState($"Finished gathering Water: {accepted} accepted.");
            waterSource.ReleaseClaim(bb.mover.gameObject);
            bb.mover.ClearTarget();
            bb.currentTarget = null;
            return _state = NodeState.Success; // Success means proceed to deposit
        }

        // If source still has water AND inventory still has space, keep running this node (Gathering loop)
        return _state = NodeState.Running;
    }
}