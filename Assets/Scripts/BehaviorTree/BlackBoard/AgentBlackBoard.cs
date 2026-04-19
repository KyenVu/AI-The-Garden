using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBlackBoard
{
    public AgentStatsManager stats;

    // --- SEMESTER 1 vs SEMESTER 2 MOVEMENT ---
    public AgentMover mover;            // For Semester 1 (A* Pathfinding)
    public AgentBrain_ML mlBrain;       // For Semester 2 (Neural Network)

    public AgentInventory inventory;
    public AgentStateUI ui;

    [Header("Explorer Settings")]
    public bool isExplorer = false;
    public int fogTilesRevealed = 0;
    public int maxExplorationsBeforeReturn = 3;

    [Header("Gossip Settings")]
    public float communicationRadius = 5f;
    public LayerMask agentLayer;
    public AgentAI chatPartner = null;
    public float gossipCooldownTimer = 0f;
    public int currentTreeTick = 0;

    // --- MEMORY SYSTEMS ---
    public List<Food> knownFoods = new List<Food>();
    public List<Tree> knownTrees = new List<Tree>();
    public List<WaterSource> knownWaters = new List<WaterSource>();

    public List<Food> localFoundFoods = new List<Food>();
    public List<Tree> localFoundTrees = new List<Tree>();
    public List<WaterSource> localFoundWaters = new List<WaterSource>();

    // External references 
    public FireBase baseRef;
    public DayNight dayNight;

    // --- TARGET HANDOFF SYSTEM ---
    public Transform targetFood;
    public Transform targetWater;
    public Transform targetTree;
    public Transform currentTarget;

    // THE CRITICAL NEW VARIABLE: Used by MLMoveToDestinationNode to pass coordinates to the brain
    public Transform destinationObject;

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