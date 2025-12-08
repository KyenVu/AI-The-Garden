using System.Collections.Generic;

public class MemorySelector : Node
{
    private List<Node> children;
    private int currentChild = 0;

    public MemorySelector(List<Node> nodes)
    {
        children = nodes;
    }

    public override NodeState Evaluate()
    {
        // Continue from the last running child
        for (int i = currentChild; i < children.Count; i++)
        {
            NodeState result = children[i].Evaluate();

            switch (result)
            {
                case NodeState.Running:
                    currentChild = i;          // Remember this child
                    _state = NodeState.Running;
                    return _state;

                case NodeState.Success:
                    currentChild = 0;          // Reset for next tick
                    _state = NodeState.Success;
                    return _state;

                case NodeState.Failure:
                    // Try next child
                    continue;
            }
        }

        // If all children fail
        currentChild = 0;
        _state = NodeState.Failure;
        return _state;
    }
}
