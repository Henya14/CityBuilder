using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarNavigation : MonoBehaviour
{
    [SerializeField] float speed = 1.0f;
    public Vector3Int CurrentGridPosition {get; set;}

    Direction facingDirection = Direction.West;
    Queue<Direction> directions = new Queue<Direction>();
    Direction nextDirection = Direction.South;
    Vector3 movementVector = new Vector3(0, 0, 0);
    float previousRotationY = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        directions.Enqueue(Direction.North);
    }

    // Update is called once per frame
    void Update()
    {      
        
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            nextDirection = Direction.North;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            nextDirection = Direction.South;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            nextDirection = Direction.West;
        } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            nextDirection = Direction.East;
        }
        if (directions.Count != 0)
        {
            movementVector = new Vector3(0, 0, 0);
            //var nextDirection = directions.Dequeue();
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
            
            if (facingDirection != nextDirection) {
                
                this.transform.Rotate(Vector3.down, -previousRotationY); 
                this.transform.Rotate(Vector3.down, rotationY); 
                facingDirection = nextDirection;
                previousRotationY = rotationY;
            }
        }
        this.transform.position += movementVector * speed * Time.deltaTime;
    }
}
