// File: Scripts/Inventory/AgentInventory.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class AgentInventory : MonoBehaviour
{
    [Header("Capacity per resource")]
    public int maxWood = 50;
    public int maxFood = 5;
    public int maxWater = 50;

    // current amounts
    private int wood;
    private int food;
    private int water;

    // Events
    public event Action OnInventoryChanged; // general change
    public event Action<ResourceType, int> OnResourceChanged; // (type, newAmount)
    public event Action<ResourceType> OnResourceFull;
    public event Action<ResourceType> OnResourceEmpty;

    // Public read-only accessors
    public int Wood => wood;
    public int Food => food;
    public int Water => water;

    public int GetCapacity(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Wood: return maxWood;
            case ResourceType.Food: return maxFood;
            case ResourceType.Water: return maxWater;
            default: return 0;
        }
    }

    public int GetAmount(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Wood: return wood;
            case ResourceType.Food: return food;
            case ResourceType.Water: return water;
            default: return 0;
        }
    }

    public bool IsFull(ResourceType type)
    {
        return GetAmount(type) >= GetCapacity(type);
    }

    public bool IsEmpty(ResourceType type)
    {
        return GetAmount(type) <= 0;
    }

    /// <summary>
    /// Try to add resource. Returns the actual amount accepted (may be < requested if near capacity).
    /// </summary>
    public int AddResource(ResourceType type, int amount)
    {
        if (amount <= 0) return 0;

        int accepted = 0;
        switch (type)
        {
            case ResourceType.Wood:
                accepted = Mathf.Min(amount, maxWood - wood);
                wood += accepted;
                break;
            case ResourceType.Food:
                accepted = Mathf.Min(amount, maxFood - food);
                food += accepted;
                break;
            case ResourceType.Water:
                accepted = Mathf.Min(amount, maxWater - water);
                water += accepted;
                break;
        }

        if (accepted > 0)
        {
            OnResourceChanged?.Invoke(type, GetAmount(type));
            OnInventoryChanged?.Invoke();

            if (IsFull(type))
                OnResourceFull?.Invoke(type);
        }

        return accepted;
    }

    /// <summary>
    /// Try to remove resource. Returns actual removed (may be less if not enough).
    /// </summary>
    public int RemoveResource(ResourceType type, int amount)
    {
        if (amount <= 0) return 0;

        int removed = 0;
        switch (type)
        {
            case ResourceType.Wood:
                removed = Mathf.Min(amount, wood);
                wood -= removed;
                break;
            case ResourceType.Food:
                removed = Mathf.Min(amount, food);
                food -= removed;
                break;
            case ResourceType.Water:
                removed = Mathf.Min(amount, water);
                water -= removed;
                break;
        }

        if (removed > 0)
        {
            OnResourceChanged?.Invoke(type, GetAmount(type));
            OnInventoryChanged?.Invoke();

            if (IsEmpty(type))
                OnResourceEmpty?.Invoke(type);
        }

        return removed;
    }

    /// <summary>
    /// Transfer up to amount from this inventory to a target storage (IStorage).
    /// Returns actual transferred amount.
    /// </summary>
    public int TransferTo(I_Storage target, ResourceType type, int amount)
    {
        if (target == null || amount <= 0) return 0;

        int have = GetAmount(type);
        int toTransfer = Mathf.Min(have, amount);
        if (toTransfer <= 0) return 0;

        int accepted = target.AddResource(type, toTransfer);
        if (accepted > 0)
        {
            RemoveResource(type, accepted);
        }

        return accepted;
    }

    /// <summary>
    /// Convenience: empty full inventory into target storage (tries all types).
    /// </summary>
    public void DepositAllTo(I_Storage storage)
    {
        if (storage == null) return;
        foreach (ResourceType t in Enum.GetValues(typeof(ResourceType)))
        {
            int amt = GetAmount(t);
            if (amt > 0)
                TransferTo(storage, t, amt);
        }
    }
}
