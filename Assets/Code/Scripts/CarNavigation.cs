using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;




public class CarNavigation : MonoBehaviour
{

    public delegate void OnReachedDestination(CarNavigation car);
    public event OnReachedDestination ReachedDestination;
    [SerializeField] float speed = 1.0f;
    [SerializeField] float onedirectionDistance = 0.5f;
    public Vector3Int CurrentGridPosition { get; set; }

    private bool _goingToWork;
    private bool _goingToShop;
    private bool _goingHome;
    public bool GoingToWork
    {
        get { return _goingToWork; }
        set
        {
            _goingToWork = value;
            _goingToShop = value == true ? false : _goingToShop;
            _goingHome = value == true ? false : _goingHome;
        }
    }

    public bool GoingToShop
    {
        get { return _goingToShop; }
        set
        {
            _goingToShop = value;
            _goingToWork = value == true ? false : _goingToWork;
            _goingHome = value == true ? false : _goingHome;
        }
    }

    public bool GoingHome
    {
        get { return _goingHome; }
        set
        {
            _goingHome = value;
            _goingToWork = value == true ? false : _goingToWork;
            _goingToShop = value == true ? false : _goingToShop;
        }
    }

    Direction facingDirection = Direction.West;
    Queue<Direction> directions = null;
    Direction nextDirection;
    Direction previousDirection;
    Vector3 movementVector = new Vector3(0, 0, 0);
    float previousRotationY = 0.0f;
    float traveledDistance = 0.0f;
    private bool reachedGoal = false;
    private bool movingToNextPoint = false;
    private bool reachedNextPoint = false;
    private GameObject currentNode = null;
    private GameObject nextNode = null;
    private float minDistanceToNextPoint = float.MaxValue;
    private bool waitingOnNextRoad = true;
    bool firstPass = true;
    public bool started = false;
    private List<GraphSearchNode<SelectableObject>> _route = new List<GraphSearchNode<SelectableObject>>();
    private DebugLabel debugLabel;
    public List<GraphSearchNode<SelectableObject>> Route
    {
        get { return _route; }
        set { _route = value; }
    }
    private int currentRouteIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        var debugLabelObject = gameObject.transform.Find("DebugLabel");
        debugLabel = debugLabelObject.GetComponent<DebugLabel>();
        speed = UnityEngine.Random.Range(1.5f, 4.5f);
    }

    public void InitializeRoute(List<GraphSearchNode<SelectableObject>> route)
    {
        Route = route;
        currentRouteIndex = 0;
        started = true;
        var destinationNode = route[route.Count - 1].GraphNode.Value.GetGameObject();
        gameObject.name = $"Car to {destinationNode.name} {gameObject.GetInstanceID()}";
    }

    // Update is called once per frame
    void Update()
    {
        if (debugLabel != null)
        {
            debugLabel.SetText("");
            // debugLabel.SetText($"Car Navigation\nReached: {reachedGoal}\nMovingToNext: {movingToNextPoint}\nCurrentIndex: {currentRouteIndex}/{_route.Count}");
        }
        if (!started)
        {
            return;
        }

        if (currentRouteIndex >= Route.Count || reachedGoal)
        {
            Remove();
            return;
        }

        currentNode = Route[currentRouteIndex].GraphNode.Value.GetGameObject();
        nextNode = currentRouteIndex + 1 < Route.Count ? Route[currentRouteIndex + 1].GraphNode.Value.GetGameObject() : null;

        var currentRoad = currentNode.GetComponent<Road>();
        var nextRoad = nextNode != null ? nextNode.GetComponent<Road>() : null;
        if (nextRoad == null)
        {
            reachedGoal = true;
            return;
        }

        if (currentRoad == null)
        {
            waitingOnNextRoad = true;
        }

        if (waitingOnNextRoad)
        {
            movingToNextPoint = false;
            if (nextRoad.isAtCapacity())
            {
                // Still waiting for space on next road
                return;
            }

            if (currentRoad != null && currentRoad.GetIndexOfCar(this) != 0)
            {
                return;
            }

            nextRoad.AddCarWaitingToEnter(this, currentRoad?.GetYieldsToRoads() ?? null, currentRoad);
            return;
        }

        else if (!movingToNextPoint)
        {
            if (currentRoad != null && !currentRoad.ContainsCar(this))
            {
                currentRoad.AddCarToRoad(this);
            }


            movingToNextPoint = true;
        }

        else if (movingToNextPoint)
        {
            var currentNodePosition = currentNode.transform.position;
            var nextNodePosition = nextNode.transform.position;
            var directionToNextNode = (nextNodePosition - currentNodePosition).normalized;
            var vectorToNextNode = nextNodePosition - currentNodePosition;
            var distanceToNextNode = Vector3.Distance(transform.position, nextNodePosition);
            var carIndexOnRoad = currentRoad != null ? currentRoad.GetIndexOfCar(this) : 0;
            var currentRoadCapacity = currentRoad != null ? currentRoad.capacity : 1;
            var carDestination = vectorToNextNode * (currentRoadCapacity - carIndexOnRoad) / (float)currentRoadCapacity + currentNodePosition;


            var carBeforeThisCar = currentRoad != null && carIndexOnRoad > 0 ? currentRoad.GetCarAtIndex(carIndexOnRoad - 1) : null;
            if (carBeforeThisCar != null)
            {
                var distanceToCarBefore = Vector3.Distance(transform.position, carBeforeThisCar.transform.position);
                if (distanceToCarBefore < 2.0f)
                {
                    // Too close to the car in front, slow down
                    speed = Mathf.Lerp(speed, 0.0f, Time.deltaTime * 5f);
                }
                else
                {
                    // Safe distance, can speed up
                    speed = Mathf.Lerp(speed, UnityEngine.Random.Range(3.5f, 7.5f), Time.deltaTime * 2f);
                }
            }
            else
            {
                // No car in front, can speed up to normal speed
                speed = Mathf.Lerp(speed, UnityEngine.Random.Range(3.5f, 7.5f), Time.deltaTime * 2f);
            }

            var movementVector = (carDestination - transform.position).normalized * speed * Time.deltaTime;
            if (movementVector.magnitude > 0.01f)
            {
                transform.position += movementVector;
            }
            // do not look at when destination behiond car
            if (movementVector.magnitude > 0.01f)
            {
                transform.LookAt(carDestination);
            }

            minDistanceToNextPoint = Mathf.Min(minDistanceToNextPoint, distanceToNextNode);
            //debugLabel.SetText($"Car Navigation\nReached: {reachedGoal}\nMovingToNext: {movingToNextPoint}\nCurrentIndex: {currentRouteIndex}/{_route.Count}\nDistanceToNextNode: {distanceToNextNode}\nDistance to destination: {Vector3.Distance(transform.position, carDestination)} \n Car index on road: {carIndexOnRoad}");
            VisualizeDestination(carDestination);
            if (distanceToNextNode < 0.1f)
            {
                // Reached next node
                transform.position = nextNodePosition;
                waitingOnNextRoad = true;
                movingToNextPoint = false;
            }
        }
    }

    private void UpdateLogicForTileBasedCars()
    {
        if (!reachedGoal)
        {
            movementVector = new Vector3(0, 0, 0);
            if (this.traveledDistance > this.onedirectionDistance || firstPass)
            {
                firstPass = false;
                this.traveledDistance = 0.0f;
                if (directions.Count != 0)
                {
                    previousDirection = nextDirection;
                    nextDirection = directions.Dequeue();
                }
                else
                {
                    reachedGoal = true;
                }

            }
            var rotationY = 0.0f;
            switch (nextDirection)
            {
                case Direction.North:
                    movementVector.z = 1.0f;
                    rotationY = -90.0f;
                    break;
                case Direction.South:
                    movementVector.z = -1.0f;
                    rotationY = 90.0f;
                    break;
                case Direction.West:
                    movementVector.x = -1.0f;
                    rotationY = 0.0f;
                    break;
                case Direction.East:
                    movementVector.x = 1.0f;
                    rotationY = 180.0f;
                    break;
            }

            if (facingDirection != nextDirection)
            {

                this.transform.Rotate(Vector3.down, -previousRotationY);
                this.transform.Rotate(Vector3.down, rotationY);
                facingDirection = nextDirection;
                previousRotationY = rotationY;
            }
            var directionVector = movementVector * speed * Time.deltaTime;
            this.transform.position += directionVector;

            this.traveledDistance += directionVector.magnitude;
        }
        else
        {
            Remove();
        }
    }

    public void SetDirections(List<Direction> directions)
    {
        this.directions = new Queue<Direction>();
        foreach (var direction in directions)
        {
            this.directions.Enqueue(direction);
        }
        reachedGoal = false;
        started = true;
    }

    /// <summary>
    /// Removes this car from any roads it is currently registered on.
    /// </summary>
    void CleanupFromRoads()
    {
        if (currentNode != null)
        {
            var currentRoad = currentNode.GetComponent<Road>();
            if (currentRoad != null)
            {
                currentRoad.RemoveCarFromRoad(this);
            }
        }
        if (nextNode != null)
        {
            var nextRoad = nextNode.GetComponent<Road>();
            if (nextRoad != null)
            {
                nextRoad.RemoveCarFromRoad(this);
            }
        }
    }

    void OnDestroy()
    {
        // Safety net: ensure car is removed from all roads when destroyed
        CleanupFromRoads();
    }

    void Remove()
    {
        ReachedDestination?.Invoke(this);
        CleanupFromRoads();
        Destroy(gameObject);
    }

    void VisualizeDestination(Vector3 destination)
    {
        //Debug.DrawLine(transform.position, destination, Color.red, 1.0f);
    }

    public void CarCanEnterRoad(Road previousRoad, Road nextRoad)
    {
        previousRoad?.RemoveCarFromRoad(this);
        nextRoad.AddCarToRoad(this);
        waitingOnNextRoad = false;
        currentRouteIndex = _route.FindIndex(s => s.GraphNode.Value.GetGameObject() == nextRoad.gameObject);
        if (currentRouteIndex == -1)
        {
            Debug.LogError("Current route does not contain the next road!");
        }
        minDistanceToNextPoint = float.MaxValue;
    }
}
