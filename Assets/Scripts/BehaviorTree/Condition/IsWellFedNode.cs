using UnityEngine;

// fuzzy check whether agent is well-fed (food+water sufficiently high)
public class IsWellFedNode : Node
{
    private AgentBlackBoard bb;
    private float centerThreshold;
    private float fuzzRange;

    public IsWellFedNode(AgentBlackBoard blackBoard, float center = 0.6f, float fuzz = 0.15f)
    {
        this.bb = blackBoard;
        this.centerThreshold = center;
        this.fuzzRange = fuzz;
    }

    public override NodeState Evaluate()
    {
        float hungerPct = bb.stats.hunger.GetPercent();   // 0..1 where 1 = full
        float thirstPct = bb.stats.thirst.GetPercent();
        float staminaPct = bb.stats.stamina.GetPercent();

        // build a small random offset so not all agents behave the same each frame
        float randOffset = Random.Range(-fuzzRange, fuzzRange);
        float effectiveThreshold = centerThreshold + randOffset;

        //food and water must be above threshold, and have stamina.
        bool wellFed = (hungerPct >= effectiveThreshold) && (thirstPct >= effectiveThreshold) && (staminaPct > 0f);

        Debug.Log("Well Fed");
        return _state = (wellFed ? NodeState.Success : NodeState.Failure);
    }
}
