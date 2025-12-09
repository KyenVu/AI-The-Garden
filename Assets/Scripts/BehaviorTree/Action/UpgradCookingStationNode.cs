// File: Scripts/BehaviorTree/Action/UpgradeCookingStationNode.cs (MODIFIED)

using UnityEngine;

public class UpgradeCookingStationNode : Node
{
    private AgentBlackBoard bb;
    private float upgradeTime = 2f;
    private float timer = 0f;
    private const int UPGRADE_COST = 5;

    public UpgradeCookingStationNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        CookingStation station = bb.currentTarget?.GetComponent<CookingStation>();

        // Fail if station is missing or agent hasn't arrived
        if (station == null || station.level >= station.maxLevel)
        {
            // Note: We don't check !HasReachedDestination() because MoveToDestinationNode
            // handles running until arrival. If it returns Failure, we assume arrival failed.
            return _state = NodeState.Failure;
        }

        // CRUCIAL: The agent must be at the destination for the sequence to proceed to this node.
        if (!bb.mover.HasReachedDestination())
        {
            // This should ideally never happen if the previous node (MoveToDestinationNode) succeeded.
            // But if it does, we fail this part of the sequence.
            return _state = NodeState.Failure;
        }

        // 1. Simulate upgrade time
        bb.ui?.SetState($"Upgrading Station... ({timer:0.0}/{upgradeTime:0.0})");
        timer += Time.deltaTime;

        if (timer < upgradeTime)
            return _state = NodeState.Running;

        // 2. Attempt final upgrade and cost deduction
        timer = 0;

        int removedFood = bb.baseRef.RemoveResource(ResourceType.Food, UPGRADE_COST);

        if (removedFood == UPGRADE_COST)
        {
            if (station.TryUpgrade())
            {
                bb.ui?.SetState($"Station Upgraded to Lv.{station.level}!");
                bb.mover.ClearTarget();
                return _state = NodeState.Success;
            }
            return _state = NodeState.Failure;
        }
        else
        {
            bb.ui?.SetState("Upgrade failed: Food cost missing.");
            return _state = NodeState.Failure;
        }
    }
}