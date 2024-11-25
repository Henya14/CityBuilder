using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

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
    public bool OnRepeat { get { return m_onRepeat; } set {  m_onRepeat = value;
            if(value)
            {
                m_counter = m_repeatTime;
                TimeManager.OnMinuteChanged += CounterCheck;
            }
            else
            {
                TimeManager.OnMinuteChanged -= CounterCheck;
            }
        } }
    [SerializeField]
    private int m_counter;
    public int Counter { get { return m_counter; } }

    [SerializeField]
    private int m_repeatTime;
    public int RepeatTime { get { return m_repeatTime; } set { m_repeatTime = value; } } 
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
        if (!OnRepeat && m_carrier!= null && m_carrier.GetComponent<AbstractCarrier>().Status == CarrierStatus.CompletedRoute)
            Destroy(m_carrier);
        if(!(this.name.Contains(m_start.GetGameObject().name) && this.name.Contains(m_destination.GetGameObject().name)))
            Destroy(this.gameObject);
    }
    public static bool CanBeMade(TransportationStart start, TransportationDestination destination, AbstractCarrier abstractCarrier)
    {
        if (start == null || destination == null || abstractCarrier == null) return false;
        string type =start.GetResourceType();
        if(destination.Acceptable(type))
            return abstractCarrier.CanTransportBetween(start, destination);
        return false;
    }
    public static Route CreateRoute(TransportationStart start, TransportationDestination destination, AbstractCarrier abstractCarrier, bool repeat, int repeattime, GameObject parent)
    {
        if (!CanBeMade(start, destination, abstractCarrier)) return null;
        var o = new GameObject("Route: "+start.GetGameObject().name+" -> "+destination.GetGameObject().name );
        o.transform.parent = parent.transform;
        Route route = o.AddComponent<Route>();
        route.m_start = start;
        route.m_destination = destination;
        route.m_onRepeat = repeat;
        route.m_repeatTime = repeattime;
        route.m_carrier = abstractCarrier.gameObject;
        route.m_type = start.GetResourceType();
        return route;

    }
    public void SetCarrier(GameObject carrier)
    {
        if (carrier.GetComponent<AbstractCarrier>() != null)
        {
            m_carrier = carrier;
        }
    }
    public bool StartCarrier(float amount)
    {
        var ret = false;
        if (m_carrier == null) return ret;
        m_carrier = Instantiate(m_carrier);
        m_carrier.transform.parent = this.transform;
        m_carrier.GetComponent<AbstractCarrier>().SetRoute(this);
        ret = m_carrier.GetComponent<AbstractCarrier>().WantedAmount(amount);
        m_carrier.GetComponent<AbstractCarrier>().Transport();
        if (OnRepeat)
        {
            m_counter = RepeatTime;
            TimeManager.OnMinuteChanged += CounterCheck;
        }
        return ret;
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
    private void CounterCheck()
    {
        if (m_counter <= 0)
        {
            if(OnRepeat)
            {
                ReStartCarrier();
            }
        }
        else
        {
            m_counter--;
        }
    }
    private void ReStartCarrier()
    {
        if(m_carrier.GetComponent<AbstractCarrier>().Restart())
        {
            m_counter = RepeatTime;
        }
    }
}
