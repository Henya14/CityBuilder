using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;


public abstract class AbstractBuildingType : MonoBehaviour
{
    public string buildingName { get; private set; }
    private BuildingData buildingData;
    public Vector2Int size { get; set; }
    public List<Vector3Int> gridPositions { get; set; } = new List<Vector3Int>();
    public List<Vector3> prefabPlacePositions { get; set; } = new List<Vector3>();
    public bool isAvailable { get; set; }
    public Dictionary<Vector3Int, GameObject> buildings { get; set; } = new Dictionary<Vector3Int, GameObject>();

    // public Dictionary<Vector3Int, NeighbourData> neighbourDatasForPositions { get; private set; } = new Dictionary<Vector3Int, NeighbourData>();
    private Dictionary<Vector3Int, SelectionManager> selectionManagers  = new Dictionary<Vector3Int, SelectionManager>();
  
    public GameObject twoWayStraight;
    public GameObject twoWayCurvy;
    public GameObject threeWay;
    public GameObject fourWay;

    public virtual void Init(BuildingData buildingData)
    {
        buildingName = buildingData.Name;
        this.buildingData = buildingData;
    }

    public virtual void PlaceAtPosition(Dictionary<(Vector3, Quaternion), List<Vector3Int>> prefabPlacePositionsAndCorrespondingGridPositions, Dictionary<Vector3Int, NeighbourData> neigbours, GridManager gridManager, GameObject prefab = default)
    {
        Remove();
        prefabPlacePositionsAndCorrespondingGridPositions.Values.ToList().ForEach(l => this.gridPositions.AddRange(l));
        foreach (var ((gamePosition, rotation), gridPositions) in prefabPlacePositionsAndCorrespondingGridPositions)
        {
            GameObject building;
            if (prefab == default) {
                building = Instantiate(buildingData.prefab);
            } else {
                building = prefab;
            }
            building.transform.position = gamePosition;
            building.transform.rotation = rotation;
            var selectionManager = building.AddComponent<SelectionManager>();
            selectionManager.Init(new Vector3Int(), buildingData.Description, buildingData.buildingType.ToSelectableObjectType(), gridManager);
            foreach (var gridPosition in gridPositions) {
                selectionManagers.Add(gridPosition, selectionManager);
                buildings.Add(gridPosition, gameObject);
                selectionManager.SetGridPosition(gridPosition);
            }
            selectionManager.SetGridPositions(gridPositions);
        }

        // neighbourDatasForPositions = neigbours;
        // foreach (var neighbourDatasForPosition in neighbourDatasForPositions)
        // {
        //     foreach (var neighbourForGridPosition in neighbourDatasForPosition.Value.NeighboursForGridPositions)
        //     {
        //         var currentTilePosition = neighbourDatasForPosition.Key;
        //         var neigbourPosition = neighbourForGridPosition.Key;
        //         var neighbour = neighbourForGridPosition.Value;
        //         if (neighbour != null)
        //         {
        //             neighbour.SetNeighbourForPosition(neigbourPosition, currentTilePosition, this);
        //         }
        //     }
        // }
    }

    public virtual void Remove()
    {
        gridPositions.Clear();
        foreach (var building in buildings.Values)
        {
            Destroy(building);
        }

        buildings.Clear();
    }

    // public virtual void SetNeighbourForPosition(Vector3Int position, Vector3Int neighbourPosition, AbstractBuildingType neighbour)
    // {
    //     var neighbourDataForPosition = neighbourDatasForPositions[position];
    //     neighbourDataForPosition.SetNeighbour(neighbourPosition, neighbour);
    // }

    public virtual GameObject GetBuildingPrefabForPosition(Vector3Int position)
    {
        return buildings.GetValueOrDefault(position, null)?.gameObject;
    }

    public string GetDescription()
    {
        return buildingData.Description;
    }

    public SelectionManager GetSelectionManagerForGridPosition(Vector3Int gridPosition)
    {
        return selectionManagers.GetValueOrDefault(gridPosition, null);
    }
    public BuildingData GetBuildingData()
    {
        return buildingData;
    }
}
