using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ResourceStorage : MonoBehaviour
{
    private ResourceManager resourceManager;
    public Resource Resource { get; private set; }
    private float m_amount;
    public float StoredAmount { get { return m_amount; } }
    public float Capacity { get; private set; }
    

    void Start()
    {
        m_amount = 0;
        resourceManager = FindAnyObjectByType<ResourceManager>();
        Capacity = 20;
    }

    public bool AssignToResource(string resource)
    {
        if (Resource != null)
        {
            bool ckRemoveFalse = Resource.RemoveStorage(this);
            if (!ckRemoveFalse) {
                Debug.Log($"Failed to remove storage({this.name}) from resource ({Resource.ResourceName}) ");
                return ckRemoveFalse; 
            }
        }

        Resource = resourceManager.FindResourceByName(resource);
        if (Resource == null)
        {
            Debug.Log($"Failed to find resource ({resource}) (for storage({this.name}))");
            return false;
        }

        bool ckAddFalse = Resource.AddStorage(this);
        if (!ckAddFalse)
        {
            Debug.Log($"Failed to remove storage({this.name}) from resource ({Resource.ResourceName}) ");
            return ckAddFalse;
        }

        Debug.Log($"Storage({this.name}) succesfully assigned to new resource ({Resource.ResourceName})");
        return true;
    }

    public bool AddResource(int amount)
    {
        if (Resource == null)
        {
            Debug.Log($"No resource assigned to storage{this.name}");
            return false; 
        }
        if (m_amount + amount > Capacity)
        {
            return false;
        }
        m_amount += amount;
        return true;
        
    }
    public bool TakeResource(int amount)
    {
        if (Resource == null)
        {
            Debug.Log($"No resource assigned to storage{this.name}");
            return false;
        }
        if (m_amount - amount < 0)
        {
            return false;
        }
        m_amount -= amount;
        return true;
    }
    private void OnDestroy()
    {
        if (Resource != null)
        {
            if (Resource.RemoveStorage(this))
                Debug.Log($"{this.name} storage is destroyed and SUCCESSFULLY removed from {Resource.name}");
            else
                Debug.Log($"{this.name} storage is destroyed but FAILED to remove from {Resource.name}");
        }
        else
        {
            Debug.Log($"{this.name} storage is destroyed but FAILED to find Resource");
        }
    }
}
