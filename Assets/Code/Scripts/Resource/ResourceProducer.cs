using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public class ResourceProducer : ResourceStorage
{
    [SerializeField]
    public string Type;
    [SerializeField]
    public bool TurnedOn {  get; private set; }
    [SerializeField]
    public bool IsRunning {  get; private set; }
    [SerializeField]
    public bool SomethingMissing { get; private set; }
    [SerializeField]
    public string Reason { get; private set; }
    private Dictionary<string, float> m_forProcessing; //Resources for recipe
    private Dictionary<string, float> m_needMore; //(missing)Resources for recipe 
    private int m_cycleUntilReCheck;


    // Start is called before the first frame update
    void Start()
    {
        m_cycleUntilReCheck = 0;
        //Subscribe to hour or minute change
        if (Type!=null)
            this.AssignToResource(Type);
    }
    [SerializeField]
    public void Switch()
    {
        TurnedOn = !TurnedOn;
        if (TurnedOn)
        {
            RunProducer();
        }
        else
        {
            IsRunning = false;
            Reason = "Turned OFF";
        }
    }
    private void RunProducer()
    {
        CheckRecipe();
        if (SomethingMissing)
        {
            IsRunning = false;
            Reason = "Something missing for the recipe";
        }
        else
        {
            Reason = string.Empty;
            m_cycleUntilReCheck = 60;
            IsRunning = true;
        }
    }
    private void CheckRecipe()
    {
        bool stM = false;
        var recipe = Resource.GetRecipe();
        foreach(var item in recipe)
        {
            if (m_forProcessing.ContainsKey(item.Key)) //check if the producer has the necessary resource
            {
                if (m_forProcessing[item.Key] >= item.Value) //check if the producer has the necessary amount
                {
                    continue;
                }
            }
            NeedMore(item);
            stM = true;
        }
        SomethingMissing = stM;
    }
    private void NeedMore(KeyValuePair<string, float> item)
    {
        float amount = item.Value;
        string res = item.Key;
        if(m_forProcessing.ContainsKey(res)) 
        { 
            amount -= m_forProcessing[res];
        }
        if (!m_needMore.ContainsKey(res))
        {
            m_needMore[res] = amount;
        }
        else
        {
            m_needMore.Add(res, amount);
        }
    }


    public void addResource(KeyValuePair<string, float> item)
    {
        float amount = item.Value;
        string res = item.Key;

        // Add to resources for processing dict.
        if (m_forProcessing.ContainsKey(res))
        {
            m_forProcessing[res] += amount;
        }
        else
        {
            m_forProcessing.Add(res, amount);
        }

        // Remove from resources needed to processing dict.
        if (m_needMore.ContainsKey(res))
        {
            m_needMore[res] -= amount;
            if (m_needMore[res] <= 0)
            {
                m_needMore.Remove(res);
            }
        }
    }
    public new bool AssignToResource(string resource)
    {
        Resource originalResource = Resource;
        bool re = base.AssignToResource(resource);
        if(re)
        {
            if (originalResource != null)
            {
                bool ckRemoveFalse = originalResource.RemoveProducer(this);
                if (!ckRemoveFalse)
                {
                    Debug.Log($"Failed to remove producer({this.name}) from resource ({originalResource.ResourceName}) ");
                    base.AssignToResource(originalResource.ResourceName);
                    return ckRemoveFalse;
                }
            }

            bool ckAddFalse = Resource.AddProducer(this);
            if (!ckAddFalse)
            {
                Debug.Log($"Failed to remove producer({this.name}) from resource ({originalResource.ResourceName}) ");
                base.AssignToResource(originalResource.ResourceName);
                return ckAddFalse;
            }

            Debug.Log($"Producer({this.name}) succesfully assigned to new resource ({Resource.ResourceName})");
            return true;
        }
        else
        {
            Debug.Log($"Failed to remove producer({this.name}) STORAGE from resource ({originalResource.ResourceName}) ");
        }
        return false;
    }
    public void Produce()
    {
        if(Resource != null && IsRunning)
        {
            if (m_cycleUntilReCheck > 0)
            {
                float rate = Resource.GetRatePerMinute();
                bool isFull = base.AddResource(rate);
                if (isFull)
                {
                    IsRunning = false;
                    Reason = "Storage full";
                }
                else
                {
                    m_cycleUntilReCheck--;
                }
            }
            else
            {
                RunProducer();
            }
        }

    }

}
