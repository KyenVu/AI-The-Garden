using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Sequence node that remembers which child was running last.
/// It continues from that child until the sequence completes.
/// </summary>
public class MemorySequence : Node
{
    private List<Node> children;
    private int currentChild = 0; // keeps track of running child

    public MemorySequence(List<Node> nodes)
    {
        children = nodes;
    }

    public override NodeState Evaluate()
    {
        // If no children, fail
        if (children == null || children.Count == 0)
            return _state = NodeState.Failure;

        while (currentChild < children.Count)
        {
            Node child = children[currentChild];
            NodeState childState = child.Evaluate();

            switch (childState)
            {
                case NodeState.Running:
                    _state = NodeState.Running;
                    return _state;

                case NodeState.Failure:
                    // reset for next run
                    currentChild = 0;
                    _state = NodeState.Failure;
                    return _state;

                case NodeState.Success:
                    // move to next child
                    currentChild++;
                    break;
            }
        }

        // All children succeeded
        currentChild = 0; // reset for next evaluation
        _state = NodeState.Success;
        return _state;
    }
}
