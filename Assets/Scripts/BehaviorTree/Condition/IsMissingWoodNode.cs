using UnityEngine;

public class IsMissingWoodNode : Node
{
    private AgentBlackBoard bb;

    public IsMissingWoodNode(AgentBlackBoard blackboard)
    {
        this.bb = blackboard;
    }

    public override NodeState Evaluate()
    {
        // 1. SAFETY CHECK: Find the base if it was missed on Frame 1
        if (bb.baseRef == null)
        {
            bb.baseRef = GameObject.FindAnyObjectByType<FireBase>();

            // If it's STILL null, safely fail
            if (bb.baseRef == null) return _state = NodeState.Failure;
        }

        // 2. Check if the base actually needs wood
        return _state = bb.baseRef.HasSpaceFor(ResourceType.Wood)
            ? NodeState.Success
            : NodeState.Failure;
    }
}