using System.Collections.Generic;
using UnityEngine;

public class MemorySequence : Node
{
    private List<Node> children;
    private int currentChild = 0;
    private AgentBlackBoard bb;
    private string sequenceName;
    private int lastEvalFrame = -1; // <--- NEW: Tracks interruptions

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

        // --- NEW: INTERRUPT RESET LOGIC ---
        // If more than 1 frame has passed since we last ran, we were interrupted by a higher priority!
        // We reset the sequence so the agent correctly finds a new target when they resume.
        if (lastEvalFrame != -1 && Time.frameCount - lastEvalFrame > 1 && currentChild > 0)
        {
            currentChild = 0;
        }
        lastEvalFrame = Time.frameCount;
        // ----------------------------------

        while (currentChild < children.Count)
        {
            Node child = children[currentChild];

            if (_state != NodeState.Running)
                Log(bb, "DECISION", $"[{sequenceName}] Step: <b>{child.GetType().Name}</b>");

            NodeState childState = child.Evaluate();
            switch (childState)
            {
                case NodeState.Running:
                    return _state = NodeState.Running;

                case NodeState.Failure:
                    currentChild = 0;
                    return _state = NodeState.Failure;

                case NodeState.Success:
                    currentChild++;
                    _state = NodeState.Success;
                    break;
            }
        }

        currentChild = 0;
        return _state = NodeState.Success;
    }
}