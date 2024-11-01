using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceProducer : ResourceStorage
{
    public bool TurnedOn {  get; private set; }
    public bool IsRunning {  get; private set; }
    public bool SomethingMissing { get; private set; }
    private Dictionary<string, float> m_forProcessing; //Resources for recipe
    private Dictionary<string, float> m_needMore; //Resources for recipe


    // Start is called before the first frame update
    void Start()
    {
        //Subscribe to hour or minute change
        
    }
    public void Switch()
    {
        TurnedOn = !TurnedOn;
        if (TurnedOn)
        {
            CheckRecipe();
            if (SomethingMissing)
            {
                IsRunning = false;
            }
            else
            {
                IsRunning = true;
            }
        }
        else
        {
            IsRunning = false;
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

}
