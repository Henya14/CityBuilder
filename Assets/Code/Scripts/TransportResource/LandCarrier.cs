using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandCarrier : AbstractCarrier
{
    private LandCarrierTypes m_type;
    public LandCarrierTypes Type {  
        get { 
            return m_type; 
        } 
        set { 
            if (m_type != value)
            {
                switch (m_type)
                {
                    case LandCarrierTypes.Car:
                        Capacity = 1.0F; break;
                    case LandCarrierTypes.Truck:
                        Capacity = 10.0F; break;
                }
            }

        } 
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public enum LandCarrierTypes
{
    Car,
    Truck
}