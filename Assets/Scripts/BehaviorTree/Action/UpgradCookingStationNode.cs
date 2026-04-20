using UnityEngine;

public class UpgradeCookingStationNode : Node
{
    private AgentBlackBoard bb;
    private float upgradeTime = 2f;
    private float timer = 0f;
    private const int UPGRADE_COST = 5;

    public UpgradeCookingStationNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        CookingStation station = bb.currentTarget?.GetComponent<CookingStation>();

        if (station == null || station.level >= station.maxLevel)
        {
            return _state = NodeState.Failure;
        }

        bool hasReached = false;
        if (bb.mlBrain != null)
            hasReached = bb.mlBrain.HasReachedTarget(); 
        else if (bb.mover != null)
            hasReached = bb.mover.HasReachedDestination(); 

        if (!hasReached)
        {
            return _state = NodeState.Failure;
        }


        bb.ui?.SetState($"Upgrading Station... ({timer:0.0}/{upgradeTime:0.0})");
        timer += Time.deltaTime;

        if (timer < upgradeTime)
            return _state = NodeState.Running;

        timer = 0;
        int removedFood = bb.baseRef.RemoveResource(ResourceType.Food, UPGRADE_COST);

        if (removedFood == UPGRADE_COST)
        {
            if (station.TryUpgrade())
            {
                bb.ui?.SetState($"Station Upgraded to Lv.{station.level}!");

                if (bb.mover != null) bb.mover.ClearTarget();
                if (bb.mlBrain != null) bb.mlBrain.ClearTarget();

                return _state = NodeState.Success;
            }
            return _state = NodeState.Failure;
        }
        else
        {
            bb.ui?.SetState("Upgrade failed: Food cost missing.");
            return _state = NodeState.Failure;
        }
    }
}