using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
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

    public Dictionary<Vector3Int, NeighbourData> neighbourDatasForPositions { get; private set; } = new Dictionary<Vector3Int, NeighbourData>();

    public virtual void Init(BuildingData buildingData)
    {
        this.buildingData = buildingData;
    }

    public virtual void PlaceAtPosition(List<Vector3Int> gridPositions, List<Vector3> prefabPlacePositions, Dictionary<Vector3Int, NeighbourData> neigbours)
    {
        Remove();
        this.gridPositions.AddRange(gridPositions);
        foreach (var gamePosition in prefabPlacePositions)
        {
            var building = Instantiate(buildingData.prefab);
            building.transform.position = gamePosition;
            building.AddComponent<SelectionManager>();
            buildings.Add(gridPositions[0], building);
        }


        neighbourDatasForPositions = neigbours;
        foreach (var neighbourDatasForPosition in neighbourDatasForPositions)
        {
            foreach (var neighbourForGridPosition in neighbourDatasForPosition.Value.neighboursForGridPositions)
            {
                var currentTilePosition = neighbourDatasForPosition.Key;
                var neigbourPosition = neighbourForGridPosition.Key;
                var neighbour = neighbourForGridPosition.Value;
                if (neighbour != null)
                {
                    neighbour.SetNeighbourForPosition(neigbourPosition, currentTilePosition, this);
                }
            }
        }
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

    public void SetNeighbourForPosition(Vector3Int position, Vector3Int neighbourPosition, AbstractBuildingType neighbour)
    {
        var neighbourDataForPosition = neighbourDatasForPositions[position];
        neighbourDataForPosition.SetNeighbour(neighbourPosition, neighbour);
    }

    public void GetBuildingPrefabForPosition(Vector3Int position) {
        //return buildings.Values.ToList()[0].gameObject;
    }
}
