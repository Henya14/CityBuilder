using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public enum BuildingType {
    Road, IndividualBuilding, Zone

    
}

public static class BuildingTypeMethods {
    public static SelectableObjectType ToSelectableObjectType(this BuildingType type) {

        switch (type) {
            case BuildingType.Road:
                return SelectableObjectType.Road;
            case BuildingType.IndividualBuilding:
                return SelectableObjectType.Building;
            case BuildingType.Zone:
                return SelectableObjectType.Zone;
            default:
                return SelectableObjectType.Building;
        }
    }
}

[CreateAssetMenu]
public class BuildingData : ScriptableObject
{
   [SerializeField] public string Name;
   [SerializeField] public string Description;
   public BuildingType buildingType;
   public int price;
   public Vector2Int size;
   public bool isAvailable;
   public StyleBackground image;


    [SerializeField] public GameObject prefab;
    [SerializeField] public Texture2D buildingPicture;
    [SerializeField] Vector3 scales;
    [SerializeField] public bool buyableBuilding = false;

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
        get { return Name; }
    }
}
