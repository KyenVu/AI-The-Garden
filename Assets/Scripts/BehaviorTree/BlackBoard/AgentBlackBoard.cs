using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBlackBoard
{
    public AgentStatsManager stats;
    public AgentMover mover;
    public AgentInventory inventory;
    public AgentStateUI ui;

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
}
