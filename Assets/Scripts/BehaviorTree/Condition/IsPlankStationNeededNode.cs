using UnityEngine;

public class IsPlankStationNeededNode : Node
{
    private AgentBlackBoard bb;

    private const int BUILD_COST_WOOD = 40;


    public IsPlankStationNeededNode(AgentBlackBoard blackBoard) { this.bb = blackBoard; }

    public override NodeState Evaluate()
    {
        if (bb.baseRef == null)
        {
            // Try to find it again!
            bb.baseRef = GameObject.FindAnyObjectByType<FireBase>();

            // If it's STILL null, safely fail so it doesn't crash
            if (bb.baseRef == null) return NodeState.Failure;
        }
        // 2. Are we already built?
        if (PlankStation.Instance != null)
        {
            return _state = NodeState.Failure;
        }

        // 3. Can we afford it?
        if (bb.baseRef.GetAmount(ResourceType.Wood) < BUILD_COST_WOOD)
        {
            return _state = NodeState.Failure;
        }

        // Let's build it!
        return _state = NodeState.Success;
    }
}