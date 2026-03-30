using System.Collections.Generic;
using UnityEngine;

// --- 1. DEFINE THE ENUM ---
public enum AgentRole
{
    Worker,
    Explorer
}

public class AgentAI : MonoBehaviour
{
    private Node root;
    public AgentBlackBoard bb = new AgentBlackBoard();

    [Header("Role Settings")]
    public AgentRole agentRole = AgentRole.Worker;

    [Header("Food Search Settings")]
    public LayerMask foodLayer;
    public float foodSearchRadius = 8f;

    [Header("Water Search Settings")]
    public LayerMask waterLayer;
    public float waterSearchRadius = 8f;

    [Header("Tree Search Settings")]
    public LayerMask treeLayer;
    public float treeSearchRadius = 10f;

    [Header("Gossip Settings")]
    public LayerMask agentLayer;         // <--- NEW: Only scan for this layer
    public float gossipInterval = 0.5f;  // <--- NEW: Check twice a second
    private float gossipTimer = 0f;

    [Header("Construction Settings")]
    public GameObject cookingStation;
    public GameObject waterStation;
    public GameObject plankStation;

    void Start()
    {
        // Fill blackboard
        bb.stats = GetComponent<AgentStatsManager>();
        bb.mover = GetComponent<AgentMover>();
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
        // COMMON SURVIVAL NODES (Shared by ALL roles)
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
            new MoveToDestinationNode(bb),
            new EatFoodNode(bb.stats, bb.mover, bb.ui)
        });

        var thirstSequence = new MemorySequence(bb, "Thirst Sequence", new List<Node> {
            new IsThirstyNode(bb, 0.25f),
            findConsumableWaterSelector,  
            new MoveToDestinationNode(bb),
            new DrinkWaterNode(bb.stats, bb.mover, bb.ui)
        });

        var nightRestSequence = new MemorySequence(bb, "Night Rest Sequence", new List<Node> {
            new IsNightNode(bb),
            new FindBaseNode(bb),
            new MoveToDestinationNode(bb),
            new RestNode(bb)
        });

        // ==========================================
        // WORKER-SPECIFIC NODES
        // ==========================================
        var chopWoodSequence = new MemorySequence(bb, "ChopWood Sequence", new List<Node> {
            new IsMissingWoodNode(bb.baseRef),
            new IsInventoryNotFullNode(bb, ResourceType.Wood),
            new FindTreeNode(bb),
            new MoveToDestinationNode(bb),
            new ChopTreeNode(bb),
            new FindBaseNode(bb),
            new MoveToDestinationNode(bb),
            new DepositNode(bb)
        });

        var gatherFoodSequence = new MemorySequence(bb, "Gather Food Sequence", new List<Node> {
            new IsBaseStorageNeededNode(bb, ResourceType.Food),
            new IsInventoryNotFullNode(bb, ResourceType.Food),
            new FindFoodNode(bb),
            new MoveToDestinationNode(bb),
            new GatherFoodNode(bb),
            new FindBaseNode(bb),
            new MoveToDestinationNode(bb),
            new DepositNode(bb)
        });

        var gatherWaterSequence = new MemorySequence(bb, "Gather Water Sequence", new List<Node> {
            new IsBaseStorageNeededNode(bb, ResourceType.Water),
            new IsInventoryNotFullNode(bb, ResourceType.Water),
            new FindWaterNode(bb),
            new MoveToDestinationNode(bb),
            new GatherWaterNode(bb),
            new FindBaseNode(bb),
            new MoveToDestinationNode(bb),
            new DepositNode(bb)
        });

        var buildFoodStationSequence = new MemorySequence(bb, "Build Station Sequence", new List<Node> {
            new IsCookingStationNeededNode(bb),
            new FindBuildSiteNode(bb),
            new MoveToDestinationNode(bb),
            new BuildCookingStationNode(bb)
        });
        var buildWaterStationSequence = new MemorySequence(bb, "Build Water Station Sequence", new List<Node> {
            new IsWaterStationNeededNode(bb),
            new FindBuildSiteNode(bb),
            new MoveToDestinationNode(bb),
            new BuildWaterStationNode(bb)
        });
        var buildPlankStationSequence = new MemorySequence(bb, "Build Plank Station Sequence", new List<Node> {
            new IsPlankStationNeededNode(bb),
            new FindBuildSiteNode(bb),
            new MoveToDestinationNode(bb),
            new BuildPlankStationNode(bb)
        });
        var upgradeStationSequence = new MemorySequence(bb, "Upgrade Station Sequence", new List<Node> {
            new IsCookingStationReadyForUpgradeNode(bb),
            new SetUpgradeTargetNode(bb),
            new MoveToDestinationNode(bb),
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
            new MoveAndScoutNode(bb)
        });
      
        var returnKnowledgeSequence = new MemorySequence(bb, "Return Knowledge Sequence", new List<Node> {
            new IsReadyToReportNode(bb),
            new FindBaseNode(bb),
            new MoveToDestinationNode(bb),
            new ShareKnowledgeNode(bb)
        });

        // ==========================================
        // ASSEMBLE THE ROOT BASED ON ROLE
        // ==========================================
        if (agentRole == AgentRole.Worker)
        {
            root = new Selector( new List<Node> {
                nightRestSequence,
                thirstSequence,
                hungerSequence,
               // buildFoodStationSequence,
                buildPlankStationSequence,
                //buildWaterStationSequence,
                upgradeStationSequence,
                agentLifestyle
            });
        }
        else if (agentRole == AgentRole.Explorer)
        {
            root = new Selector( new List<Node> {
                nightRestSequence,
                thirstSequence,
                hungerSequence,
                returnKnowledgeSequence,
                exploreSequence
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

        // Draw the gossip range in green
        if (bb != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, bb.communicationRadius);
        }
    }
}