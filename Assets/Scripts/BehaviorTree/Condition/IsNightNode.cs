// File: Scripts/BehaviorTree/Condition/IsNightNode.cs (New File)

using UnityEngine;

public class IsNightNode : Node
{
    private AgentBlackBoard bb;

    public IsNightNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        if (bb.dayNight == null) return _state = NodeState.Failure;

        if (bb.dayNight.IsNight) // IsNight uses timeOfDay >= 0.5f
        {
            bb.ui?.SetState("Night - Seeking Shelter");
            _state = NodeState.Success;
        }
        else
        {
            bb.ui?.SetState("Day - Active");
            _state = NodeState.Failure;
        }
        return _state;
    }
}