using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

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
   public StyleBackground image;


    [SerializeField] public GameObject prefab;
    [SerializeField] public Texture2D buildingPicture;
    [SerializeField] Vector3 scales;
    [SerializeField] bool buyableBuilding = false;

    public Texture2D BuildingPicture {
        get { return buildingPicture; }
    }
    public GameObject Prefab {
        get { return prefab; }
    }
    public Vector3 Scales {
        get { return scales; }
    }
    public bool BuyableBuilding {
        get { return buyableBuilding; }
    }
    public string BuildingName {
        get { return buildingName; }
    }
}
