using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public enum Direction {
    North,
    South, 
    East,
    West
}

public abstract class AbstractBuildingType : MonoBehaviour
{
    public string buildingName {get; private set;}
    private BuildingData buildingData;
    public Vector2Int size {get; set;}
    public List<Vector3Int> gridPositions {get; set;} = new List<Vector3Int>();
    public bool isAvailable {get; set;}
    public GameObject building {get; set;}

    private Dictionary<Direction, AbstractBuildingType> neighborDictionary = new Dictionary<Direction, AbstractBuildingType> {
        {Direction.North, null},
        {Direction.South, null},
        {Direction.East, null},
        {Direction.West, null},
    };

    public virtual void Init(BuildingData buildingData) {
        this.buildingData = buildingData;
       
    }

    public virtual void PlaceAtPosition(List<Vector3Int> gridPositions, Vector3 gamePosition) {
        this.gridPositions.Clear();
        this.gridPositions.AddRange(gridPositions);
        var building = Instantiate(buildingData.prefab);
        building.transform.position = gamePosition;
    }

    public virtual void Remove() {
        gridPositions.Clear();
        if (building != null) {
            Destroy(building);
        }
    }
    
    public void SetNeighbor (Direction direction, AbstractBuildingType neighbor) {
        neighborDictionary[direction] = neighbor;
    }
}
