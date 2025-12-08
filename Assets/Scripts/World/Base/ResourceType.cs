// File: Scripts/Inventory/ResourceTypes.cs
using System;

public enum ResourceType
{
    Wood,
    Food,
    Water
}

[Serializable]
public struct ResourceAmount
{
    public ResourceType type;
    public int amount;

    public ResourceAmount(ResourceType t, int a)
    {
        type = t;
        amount = a;
    }
}
