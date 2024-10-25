using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class RawMaterialManager : MonoBehaviour
{
    public static List<RawMaterialWithRearity> rawMaterials;
    [SerializeField] public MapGenarator mapGenarator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static List<RawMaterialWithRearity> GetRawMaterials(){
        if(rawMaterials == null)
        {
            LoadRawMaterials();
        }
        return rawMaterials;
    }
    public static void LoadRawMaterials()
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

        
    }
}
