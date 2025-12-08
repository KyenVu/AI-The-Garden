using UnityEngine;

[CreateAssetMenu(fileName = "SO_FoodData", menuName = "Data/FoodData", order = 1)]
public class SO_FoodData : ScriptableObject
{
    [Header("Basic Info")]
    public string foodName = "Food";

    [Header("Stats")]
    [Tooltip("How much hunger this food restores when eaten.")]
    public int nutritionValue = 30;
    [Tooltip("How much stamina this food restores when eaten.")]
    public float staminaRestore = 10f;

    [Header("Other Settings")]
    public bool destroyAfterEat = true;
    public AudioClip eatSound;
}
