using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class RawMaterialManager : MonoBehaviour
{
    [SerializeField] private List<RawMaterialWithRearity> rawMaterials;
    [SerializeField] private List<Rect> RawMaterialPlaces;
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
        rawMaterials = ResourceImporter.GetRawMaterials();
        foreach(var rawMaterial in rawMaterials)
        {
            var resourceManager = FindAnyObjectByType<ResourceManager>();
            var o = new GameObject(rawMaterial.Type);
            o.transform.parent = resourceManager.transform;
            Resource.CreateComponent(o, rawMaterial.Type, "", rawMaterial.GatheredAmountPerHour, new Dictionary<string, float>());
        }

        if(rawMaterials.Count == 0) 
        {
            Debug.LogWarning("Raw Materials not found");
        }
    }
    public void AddRect(Rect rect)
    {
        if(RawMaterialPlaces == null)
        {
            RawMaterialPlaces = new List<Rect>();
        }
        RawMaterialPlaces.Add(rect);
    }
}
