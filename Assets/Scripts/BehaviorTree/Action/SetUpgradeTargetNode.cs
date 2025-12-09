// File: Scripts/BehaviorTree/Action/SetUpgradeTargetNode.cs (New File)

using UnityEngine;

public class SetUpgradeTargetNode : Node
{
    private AgentBlackBoard bb;

    public SetUpgradeTargetNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        CookingStation station = CookingStation.Instance;

        if (station == null)
        {
            bb.ui?.SetState("Upgrade Target Missing");
            return _state = NodeState.Failure;
        }

        // Set the station as the target for the Mover and BlackBoard
        bb.currentTarget = station.transform;
        bb.mover.SetTarget(station.transform);

        bb.ui?.SetState("Targeting Cooking Station for Upgrade");
        return _state = NodeState.Success;
    }
}