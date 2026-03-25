using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// A Composite Node that executes its children in order. 
/// Unlike a standard Sequence, it "remembers" the currently running child 
/// and resumes from that index on the next tick until the entire sequence succeeds or any child fails.
/// </summary>
public class MemorySequence : Node
{
    private List<Node> children;
    private int currentChild = 0;
    private AgentBlackBoard bb;
    private string sequenceName;
   
    public MemorySequence(AgentBlackBoard blackBoard, string name, List<Node> nodes)
    {
        this.bb = blackBoard;
        this.sequenceName = name; 
        this.children = nodes;
    }

    public override NodeState Evaluate()
    {
        if (children == null || children.Count == 0)
            return _state = NodeState.Failure;

        while (currentChild < children.Count)
        {
            Node child = children[currentChild];

            // LOGGING: Now uses sequenceName and child class name
            if (_state != NodeState.Running)
            {
                Log(bb, "DECISION", $"[{sequenceName}] Step: <b>{child.GetType().Name}</b>");
            }

            NodeState childState = child.Evaluate();
            switch (childState)
            {
                case NodeState.Running:
                    _state = NodeState.Running;
                    return _state;

                case NodeState.Failure:
                    currentChild = 0;
                    _state = NodeState.Failure;
                    return _state;

                case NodeState.Success:
                    currentChild++;
                    _state = NodeState.Success;
                    break;
            }
        }

        Log(bb, "DECISION", $"[{sequenceName}] <color=green>COMPLETED</color>");
        currentChild = 0;
        _state = NodeState.Success;
        return _state;
    }
}