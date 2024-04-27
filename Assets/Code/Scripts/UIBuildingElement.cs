using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "New Building", menuName = "Buildings/Building")]
public class UIBuildingElement : ScriptableObject
{
    [SerializeField] Image buildingPicture;
    [SerializeField] GameObject buildingPrefab;
    [SerializeField] Vector3 scales;
    // Start is called before the first frame update
    public Image BuildingPicture {
        get { return buildingPicture; }
    }
    public GameObject BuildingPrefab {
        get { return buildingPrefab; }
    }
    public Vector3 Scales {
        get { return scales; }
    }
}
