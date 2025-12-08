using System.Collections.Generic;
using UnityEngine;

public class Sequence : Node
{
    protected List<Node> children = new List<Node>();

    public Sequence(List<Node> nodes)
    {
        children = nodes;
    }

    public override NodeState Evaluate()
    {
        bool anyChildRunning = false;

        foreach (Node node in children)
        {

            NodeState result = node.Evaluate();

            switch (result)
            {
                case NodeState.Failure:
                    _state = NodeState.Failure;
                    return _state;

                case NodeState.Running:
                    anyChildRunning = true;
                    return _state = NodeState.Running;

                case NodeState.Success:
                    continue;
            }
        }

        // All children succeeded
        _state = anyChildRunning ? NodeState.Running : NodeState.Success;
        return _state;
    }
}

