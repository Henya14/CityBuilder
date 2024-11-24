using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface TransportationDestination
{
    public bool Deliver(string type, float amount);
    public bool Acceptable(string type);
    public GameObject GetGameObject();
}
