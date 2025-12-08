// File: FireBase.cs
using System;
using UnityEngine;
using UnityEngine.Events;


public class FireBase : MonoBehaviour, I_Interactable, I_Storage
{
    [Header("Storage (editable)")]
    public FireBaseStorage storage = new FireBaseStorage()
    {
        maxFoodAmount = 25,
        maxWaterAmount = 300,
        maxWoodAmount = 500
    };

    [Header("Notifications (UnityEvents for inspector)")]
    // (resourceType, current, max)
    public UnityEvent<ResourceType, int, int> OnStorageChanged;
    public UnityEvent<ResourceType> OnStorageFull;
    public UnityEvent<ResourceType> OnStorageEmpty;
    public UnityEvent OnUpgraded;

    // C# events
    public event Action<ResourceType, int, int> StorageChanged;
    public event Action<ResourceType> StorageFull;
    public event Action<ResourceType> StorageEmpty;
    public event Action Upgraded;

    [Header("Thresholds & Settings")]
    [Range(0f, 1f)] public float agentReturnThreshold = 0.6f;
    public int upgradeCostWood = 50;

    private void Awake()
    {
        // ensure storage is not null and has sane defaults
        if (storage == null)
            storage = new FireBaseStorage();

        // clamp current values to capacity
        storage.currentFoodAmount = Mathf.Clamp(storage.currentFoodAmount, 0, storage.maxFoodAmount);
        storage.currentWaterAmount = Mathf.Clamp(storage.currentWaterAmount, 0, storage.maxWaterAmount);
        storage.currentWoodAmount = Mathf.Clamp(storage.currentWoodAmount, 0, storage.maxWoodAmount);
    }

    // I_Interactable
    public void OnHoverEnter() => SetHighlight(true);
    public void OnHoverExit() => SetHighlight(false);
    void SetHighlight(bool on)
    {
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.color = on ? Color.yellow : Color.white;
    }

    public void Interact(GameObject interactor)
    {
        Debug.Log($"FireBase clicked by {interactor?.name ?? "Player"}");

    }

    // -------------------------------------------------------------------------
    // IStorage implementation (wrapping your Deposit/Withdraw logic)
    // -------------------------------------------------------------------------
    public int AddResource(ResourceType type, int amount)
    {
        Debug.Log($"FireBase: Adding {amount} of {type}");
        Debug.Log(storage.currentWoodAmount);
        return Deposit(type, amount);
    }

    public int RemoveResource(ResourceType type, int amount)
    {
        return Withdraw(type, amount);
    }

    public int GetAmount(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Food: return storage.currentFoodAmount;
            case ResourceType.Water: return storage.currentWaterAmount;
            case ResourceType.Wood: return storage.currentWoodAmount;
            default: return 0;
        }
    }

    public int GetCapacity(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Food: return storage.maxFoodAmount;
            case ResourceType.Water: return storage.maxWaterAmount;
            case ResourceType.Wood: return storage.maxWoodAmount;
            default: return 0;
        }
    }

    // Deposit returns amount accepted
    public int Deposit(ResourceType type, int amount)
    {
        if (amount <= 0) return 0;

        int accepted = 0;
        switch (type)
        {
            case ResourceType.Food:
                int freeFood = storage.maxFoodAmount - storage.currentFoodAmount;
                accepted = Mathf.Clamp(amount, 0, freeFood);
                storage.currentFoodAmount += accepted;
                break;

            case ResourceType.Water:
                int freeWater = storage.maxWaterAmount - storage.currentWaterAmount;
                accepted = Mathf.Clamp(amount, 0, freeWater);
                storage.currentWaterAmount += accepted;
                break;

            case ResourceType.Wood:
                int freeWood = storage.maxWoodAmount - storage.currentWoodAmount;
                accepted = Mathf.Clamp(amount, 0, freeWood);
                storage.currentWoodAmount += accepted;
                break;
        }

        if (accepted > 0) NotifyChange(type);
        return accepted;
    }

    // Withdraw returns amount actually given
    public int Withdraw(ResourceType type, int amount)
    {
        if (amount <= 0) return 0;

        int given = 0;
        switch (type)
        {
            case ResourceType.Food:
                given = Mathf.Clamp(amount, 0, storage.currentFoodAmount);
                storage.currentFoodAmount -= given;
                break;
            case ResourceType.Water:
                given = Mathf.Clamp(amount, 0, storage.currentWaterAmount);
                storage.currentWaterAmount -= given;
                break;
            case ResourceType.Wood:
                given = Mathf.Clamp(amount, 0, storage.currentWoodAmount);
                storage.currentWoodAmount -= given;
                break;
        }

        if (given > 0) NotifyChange(type);
        return given;
    }

    public void Upgrade(float percentIncrease)
    {
        percentIncrease = Mathf.Max(0f, percentIncrease);
        storage.maxFoodAmount = Mathf.CeilToInt(storage.maxFoodAmount * (1f + percentIncrease));
        storage.maxWaterAmount = Mathf.CeilToInt(storage.maxWaterAmount * (1f + percentIncrease));
        storage.maxWoodAmount = Mathf.CeilToInt(storage.maxWoodAmount * (1f + percentIncrease));
        Upgraded?.Invoke();
        OnUpgraded?.Invoke();
        Debug.Log($"FireBase upgraded +{percentIncrease * 100f}% capacities");
    }

    private void NotifyChange(ResourceType type)
    {
        int cur = GetAmount(type);
        int max = GetCapacity(type);

        StorageChanged?.Invoke(type, cur, max);
        OnStorageChanged?.Invoke(type, cur, max);

        if (cur <= 0)
        {
            StorageEmpty?.Invoke(type);
            OnStorageEmpty?.Invoke(type);
        }
        else if (cur >= max)
        {
            StorageFull?.Invoke(type);
            OnStorageFull?.Invoke(type);
        }
    }

    public bool HasSpaceFor(ResourceType type)
    {
        return GetAmount(type) < GetCapacity(type);
    }

    public float GetFillPercent(ResourceType type)
    {
        int cap = GetCapacity(type);
        if (cap <= 0) return 0f;
        return (float)GetAmount(type) / cap;
    }
}


[Serializable]
public class FireBaseStorage
{
    [Header("Capacities")]
    public int maxFoodAmount = 200;
    public int maxWaterAmount = 300;
    public int maxWoodAmount = 500;

    [Header("Current")]
    public int currentFoodAmount = 0;
    public int currentWaterAmount = 0;
    public int currentWoodAmount = 0;

    public float GetFillPercent(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Food: return maxFoodAmount <= 0 ? 0f : (float)currentFoodAmount / maxFoodAmount;
            case ResourceType.Water: return maxWaterAmount <= 0 ? 0f : (float)currentWaterAmount / maxWaterAmount;
            case ResourceType.Wood: return maxWoodAmount <= 0 ? 0f : (float)currentWoodAmount / maxWoodAmount;
            default: return 0f;
        }
    }
}
