using UnityEngine;

public class IsPlankStationNeededNode : Node
{
    private AgentBlackBoard bb;

    private const int BUILD_COST_WOOD = 40;


    public IsPlankStationNeededNode(AgentBlackBoard blackBoard) { this.bb = blackBoard; }

    public override NodeState Evaluate()
    {
        if (PlankStation.Instance != null) return _state = NodeState.Failure; // Already built!

        if (bb.baseRef.GetAmount(ResourceType.Wood) < BUILD_COST_WOOD)
        {
            return _state = NodeState.Failure; // Can't afford
        }

        return _state = NodeState.Success; // Let's build it!
    }
}