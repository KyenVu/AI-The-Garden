//using System.Diagnostics;
using UnityEngine;

public class MoveToDestinationNode : Node
{
    private AgentBlackBoard bb;

    public MoveToDestinationNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        if (!bb.mover.HasReachedDestination())
        {
            bb.mover.FollowPathTiles();
            bb.ui?.SetState("Moving...");
            return _state = NodeState.Running;
        }
        Debug.Log("Moving");
        bb.ui?.SetState("Arrived");
        return _state = NodeState.Success;
    }
}
