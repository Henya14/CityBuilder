using System.Collections;
using System.Collections.Generic;
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
    public bool MakeRoute(GameObject carrier)
    {
        if(carrier == null || !transports.Contains(carrier)) return false;
        return false; 

    }
}
