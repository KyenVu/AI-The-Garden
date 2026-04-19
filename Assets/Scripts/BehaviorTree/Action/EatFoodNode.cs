using UnityEngine;

public class EatFoodNode : Node
{
    private AgentBlackBoard bb;
    private float eatDuration = 2f;
    private float eatTimer = 0f;

    public EatFoodNode(AgentBlackBoard bb)
    {
        this.bb = bb;
    }

    public override NodeState Evaluate()
    {
        // --- UNIVERSAL SETUP ---
        GameObject agentObj = bb.mlBrain != null ? bb.mlBrain.gameObject : bb.mover.gameObject;

        // ML Brain handles arrival in its own node. Mover checks it here.
        bool isMoving = bb.mlBrain != null ? false : (bb.mover != null && !bb.mover.HasReachedDestination());

        if (bb.currentTarget == null || isMoving)
        {
            bb.ui?.SetState("Moving to Food Source...");
            return _state = NodeState.Running;
        }

        bb.ui?.SetState("Eating...");
        eatTimer += Time.deltaTime;

        if (eatTimer >= eatDuration)
        {
            eatTimer = 0f;

            CookingStation station = bb.currentTarget.GetComponent<CookingStation>();
            if (station != null)
            {
                if (station.cookedFoodAvailable > 0)
                {
                    int nutritionValue = station.GatherFood(bb.stats);
                    bb.stats.EatFood(nutritionValue);
                    station.cookedFoodAvailable -= nutritionValue;

                    station.ReleaseClaim(agentObj);
                    ClearAgentTarget();
                    return _state = NodeState.Success;
                }
                else
                {
                    station.ReleaseClaim(agentObj);
                    ClearAgentTarget();
                    return _state = NodeState.Failure;
                }
            }

            Food food = bb.currentTarget.GetComponent<Food>();
            if (food != null && food.data != null)
            {
                bb.stats.EatFood(food.data.nutritionValue);
                bb.stats.Rest(food.data.staminaRestore);
                food.OnEaten();
            }

            ClearAgentTarget();
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