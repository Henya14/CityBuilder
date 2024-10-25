using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    [SerializeField]
    private string m_resname ;
    public string ResourceName {  get { return m_resname; } }
    [SerializeField]
    private float m_amount_per_hour;
    [SerializeField]
    private Dictionary<string, float> m_recipe; //Resource m_amount pairs
    //Producer list
    //Storage list
    public List<ResourceStorage> Storages = new List<ResourceStorage>();
    [SerializeField]
    private string m_description;
    /*
    [SerializeField]
    private string iconPath;
    [SerializeField]
    private string icon;
    */
    public static void CreateComponent(GameObject gameObject,string resname, string description, float amount_per_hour, Dictionary<string, float> recipe) 
    {
        Resource res = gameObject.AddComponent<Resource>();
        res.Parameters(resname, description, amount_per_hour, recipe);
    }
    public void Parameters(string resname, string description, float amount_per_hour, Dictionary<string, float> recipe)
    {
        m_resname = resname;
        m_description = description;
        m_amount_per_hour = amount_per_hour;
        m_recipe = recipe;
        foreach(var rec in recipe)
        {
            Debug.Log($"{rec.Key}, {rec.Value}");
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    
    public float GetAmount()
    {
        float sum = 0;
        foreach(var storage in Storages)
        {
            sum += storage.StoredAmount;
        }
        return sum;
    }
    public float GetRate()
    {
        return m_amount_per_hour;
    }
    public bool AddStorage(ResourceStorage storage)
    {
        Storages.Add(storage);
        return Storages.Contains(storage);
    }
    public bool RemoveStorage(ResourceStorage storage)
    {
        return Storages.Remove(storage);
    }
}
