using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Route : MonoBehaviour
{
    [SerializeField]
    private GameObject startS;
    [SerializeField]
    private GameObject destS;
    [SerializeField]
    private TransportationStart m_start;
    public TransportationStart StartingStation { get { return m_start; } }
    [SerializeField]
    private TransportationDestination m_destination;
    public TransportationDestination Destination { get { return m_destination; } }
    [SerializeField]
    private bool m_onRepeat;
    public bool OnRepeat { get { return m_onRepeat; } set {  m_onRepeat = value; } }
    [SerializeField]
    private int m_repeatTime;
    public int RepeatTime { get; set; } 
    [SerializeField]
    private string m_type;
    public string Type { get { return m_type; } }
    [SerializeField]
    private GameObject m_carrier;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static bool CanBeMade(TransportationStart start, TransportationDestination destination, AbstractCarrier abstractCarrier)
    {
        if (start == null || destination == null || abstractCarrier == null) return false;
        string type =start.GetResourceType();
        if(destination.Acceptable(type))
            return abstractCarrier.CanTransportBetween(start, destination);
        return false;
    }
    public static void CreateRoute(TransportationStart start, TransportationDestination destination, AbstractCarrier abstractCarrier, bool repeat, int repeattime, GameObject parent)
    {
        if(!CanBeMade(start, destination, abstractCarrier)) return;
        var o = new GameObject("Route: "+start.GetGameObject().name+" -> "+destination.GetGameObject().name );
        o.transform.parent = parent.transform;
        Route route = o.AddComponent<Route>();
        route.m_start = start;
        route.m_destination = destination;
        route.m_onRepeat = repeat;
        route.m_repeatTime = repeattime;
        route.m_carrier = abstractCarrier.gameObject;
        route.m_type = start.GetResourceType();
        route.StartCarrier();

    }
    public void SetCarrier(GameObject carrier)
    {
        if (carrier.GetComponent<AbstractCarrier>() != null)
        {
            m_carrier = carrier;
        }
    }
    public void StartCarrier()
    {
        if (m_carrier == null) return;
        m_carrier = Instantiate(m_carrier);
        m_carrier.GetComponent<AbstractCarrier>().Transport();
        if (OnRepeat)
        {
            m_repeatTime = RepeatTime;
            TimeManager.OnMinuteChanged += RepeatChecker;
        }
    }

    public bool SetStation()
    {
        m_start = startS.GetComponent<TransportationStart>();
        m_destination = destS.GetComponent<TransportationDestination>();
        if(m_start != null && m_destination != null)
        {
            m_type = m_start.GetResourceType();
            if(m_destination.Acceptable(Type)) 
                return true;

        }
        return false;
    }
    private void RepeatChecker()
    {
        if(m_repeatTime<=0)
        {
            ReStartCarrier();
        }
        else
        {
            m_repeatTime--;
        }
    }
    private void ReStartCarrier()
    {
        if(m_carrier.GetComponent<AbstractCarrier>().Restart())
        {
            m_repeatTime=RepeatTime;
        }
    }
}
