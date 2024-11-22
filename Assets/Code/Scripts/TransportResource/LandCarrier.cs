using System.Collections;
using System.Collections.Generic;
using Unity.Loading;
using Unity.VisualScripting;
using UnityEngine;

public class LandCarrier : AbstractCarrier
{
    [SerializeField]
    private LandCarrierTypes m_type;
    public LandCarrierTypes Type {  
        get { 
            return m_type; 
        } 
        set { 
            switch (m_type)
            {
                case LandCarrierTypes.Car:
                    Capacity = 1.0F; break;
                case LandCarrierTypes.Truck:
                    Capacity = 10.0F; break;
            }

        } 
    }

    public override void Transport()
    {
        if(Status == CarrierStatus.Setup)
        {
            if(Route!=null && CarriedAmount != 0.0F)
            {
                NextStatus();
                NextStep();
            }
        }
    }
    private void NextStep()
    {
        switch(Status)
        {
            case CarrierStatus.ReadyToStart:
            case CarrierStatus.LoadingIn:
                TimeManager.OnMinuteChanged += this.LoadIn;
                break;
            case CarrierStatus.OnRoad:
                MoveToDestination();
                break;
            case CarrierStatus.Arrived:
                TimeManager.OnMinuteChanged += this.PackOut;
                break;
            case CarrierStatus.PackedOut:
                DeliveryCompleted();
                break;
            case CarrierStatus.CompletedRoute:
                if (!Route.OnRepeat) Destroy(this.gameObject);
                break;
        }
    }
    private void LoadIn()
    {
        if(Status ==CarrierStatus.ReadyToStart)
        {
            NextStatus();
        }
        TransportationStart start = Route.StartingStation;
        if(start != null)
        {
            if (start.Transfer(Route.Type, CarriedAmount))
            {
                Debug.Log("Loaded");
                TimeManager.OnMinuteChanged -= this.LoadIn;
                NextStatus();
                NextStep();
                return;
            }
            Debug.Log("Wait to be loaded");
        }
        else
        {
            Debug.LogWarning("StartingStation not found");
        }

    }
    private void MoveToDestination()
    {
        //Animate to move from start to station
        NextStatus();
        NextStep();
    }
    private void PackOut()
    {
        TransportationDestination destination = Route.Destination;
        if (destination != null)
        {
            if (destination.Deliver(Route.Type, CarriedAmount))
            {
                TimeManager.OnMinuteChanged -= this.PackOut;
                NextStatus();
                NextStep();
                return;
            }
        }
        else
        {
            Debug.LogWarning("Destination not found");
        }

    }
    private void DeliveryCompleted()
    {
        Debug.Log($"{Route.Type} {CarriedAmount} resource delivered");
        //TODO Animate to move from start to station
        NextStatus();
        NextStep();
    }

    // Start is called before the first frame update
    void Start()
    {
        Type = Type;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public enum LandCarrierTypes
{
    Car,
    Truck
}