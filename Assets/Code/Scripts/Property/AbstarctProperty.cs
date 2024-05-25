using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstarctProperty : MonoBehaviour
{
     public int Capacity { get; private set; } = 4;
     public int HeadCount { get; private set; } = 0;
    [SerializeField] public PropertyType PropertyType { get; protected set; }
    public int MaxCapacity { get; protected set; } = 100;

    public GameObject PropertyGameObject { get; set; }
    public SelectionManager SelectionManager {get; set;}


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void AddPerson()
    {
        if (Capacity > HeadCount)
        {
            HeadCount++;
        }
    }
    public void IncreaseCapacity(int add)
    {
        if(Capacity+add<MaxCapacity)
            Capacity += add;
    } 

}

public enum PropertyType
{
    Residental,
    Industrial,
    Shopping
}