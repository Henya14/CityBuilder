using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoppingProperty : AbstarctProperty
{
    // Start is called before the first frame update
    protected override void Start()
    {
        this.PropertyType = PropertyType.Shopping;
         base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
