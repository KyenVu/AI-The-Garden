// DepositNode.cs
using UnityEngine;

public class DepositNode : Node
{
    private AgentBlackBoard bb;

    public DepositNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        if (!bb.mover.HasReachedDestination())
        {
            bb.ui?.SetState("Moving to base...");
            return _state = NodeState.Running;
        }

        // find nearby FireBase at currentTarget
        if (bb.mover.currentTarget == null)
        {
            bb.ui?.SetState("No base to deposit to");
            return _state = NodeState.Failure;
        }

        var fb = bb.mover.currentTarget.GetComponent<FireBase>();
        if (fb == null)
        {
            bb.ui?.SetState("Target is not a base");
            return _state = NodeState.Failure;
        }

        // deposit everything (or only specific types)
        foreach (ResourceType t in System.Enum.GetValues(typeof(ResourceType)))
        {
            int have = bb.inventory.GetAmount(t);
            if (have <= 0) continue;

            int moved = bb.inventory.TransferTo(fb, t, have);
            if (moved > 0)
            {
                bb.ui?.SetState($"Deposited {moved} {t}");
            }
        }

        return _state = NodeState.Success;
    }
}
