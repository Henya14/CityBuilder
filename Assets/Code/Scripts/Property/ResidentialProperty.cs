using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResidentialProperty : AbstarctProperty
{
    PropertyType propertyType=PropertyType.Residental;
    // Start is called before the first frame update
    void Start()
    {
        this.PropertyType = PropertyType.Residental;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
