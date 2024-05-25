using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerPlant : MonoBehaviour
{
    void Start()
    {
        TimeManager.OnHourChanged += processBuilding;
    }
    public void processBuilding()
    {
        if (PlayerBalance.Coal >= 22)
        {
            PlayerBalance.Coal = -22;
            PlayerBalance.Electricity = -25;
        }
    }
}
