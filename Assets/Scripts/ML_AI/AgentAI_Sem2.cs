using System.Collections.Generic;
using UnityEngine;

public class AgentAI_Sem2 : MonoBehaviour
{
    private Node root;
    public AgentBlackBoard bb = new AgentBlackBoard();

    [Header("Role Settings")]
    public AgentRole agentRole = AgentRole.Worker;

    [Header("Search Settings")]
    public LayerMask foodLayer;
    public float foodSearchRadius = 8f;

    public LayerMask waterLayer;
    public float waterSearchRadius = 8f;

    public LayerMask treeLayer;
    public float treeSearchRadius = 10f;

    [Header("Gossip Settings")]
    public LayerMask agentLayer;
    public float gossipInterval = 0.5f;
    private float gossipTimer = 0f;

    [Header("Construction Settings")]
    public GameObject cookingStation;
    public GameObject waterStation;
    public GameObject plankStation;

    void Start()
    {
        // 1. Fill Blackboard with Semester 2 Components
        bb.stats = GetComponent<AgentStatsManager>();
        bb.mlBrain = GetComponent<AgentBrain_ML>();
        bb.inventory = GetComponent<AgentInventory>();
        bb.ui = GetComponentInChildren<AgentStateUI>();
        bb.baseRef = FindAnyObjectByType<FireBase>();
        bb.dayNight = FindAnyObjectByType<DayNight>();

        bb.cookingStationPrefab = cookingStation;
        bb.waterStationPrefab = waterStation;
        bb.plankStationPrefab = plankStation;
        bb.foodLayer = foodLayer;
        bb.waterLayer = waterLayer;
        bb.treeLayer = treeLayer;
        bb.foodSearchRadius = foodSearchRadius;
        bb.waterSearchRadius = waterSearchRadius;
        bb.treeSearchRadius = treeSearchRadius;

        // ==========================================
        // COMMON SURVIVAL NODES (Top Priority)
        // ==========================================
        var findConsumableFoodSelector = new Selector(new List<Node> {
            new FindStationFoodNode(bb),
            new FindFoodNode(bb)
        });

        var findConsumableWaterSelector = new Selector(new List<Node> {
            new FindStationWaterNode(bb),
            new FindWaterNode(bb)
        });

        var hungerSequence = new MemorySequence(bb, "Hunger Sequence", new List<Node> {
            new IsHungryNode(bb, 0.25f),
            findConsumableFoodSelector,
            new MLMoveToDestinationNode(bb),
            new EatFoodNode(bb)
        });

        var thirstSequence = new MemorySequence(bb, "Thirst Sequence", new List<Node> {
            new IsThirstyNode(bb, 0.25f),
            findConsumableWaterSelector,
            new MLMoveToDestinationNode(bb),
            new DrinkWaterNode(bb)
        });

        var nightRestSequence = new MemorySequence(bb, "Night Rest Sequence", new List<Node> {
            new IsNightNode(bb),
            new FindBaseNode(bb),
            new MLMoveToDestinationNode(bb),
            new RestNode(bb)
        });

        // ==========================================
        // WORKER-SPECIFIC NODES (Maintenance & Building)
        // ==========================================
        var chopWoodSequence = new MemorySequence(bb, "ChopWood Sequence", new List<Node> {
            new IsMissingWoodNode(bb),
            new IsInventoryNotFullNode(bb, ResourceType.Wood),
            new FindTreeNode(bb),
            new MLMoveToDestinationNode(bb),
            new ChopTreeNode(bb),
            new FindBaseNode(bb),
            new MLMoveToDestinationNode(bb),
            new DepositNode(bb)
        });

        var gatherFoodSequence = new MemorySequence(bb, "Gather Food Sequence", new List<Node> {
            new IsBaseStorageNeededNode(bb, ResourceType.Food),
            new IsInventoryNotFullNode(bb, ResourceType.Food),
            new FindFoodNode(bb),
            new MLMoveToDestinationNode(bb),
            new GatherFoodNode(bb),
            new FindBaseNode(bb),
            new MLMoveToDestinationNode(bb),
            new DepositNode(bb)
        });

        var gatherWaterSequence = new MemorySequence(bb, "Gather Water Sequence", new List<Node> {
            new IsBaseStorageNeededNode(bb, ResourceType.Water),
            new IsInventoryNotFullNode(bb, ResourceType.Water),
            new FindWaterNode(bb),
            new MLMoveToDestinationNode(bb),
            new GatherWaterNode(bb),
            new FindBaseNode(bb),
            new MLMoveToDestinationNode(bb),
            new DepositNode(bb)
        });

        var buildFoodStationSequence = new MemorySequence(bb, "Build Station Sequence", new List<Node> {
            new IsCookingStationNeededNode(bb),
            new FindBuildSiteNode(bb),
            new MLMoveToDestinationNode(bb),
            new BuildCookingStationNode(bb)
        });

        var buildWaterStationSequence = new MemorySequence(bb, "Build Water Station Sequence", new List<Node> {
            new IsWaterStationNeededNode(bb),
            new FindBuildSiteNode(bb),
            new MLMoveToDestinationNode(bb),
            new BuildWaterStationNode(bb)
        });

        var buildPlankStationSequence = new MemorySequence(bb, "Build Plank Station Sequence", new List<Node> {
            new IsPlankStationNeededNode(bb),
            new FindBuildSiteNode(bb),
            new MLMoveToDestinationNode(bb),
            new BuildPlankStationNode(bb)
        });

        var upgradeStationSequence = new MemorySequence(bb, "Upgrade Station Sequence", new List<Node> {
            new IsCookingStationReadyForUpgradeNode(bb),
            new SetUpgradeTargetNode(bb),
            new MLMoveToDestinationNode(bb),
            new UpgradeCookingStationNode(bb)
        });

        var baseMaintenanceSelector = new RandomSelector(new List<Node> {
            chopWoodSequence,
            gatherFoodSequence,
            gatherWaterSequence
        });

        var agentLifestyle = new MemorySequence(bb, "Agent Life Sequence", new List<Node> {
            new IsWellFedNode(bb, 0.3f, 0.15f),
            baseMaintenanceSelector
        });

        // ==========================================
        // EXPLORER-SPECIFIC NODES
        // ==========================================
        var exploreSequence = new MemorySequence(bb, "Explore Sequence", new List<Node> {
            new PickExplorationTargetNode(bb),
            new MLMoveToDestinationNode(bb), // Added ML Move for the Explorer
            new MoveAndScoutNode(bb)
        });

        var returnKnowledgeSequence = new MemorySequence(bb, "Return Knowledge Sequence", new List<Node> {
            new IsReadyToReportNode(bb),
            new FindBaseNode(bb),
            new MLMoveToDestinationNode(bb),
            new ShareKnowledgeNode(bb)
        });

        // ==========================================
        // ASSEMBLE THE ROOT 
        // ==========================================
        if (agentRole == AgentRole.Worker)
        {
            root = new Selector(new List<Node> {
                nightRestSequence,
                thirstSequence,
                hungerSequence,
                buildFoodStationSequence,
                buildWaterStationSequence,
                buildPlankStationSequence,
                upgradeStationSequence,
                agentLifestyle,
                new IdleNode(bb) // Fallback if everything is perfect
            });
        }
        else if (agentRole == AgentRole.Explorer)
        {
            root = new Selector(new List<Node> {
                nightRestSequence,
                thirstSequence,
                hungerSequence,
                returnKnowledgeSequence,
                exploreSequence,
                new IdleNode(bb)
            });
        }
    }

    void Update()
    {
        if (root != null)
        {
            root.Evaluate();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, foodSearchRadius);

        if (bb != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, bb.communicationRadius);
        }
    }
}

// ==========================================
// SEMESTER 2 CUSTOM NODES
// ==========================================

public class MLMoveToDestinationNode : Node
{
    private AgentBlackBoard bb;

    public MLMoveToDestinationNode(AgentBlackBoard blackboard)
    {
        this.bb = blackboard;
    }

    public override NodeState Evaluate()
    {
        if (bb.destinationObject == null) return NodeState.Failure;

        Vector2 targetPos = new Vector2(bb.destinationObject.position.x, bb.destinationObject.position.y);
        bb.mlBrain.SetExternalTarget(targetPos);

        if (bb.mlBrain.HasReachedTarget())
        {
            bb.mlBrain.ClearTarget();
            return NodeState.Success;
        }

        return NodeState.Running;
    }
}

public class IdleNode : Node
{
    private AgentBlackBoard bb;

    public IdleNode(AgentBlackBoard blackboard)
    {
        this.bb = blackboard;
    }

    public override NodeState Evaluate()
    {
        bb.mlBrain?.ClearTarget();
        if (bb.ui != null) bb.ui.SetState("Idling...");
        return NodeState.Success;
    }
}