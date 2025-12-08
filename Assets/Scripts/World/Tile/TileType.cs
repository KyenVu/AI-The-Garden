using UnityEngine;

[CreateAssetMenu(fileName = "New Tile Type", menuName = "World/Tile Type")]
public class TileType : ScriptableObject
{
    [Header("Basic Info")]
    public string id = "Food";
    public Sprite sprite;
    public int size = 1;

    [Header("Gameplay")]
    public bool walkable = true;
    public float moveCost = 1f;

    [Header("Rendering")]
    public string sortingLayer = "Default";
    public int orderInLayer = 0;

    [Header("Physics")]
    [Tooltip("Unity physics layer (by name) for this tile type.")]
    public string physicsLayer = "Default";

    [Header("Collision")]
    public bool addCollider = false;
    public bool isTrigger = true;

}
