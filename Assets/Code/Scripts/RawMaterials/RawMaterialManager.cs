using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class RawMaterialManager : MonoBehaviour
{
    [SerializeField] private List<RawMaterialWithRearity> rawMaterials;
    [SerializeField] private Dictionary<Rect,string> rawMaterialPlaces;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public  List<RawMaterialWithRearity> GetRawMaterials(){
        if(rawMaterials == null || rawMaterials.Count == 0)
        {
            LoadRawMaterials();
        }
        return rawMaterials;
    }
    public void LoadRawMaterials()
    {
        //Beolvasas
        rawMaterials = new List<RawMaterialWithRearity>();
        rawMaterialPlaces = new Dictionary<Rect, string>();
        rawMaterials = ResourceImporter.GetRawMaterials();
        var resourceManager = FindAnyObjectByType<ResourceManager>();
        foreach (var rawMaterial in rawMaterials)
        {
            resourceManager.AddResourceAsRaw(rawMaterial);
        }

        if(rawMaterials.Count == 0) 
        {
            Debug.LogWarning("Raw Materials not found");
        }
    }
    public void AddRect(Rect rect, string type)
    {
        if(rawMaterialPlaces == null)
        {
            rawMaterialPlaces = new Dictionary<Rect, string>();
        }
        if(!rawMaterialPlaces.ContainsKey(rect))
        {
            rawMaterialPlaces.Add(rect,type);
            /*
            var sp1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var sp2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sp1.transform.position = (new Vector3(rect.xMin,0,rect.yMin));
            sp2.transform.position = (new Vector3(rect.xMax, 0, rect.yMax));
            sp1.name = "TopRes";
            sp2.name = "BottomRes";
            */
        }
    }
    public bool IsOnRawMaterial(Rect rect)
    {
        bool overlaps=false;
        foreach(var raw in rawMaterialPlaces.Keys)
        {
            if(raw.Overlaps(rect, true)){
                overlaps = true;
                break;
            }
        }
        return overlaps;
    }
}
