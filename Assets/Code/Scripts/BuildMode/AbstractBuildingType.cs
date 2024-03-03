using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public abstract class AbstractBuildingType : MonoBehaviour
{
    public string buildingName {get; private set;}
    private BuildingData buildingData;
    public Vector2Int size {get; set;}
    public List<Vector3Int> gridPositions {get; set;} = new List<Vector3Int>();
    public bool isAvailable {get; set;}
    public GameObject building {get; set;}

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
    }
    
    
}
