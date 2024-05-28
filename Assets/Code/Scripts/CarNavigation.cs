using System.Collections;
using System.Collections.Generic;
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
    bool firstPass = true;
    public bool starteda = false;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!this.starteda) {
            return;
        }


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
            //Remove();
        }

    }

    public void SetDirections(List<Direction> directions) {
        this.directions = new Queue<Direction>(); 
        foreach (var direction in directions) {
            this.directions.Enqueue(direction);
        }
        reachedPosition = false;
        starteda = true;
        Debug.Log($"hee: {starteda}");
    }

    public void Starta() {
        starteda = true;
    }

    void Remove()
    {
        Destroy(gameObject);
    }
}
