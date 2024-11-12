using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{

    [SerializeField] //To see in inspector
    public static float epsilon = 0.001F;

    [SerializeField] //To see in inspector
    private string m_resname ;
    public string ResourceName {  get { return m_resname; } }

    [SerializeField] //To see in inspector
    private float m_amount_per_hour;

    [SerializeField] //To see in inspector
    private Dictionary<string, float> m_recipe; //Resource m_amount pairs

    [SerializeField] //To see in inspector
    private List<ResourceProducer> Producers = new List<ResourceProducer>();

    [SerializeField] //To see in inspector
    private List<ResourceStorage> Storages = new List<ResourceStorage>();

    [SerializeField] //To see in inspector
    private string m_description;

    [SerializeField] public bool log = false;
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
        TimeManager.OnMinuteChanged += LogAmount;
    }
    public void LogAmount()
    {
        if(log == true)
            Debug.Log(GetAmount());
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
    public float GetRatePerHour()
    {
        return m_amount_per_hour;
    }
    public float GetRatePerMinute()
    {
        return m_amount_per_hour/60;
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
    public Dictionary<string, float> GetRecipe()
    {
        return m_recipe;
    }
    public bool AddProducer(ResourceProducer producer)
    {
        Producers.Add(producer);
        return Producers.Contains(producer);
    }
    public bool RemoveProducer(ResourceProducer producer)
    {
        return Producers.Remove(producer);
    }
    public static bool EpsilonCheck(float a, float b)
    {
        float diff= a-b;
        if (diff > -epsilon && diff < epsilon) 
        {
            return true;
        }
        return false;
    }
}
