using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;

public class Road : AbstractBuildingType
{
    [SerializeField] public int capacity { get; set; } = 3;
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
            var newWeight = baseWeight + 10f * ((float)_currentUsage / capacity);
            if (newWeight > Weight)
            {
                Weight = newWeight;
            }
            
        }
    }

    private float degrationSpeed = 0.3f;
    public void RefreshWeight(float timeSinceLastRecalculation)
    {
        if (Weight == GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT)
        {
            return;
        }
        var newWeight = Weight - degrationSpeed * timeSinceLastRecalculation;
        newWeight = Mathf.Clamp(newWeight, 1.0f, float.MaxValue);
        if (newWeight < Weight)        {
            Weight = newWeight;
        }
    }

    private List<CarNavigation> carsOnRoad = new List<CarNavigation>();

    public struct CarWaitingToEnter
    {
        public CarNavigation car;
        public float timeEnteredQueue;
        public List<Road> yieldsToRoads;
        public Road onRoad;
    }
    private List<CarWaitingToEnter> carsWaitingToEnter = new List<CarWaitingToEnter>();


    public void AddCarWaitingToEnter(CarNavigation car, List<Road> yieldsToRoads, Road onRoad)
    {
        if (carsWaitingToEnter.Exists(c => c.car == car))
        {
            return; // Car is already in the waiting list
        }
        carsWaitingToEnter.Add(new CarWaitingToEnter
        {
            car = car,
            timeEnteredQueue = Time.time,
            yieldsToRoads = yieldsToRoads,
            onRoad = onRoad
        });
    }


    public void RemoveCarWaitingToEnter(CarWaitingToEnter carWaiting)
    {
        carsWaitingToEnter.Remove(carWaiting);
        carWaiting.car.CarCanEnterRoad(carWaiting.onRoad, this);
    }

    public List<Road> GetYieldsToRoads()
    {
        return yieldsToRoads;
    }

    public void AddCarToRoad(CarNavigation car)
    {
        carsOnRoad.Add(car);
        IncrementUsage(1);
    }

    public bool ContainsCar(CarNavigation car)
    {
        return carsOnRoad.Contains(car);
    }

    public CarNavigation GetCarAtIndex(int index)
    {
        if (index >= 0 && index < carsOnRoad.Count)
        {
            return carsOnRoad[index];
        }
        return null;
    }

    public void RemoveCarFromRoad(CarNavigation car)
    {
        if (carsOnRoad.Contains(car))
        {
            carsOnRoad.Remove(car);
            IncrementUsage(-1);
        }
    }

    public int GetIndexOfCar(CarNavigation car)
    {
        return carsOnRoad.IndexOf(car);
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
    public bool ShouldYield(List<Road> yieldsToRoads = null, List<Road> otherRoads = null)
    {
        if (yieldsToRoads == null || otherRoads == null)
        {
            return false;
        }
        if (yieldsToRoads.Count == 0)
        {
            return false;
        }
        foreach (var road in otherRoads)
        {
            if (yieldsToRoads.Contains(road) && road.carsOnRoad.Count > 0)
            {
                return true;
            }
        }

        return false;
    }
    

    // Start is called before the first frame update
    void Start()
    {
        RefreshDebugLabel();
    }

    // Update is called once per frame
    void Update()
    {
        //VisualizeYieldsToRoads();
        CheckIfCarsCanEnterInLine();
        
    }

    public void CheckIfCarsCanEnterInLine()
    {
      
      if (isAtCapacity())
      {
          return;
      }
      if (carsWaitingToEnter.Count == 0) {
          return;
      }

        //RemoveCarWaitingToEnter(carsWaitingToEnter[0]);
        for (int i = 0; i < carsWaitingToEnter.Count; i++)
        {
            var carWaiting = carsWaitingToEnter[i];
            var road = carWaiting.onRoad;
            var yieldsToRoads = carWaiting.yieldsToRoads;
            var otherRoads = carsWaitingToEnter.ConvertAll(c => c.onRoad).Where(r => r != road).ToList();
            var waitingTime = Time.time - carWaiting.timeEnteredQueue;
            if (waitingTime > 5f)
            {
                Debug.LogWarning($"Car has been waiting to enter road {RoadName} for {waitingTime} seconds. Check if there is a traffic jam or if the yielding logic is too strict.");
            }
            if (road == null)
            {
                if (carsWaitingToEnter.Count(cwe => cwe.onRoad == null) == carsWaitingToEnter.Count)
                {
                   RemoveCarWaitingToEnter(carWaiting);
                   break; // If all waiting cars are waiting to enter from a null road, allow the first one to enter to prevent deadlock
                }
                continue;
            }
            if (ShouldYield(yieldsToRoads, otherRoads))
            {
                continue;
            }

            RemoveCarWaitingToEnter(carWaiting);
            break; // Only allow one car to enter per frame to prevent multiple cars entering at once when capacity allows for only one more car

        }

    }

    public void IncrementUsage(int amount)
    {
        currentUsage = carsOnRoad.Count;
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
        return carsOnRoad.Count >= capacity;
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

    public float MaxWeight() {
        return baseWeight + 10f;
    }
}
