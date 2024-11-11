using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ResourceStorage : MonoBehaviour
{
    private ResourceManager resourceManager;
    private Resource m_resource;
    public Resource Resource { get { return m_resource; } }
    private float m_amount;
    public float StoredAmount { get { return m_amount; } }
    private float m_capacity;
    public float Capacity { get { return m_capacity; } }
    

    void Start()
    {
        m_amount = 0;
        resourceManager = FindAnyObjectByType<ResourceManager>();
        m_capacity = 20;
    }

    public bool AssignToResource(string resource)
    {
        if (m_resource != null)
        {
            bool ckRemoveFalse = m_resource.RemoveStorage(this);
            if (!ckRemoveFalse) {
                Debug.Log($"Failed to remove storage({this.name}) from resource ({m_resource.ResourceName}) ");
                return ckRemoveFalse; 
            }
        }

        m_resource = resourceManager.FindResourceByName(resource);
        if (m_resource == null)
        {
            Debug.Log($"Failed to find resource ({resource}) (for storage({this.name}))");
            return false;
        }

        bool ckAddFalse = m_resource.AddStorage(this);
        if (!ckAddFalse)
        {
            Debug.Log($"Failed to remove storage({this.name}) from resource ({m_resource.ResourceName}) ");
            return ckAddFalse;
        }

        Debug.Log($"Storage({this.name}) succesfully assigned to new resource ({m_resource.ResourceName})");
        return true;
    }

    public bool AddResource(float amount)
    {
        if (m_resource == null)
        {
            Debug.Log($"No resource assigned to storage{this.name}");
            return false; 
        }
        if (m_amount + amount > m_capacity)
        {
            return false;
        }
        m_amount += amount;
        return true;
        
    }
    public bool TakeResource(int amount)
    {
        if (m_resource == null)
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
        if (m_resource != null)
        {
            if (m_resource.RemoveStorage(this))
                Debug.Log($"{this.name} storage is destroyed and SUCCESSFULLY removed from {m_resource.name}");
            else
                Debug.Log($"{this.name} storage is destroyed but FAILED to remove from {m_resource.name}");
        }
        else
        {
            Debug.Log($"{this.name} storage is destroyed but FAILED to find Resource");
        }
    }
}
