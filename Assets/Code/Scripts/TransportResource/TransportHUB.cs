using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TransportHUB : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> transports;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public List<GameObject> GetCarriers()
    {
        return transports;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public Route MakeRoute(TransportationStart start, TransportationDestination destination, GameObject abstractCarrier, bool repeat, int repeattime)
    {
        if(abstractCarrier == null || !transports.Contains(abstractCarrier)) return null;

        Route route = Route.CreateRoute(start, destination, abstractCarrier, repeat, repeattime, this.gameObject);
        return route; 

    }
    public bool StartCarrier(Route route, float amount)
    {
        if(route == null || GetComponentsInChildren<Route>().Where( ro => ro.Equals(route)).ToList().Count==0) return false;
        return route.StartCarrier(amount);
    }
}
