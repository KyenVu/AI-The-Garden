using System.Collections.Generic;
using UnityEngine;

public class AgentAI : MonoBehaviour
{
    private Node root;
    public AgentBlackBoard bb = new AgentBlackBoard();

    //private AgentStatsManager stats;
    //private AgentMover mover;
    //private AgentInventory inventory;

    [Header("Food Search Settings")]
    public LayerMask foodLayer;
    public float foodSearchRadius = 8f;

    [Header("Water Search Settings")]
    public LayerMask waterLayer;
    public float waterSearchRadius = 8f;

    [Header("Tree Search Settings")]
    public LayerMask treeLayer;
    public float treeSearchRadius = 10f;

    [Header("Construction Settings")]
    public GameObject cookingStation;
    //public GameObject sawmill;

    //[Header("UI")]
    //public AgentStateUI stateUI;

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

        bb.foodLayer = foodLayer;
        bb.waterLayer = waterLayer;
        bb.treeLayer = treeLayer;

        bb.foodSearchRadius = foodSearchRadius;
        bb.waterSearchRadius = waterSearchRadius;
        bb.treeSearchRadius = treeSearchRadius;

        // Build BT using blackboard-powered nodes

        var findConsumableFoodSelector = new Selector(new List<Node>
    {
        new FindStationFoodNode(bb), // PRIORITY 1: Check Cooking Station
        new FindFoodNode(bb)         // PRIORITY 2: Check World Food
    });

        // --- HUNGER SEQUENCE ---

        var hungerSequence = new MemorySequence(new List<Node> {
            new IsHungryNode(bb, 0.25f),
            findConsumableFoodSelector,
            new MoveToDestinationNode(bb),
            new EatFoodNode(bb.stats, bb.mover, bb.ui)
        });

        // --- THIRST SEQUENCE ---

        var thirstSequence = new MemorySequence(new List<Node> {
            new IsThirstyNode(bb, 0.25f),
            new FindWaterNode(bb),
            new MoveToDestinationNode(bb),
            new DrinkWaterNode(bb.stats, bb.mover, bb.ui)
        });

        // --- CHOP WOOD SEQUENCE ---

        var chopWoodSequence = new MemorySequence(new List<Node> {
            new IsMissingWoodNode(bb.baseRef),
            new IsInventoryNotFullNode(bb, ResourceType.Wood),   // NEW: Ensures agent has space
            new FindTreeNode(bb),
            new MoveToDestinationNode(bb),
            new ChopTreeNode(bb),
            new FindBaseNode(bb),
            new MoveToDestinationNode(bb),
            new DepositNode(bb)
        });

        // --- GATHER FOOD SEQUENCE ---

        var gatherFoodSequence = new MemorySequence(new List<Node> {
            new IsBaseStorageNeededNode(bb, ResourceType.Food),
            new IsInventoryNotFullNode(bb, ResourceType.Food),
            new FindFoodNode(bb),
            new MoveToDestinationNode(bb),
            new GatherFoodNode(bb), // <<< NEW ACTION NODE
            new FindBaseNode(bb),
            new MoveToDestinationNode(bb),
            new DepositNode(bb)
        });

        // --- GATHER WATER SEQUENCE ---

        var gatherWaterSequence = new MemorySequence(new List<Node> {
            new IsBaseStorageNeededNode(bb, ResourceType.Water),
            new IsInventoryNotFullNode(bb, ResourceType.Water),
            new FindWaterNode(bb),
            new MoveToDestinationNode(bb),
            new GatherWaterNode(bb), // <<< NEW ACTION NODE
            new FindBaseNode(bb),
            new MoveToDestinationNode(bb),
            new DepositNode(bb)
        });
        // --- NIGHT REST SEQUENCE ---

        var nightRestSequence = new MemorySequence(new List<Node> {
            new IsNightNode(bb), // Condition: Is it night?
            new FindBaseNode(bb), // Reuse FindBaseNode to set the currentTarget (the base)
            new MoveToDestinationNode(bb),
            new RestNode(bb)
        });
        // --- BUILD COOKING STATION SEQUENCE ---

        var buildStationSequence = new MemorySequence(new List<Node> {
            new IsCookingStationNeededNode(bb), // Condition: Base Food <= 10 AND no station
            new FindBuildSiteNode(bb),             // Targets a neighbor tile near the base (due to your modification)
            new MoveToDestinationNode(bb),
            new BuildCookingStationNode(bb)    // Action: Build and deduct cost
        });

        // ---  UPGRADE SEQUENCE ---

        var upgradeStationSequence = new MemorySequence(new List<Node> {
            new IsCookingStationReadyForUpgradeNode(bb),
            new SetUpgradeTargetNode(bb),
            new MoveToDestinationNode(bb),
            new UpgradeCookingStationNode(bb)
        });

        // Selector for Base Maintenance 
        var baseMaintenanceSelector = new RandomSelector(new List<Node> {
            chopWoodSequence,
            gatherFoodSequence,
            gatherWaterSequence
        });

        // Gated Lifestyle Sequence (Only runs if agent is well-fed)
        var agentLifestyle = new MemorySequence(new List<Node> {
            new IsWellFedNode(bb, 0.3f, 0.15f), // Needs must be high to enter this loop
            baseMaintenanceSelector
        });


        // --- ROOT SELECTOR 
        root = new MemorySelector(new List<Node> {
            nightRestSequence,
            thirstSequence,
            hungerSequence,
            buildStationSequence,
            upgradeStationSequence,
            agentLifestyle // This becomes the new default behavior
        });

        //Create PrioritySelector — choose based on NEED SEVERITY
        //root = new PrioritySelector(
        //    new List<Node> { thirstSequence, hungerSequence },
        //    (children) =>
        //    {
        //        float hungerUrgency = 1 - bb.stats.hunger.GetPercent();
        //        float thirstUrgency = 1 - bb.stats.thirst.GetPercent();

        //        if (thirstUrgency > hungerUrgency) return thirstSequence;
        //        else return hungerSequence;
        //    }
        //);

    }


    void Update()
    {
        root.Evaluate();  // BT runs every frame
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, foodSearchRadius); // searchRadius
    }
}
