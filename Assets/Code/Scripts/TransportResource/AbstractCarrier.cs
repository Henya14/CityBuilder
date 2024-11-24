using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractCarrier : MonoBehaviour
{
    [SerializeField]
    private float m_cpacity;
    public float Capacity { 
        get { return m_cpacity; } 
        protected set { 
            if(value < 0) { 
                m_cpacity = 0; 
                m_carriedAmount = 0;
            }
            else
            {
                m_cpacity = value;
                if(m_carriedAmount > m_cpacity) {  
                    m_carriedAmount = m_cpacity; 
                }
            }
        } 
    }
    [SerializeField]
    private float m_carriedAmount;
    public float CarriedAmount { 
        get { return m_carriedAmount; } 
        protected set {
            if(value < 0) m_carriedAmount = 0;
            if(value >= Capacity) m_carriedAmount = Capacity;
            else m_carriedAmount = value;
        } 
    }
    [SerializeField]
    private Route m_route;
    public Route Route { get { return m_route; } protected set { m_route = value; } }

    [SerializeField]
    private CarrierStatus m_status;
    public CarrierStatus Status { get { return m_status; } protected set { m_status = value; } }

    void Start()
    {
        m_status = CarrierStatus.Setup;    
    }

    public abstract void Transport();
    public bool SetRoute(Route route)
    {
        if (Status != CarrierStatus.Setup)
        {
            return false;
        }
        Route = route;
        return true;
    }
    public bool WantedAmount(float amount) 
    {
        if(Status != CarrierStatus.Setup)
        {
            return false;
        }

        if(amount > Capacity)
        {
            CarriedAmount = Capacity;
            return false;
        }
        else
        {
            CarriedAmount = amount;
            return true;
        }

    }
    protected void NextStatus()
    {
        if (m_status != CarrierStatus.CompletedRoute)
            m_status += 1;
    }
    public bool Restart()
    {
        if (m_status == CarrierStatus.CompletedRoute){
            m_status = CarrierStatus.Setup;
            Transport();
            return true;
        }
        return false;

    }
    public abstract bool CanTransportBetween(TransportationStart start, TransportationDestination destination);
}

public enum CarrierStatus
{
    Setup,
    ReadyToStart,
    LoadingIn,
    OnRoad,
    Arrived,
    PackedOut,
    CompletedRoute
}