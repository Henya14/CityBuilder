using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public enum Direction
{
    North,
    South,
    East,
    West
}

public abstract class AbstractBuildingType : MonoBehaviour
{
    public string buildingName { get; private set; }
    private BuildingData buildingData;
    public Vector2Int size { get; set; }
    public List<Vector3Int> gridPositions { get; set; } = new List<Vector3Int>();
    public bool isAvailable { get; set; }
    public List<GameObject> buildings { get; set; } = new List<GameObject>();

    private Dictionary<Vector3Int, NeighbourData> neighborDictionary = new Dictionary<Vector3Int, NeighbourData>();

    public virtual void Init(BuildingData buildingData)
    {
        this.buildingData = buildingData;
    }

    public virtual void PlaceAtPosition(List<Vector3Int> gridPositions, List<Vector3> gamePositions, Dictionary<Vector3Int, NeighbourData> neigbours)
    {
        Remove();
        this.gridPositions.AddRange(gridPositions);
        foreach (var gamePosition in gamePositions)
        {
            var building = Instantiate(buildingData.prefab);
            building.transform.position = gamePosition;
            buildings.Add(building);
        }
    }

    public virtual void Remove()
    {
        gridPositions.Clear();
        foreach (var building in buildings)
        {
            Destroy(building);
        }
    }

    public void SetNeighbor(Vector3Int position, NeighbourData neighbor)
    {
        neighborDictionary[position] = neighbor;
    }
}
