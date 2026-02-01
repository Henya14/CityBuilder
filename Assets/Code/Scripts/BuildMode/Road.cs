using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;

public class Road : AbstractBuildingType
{
    [SerializeField] int capacity = 2;
    [SerializeField] public float baseWeight = 10f;
    private float _weight = 0f;
    [SerializeField] public float Weight
    {
        get
        {
            return _weight;
        }
        set
        {
            _weight = value;
        }
    }
    private int _currentUsage = 0;

    public int currentUsage
    {
        get
        {
            return _currentUsage;
        }
        set
        {
            _currentUsage = value;
            Weight =  baseWeight + 10f * ((float)_currentUsage / capacity);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        RefreshDebugLabel();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void IncrementUsage(int amount)
    {
        currentUsage += amount;
        currentUsage = Mathf.Clamp(currentUsage, 0, capacity);
        RefreshDebugLabel();
    }

    public void RefreshDebugLabel()
    {
        var debugLabelObject = gameObject.transform.Find("DebugLabel");
        if (debugLabelObject != null)
        {
            var debugLabel = debugLabelObject.GetComponent<DebugLabel>();
            if (debugLabel != null)
            {
                debugLabel.SetText($"Road\nUsage: {currentUsage}/{capacity}\nWeight: {Weight}");
            }
        }
    }

    public bool isAtCapacity()
    {
        return currentUsage >= capacity;
    }

    // public override void SetNeighbourForPosition(Vector3Int position, Vector3Int neighbourPosition, AbstractBuildingType neighbour)
    // {
    //     base.SetNeighbourForPosition(position, neighbourPosition, neighbour);
    //     /*var numberOfRoadNeighbours = 0;
    //     foreach (var positionWithBuilding in neighbourDatasForPositions[position].NeighboursForGridPositions) 
    //     {
    //         if (positionWithBuilding.Value is Road) {
    //             numberOfRoadNeighbours++;
    //         }
    //     }

    //     Destroy(buildings[position]);
    //     buildings.Remove(position);
    //     GameObject prefabToInst;
    //     switch(numberOfRoadNeighbours) {
    //         case 0:
    //             prefabToInst = twoWayStraight;
    //             break;
    //         case 1:
    //             prefabToInst = twoWayStraight;
    //             break;
    //         case 2:
    //             prefabToInst = twoWayCurvy;
    //             break;
    //         case 3:
    //             prefabToInst = threeWay;
    //             break;
    //         case 4:
    //             prefabToInst = fourWay;
    //             break;
    //         default:
    //             throw new Exception("Anyád picsája");
    //     }

    //     var buil = Instantiate(prefabToInst);
    //     buil.transform.position = position; 
    //     buildings.Add(position, buil);*/
    // }
}
