using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public abstract class AbstractBuildingType : MonoBehaviour
{
    public string buildingName {get; private set;}
    private BuildingData buildingData;
    public Vector2Int size {get; set;}
    public Vector3Int gridPosition {get; set;}

    public bool isAvailable {get; set;}
    public GameObject building {get; set;}

    public virtual void Init(BuildingData buildingData) {
        this.buildingData = buildingData;
       
    }

    public virtual void PlaceAtPosition(Vector3Int gridPosition, Vector3 gamePosition) {
        this.gridPosition = gridPosition;
        var building = Instantiate(buildingData.prefab);
        building.transform.position = gamePosition;
    }

    public virtual void Remove() {
        gridPosition = new Vector3Int(-1, -1, -1);
    }
    
    
}
