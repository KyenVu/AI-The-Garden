using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "GridConfig", menuName = "World/Config/GridConfig")]
public class GridConfig : ScriptableObject
{
    [Min(01)] public int width = 10;
    [Min(01)] public int height = 10;
    [Min(01)] public int seed = 12345;

    [Header("Tiles")]
    public TileType defaultTile;
    public List<TileTypeWeight> tilePalette;

}
[System.Serializable]
public class TileTypeWeight
{
    public TileType tileType;
    [Range(0, 1)] public float weight = 0.2f;
}