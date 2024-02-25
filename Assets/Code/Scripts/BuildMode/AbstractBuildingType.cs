using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractBuildingType : MonoBehaviour
{
    public string buildingName {get; private set;}
    public Vector2Int size {get; set;}
    public Vector3Int? placedAtPosition;
    public bool isAvailable {get; set;}


    protected AbstractBuildingType(string name, Vector2Int size) {
        buildingName = name;
        this.size = size;
    }

    public virtual void PlaceAtPosition(Vector3Int position) {
        placedAtPosition = position;
    }

    public virtual void Remove() {
        placedAtPosition = null;
    }
}
