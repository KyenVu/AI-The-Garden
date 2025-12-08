using UnityEngine;

[CreateAssetMenu(fileName = "SO_TreeData", menuName = "Data/TreeData", order = 3)]
public class SO_TreeData : ScriptableObject
{
    [Header("Tree Settings")]
    [Tooltip("The maxium woods of the tree")]
    public int maximumWoods = 100;
    [Tooltip("Amount of woods get each time")]
    public int woods = 10;

    [Header("Visual & Audio")]
    public Sprite[] woodsprites;
    public AudioClip cutSound;
}
