using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ResourceStorage : MonoBehaviour
{
    private ResourceManager resourceManager;
    [SerializeField] //To see in inspector
    private Resource m_resource = null;
    public Resource Resource { get { return m_resource; } }
    [SerializeField] //To see in inspector
    private float m_amount=0.0F;
    public float StoredAmount { get { return m_amount; } }
    [SerializeField] //To see in inspector
    private float m_capacity = 20.0F;
    public float Capacity { get { return m_capacity; } }
    

    void Start()
    {
        resourceManager = FindObjectOfType<ResourceManager>();
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

        if(resourceManager == null) { 
            Debug.Log("Null Managaer, research for manager");
            resourceManager = FindObjectOfType<ResourceManager>();
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
        m_amount = 0.0F;
        return true;
    }

    public bool AddResource(float amount)
    {
        if (m_resource == null)
        {
            Debug.Log($"No resource assigned to storage{this.name}");
            return false; 
        }

        if (Resource.EqualCheckWithEpsilon(m_amount + amount, m_capacity))
        {
            m_amount = m_capacity;
            return true;
        }

        if (m_amount + amount > m_capacity)
        {
            return false;
        }
        m_amount += amount;
        return true;
        
    }
    public bool TakeResource(float amount)
    {
        if (m_resource == null)
        {
            Debug.Log($"No resource assigned to storage{this.name}");
            return false;
        }
        if ( (m_amount + Resource.epsilon) < amount ) //If there isn't enough in storage return false
        {
            return false;
        }

        m_amount -= amount;
        if(m_amount < 0) m_amount = 0.0F;
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
