public class IsHungryNode : Node
{
    private AgentBlackBoard bb;
    private float threshold;

    public IsHungryNode(AgentBlackBoard blackBoard, float thresholdPercent = 0.2f)
    {
        this.bb = blackBoard;
        this.threshold = thresholdPercent;
    }

    public override NodeState Evaluate()
    {
        if (bb.stats.hunger.GetPercent() <= threshold)
        {
            bb.ui?.SetState("Hungry");
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
