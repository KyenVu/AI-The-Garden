public class IsThirstyNode : Node
{
    private AgentBlackBoard bb;
    private float threshold;
    public IsThirstyNode(AgentBlackBoard blackBoard, float thresholdPercent = 0.2f)
    {
        this.bb = blackBoard;
        this.threshold = thresholdPercent;
    }
    public override NodeState Evaluate()
    {
        if (bb.stats.thirst.GetPercent() <= threshold)
        {
            bb.ui?.SetState("Thirsty");
            _state = NodeState.Success;
        }
        else
        {
            bb.ui?.SetState("Idle");
            _state = NodeState.Failure;
        }
        return _state;
    }
}
