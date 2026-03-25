public enum NodeState { Success, Failure, Running }

public abstract class Node
{
    protected NodeState _state;
    public NodeState state => _state;
    public abstract NodeState Evaluate();
    protected void Log(AgentBlackBoard bb, string category, string message)
    {
        // Format: [AI:AgentName] CATEGORY: Message
        // Use colors: Stats/State = Cyan, Actions = Green, Warnings = Yellow
        string color = category == "DECISION" ? "cyan" : "white";
        UnityEngine.Debug.Log($"<b>[AI:{bb.mover.name}]</b> <color={color}>{category}:</color> {message}", bb.mover.gameObject);
    }
}
