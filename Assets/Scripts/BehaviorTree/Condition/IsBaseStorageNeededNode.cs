using UnityEngine;

public class IsBaseStorageNeededNode : Node
{
    private AgentBlackBoard bb;
    private ResourceType resourceType;

    // We pass the AgentBlackBoard to allow setting the UI state
    public IsBaseStorageNeededNode(AgentBlackBoard blackBoard, ResourceType type)
    {
        this.bb = blackBoard;
        this.resourceType = type;
        // Removed the stale 'fireBase' caching here!
    }

    public override NodeState Evaluate()
    {
        if (bb.baseRef == null)
        {
            // Try to find it again!
            bb.baseRef = GameObject.FindAnyObjectByType<FireBase>();

            // If it's STILL null, safely fail so it doesn't crash
            if (bb.baseRef == null) return NodeState.Failure;
        }

        // --- THE FIX: Use bb.baseRef directly so it's never a dead reference! ---
        bool needed = bb.baseRef.HasSpaceFor(resourceType);

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