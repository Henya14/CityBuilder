using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    [SerializeField]
    private string m_resname;
    [SerializeField]
    private double m_amount_per_hour;
    [SerializeField]
    private Dictionary<string, double> m_recipe; //Resource amount pairs
    //Producer list
    //Storage list
    [SerializeField]
    private string m_description;
    /*
    [SerializeField]
    private string iconPath;
    [SerializeField]
    private string icon;
    */
    public Resource(string resname, string description, double amount_per_hour, Dictionary<string, double> recipe) 
    {
        m_resname = resname;
        m_description = description;
        m_amount_per_hour = amount_per_hour;
        m_recipe = recipe;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public double GetAmount()
    {
        return m_amount_per_hour;
        //get amount from all storage
    }
    public double GetRate()
    {
        return m_amount_per_hour;
    }
}
