// File: Scripts/BehaviorTree/Action/RestNode.cs (New File)

using UnityEngine;

public class RestNode : Node
{
    private AgentBlackBoard bb;
    private float restDuration = 1f; // How long to rest for per evaluation
    private float staminaRestoredPerSecond = 5f;

    public RestNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        // 1. Check if the Sun is up (to interrupt resting)
        if (bb.dayNight != null && bb.dayNight.IsDay)
        {
            // Day has arrived! Stop resting.
            bb.ui?.SetState("Daylight! Resuming work.");
            return _state = NodeState.Success;
        }

        // 2. Ensure agent is at the base (no need to check movement as it should have arrived)
        // Note: You might want a better distance check here if the last MoveToNode succeeded

        // 3. REST
        bb.stats.Rest(staminaRestoredPerSecond * Time.deltaTime);
        bb.ui?.SetState($"Resting... Stamina: {bb.stats.stamina.GetPercent() * 100:0}");

        // Always running unless interrupted by day time.
        return _state = NodeState.Running;
    }
}