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
    private string _roadName;

    public string RoadName
    {
        get
        {
            return _roadName;
        }
        set
        {
            _roadName = value;
        }
    }

    [SerializeField]
    public float Weight
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
            Weight = baseWeight + 10f * ((float)_currentUsage / capacity);
        }
    }

    private List<CarNavigation> carsOnRoad = new List<CarNavigation>();

    public void AddCarToRoad(CarNavigation car)
    {
        carsOnRoad.Add(car);
        IncrementUsage(1);
    }

    public void RemoveCarFromRoad(CarNavigation car)
    {
        if (carsOnRoad.Contains(car))
        {
            carsOnRoad.Remove(car);
            IncrementUsage(-1);
        }
    }

    private List<Road> yieldsToRoads = new List<Road>();

    public void AddYieldsToRoad(Road road)
    {
        if (!yieldsToRoads.Contains(road))
        {
            yieldsToRoads.Add(road);
        }
    }

    /// <summary>
    /// Returns true if this road must yield because a road it yields to has active traffic.
    /// Used to enforce right-hand traffic priority at intersections.
    /// </summary>
    public bool ShouldYield()
    {
        foreach (var road in yieldsToRoads)
        {
            if (road != null && road.currentUsage > 0)
            {
                return true;
            }
        }
        return false;
    }



    private NavigationManager navigationManager;

    // Start is called before the first frame update
    void Start()
    {
        navigationManager = FindObjectOfType<NavigationManager>();
        RefreshDebugLabel();
    }

    // Update is called once per frame
    void Update()
    {
        VisualizeYieldsToRoads();
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

    public void VisualizeYieldsToRoads()
    {
        foreach (var road in yieldsToRoads)
        {
            if (road != null)
            {
                var y = transform.position.y + 1.0f;
                var randomColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                var destinationWithOffset = new Vector3(road.transform.position.x + 0.1f, y, road.transform.position.z + 0.1f);
                Debug.DrawLine(new Vector3(transform.position.x, y, transform.position.z), destinationWithOffset, randomColor, 1000f);
                var directionVector = destinationWithOffset - new Vector3(transform.position.x, y, transform.position.z);
                var arrowLeft = Quaternion.Euler(0, 150, 0) * directionVector.normalized * 0.5f;
                var arrowRight = Quaternion.Euler(0, -150, 0) * directionVector.normalized * 0.5f;
                Debug.DrawLine(destinationWithOffset, destinationWithOffset + arrowLeft, randomColor, 1000f);
                Debug.DrawLine(destinationWithOffset, destinationWithOffset + arrowRight, randomColor, 1000f);
                
            }
        }
    }
}
