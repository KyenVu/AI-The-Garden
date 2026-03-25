using System.Collections.Generic;
/// <summary>
/// A Composite Node that selects the first child that does not fail. 
/// It "remembers" the currently running child and resumes from that index on the next tick.
/// This ensures an agent finishes a committed task before re-evaluating higher-priority branches.
/// </summary>
public class MemorySelector : Node
{
    private List<Node> children;
    private int currentChild = 0;
    private AgentBlackBoard bb;
    
    public MemorySelector(AgentBlackBoard blackBoard, List<Node> nodes)
    {
        this.bb = blackBoard;
        this.children = nodes;
    }

    public override NodeState Evaluate()
    {
        for (int i = currentChild; i < children.Count; i++)
        {
            NodeState result = children[i].Evaluate();
            currentChild = i;

            switch (result)
            {
                case NodeState.Running:
                    return _state = NodeState.Running;

                case NodeState.Success:
                    currentChild = 0;
                    return _state = NodeState.Success;

                case NodeState.Failure:
                    continue; // Try next branch
            }
        }

        currentChild = 0;
        return _state = NodeState.Failure;
    }
}