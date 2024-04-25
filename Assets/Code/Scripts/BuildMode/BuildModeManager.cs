using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEngine;



public class BuildModeManager : MonoBehaviour
{
    private BuildingData selectedBuildingData;
    private GridManager gridManager;
    void Start() {
        gridManager = FindObjectOfType<GridManager>();
    }
    public void ObjectSelected(Dictionary<Vector3, List<Vector3Int>> placingPositionsWithGridPositions)
    {
        var gridPositions = new List<Vector3Int>();
        placingPositionsWithGridPositions.Values.ToList().ForEach(v => gridPositions.AddRange(v));
        gridPositions = gridPositions.Select(p => new Vector3Int(p.x, p.y + 1, p.z)).ToList();

        var selectedBuilding = CreateBuildingFromBuildingData(selectedBuildingData);
        List<Vector3> placePositions = new List<Vector3>();
        Dictionary<Vector3, List<Vector3Int>> updatedPlacingPositionsWithGridPositions = new Dictionary<Vector3, List<Vector3Int>>();
        foreach (var prefabPlacePosition in placingPositionsWithGridPositions.Keys)
        {
            var gamePositionY = gridManager.GetGamePositionForGridPosition(gridPositions[0]).y;
            var placePosition = new Vector3(prefabPlacePosition.x, gamePositionY, prefabPlacePosition.z);
            placePositions.Add(placePosition);
            updatedPlacingPositionsWithGridPositions[placePosition] = placingPositionsWithGridPositions[prefabPlacePosition].Select(p => new Vector3Int(p.x, p.y + 1, p.z)).ToList();
        }

        gridManager.AddBuildingToGrid(selectedBuilding, gridPositions);
        Dictionary<Vector3Int, NeighbourData> neigbours = GetNeighboursForGridPositions(gridPositions);
        selectedBuilding.PlaceAtPosition(updatedPlacingPositionsWithGridPositions, neigbours);
    }

    private Dictionary<Vector3Int, NeighbourData> GetNeighboursForGridPositions(List<Vector3Int> gridPositions)
    {
        var neigbours = new Dictionary<Vector3Int, NeighbourData>();
        foreach (var gridPosition in gridPositions)
        {
            var neighbourDictionary = gridManager.GetNeigbouringBuildingsOfTile(gridPosition);
            var neighbourData = new NeighbourData(neighbourDictionary);
            neigbours[gridPosition] = neighbourData;
        }

        return neigbours;
    }

    public List<BuildingData> GetBuildingDatas() {
        List<BuildingData> buildingDatas = Resources.LoadAll<BuildingData>("Buildings").ToList(); 
        buildingDatas = buildingDatas.Select(bd => {
            bd.isAvailable = UnityEngine.Random.Range(0, 2) == 1? true: false;
            return bd;
        }).ToList();
        return buildingDatas;
    }

    public void BuildingDataSelected(BuildingData buildingData) {
        selectedBuildingData = buildingData;
    }

    private AbstractBuildingType CreateBuildingFromBuildingData(BuildingData data) {
        
        switch (data.buildingType) {
            case BuildingType.Road:
                var createdRoad = gameObject.AddComponent<Road>();
                createdRoad.Init(data);
                return createdRoad; 
            case BuildingType.Building:
                var createdBuilding = gameObject.AddComponent<Building>();
                createdBuilding.Init(data);
                return createdBuilding; 
            case BuildingType.Zone:
                var createdZone = gameObject.AddComponent<Zone>();
                createdZone.Init(data);
                return createdZone; 
            default: 
                throw new Exception("Unknown Building Type");
        }

    }
}
