public enum NodeState { Success, Failure, Running }

public abstract class Node
{
    protected NodeState _state;
    public NodeState state => _state;
    public abstract NodeState Evaluate();
}
