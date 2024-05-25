using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class treeFarm : Building
{
    void Start()
    {
        TimeManager.OnHourChanged += processBuilding;
    }
    public void processBuilding()
    {
        PlayerBalance.Wood = 5;
    }
}
