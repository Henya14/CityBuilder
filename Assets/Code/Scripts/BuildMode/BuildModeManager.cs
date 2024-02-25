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

    public void TileSelected(Tile tile) {
        selectedBuilding.PlaceAtPosition(tile.positionInt);
    }

    public List<BuildingData> GetBuildingDatas() {
        List<BuildingData> buildingDatas = Resources.LoadAll<BuildingData>("Buildings").ToList(); 
        Debug.Log(buildingDatas);
        Debug.Log(buildingDatas.Count);
        buildingDatas = buildingDatas.Select(bd => {
            bd.isAvailable = UnityEngine.Random.Range(0, 2) == 1? true: false;
            return bd;
        }).ToList();
        return buildingDatas;
    }
    public List<AbstractBuildingType> GetBuildings() {
        List<BuildingData> buildingDatas = Resources.LoadAll<BuildingData>("Buildings").ToList(); 
        var buildings = buildingDatas.Select(bd => {
            var newBuilding = CreateBuildingFromBuildingData(bd);
            newBuilding.isAvailable =  UnityEngine.Random.Range(0, 2) == 1? true: false;
            return newBuilding;
        }).ToList();
        return buildings;
    }

    private AbstractBuildingType CreateBuildingFromBuildingData(BuildingData data) {
        
        switch (data.buildingType) {
            case BuildingType.Road:
                return new Road(data.name, data.size); 
            case BuildingType.Building:
                return new Building(data.name, data.size); 
            case BuildingType.Zone:
                return new Zone(data.name, data.size); 
            default: 
                throw new Exception("Unknown Building Type");
        }

    }
}
