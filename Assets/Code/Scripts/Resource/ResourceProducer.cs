using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceProducer : ResourceStorage
{
    public bool TurnedOn {  get; private set; }
    public bool IsRunning {  get; private set; }

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

        }
        else
        {
            IsRunning = false;
        }
    }


}
