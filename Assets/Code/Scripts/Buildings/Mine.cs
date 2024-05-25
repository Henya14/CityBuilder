using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : Building
{
    void Start()
    {
        TimeManager.OnHourChanged += processBuilding;
    }
    public void processBuilding()
    {
        PlayerBalance.Coal = 50;
    }
}
