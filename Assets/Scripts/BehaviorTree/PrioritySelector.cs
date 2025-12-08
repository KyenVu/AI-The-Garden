using System.Collections.Generic;

public class PrioritySelector : Node
{
    private List<Node> children;
    private System.Func<List<Node>, Node> getHighestPriorityNode;

    public PrioritySelector(List<Node> children, System.Func<List<Node>, Node> priorityFunction)
    {
        this.children = children;
        this.getHighestPriorityNode = priorityFunction;
    }

    public override NodeState Evaluate()
    {
        // Pick which child has highest priority using your custom function
        Node highestPriorityNode = getHighestPriorityNode(children);

        if (highestPriorityNode == null)
        {
            return _state = NodeState.Failure;
        }

        _state = highestPriorityNode.Evaluate();
        return _state;
    }
}
