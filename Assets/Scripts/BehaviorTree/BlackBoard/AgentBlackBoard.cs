using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBlackBoard
{
    public AgentStatsManager stats;
    public AgentMover mover;
    public AgentInventory inventory;
    public AgentStateUI ui;

    // Add these variables inside your AgentBlackBoard class:

    [Header("Explorer Settings")]
    public bool isExplorer = false; // Check this true for your explorer prefabs
    public int fogTilesRevealed = 0;
    public int maxExplorationsBeforeReturn = 3; // How many fog patches to reveal before reporting back

    [Header("Gossip Settings")]
    public float communicationRadius = 5f; // How close agents need to be to share info[Header("Gossip Settings")]
    public LayerMask agentLayer;
    public AgentAI chatPartner = null;
    public float gossipCooldownTimer = 0f;
    public int currentTreeTick = 0;

    // The permanent memory lists
    public List<Food> knownFoods = new List<Food>();
    public List<Tree> knownTrees = new List<Tree>();
    public List<WaterSource> knownWaters = new List<WaterSource>();
    // Local memory for the explorer to hold until it reaches the base
    public List<Food> localFoundFoods = new List<Food>();
    public List<Tree> localFoundTrees = new List<Tree>();
    public List<WaterSource> localFoundWaters = new List<WaterSource>();

    // External references 
    public FireBase baseRef;
    public DayNight dayNight;
     
    // Dynamic world data (updated by search nodes)
    public Transform targetFood;
    public Transform targetWater;
    public Transform targetTree;
    public Transform currentTarget;

    // Search radii
    public float foodSearchRadius;
    public float waterSearchRadius;
    public float treeSearchRadius;

    // Layers
    public LayerMask foodLayer;
    public LayerMask waterLayer;
    public LayerMask treeLayer;

    // Constructions
    [Header("Prefabs")]
    public GameObject cookingStationPrefab;
    public GameObject waterStationPrefab;
    public GameObject plankStationPrefab;
}
