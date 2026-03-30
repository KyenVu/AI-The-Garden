using UnityEngine;

public class IsReadyToReportNode : Node
{
    private AgentBlackBoard bb;

    public IsReadyToReportNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        // CRITICAL: Only return Success if we hit the limit.
        // If we return success at 1 or 2, the agent turns around immediately.
        if (bb.fogTilesRevealed >= bb.maxExplorationsBeforeReturn)
        {
            return NodeState.Success;
        }

        return NodeState.Failure;
    }
}