using UnityEngine;

public class DrinkWaterNode : Node
{
    private AgentBlackBoard bb;
    private float drinkDuration = 2f;
    private float drinkTimer = 0f;

    public DrinkWaterNode(AgentBlackBoard bb)
    {
        this.bb = bb;
    }

    public override NodeState Evaluate()
    {
        // --- UNIVERSAL SETUP ---
        GameObject agentObj = bb.mlBrain != null ? bb.mlBrain.gameObject : bb.mover.gameObject;
        bool isMoving = bb.mlBrain != null ? false : (bb.mover != null && !bb.mover.HasReachedDestination());

        if (bb.currentTarget == null || isMoving)
        {
            bb.ui?.SetState("Moving to Water...");
            return _state = NodeState.Running;
        }

        bb.ui?.SetState("Drinking...");
        drinkTimer += Time.deltaTime;

        if (drinkTimer >= drinkDuration)
        {
            WaterSource naturalSource = bb.currentTarget.GetComponent<WaterSource>();
            WaterStation builtStation = bb.currentTarget.GetComponent<WaterStation>();

            if (naturalSource != null)
            {
                int hydrationGained = naturalSource.Drink(bb.stats.drinkRate);
                bb.stats.DrinkWater(hydrationGained);
                naturalSource.ReleaseClaim(agentObj);
            }
            else if (builtStation != null)
            {
                builtStation.DrinkWater(bb.stats,bb);
                builtStation.ReleaseClaim(agentObj);
            }

            ClearAgentTarget();
            drinkTimer = 0f;
            return _state = NodeState.Success;
        }

        return _state = NodeState.Running;
    }

    private void ClearAgentTarget()
    {
        if (bb.mover != null) bb.mover.ClearTarget();
        if (bb.mlBrain != null) bb.mlBrain.ClearTarget();
    }
}