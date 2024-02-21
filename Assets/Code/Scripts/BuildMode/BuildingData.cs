using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BuildingType {
    Road, Building, Zone
}

[CreateAssetMenu]
public class BuildingData : ScriptableObject
{
   public string buildingName;
   public BuildingType buildingType;
   public int price;
   public Vector2Int size;
   public bool isAvailable;
   public Sprite image;
}
