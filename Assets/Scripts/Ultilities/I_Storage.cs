using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_Storage 
{
    // Returns amount actually accepted
    int AddResource(ResourceType type, int amount);

    // Returns amount actually removed
    int RemoveResource(ResourceType type, int amount);

    int GetAmount(ResourceType type);
    int GetCapacity(ResourceType type);
}
