using System.Collections.Generic;

public class Selector : Node
{
    protected List<Node> children = new List<Node>();

    public Selector(List<Node> nodes)
    {
        children = nodes;
    }

    public override NodeState Evaluate()
    {
        foreach (Node node in children)
        {
            switch (node.Evaluate())
            {
                case NodeState.Success:
                    _state = NodeState.Success;
                    return _state;
                case NodeState.Running:
                    _state = NodeState.Running;
                    return _state;
                default:
                    continue;
            }
        }
        _state = NodeState.Failure;
        return _state;
    }
}
