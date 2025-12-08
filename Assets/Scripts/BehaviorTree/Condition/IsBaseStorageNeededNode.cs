using UnityEngine;

public class IsBaseStorageNeededNode : Node
{
    private AgentBlackBoard bb;
    private FireBase fireBase;
    private ResourceType resourceType;

    // We pass the AgentBlackBoard to allow setting the UI state
    public IsBaseStorageNeededNode(AgentBlackBoard blackBoard, ResourceType type)
    {
        this.bb = blackBoard;
        this.fireBase = bb.baseRef; // Get the FireBase reference from the BlackBoard
        this.resourceType = type;
    }

    public override NodeState Evaluate()
    {
        if (fireBase == null)
        {
            bb.ui?.SetState("Base Missing");
            return _state = NodeState.Failure;
        }

        bool needed = fireBase.HasSpaceFor(resourceType);

        if (needed)
        {
            bb.ui?.SetState($"Base needs {resourceType} storage");
        }
        else
        {
            bb.ui?.SetState($"Base full of {resourceType} (Skip)");
        }

        return _state = needed ? NodeState.Success : NodeState.Failure;
    }
}