using UnityEngine;

public class IsInventoryNotFullNode : Node
{
    private AgentBlackBoard bb;
    private ResourceType resourceType;

    public IsInventoryNotFullNode(AgentBlackBoard blackBoard, ResourceType type)
    {
        this.bb = blackBoard;
        this.resourceType = type;
    }

    public override NodeState Evaluate()
    {
        if (bb.inventory.IsFull(resourceType))
        {
            bb.ui?.SetState($"{resourceType} Inventory Full (Skip)");
            _state = NodeState.Failure;
        }
        else
        {
            // Success means the agent has space and should proceed to collect
            bb.ui?.SetState($"Looking to collect {resourceType}");
            _state = NodeState.Success;
        }
        return _state;
    }
}