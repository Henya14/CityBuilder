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
    public void TileSelected(Tile tile, List<Vector3Int> selectedTilesGridPositions, Vector3 selectionCenter) {

        var gridPositions = selectedTilesGridPositions.Select( p => new Vector3Int(p.x, p.y + 1, p.z)).ToList();
        var gamePositionY = gridManager.GetGamePositionForGridPosition(gridPositions[0]).y;
        var placePosition = new Vector3(selectionCenter.x, gamePositionY, selectionCenter.z);
        selectedBuilding.PlaceAtPosition(gridPositions, placePosition);
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
