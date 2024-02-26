using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEngine;



public class BuildModeManager : MonoBehaviour
{
    private AbstractBuildingType selectedBuilding;
    private GridManager gridManager;
    void Start() {
        gridManager = FindObjectOfType<GridManager>();
    }
    public void TileSelected(Tile tile, Vector3 selectionCenter) {
        var gridPosition = new Vector3Int(tile.gridPosition.x, tile.gridPosition.y + 1, tile.gridPosition.z);
        var gamePositionY = gridManager.GetGamePositionForGridPosition(gridPosition).y;
        var placePosition = new Vector3(selectionCenter.x, gamePositionY, selectionCenter.z);
        selectedBuilding.PlaceAtPosition(gridPosition, placePosition);
        gridManager.AddBuildingToGrid(selectedBuilding);
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
        selectedBuilding = CreateBuildingFromBuildingData(buildingData);
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
