using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CarNavigation : MonoBehaviour
{
    [SerializeField] float speed = 1.0f;
    [SerializeField] float onedirectionDistance = 0.5f;
    public Vector3Int CurrentGridPosition { get; set; }

    Direction facingDirection = Direction.West;
    Queue<Direction> directions = null;
    Direction nextDirection;
    Direction previousDirection;
    Vector3 movementVector = new Vector3(0, 0, 0);
    float previousRotationY = 0.0f;
    float traveledDistance = 0.0f;
    private bool reachedPosition = false;
    private bool movingToNextPoint = false;
    private GameObject currentNode = null;
    private GameObject nextNode = null;
    private float minDistanceToNextPoint = float.MaxValue;
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
    }

    public void InitializeRoute(List<GraphSearchNode<SelectableObject>> route)
    {
        Route = route;
        currentRouteIndex = 0;
        started = true;
        var destinationNode = route[route.Count - 1].GraphNode.Value.GetGameObject();
        gameObject.name = $"Car to {destinationNode.name}";
    }

    // Update is called once per frame
    void Update()
    {
        if (debugLabel != null)
        {
            debugLabel.SetText($"Car Navigation\nReached: {reachedPosition}\nMovingToNext: {movingToNextPoint}\nCurrentIndex: {currentRouteIndex}/{_route.Count}");
        }
        if (!started)
        {
            return;
        }

         if (reachedPosition)
        {
            var currentRoad = currentNode.GetComponent<Road>();

            currentRoad.IncrementUsage(-1);
            
            Remove();
        }

        if (!reachedPosition && !movingToNextPoint)
        {
            movementVector = new Vector3(0, 0, 0);
            minDistanceToNextPoint = float.MaxValue;
            if (currentRouteIndex >= _route.Count - 1)
            {
                reachedPosition = true;
                movingToNextPoint = false;
                return;
            }
            nextNode = _route[currentRouteIndex + 1].GraphNode.Value.GetGameObject();

            if (nextNode == null)
            {
                reachedPosition = true;
                return;
            }

            var nextRoad = nextNode.GetComponent<Road>();
            if (nextRoad == null)
            {
                reachedPosition = true;
                return;
            }

            if (nextRoad.isAtCapacity())
            {
                // Wait until road is not at capacity
                return;
            } 

            currentNode = _route[currentRouteIndex].GraphNode.Value.GetGameObject();

            nextRoad.IncrementUsage(1);
            
            movingToNextPoint = true;
        }
        if (movingToNextPoint)
        {
            movementVector = (nextNode.transform.position - currentNode.transform.position).normalized;
            var distanceToNextPoint = Vector3.Distance(transform.position, nextNode.transform.position);
            if (distanceToNextPoint < minDistanceToNextPoint)
            {
                minDistanceToNextPoint = distanceToNextPoint;
            }
            if (distanceToNextPoint < 0.3f || distanceToNextPoint > minDistanceToNextPoint)
            {
                var curretRoad = currentNode.GetComponent<Road>();
                if (curretRoad != null)
                {
                    curretRoad.IncrementUsage(-1);
                }
                currentRouteIndex++;
                movingToNextPoint = false;
                transform.position = nextNode.transform.position;
                return;
            }
            var tartgetRotation = Quaternion.LookRotation(movementVector, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, tartgetRotation, Time.deltaTime * 5.0f);
            var directionVector = movementVector * speed * Time.deltaTime;
            transform.position += directionVector;
        }
       
    }

    private void UpdateLogicForTileBasedCars()
    {
        if (!reachedPosition)
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
                    reachedPosition = true;
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

    public void SetDirections(List<Direction> directions) {
        this.directions = new Queue<Direction>(); 
        foreach (var direction in directions) {
            this.directions.Enqueue(direction);
        }
        reachedPosition = false;
        started = true;
    }

    void Remove()
    {
        Destroy(gameObject);
    }
}
