using UnityEngine;

[CreateAssetMenu(fileName = "Water Source Data", menuName = "Data/Water Source Data", order = 2)]
public class SO_WaterSourceData : ScriptableObject
{
    [Header("Water Settings")]
    [Tooltip("How much thirst is restored per drinking action.")]
    public int hydrationValue = 25;

    [Tooltip("Maximum water capacity of this source.")]
    public float capacity = 100f;

    [Header("Visual & Audio")]
    public Sprite waterSprite;
    public AudioClip drinkSound;
}
